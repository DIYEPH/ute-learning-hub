"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import {
  ArrowLeft, Upload, Loader2,
  FileText, ChevronRight, AlertCircle, Plus, Trash2
} from "lucide-react";

import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import {
  DocumentUploadForm,
  type DocumentUploadFormData,
} from "@/src/components/documents/document-upload-form";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { usePdfThumbnail } from "@/src/hooks/use-pdf-thumbnail";
import {
  postApiDocument,
  postApiDocumentByIdFiles,
} from "@/src/api";
import type {
  CreateDocumentCommand,
  DocumentDetailDto,
  AddDocumentFileCommand,
} from "@/src/api/database/types.gen";

const MAX_SIZE = 100 * 1024 * 1024;

interface PendingFile {
  id: string;
  file: File;
  title: string;
}

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const { uploadFile } = useFileUpload();
  const { extractThumbnail } = usePdfThumbnail({ width: 600, quality: 0.85 });

  const [step, setStep] = useState<1 | 2>(1);
  const [formData, setFormData] = useState<DocumentUploadFormData | null>(null);
  const [pendingFiles, setPendingFiles] = useState<PendingFile[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Form inputs for adding a file
  const [currentFile, setCurrentFile] = useState<File | null>(null);
  const [currentTitle, setCurrentTitle] = useState("");
  const [sizeError, setSizeError] = useState<string | null>(null);



  const addFileToPending = () => {
    if (!currentFile) return;
    setPendingFiles(prev => [
      ...prev,
      {
        id: crypto.randomUUID(),
        file: currentFile,
        title: currentTitle.trim(),
      }
    ]);
    // Reset form
    setCurrentFile(null);
    setCurrentTitle("");
  };

  const removeFile = (id: string) => {
    setPendingFiles(prev => prev.filter(f => f.id !== id));
  };

  const createDocument = async () => {
    if (!formData || pendingFiles.length === 0) return;

    setLoading(true);
    setError(null);
    try {
      // 1. Upload document cover - auto-extract from first PDF if not provided
      let coverId: string | null = null;
      if (formData.coverFile) {
        coverId = (await uploadFile(formData.coverFile, "DocumentCover")).id ?? null;
      } else {
        // Try to auto-extract thumbnail from first PDF file
        const firstPdf = pendingFiles.find(pf =>
          pf.file.type.includes("pdf") || pf.file.name.toLowerCase().endsWith(".pdf")
        );
        if (firstPdf) {
          const thumbnail = await extractThumbnail(firstPdf.file);
          if (thumbnail) {
            const uploaded = await uploadFile(thumbnail, "DocumentCover");
            coverId = uploaded.id ?? null;
          }
        }
      }

      // 2. Create document
      const body: CreateDocumentCommand = {
        documentName: formData.documentName || "",
        description: formData.description || "",
        subjectId: formData.subjectId || null,
        typeId: formData.typeId!,
        authorIds: formData.authorIds?.length ? formData.authorIds : null,
        authors: formData.authors?.length ? formData.authors : null,
        tagIds: formData.tagIds?.length ? formData.tagIds : null,
        tagNames: formData.tagNames?.length ? formData.tagNames : null,
        visibility: (formData.visibility as any) ?? 0,
        coverFileId: coverId,
      };

      const doc = (await postApiDocument({ body })).data as DocumentDetailDto;
      if (!doc?.id) throw new Error("Không thể tạo tài liệu");

      // 3. Upload all files with auto-cover
      for (const pf of pendingFiles) {
        const mainFile = await uploadFile(pf.file, "DocumentFile");

        // Auto-extract cover for each file
        let chapterCoverId: string | null = null;
        const isPdf = pf.file.type.includes("pdf") || pf.file.name.toLowerCase().endsWith(".pdf");
        const isImage = pf.file.type.startsWith("image/");

        if (isPdf) {
          const thumbnail = await extractThumbnail(pf.file);
          if (thumbnail) {
            const coverFile = await uploadFile(thumbnail, "DocumentFileCover");
            chapterCoverId = coverFile.id ?? null;
          }
        } else if (isImage) {
          chapterCoverId = mainFile.id ?? null;
        }

        const fileBody: AddDocumentFileCommand = {
          documentId: doc.id,
          fileId: mainFile.id,
          coverFileId: chapterCoverId,
          title: pf.title || null,
        };

        await postApiDocumentByIdFiles({ path: { id: doc.id }, body: fileBody });
      }

      router.push(`/documents/${doc.id}`);
    } catch (e: any) {
      setError(e?.response?.data?.message || e?.message || "Không thể tạo tài liệu");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-4 md:p-6 max-w-4xl mx-auto space-y-6">

      {/* Header with Stepper */}
      <div className="space-y-2">
        <h1 className="text-2xl font-semibold">{t("uploadTitle")}</h1>
        <div className="flex items-center gap-2 text-sm">
          <div className={`flex items-center gap-1.5 ${step === 1 ? "text-primary font-medium" : "text-muted-foreground"}`}>
            <span className={`flex items-center justify-center w-6 h-6 rounded-full text-xs font-medium ${step === 1 ? "bg-primary text-primary-foreground" : "bg-muted"}`}>1</span>
            <span>Thông tin tài liệu</span>
          </div>
          <ChevronRight className="h-4 w-4 text-muted-foreground" />
          <div className={`flex items-center gap-1.5 ${step === 2 ? "text-primary font-medium" : "text-muted-foreground"}`}>
            <span className={`flex items-center justify-center w-6 h-6 rounded-full text-xs font-medium ${step === 2 ? "bg-primary text-primary-foreground" : "bg-muted"}`}>2</span>
            <span>Thêm chương/file</span>
          </div>
        </div>
      </div>

      {error && <div className="p-3 text-sm text-red-600 bg-red-50 rounded">{error}</div>}

      {/* Step 1: Document Info */}
      {step === 1 && (
        <>
          <DocumentUploadForm
            initialData={formData ?? undefined}
            loading={loading}
            onSubmit={d => { setFormData(d); setStep(2); }}
          />
          <div className="flex gap-4">
            <Button variant="outline" onClick={() => router.back()}>{tCommon("cancel")}</Button>
            <Button onClick={() =>
              (document.getElementById("document-upload-form") as HTMLFormElement)?.requestSubmit()
            }>
              Tiếp theo <ChevronRight className="ml-1 h-4 w-4" />
            </Button>
          </div>
        </>
      )}

      {/* Step 2: Add Files */}
      {step === 2 && formData && (
        <>
          {/* Document preview */}
          <div className="p-4 bg-muted rounded">
            <h3 className="text-sm font-semibold flex gap-2 mb-1">
              <FileText className="h-4 w-4" /> {formData.documentName}
            </h3>
            {formData.description && (
              <p className="text-xs text-muted-foreground line-clamp-1">{formData.description}</p>
            )}
          </div>

          {/* Pending files list */}
          {pendingFiles.length > 0 && (
            <div className="border rounded p-4 space-y-2">
              <h4 className="text-sm font-medium">Danh sách chương/file ({pendingFiles.length})</h4>
              {pendingFiles.map((pf, idx) => (
                <div key={pf.id} className="flex items-center gap-3 p-2 bg-muted/50 rounded">
                  <span className="text-xs text-muted-foreground w-6">{idx + 1}.</span>
                  <FileText className="h-4 w-4 text-muted-foreground shrink-0" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{pf.title || pf.file.name}</p>
                    <p className="text-xs text-muted-foreground">{(pf.file.size / 1024 / 1024).toFixed(1)} MB</p>
                  </div>
                  <Button variant="ghost" size="icon-sm" onClick={() => removeFile(pf.id)}>
                    <Trash2 className="h-4 w-4 text-red-500" />
                  </Button>
                </div>
              ))}
            </div>
          )}

          {/* Add file form */}
          <div className="border p-4 rounded space-y-4">
            <h4 className="text-sm font-medium flex items-center gap-2">
              <Plus className="h-4 w-4" /> Thêm chương/file
            </h4>
            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <Label className="text-xs">File *</Label>
                <input
                  id="file"
                  type="file"
                  accept=".pdf,image/*"
                  className="hidden"
                  onChange={e => {
                    const f = e.target.files?.[0] || null;
                    if (f && f.size > MAX_SIZE) {
                      setSizeError("File > 100MB");
                      setCurrentFile(null);
                      e.target.value = "";
                      return;
                    }
                    setSizeError(null);
                    setCurrentFile(f);
                  }}
                />
                <label htmlFor="file" className="mt-1 flex gap-2 p-2 border border-dashed rounded cursor-pointer hover:bg-muted/50">
                  <Upload size={16} /> <span className="truncate text-sm">{currentFile?.name ?? "Chọn file"}</span>
                </label>
                {sizeError && (
                  <p className="text-xs text-red-600 flex gap-1 mt-1">
                    <AlertCircle size={12} /> {sizeError}
                  </p>
                )}
              </div>

              <div>
                <Label className="text-xs">Tiêu đề (tùy chọn)</Label>
                <Input
                  value={currentTitle}
                  onChange={e => setCurrentTitle(e.target.value)}
                  className="mt-1 h-9"
                  placeholder="Chương 1, Chương 2..."
                />
              </div>
            </div>

            <Button
              variant="secondary"
              size="sm"
              onClick={addFileToPending}
              disabled={!currentFile}
            >
              <Plus className="h-4 w-4 mr-1" /> Thêm vào danh sách
            </Button>
          </div>

          {/* Actions */}
          <div className="flex gap-4">
            <Button variant="outline" onClick={() => setStep(1)}>
              <ArrowLeft className="mr-1 h-4 w-4" /> Quay lại
            </Button>
            <Button
              disabled={loading || pendingFiles.length === 0}
              onClick={createDocument}
            >
              {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4 mr-1" />}
              Tạo tài liệu ({pendingFiles.length} file)
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
