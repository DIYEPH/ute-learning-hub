"use client";

import { useMemo } from "react";
import { X, Image as ImageIcon, File, Download } from "lucide-react";
import { cn } from "@/lib/utils";
import type { MessageDto, MessageFileDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { ScrollArea } from "@/src/components/ui/scroll-area";

interface ConversationFilesSidebarProps {
  open: boolean;
  onClose: () => void;
  messages: MessageDto[];
}

interface FileItem extends MessageFileDto {
  messageId?: string;
  messageDate?: string;
  senderName?: string;
}

export function ConversationFilesSidebar({
  open,
  onClose,
  messages,
}: ConversationFilesSidebarProps) {
  // Lấy tất cả files từ messages
  const allFiles = useMemo(() => {
    const files: FileItem[] = [];
    messages.forEach((message) => {
      if (message.files && message.files.length > 0) {
        message.files.forEach((file) => {
          files.push({
            ...file,
            messageId: message.id,
            messageDate: message.createdAt,
            senderName: message.senderName,
          });
        });
      }
    });
    return files.reverse(); // Mới nhất trước
  }, [messages]);

  // Phân loại files thành images và files khác
  const { images, otherFiles } = useMemo(() => {
    const imagesList: FileItem[] = [];
    const filesList: FileItem[] = [];

    allFiles.forEach((file) => {
      const mimeType = file.mimeType?.toLowerCase() || "";
      if (mimeType.startsWith("image/")) {
        imagesList.push(file);
      } else {
        filesList.push(file);
      }
    });

    return { images: imagesList, otherFiles: filesList };
  }, [allFiles]);

  const formatFileSize = (bytes?: number) => {
    if (!bytes) return "";
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  return (
    <>
      {/* Overlay cho mobile */}
      {open && (
        <div
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <div
        className={cn(
          "fixed top-0 right-0 h-full bg-white dark:bg-slate-900 border-l border-slate-200 dark:border-slate-700 z-50 transition-transform duration-300 ease-in-out",
          "w-full md:w-80",
          open ? "translate-x-0" : "translate-x-full"
        )}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
            <h3 className="text-lg font-semibold text-foreground">
              Tệp đã gửi
            </h3>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>

          {/* Content */}
          <ScrollArea className="flex-1">
            <div className="p-4 space-y-6">
              {/* Images Section */}
              {images.length > 0 && (
                <div>
                  <div className="flex items-center gap-2 mb-3">
                    <ImageIcon className="h-4 w-4 text-slate-500" />
                    <h4 className="text-sm font-semibold text-foreground">
                      Hình ảnh ({images.length})
                    </h4>
                  </div>
                  <div className="grid grid-cols-3 gap-2">
                    {images.map((image, idx) => (
                      <a
                        key={image.fileId || idx}
                        href={image.fileUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="group relative aspect-square rounded-lg overflow-hidden border border-slate-200 dark:border-slate-700 hover:border-sky-500 transition-colors"
                      >
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                          src={image.fileUrl}
                          alt={image.fileName || "Image"}
                          className="w-full h-full object-cover"
                        />
                        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors" />
                      </a>
                    ))}
                  </div>
                </div>
              )}

              {/* Files Section */}
              {otherFiles.length > 0 && (
                <div>
                  <div className="flex items-center gap-2 mb-3">
                    <File className="h-4 w-4 text-slate-500" />
                    <h4 className="text-sm font-semibold text-foreground">
                      Tệp khác ({otherFiles.length})
                    </h4>
                  </div>
                  <div className="space-y-2">
                    {otherFiles.map((file, idx) => (
                      <a
                        key={file.fileId || idx}
                        href={file.fileUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-3 p-3 rounded-lg border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors group"
                      >
                        <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-slate-100 dark:bg-slate-800 flex items-center justify-center">
                          <File className="h-5 w-5 text-slate-600 dark:text-slate-400" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-foreground truncate">
                            {file.fileName || "Tệp không tên"}
                          </p>
                          <div className="flex items-center gap-2 mt-1">
                            <span className="text-xs text-slate-500 dark:text-slate-400">
                              {formatFileSize(file.fileSize)}
                            </span>
                            {file.senderName && (
                              <>
                                <span className="text-xs text-slate-400">•</span>
                                <span className="text-xs text-slate-500 dark:text-slate-400 truncate">
                                  {file.senderName}
                                </span>
                              </>
                            )}
                            {file.messageDate && (
                              <>
                                <span className="text-xs text-slate-400">•</span>
                                <span className="text-xs text-slate-500 dark:text-slate-400">
                                  {formatDate(file.messageDate)}
                                </span>
                              </>
                            )}
                          </div>
                        </div>
                        <Download className="h-4 w-4 text-slate-400 group-hover:text-sky-500 transition-colors flex-shrink-0" />
                      </a>
                    ))}
                  </div>
                </div>
              )}

              {/* Empty State */}
              {images.length === 0 && otherFiles.length === 0 && (
                <div className="text-center py-12">
                  <File className="h-12 w-12 mx-auto mb-3 text-slate-300 dark:text-slate-600" />
                  <p className="text-sm text-slate-500 dark:text-slate-400">
                    Chưa có tệp nào được gửi
                  </p>
                </div>
              )}
            </div>
          </ScrollArea>
        </div>
      </div>
    </>
  );
}

