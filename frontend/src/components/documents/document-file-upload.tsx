"use client";

import { useState } from "react";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, Upload, X } from "lucide-react";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { postApiDocumentByIdFiles } from "@/src/api/database/sdk.gen";
import type { AddDocumentFileCommand, DocumentDetailDto } from "@/src/api/database/types.gen";

interface DocumentFileUploadProps {
  documentId: string;
  onUploadSuccess?: () => void;
}

export function DocumentFileUpload({
  documentId,
  onUploadSuccess,
}: DocumentFileUploadProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [selectedCoverFile, setSelectedCoverFile] = useState<File | null>(null);
  const [uploadTitle, setUploadTitle] = useState("");
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { uploadFile } = useFileUpload();

  const handleUpload = async () => {
    if (!selectedFile || !documentId) return;

    setUploading(true);
    setError(null);

    try {
      // 1. Upload file chính
      const mainFile = await uploadFile(selectedFile, "DocumentFile");

      // 2. Upload ảnh bìa (nếu có)
      let coverFileId: string | undefined;
      if (selectedCoverFile) {
        const coverFile = await uploadFile(selectedCoverFile, "DocumentFileCover");
        coverFileId = coverFile.id;
      }

      // 3. Gọi API thêm DocumentFile bằng FileId/CoverFileId
      const body: AddDocumentFileCommand = {
        documentId,
        fileId: mainFile.id,
        coverFileId: coverFileId ?? null,
        title: uploadTitle || null,
        isPrimary: false,
        order: null,
        totalPages: null,
      };

      const response = await postApiDocumentByIdFiles({
        path: { id: documentId },
        body,
      });

      const updated = (response.data ?? response) as DocumentDetailDto | undefined;

      if (updated) {
        setSelectedFile(null);
        setSelectedCoverFile(null);
        setUploadTitle("");
        onUploadSuccess?.();
      }
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể upload file";
      setError(errorMessage);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
      <h3 className="text-sm font-semibold text-foreground mb-3">
        Thêm chương/file
      </h3>
      
      {error && (
        <div className="mb-3 p-2 text-xs text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}

      <div className="space-y-3">
        <div>
          <Label className="text-xs">Tệp chương/file</Label>
          <input
            type="file"
            accept=".doc,.docx,.pdf,image/*"
            onChange={(e) => {
              const file = e.target.files?.[0] || null;
              setSelectedFile(file);
              setError(null);
            }}
            className="hidden"
            id="upload-file"
            disabled={uploading}
          />
          <label
            htmlFor="upload-file"
            className={`mt-1 flex items-center gap-2 px-3 py-2 border-2 border-dashed rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 text-xs ${
              uploading ? "opacity-50 cursor-not-allowed" : ""
            }`}
          >
            <Upload size={14} />
            <span>{uploadFile ? uploadFile.name : "Chọn file"}</span>
          </label>
          {selectedFile && (
            <div className="mt-1 flex items-center justify-between p-2 bg-slate-100 dark:bg-slate-800 rounded text-xs">
              <span>{selectedFile.name}</span>
              <button
                type="button"
                onClick={() => setSelectedFile(null)}
                disabled={uploading}
                className="text-slate-500 hover:text-slate-700"
              >
                <X size={12} />
              </button>
            </div>
          )}
        </div>

        <div>
          <Label className="text-xs">Tiêu đề (tùy chọn)</Label>
          <Input
            type="text"
            value={uploadTitle}
            onChange={(e) => setUploadTitle(e.target.value)}
            placeholder="Chương I, Chương 1..."
            className="mt-1 h-8 text-xs"
            disabled={uploading}
          />
        </div>

        <div>
          <Label className="text-xs">Ảnh bìa (tùy chọn)</Label>
          <input
            type="file"
            accept="image/*"
            onChange={(e) => {
              const file = e.target.files?.[0] || null;
              setSelectedCoverFile(file);
            }}
            className="hidden"
            id="upload-cover"
            disabled={uploading}
          />
          <label
            htmlFor="upload-cover"
            className={`mt-1 flex items-center gap-2 px-3 py-2 border-2 border-dashed rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 text-xs ${
              uploading ? "opacity-50 cursor-not-allowed" : ""
            }`}
          >
            <Upload size={14} />
            <span>{selectedCoverFile ? selectedCoverFile.name : "Chọn ảnh bìa"}</span>
          </label>
          {selectedCoverFile && (
            <div className="mt-1 flex items-center justify-between p-2 bg-slate-100 dark:bg-slate-800 rounded text-xs">
              <span>{selectedCoverFile.name}</span>
              <button
                type="button"
                onClick={() => setSelectedCoverFile(null)}
                disabled={uploading}
                className="text-slate-500 hover:text-slate-700"
              >
                <X size={12} />
              </button>
            </div>
          )}
        </div>

        <Button
          onClick={handleUpload}
          disabled={!selectedFile || uploading}
          size="sm"
          className="w-full"
        >
          {uploading ? (
            <>
              <Loader2 className="mr-2 h-3 w-3 animate-spin" />
              Đang upload...
            </>
          ) : (
            "Upload"
          )}
        </Button>
      </div>
    </div>
  );
}

