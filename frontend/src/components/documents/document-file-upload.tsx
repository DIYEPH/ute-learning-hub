"use client";

import { useState } from "react";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, Upload, X } from "lucide-react";
import { getBearerToken } from "@/src/api/client";
import axios from "axios";

interface DocumentFileUploadProps {
  documentId: string;
  onUploadSuccess?: () => void;
}

export function DocumentFileUpload({
  documentId,
  onUploadSuccess,
}: DocumentFileUploadProps) {
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadCoverFile, setUploadCoverFile] = useState<File | null>(null);
  const [uploadTitle, setUploadTitle] = useState("");
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleUpload = async () => {
    if (!uploadFile || !documentId) return;

    setUploading(true);
    setError(null);

    try {
      const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7080";
      const token = getBearerToken();
      
      const formData = new FormData();
      formData.append("File", uploadFile);
      formData.append("DocumentId", documentId);
      
      if (uploadTitle) {
        formData.append("Title", uploadTitle);
      }
      
      if (uploadCoverFile) {
        formData.append("CoverFile", uploadCoverFile);
      }
      const response = await axios.post(
        `${apiBaseUrl}/api/Document/${documentId}/files`,
        formData,
        {
          headers: {
            ...(token && { Authorization: token }),
          },
        }
      );

      if (response.data) {
        setUploadFile(null);
        setUploadCoverFile(null);
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
              setUploadFile(file);
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
          {uploadFile && (
            <div className="mt-1 flex items-center justify-between p-2 bg-slate-100 dark:bg-slate-800 rounded text-xs">
              <span>{uploadFile.name}</span>
              <button
                type="button"
                onClick={() => setUploadFile(null)}
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
              setUploadCoverFile(file);
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
            <span>{uploadCoverFile ? uploadCoverFile.name : "Chọn ảnh bìa"}</span>
          </label>
          {uploadCoverFile && (
            <div className="mt-1 flex items-center justify-between p-2 bg-slate-100 dark:bg-slate-800 rounded text-xs">
              <span>{uploadCoverFile.name}</span>
              <button
                type="button"
                onClick={() => setUploadCoverFile(null)}
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
          disabled={!uploadFile || uploading}
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

