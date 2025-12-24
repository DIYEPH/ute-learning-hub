"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import {
  ArrowLeft, Upload, Loader2, X,
  Image as ImageIcon, FileText, ChevronRight, AlertCircle
} from "lucide-react";

import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import {
  DocumentUploadForm,
  type DocumentUploadFormData,
} from "@/src/components/documents/document-upload-form";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import {
  postApiDocument,
  postApiDocumentByIdFiles,
} from "@/src/api/database/sdk.gen";
import type {
  CreateDocumentCommand,
  DocumentDetailDto,
  AddDocumentFileCommand,
} from "@/src/api/database/types.gen";

const MAX_SIZE = 100 * 1024 * 1024;

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const { uploadFile } = useFileUpload();

  const [step, setStep] = useState<1 | 2>(1);
  const [formData, setFormData] = useState<DocumentUploadFormData | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [cover, setCover] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [title, setTitle] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sizeError, setSizeError] = useState<string | null>(null);

  const onCoverChange = (f: File | null) => {
    setCover(f);
    if (!f) return setPreview(null);
    const r = new FileReader();
    r.onloadend = () => setPreview(r.result as string);
    r.readAsDataURL(f);
  };

  const createDocument = async () => {
    if (!formData || !file) return setError("Vui lòng chọn file");

    setLoading(true); setError(null);
    try {
      let coverId: string | null = null;
      if (formData.coverFile)
        coverId = (await uploadFile(formData.coverFile, "DocumentCover")).id ?? null;

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

      const main = await uploadFile(file, "DocumentFile");
      let chapterCoverId: string | null = null;
      if (cover) chapterCoverId = (await uploadFile(cover, "DocumentFileCover")).id ?? null;

      const fileBody: AddDocumentFileCommand = {
        documentId: doc.id,
        fileId: main.id,
        coverFileId: chapterCoverId,
        title: title.trim() || null,
      };

      await postApiDocumentByIdFiles({ path: { id: doc.id }, body: fileBody });
      router.push(`/documents/${doc.id}`);
    } catch (e: any) {
      setError(e?.response?.data?.message || e?.message || "Không thể tạo tài liệu");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-4 md:p-6 max-w-4xl mx-auto space-y-6">

      <div className="flex items-center gap-4">
        <h1 className="text-2xl font-semibold">{t("uploadTitle")}</h1>
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <span className={step === 1 ? "text-primary font-medium" : ""}>1</span>
          <ChevronRight className="h-4 w-4" />
          <span className={step === 2 ? "text-primary font-medium" : ""}>2</span>
        </div>
      </div>

      {error && <div className="p-3 text-sm text-red-600 bg-red-50 rounded">{error}</div>}

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

      {step === 2 && formData && (
        <>
          <div className="p-4 bg-slate-50 rounded">
            <h3 className="text-sm font-semibold flex gap-2 mb-1">
              <FileText className="h-4 w-4" /> {formData.documentName}
            </h3>
            {formData.description && (
              <p className="text-xs text-slate-500 line-clamp-1">{formData.description}</p>
            )}
          </div>

          <div className="border p-4 rounded space-y-4">
            <div className="grid md:grid-cols-3 gap-4">
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
                      setFile(null); e.target.value = "";
                      return;
                    }
                    setSizeError(null); setFile(f);
                  }}
                />
                <label htmlFor="file" className="mt-1 flex gap-2 p-2 border border-dashed rounded cursor-pointer">
                  <Upload size={16} /> <span className="truncate">{file?.name ?? "Chọn file"}</span>
                </label>
                {sizeError && (
                  <p className="text-xs text-red-600 flex gap-1 mt-1">
                    <AlertCircle size={12} /> {sizeError}
                  </p>
                )}
              </div>

              <div>
                <Label className="text-xs">Tiêu đề</Label>
                <Input value={title} onChange={e => setTitle(e.target.value)} className="mt-1 h-9" />
              </div>

              <div>
                <Label className="text-xs">Ảnh bìa</Label>
                <input id="cover" type="file" accept="image/*" className="hidden"
                  onChange={e => onCoverChange(e.target.files?.[0] || null)} />
                <label htmlFor="cover" className="mt-1 flex gap-2 p-2 border border-dashed rounded cursor-pointer">
                  <ImageIcon size={16} /> <span className="truncate">{cover?.name ?? "Chọn ảnh"}</span>
                </label>
              </div>
            </div>

            {preview && (
              <div className="relative inline-block">
                <img src={preview} className="h-20 rounded border" />
                <button onClick={() => onCoverChange(null)}
                  className="absolute -top-1 -right-1 bg-red-500 text-white rounded-full p-0.5">
                  <X size={12} />
                </button>
              </div>
            )}
          </div>

          <div className="flex gap-4">
            <Button variant="outline" onClick={() => setStep(1)}>
              <ArrowLeft className="mr-1 h-4 w-4" /> Quay lại
            </Button>
            <Button disabled={loading || !file} onClick={createDocument}>
              {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4 mr-1" />}
              Tạo tài liệu
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
