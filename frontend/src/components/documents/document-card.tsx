import Link from "next/link";
import { cn } from "@/lib/utils";
import { DocumentMenu } from "@/src/components/documents/document-menu";
import { FileText, MessageSquare, ThumbsUp, ThumbsDown } from "lucide-react";
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
        "relative flex h-full flex-col rounded-2xl border border-slate-200/80 bg-white/95 shadow-sm transition-all duration-200 cursor-pointer overflow-hidden",
        "hover:shadow-lg hover:border-sky-200 hover:bg-white",
        "dark:bg-slate-900/70 dark:border-slate-700 dark:hover:border-sky-500/60",
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
      <div className="relative p-1 pb-2">
        <div className="aspect-[4/3] w-full rounded-2xl bg-gradient-to-br from-slate-50 via-slate-100 to-slate-200 flex items-center justify-center overflow-hidden border border-slate-200 dark:from-slate-800 dark:via-slate-900 dark:to-slate-950 dark:border-slate-700">
          {thumbnailUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={thumbnailUrl}
              alt={title}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="h-full w-full flex flex-col items-center justify-center gap-1 text-slate-500 px-2 text-center leading-snug">
              <span className="text-xs font-semibold uppercase tracking-wide">
                {normalizeMimeType(fileMimeType) ?? "FILE"}
              </span>
            </div>
          )}
        </div>
        {fileCount !== undefined && (
          <div className="absolute bottom-3 right-3 bg-black/60 backdrop-blur-sm text-white text-xs font-semibold px-2 py-1 rounded-md flex items-center gap-1">
            <FileText className="h-3 w-3" />
            <span>{fileCount}</span>
          </div>
        )}
      </div>

      {/* Info */}
      <div className="px-4 pb-4 pt-1 flex flex-col gap-1 min-h-[80px]">
        <p className="text-sm font-semibold text-sky-700 leading-snug line-clamp-3 dark:text-sky-300">
          {title}
        </p>
        <p
          className={cn(
            "mt-2 text-xs font-medium text-slate-500 line-clamp-1 dark:text-slate-400",
            !hasSubject && "invisible"
          )}
        >
          {hasSubject ? subjectName : "placeholder-subject"}
        </p>
        <div
          className={cn(
            "mt-2 flex flex-wrap gap-1 min-h-[18px]",
            visibleTags.length === 0 && "invisible"
          )}
        >
          {mainTags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-slate-600 dark:bg-slate-800 dark:text-slate-300"
            >
              {tag}
            </span>
          ))}
          {extraTagCount > 0 && (
            <span className="text-[10px] font-medium text-slate-500 dark:text-slate-400">
              +{extraTagCount} …
            </span>
          )}
        </div>

        {/* Stats */}
        {(commentCount !== undefined || usefulCount !== undefined || notUsefulCount !== undefined) && (
          <div className="mt-2 flex items-center gap-3 text-xs text-slate-500 dark:text-slate-400 pt-2 border-t border-slate-200 dark:border-slate-700">
            {usefulCount !== undefined && (
              <div className="flex items-center gap-1.5" title="Hữu ích">
                <ThumbsUp className={`h-4 w-4 flex-shrink-0 ${usefulCount > 0 ? 'text-emerald-600 dark:text-emerald-400 fill-emerald-600 dark:fill-emerald-400' : 'text-slate-400 dark:text-slate-500'}`} />
                <span className={`font-medium ${usefulCount > 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-slate-500 dark:text-slate-400'}`}>{usefulCount}</span>
              </div>
            )}
            {notUsefulCount !== undefined && (
              <div className="flex items-center gap-1.5" title="Không hữu ích">
                <ThumbsDown className={`h-4 w-4 flex-shrink-0 ${notUsefulCount > 0 ? 'text-red-600 dark:text-red-400 fill-red-600 dark:fill-red-400' : 'text-slate-400 dark:text-slate-500'}`} />
                <span className={`font-medium ${notUsefulCount > 0 ? 'text-red-600 dark:text-red-400' : 'text-slate-500 dark:text-slate-400'}`}>{notUsefulCount}</span>
              </div>
            )}
            {commentCount !== undefined && (
              <div className="flex items-center gap-1.5" title="Số bình luận">
                <MessageSquare className="h-4 w-4 flex-shrink-0 text-slate-600 dark:text-slate-400" />
                <span className="font-medium">{commentCount}</span>
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


