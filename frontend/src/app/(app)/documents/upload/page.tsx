"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { DocumentUploadForm, type DocumentUploadFormData } from "@/src/components/documents/document-upload-form";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { postApiDocument, postApiDocumentByIdFiles } from "@/src/api/database/sdk.gen";
import type { CreateDocumentCommand, DocumentDetailDto, AddDocumentFileCommand } from "@/src/api/database/types.gen";
import { ArrowLeft, Upload, Loader2, X, Image as ImageIcon, FileText, ChevronRight, AlertCircle } from "lucide-react";

// File size limit: 100MB (matches backend)
const MAX_FILE_SIZE_MB = 100;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();

  // Wizard state
  const [step, setStep] = useState<1 | 2>(1);
  const [formData, setFormData] = useState<DocumentUploadFormData | null>(null);

  // Step 2 state
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [selectedCoverFile, setSelectedCoverFile] = useState<File | null>(null);
  const [coverPreview, setCoverPreview] = useState<string | null>(null);
  const [chapterTitle, setChapterTitle] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fileSizeError, setFileSizeError] = useState<string | null>(null);

  const { uploadFile } = useFileUpload();

  // Step 1: Save form data and go to Step 2
  const handleStep1Submit = async (data: DocumentUploadFormData) => {
    setFormData(data);
    setStep(2);
    setError(null);
  };

  // Handle cover file change in Step 2
  const handleCoverFileChange = (file: File | null) => {
    setSelectedCoverFile(file);
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setCoverPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    } else {
      setCoverPreview(null);
    }
  };

  // Step 2: Create document with first file
  const handleCreateDocument = async () => {
    if (!formData || !selectedFile) {
      setError("Vui lòng chọn file để tải lên");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // 1. Upload document cover image (if any)
      let documentCoverFileId: string | null = null;
      if (formData.coverFile) {
        const uploadedCover = await uploadFile(formData.coverFile, "DocumentCover");
        documentCoverFileId = uploadedCover.id ?? null;
      }

      // 2. Create the document
      const body: CreateDocumentCommand = {
        documentName: formData.documentName || "",
        description: formData.description || "",
        subjectId: formData.subjectId || null,
        typeId: formData.typeId!,
        authorIds:
          formData.authorIds && formData.authorIds.length > 0
            ? formData.authorIds
            : null,
        authors:
          formData.authors && formData.authors.length > 0
            ? formData.authors
            : null,
        tagIds:
          formData.tagIds && formData.tagIds.length > 0 ? formData.tagIds : null,
        tagNames:
          formData.tagNames && formData.tagNames.length > 0 ? formData.tagNames : null,
        visibility: (formData.visibility as any) ?? 0,
        coverFileId: documentCoverFileId,
      };

      const response = await postApiDocument({ body });
      const created = (response.data ?? response) as DocumentDetailDto | undefined;

      if (!created?.id) {
        throw new Error("Không thể tạo tài liệu");
      }

      // 3. Upload the first chapter/file
      const mainFile = await uploadFile(selectedFile, "DocumentFile");

      // Upload chapter cover (if any)
      let chapterCoverFileId: string | undefined;
      if (selectedCoverFile) {
        const coverFile = await uploadFile(selectedCoverFile, "DocumentFileCover");
        chapterCoverFileId = coverFile.id;
      }

      // 4. Add the file to the document
      const fileBody: AddDocumentFileCommand = {
        documentId: created.id,
        fileId: mainFile.id,
        coverFileId: chapterCoverFileId ?? null,
        title: chapterTitle.trim() || null,
      };

      await postApiDocumentByIdFiles({
        path: { id: created.id },
        body: fileBody,
        throwOnError: true,
      });

      // 5. Redirect to the document detail page
      router.push(`/documents/${created.id}`);
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể tạo tài liệu";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Go back to Step 1
  const handleBack = () => {
    setStep(1);
    setError(null);
  };

  return (
    <div className="p-4 md:p-6 max-w-4xl mx-auto">
      {/* Header with step indicator */}
      <div className="flex items-center gap-4 mb-6">
        <h1 className="text-2xl font-semibold text-foreground">
          {t("uploadTitle")}
        </h1>
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <span className={step === 1 ? "text-primary font-medium" : ""}>
            1. Thông tin
          </span>
          <ChevronRight className="h-4 w-4" />
          <span className={step === 2 ? "text-primary font-medium" : ""}>
            2. Tải file
          </span>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}

      {/* Step 1: Document Info Form */}
      {step === 1 && (
        <>
          <DocumentUploadForm
            initialData={formData ?? undefined}
            onSubmit={handleStep1Submit}
            loading={loading}
          />
          <div className="flex gap-4 mt-6">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.back()}
              disabled={loading}
            >
              {tCommon("cancel")}
            </Button>
            <Button
              type="button"
              onClick={() => {
                const form = document.getElementById("document-upload-form") as HTMLFormElement;
                if (form) {
                  form.requestSubmit();
                }
              }}
              disabled={loading}
            >
              Tiếp theo
              <ChevronRight className="ml-1 h-4 w-4" />
            </Button>
          </div>
        </>
      )}

      {/* Step 2: Upload First File */}
      {step === 2 && formData && (
        <div className="space-y-6">
          {/* Document Summary */}
          <div className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-lg">
            <h3 className="text-sm font-semibold text-foreground mb-3 flex items-center gap-2">
              <FileText className="h-4 w-4" />
              Thông tin tài liệu
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
              <div>
                <span className="text-slate-500">Tên:</span>{" "}
                <span className="font-medium">{formData.documentName}</span>
              </div>
              {formData.description && (
                <div className="md:col-span-2">
                  <span className="text-slate-500">Mô tả:</span>{" "}
                  <span className="line-clamp-1">{formData.description}</span>
                </div>
              )}
            </div>
          </div>

          {/* File Upload Section */}
          <div className="border border-slate-200 dark:border-slate-700 rounded-lg p-4">
            <h3 className="text-sm font-semibold text-foreground mb-4">
              Tải lên chương/file đầu tiên <span className="text-red-500">*</span>
            </h3>

            <div className="grid gap-4 md:grid-cols-3">
              {/* File selection */}
              <div>
                <Label className="text-xs">
                  Tệp PDF/Ảnh <span className="text-red-500">*</span>
                </Label>
                <input
                  type="file"
                  accept=".pdf,image/*"
                  onChange={(e) => {
                    const file = e.target.files?.[0] || null;
                    if (file && file.size > MAX_FILE_SIZE_BYTES) {
                      setFileSizeError(`File quá lớn! Giới hạn ${MAX_FILE_SIZE_MB}MB, file của bạn ${(file.size / 1024 / 1024).toFixed(1)}MB`);
                      setSelectedFile(null);
                      e.target.value = "";
                      return;
                    }
                    setFileSizeError(null);
                    setSelectedFile(file);
                  }}
                  className="hidden"
                  id="upload-file"
                  disabled={loading}
                />
                <label
                  htmlFor="upload-file"
                  className={`mt-1 flex items-center gap-2 px-3 py-2 border border-dashed rounded cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 text-sm transition-colors ${loading ? "opacity-50 cursor-not-allowed" : ""
                    } ${selectedFile ? "border-primary bg-primary/5" : ""} ${fileSizeError ? "border-red-300 bg-red-50 dark:bg-red-950/30" : ""}`}
                >
                  <Upload size={16} />
                  <span className="truncate">
                    {selectedFile ? selectedFile.name : "Chọn file"}
                  </span>
                </label>
                <p className="mt-1 text-[10px] text-slate-400">Tối đa {MAX_FILE_SIZE_MB}MB</p>
                {fileSizeError && (
                  <p className="mt-1 text-xs text-red-600 dark:text-red-400 flex items-center gap-1">
                    <AlertCircle size={12} />
                    {fileSizeError}
                  </p>
                )}
              </div>

              {/* Title input */}
              <div>
                <Label className="text-xs">Tiêu đề chương (tùy chọn)</Label>
                <Input
                  type="text"
                  value={chapterTitle}
                  onChange={(e) => setChapterTitle(e.target.value)}
                  placeholder="Chương I, Phần 1..."
                  className="mt-1 h-9 text-sm"
                  disabled={loading}
                />
              </div>

              {/* Cover image selection */}
              <div>
                <Label className="text-xs">Ảnh bìa chương (tùy chọn)</Label>
                <input
                  type="file"
                  accept="image/*"
                  onChange={(e) => {
                    const file = e.target.files?.[0] || null;
                    handleCoverFileChange(file);
                  }}
                  className="hidden"
                  id="upload-cover"
                  disabled={loading}
                />
                <label
                  htmlFor="upload-cover"
                  className={`mt-1 flex items-center gap-2 px-3 py-2 border border-dashed rounded cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 text-sm transition-colors ${loading ? "opacity-50 cursor-not-allowed" : ""
                    }`}
                >
                  <ImageIcon size={16} />
                  <span className="truncate">
                    {selectedCoverFile ? selectedCoverFile.name : "Chọn ảnh"}
                  </span>
                </label>
              </div>
            </div>

            {/* Cover preview */}
            {coverPreview && (
              <div className="mt-3 relative inline-block">
                <img
                  src={coverPreview}
                  alt="Preview"
                  className="h-20 w-auto object-contain rounded border border-slate-200 dark:border-slate-700"
                />
                <button
                  type="button"
                  onClick={() => handleCoverFileChange(null)}
                  disabled={loading}
                  className="absolute -top-1 -right-1 p-0.5 bg-red-500 text-white rounded-full hover:bg-red-600 transition-colors"
                  title="Xóa ảnh"
                >
                  <X size={12} />
                </button>
              </div>
            )}
          </div>

          {/* Action buttons */}
          <div className="flex gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={handleBack}
              disabled={loading}
            >
              <ArrowLeft className="mr-1 h-4 w-4" />
              Quay lại
            </Button>
            <Button
              type="button"
              onClick={handleCreateDocument}
              disabled={loading || !selectedFile}
            >
              {loading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Đang tạo...
                </>
              ) : (
                <>
                  <Upload className="mr-1 h-4 w-4" />
                  Tạo tài liệu
                </>
              )}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
