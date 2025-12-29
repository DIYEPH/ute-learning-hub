import Link from "next/link";
import { cn } from "@/lib/utils";
import { DocumentMenu } from "@/src/components/documents/document-menu";
import { FileText, MessageSquare, ThumbsUp, ThumbsDown, Eye, BookOpen } from "lucide-react";
import { getFileUrlById } from "@/src/lib/file-url";

export interface DocumentCardProps {
  id?: string;
  title: string;
  subjectName?: string;
  thumbnailFileId?: string | null;
  fileMimeType?: string;
  tags?: string[];
  href?: string;
  className?: string;
  fileCount?: number;
  commentCount?: number;
  usefulCount?: number;
  notUsefulCount?: number;
  totalViewCount?: number;
  /** Reading progress - last page read */
  lastPage?: number;
  /** Reading progress - total pages */
  totalPages?: number | null;
  onEdit?: () => void;
  onDelete?: () => void;
  onReport?: () => void;
}

export function DocumentCard({
  id,
  title,
  subjectName,
  thumbnailFileId,
  fileMimeType,
  tags,
  href,
  className,
  fileCount,
  commentCount,
  usefulCount,
  notUsefulCount,
  totalViewCount,
  lastPage,
  totalPages,
  onEdit,
  onDelete,
  onReport,
}: DocumentCardProps) {
  const resolvedHref = href ?? (id ? `/documents/${id}` : "#");

  const showMenu = Boolean(onEdit || onDelete || onReport);
  const hasSubject = Boolean(subjectName);
  const visibleTags = tags?.filter(Boolean) ?? [];
  const mainTags = visibleTags.slice(0, 2);
  const extraTagCount = visibleTags.length > 2 ? visibleTags.length - 2 : 0;
  const thumbnailUrl = getFileUrlById(thumbnailFileId);

  const content = (
    <div
      className={cn(
        "relative flex h-full flex-col border border-border bg-card shadow-sm transition-all duration-200 cursor-pointer overflow-hidden",
        "hover:shadow-lg hover:border-primary/50",
        className
      )}
    >
      {showMenu && (
        <div className="absolute right-2 top-2 z-10">
          <DocumentMenu
            onEdit={onEdit}
            onDelete={onDelete}
            onReport={onReport}
          />
        </div>
      )}

      {/* Thumbnail */}
      <div className="relative p-1 pb-1">
        <div className="aspect-[5/3] w-full bg-muted flex items-center justify-center overflow-hidden border border-border">
          {thumbnailUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={thumbnailUrl}
              alt={title}
              className="h-full w-full object-contain"
            />
          ) : (
            <div className="h-full w-full flex flex-col items-center justify-center gap-1 text-muted-foreground px-2 text-center leading-snug">
              <span className="text-xs font-semibold uppercase tracking-wide">
                {normalizeMimeType(fileMimeType) ?? "FILE"}
              </span>
            </div>
          )}
        </div>
        {fileCount !== undefined && (
          <div className="absolute bottom-2 right-2 bg-black/60 backdrop-blur-sm text-white text-[10px] font-semibold px-1.5 py-0.5 rounded flex items-center gap-0.5">
            <FileText className="h-2.5 w-2.5" />
            <span>{fileCount}</span>
          </div>
        )}
        {/* Reading progress badge */}
        {lastPage !== undefined && lastPage > 0 && (
          <div className="absolute bottom-2 left-2 bg-primary/90 backdrop-blur-sm text-primary-foreground text-[10px] font-semibold px-1.5 py-0.5 rounded flex items-center gap-1">
            <BookOpen className="h-2.5 w-2.5" />
            <span>
              Trang {lastPage}{totalPages ? `/${totalPages}` : ""}
            </span>
          </div>
        )}
      </div>

      {/* Info */}
      <div className="px-2 pb-2 pt-0.5 flex flex-col gap-0.5 min-h-[60px]">
        <p className="text-xs font-semibold text-primary leading-tight line-clamp-2">
          {title}
        </p>
        <p
          className={cn(
            "mt-1 text-[10px] font-medium text-muted-foreground line-clamp-1",
            !hasSubject && "invisible"
          )}
        >
          {hasSubject ? subjectName : "placeholder-subject"}
        </p>
        <div
          className={cn(
            "mt-1 flex flex-wrap gap-0.5 min-h-[14px]",
            visibleTags.length === 0 && "invisible"
          )}
        >
          {mainTags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-secondary px-1.5 py-0 text-[9px] font-semibold uppercase tracking-wide text-secondary-foreground"
            >
              {tag}
            </span>
          ))}
          {extraTagCount > 0 && (
            <span className="text-[9px] font-medium text-muted-foreground">
              +{extraTagCount} …
            </span>
          )}
        </div>

        {/* Stats */}
        {(commentCount !== undefined || usefulCount !== undefined || notUsefulCount !== undefined) && (
          <div className="mt-1 flex items-center gap-2 text-[10px] text-muted-foreground pt-1 border-t border-border">
            {usefulCount !== undefined && (
              <div className="flex items-center gap-1" title="Hữu ích">
                <ThumbsUp className="h-3 w-3 flex-shrink-0" />
                <span className="font-medium">{usefulCount}</span>
              </div>
            )}
            {notUsefulCount !== undefined && (
              <div className="flex items-center gap-1" title="Không hữu ích">
                <ThumbsDown className="h-3 w-3 flex-shrink-0" />
                <span className="font-medium">{notUsefulCount}</span>
              </div>
            )}
            {commentCount !== undefined && (
              <div className="flex items-center gap-1" title="Số bình luận">
                <MessageSquare className="h-3 w-3 flex-shrink-0" />
                <span className="font-medium">{commentCount}</span>
              </div>
            )}
            {totalViewCount !== undefined && (
              <div className="flex items-center gap-1" title="Lượt xem">
                <Eye className="h-3 w-3 flex-shrink-0" />
                <span className="font-medium">{totalViewCount}</span>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );

  if (!resolvedHref || resolvedHref === "#") {
    return content;
  }

  return (
    <Link href={resolvedHref} className="block">
      {content}
    </Link>
  );
}

function normalizeMimeType(mimeType?: string) {
  if (!mimeType) return null;
  const lower = mimeType.toLowerCase();
  if (lower.includes("pdf")) return "PDF";
  if (lower.includes("word") || lower.includes("msword")) return "DOC";
  if (lower.includes("presentation") || lower.includes("powerpoint")) return "PPT";
  if (lower.startsWith("image/")) return "IMG";
  if (lower.includes("excel")) return "XLS";
  if (lower.includes("text")) return "TXT";
  return mimeType.split("/").pop()?.toUpperCase() ?? "FILE";
}



