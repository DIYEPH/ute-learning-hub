"use client";

import { useState } from "react";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, Upload, AlertCircle } from "lucide-react";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { usePdfThumbnail } from "@/src/hooks/use-pdf-thumbnail";
import { postApiDocumentByIdFiles } from "@/src/api";
import type { AddDocumentFileCommand } from "@/src/api/database/types.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

// File size limit: 100MB (matches backend)
const MAX_FILE_SIZE_MB = 100;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

interface DocumentFileUploadProps {
  documentId: string;
  onUploadSuccess?: () => void;
}

export function DocumentFileUpload({
  documentId,
  onUploadSuccess,
}: DocumentFileUploadProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadTitle, setUploadTitle] = useState("");
  const [uploading, setUploading] = useState(false);
  const [fileSizeError, setFileSizeError] = useState<string | null>(null);
  const { uploadFile } = useFileUpload();
  const { extractThumbnail } = usePdfThumbnail({ width: 400, quality: 0.85 });
  const { success: notifySuccess, error: notifyError } = useNotification();

  const handleUpload = async () => {
    if (!selectedFile || !documentId) return;

    setUploading(true);

    try {
      // Upload file chính
      const mainFile = await uploadFile(selectedFile, "DocumentFile");
      console.log("[Upload] mainFile:", mainFile);

      // Auto-extract cover
      let coverFileId: string | null = null;
      const isPdf = selectedFile.type.includes("pdf") || selectedFile.name.toLowerCase().endsWith(".pdf");
      const isImage = selectedFile.type.startsWith("image/");
      console.log("[Upload] isPdf:", isPdf, "isImage:", isImage);

      if (isPdf) {
        // Extract first page as cover
        console.log("[Upload] Extracting thumbnail...");
        const thumbnail = await extractThumbnail(selectedFile);
        console.log("[Upload] thumbnail:", thumbnail);
        if (thumbnail) {
          console.log("[Upload] Uploading cover...");
          const coverFile = await uploadFile(thumbnail, "DocumentFileCover");
          console.log("[Upload] coverFile:", coverFile);
          coverFileId = coverFile.id ?? null;
        }
      } else if (isImage) {
        // For images, use the same file as cover
        coverFileId = mainFile.id ?? null;
      }

      console.log("[Upload] Final coverFileId:", coverFileId);

      // Gọi API thêm DocumentFile
      const body: AddDocumentFileCommand = {
        documentId,
        fileId: mainFile.id,
        coverFileId,
        title: uploadTitle.trim() || null,
      };
      console.log("[Upload] body:", body);

      await postApiDocumentByIdFiles({
        path: { id: documentId },
        body,
        throwOnError: true,
      });

      // Reset form
      setSelectedFile(null);
      setUploadTitle("");

      notifySuccess("Đã thêm file thành công");
      onUploadSuccess?.();
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể upload file";
      notifyError(errorMessage);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <h3 className="text-xs font-semibold text-foreground mb-3">
        Thêm chương/file
      </h3>

      {/* 2-column grid layout */}
      <div className="grid gap-3 md:grid-cols-2 mb-3">
        {/* File selection */}
        <div>
          <Label className="text-[11px]">
            Tệp chương/file <span className="text-red-500">*</span>
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
            disabled={uploading}
          />
          <label
            htmlFor="upload-file"
            className={`mt-1 flex items-center gap-2 px-3 py-2 border border-dashed rounded cursor-pointer hover:bg-muted text-xs transition-colors ${uploading ? "opacity-50 cursor-not-allowed" : ""
              } ${fileSizeError ? "border-red-300 bg-red-50 dark:bg-red-950/30" : ""}`}
          >
            <Upload size={14} />
            <span className="truncate">{selectedFile ? selectedFile.name : "Chọn file"}</span>
          </label>
          <p className="mt-0.5 text-[10px] text-muted-foreground">Tối đa {MAX_FILE_SIZE_MB}MB</p>
          {fileSizeError && (
            <p className="mt-1 text-xs text-red-600 dark:text-red-400 flex items-center gap-1">
              <AlertCircle size={12} />
              {fileSizeError}
            </p>
          )}
        </div>

        {/* Title input */}
        <div>
          <Label className="text-[11px]">Tiêu đề (tùy chọn)</Label>
          <Input
            type="text"
            value={uploadTitle}
            onChange={(e) => setUploadTitle(e.target.value)}
            placeholder="Chương I, Chương 1..."
            className="mt-1 h-9 text-xs"
            disabled={uploading}
          />
        </div>
      </div>

      {/* Upload button */}
      <Button
        onClick={handleUpload}
        disabled={!selectedFile || uploading}
        size="sm"
        className="w-full h-9"
      >
        {uploading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Đang upload...
          </>
        ) : (
          "Upload"
        )}
      </Button>
    </div>
  );
}

