import Link from "next/link";
import { cn } from "@/lib/utils";
import { DocumentMenu } from "@/src/components/documents/document-menu";
import { FileText, ThumbsUp, Bookmark, Eye } from "lucide-react";
import { getFileUrlById } from "@/src/lib/file-url";
import { Button } from "@/src/components/ui/button";

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
  onSave?: () => void;
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
  usefulCount,
  notUsefulCount,
  totalViewCount,
  lastPage,
  totalPages,
  onEdit,
  onDelete,
  onReport,
  onSave,
}: DocumentCardProps) {
  const resolvedHref = href ?? (id ? `/documents/${id}` : "#");

  const showMenu = Boolean(onEdit || onDelete || onReport);
  const thumbnailUrl = getFileUrlById(thumbnailFileId);

  // Calculate rating percentage
  const totalReviews = (usefulCount || 0) + (notUsefulCount || 0);
  const ratingPercent = totalReviews > 0
    ? Math.round((usefulCount || 0) / totalReviews * 100)
    : null;

  const content = (
    <div
      className={cn(
        "group relative flex h-full flex-col bg-card rounded-xl border border-border/50 shadow-sm transition-all duration-200 cursor-pointer overflow-hidden",
        "hover:shadow-md hover:border-primary/30 hover:-translate-y-0.5",
        className
      )}
    >
      {showMenu && (
        <div className="absolute right-2 top-2 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
          <DocumentMenu
            onEdit={onEdit}
            onDelete={onDelete}
            onReport={onReport}
          />
        </div>
      )}

      {/* Thumbnail */}
      <div className="relative p-3 pb-2">
        <div className="aspect-4/3 w-full bg-linear-to-br from-slate-100 to-slate-50 dark:from-slate-800 dark:to-slate-900 flex items-center justify-center overflow-hidden rounded-lg">
          {thumbnailUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={thumbnailUrl}
              alt={title}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="h-full w-full flex flex-col items-center justify-center gap-2 text-muted-foreground/60">
              <FileText className="h-10 w-10" />
              <span className="text-xs font-medium uppercase tracking-wider">
                {normalizeMimeType(fileMimeType) ?? "PDF"}
              </span>
            </div>
          )}
        </div>

        {/* File count badge */}
        {fileCount !== undefined && fileCount > 0 && (
          <div className="absolute bottom-4 right-5 bg-black/70 backdrop-blur-sm text-white text-xs font-medium px-2 py-1 rounded-md">
            {fileCount} file
          </div>
        )}
      </div>

      {/* Info */}
      <div className="px-3 pb-3 flex flex-col gap-1.5 flex-1">
        {/* Title */}
        <h3 className="text-sm font-semibold text-foreground leading-snug line-clamp-2 group-hover:text-primary transition-colors">
          {title}
        </h3>

        {/* Subject */}
        {subjectName && (
          <p className="text-xs text-muted-foreground line-clamp-1">
            {subjectName}
          </p>
        )}

        {/* Tags */}
        {tags && tags.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {tags.slice(0, 2).map((tag) => (
              <span
                key={tag}
                className="px-1.5 py-0.5 text-[10px] font-medium bg-muted text-muted-foreground rounded"
              >
                {tag}
              </span>
            ))}
            {tags.length > 2 && (
              <span className="text-[10px] text-muted-foreground">+{tags.length - 2}</span>
            )}
          </div>
        )}

        {/* Stats: Rating & Views */}
        <div className="flex items-center gap-3 mt-auto pt-2">
          {ratingPercent !== null && (
            <div className="flex items-center gap-1 text-emerald-600 dark:text-emerald-400">
              <ThumbsUp className="h-3.5 w-3.5" />
              <span className="text-xs font-semibold">{ratingPercent}%</span>
              <span className="text-xs text-muted-foreground">({totalReviews})</span>
            </div>
          )}
          {totalViewCount !== undefined && totalViewCount > 0 && (
            <div className="flex items-center gap-1 text-muted-foreground">
              <Eye className="h-3.5 w-3.5" />
              <span className="text-xs font-medium">{formatViewCount(totalViewCount)}</span>
            </div>
          )}
        </div>

        {/* Save button */}
        {onSave && (
          <Button
            variant="outline"
            size="sm"
            className="mt-2 w-full gap-1.5 text-xs"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onSave();
            }}
          >
            <Bookmark className="h-3.5 w-3.5" />
            Save
          </Button>
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

function formatViewCount(count: number): string {
  if (count >= 1000000) {
    return (count / 1000000).toFixed(1).replace(/\.0$/, '') + 'M';
  }
  if (count >= 1000) {
    return (count / 1000).toFixed(1).replace(/\.0$/, '') + 'K';
  }
  return count.toString();
}
