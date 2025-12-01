import Link from "next/link";
import { cn } from "@/lib/utils";

export interface DocumentCardProps {
  id?: string;
  title: string;
  subjectName?: string;
  pageCount?: number | null;
  thumbnailUrl?: string | null;
  href?: string;
  className?: string;
}

export function DocumentCard({
  id,
  title,
  subjectName,
  pageCount,
  thumbnailUrl,
  href,
  className,
}: DocumentCardProps) {
  const resolvedHref = href ?? (id ? `/documents/${id}` : "#");

  const content = (
    <div
      className={cn(
        "flex flex-col rounded-2xl bg-white shadow-sm hover:shadow-md transition-shadow cursor-pointer overflow-hidden",
        "border border-slate-100",
        className
      )}
    >
      {/* Thumbnail */}
      <div className="relative p-3 pb-2">
        <div className="mx-auto aspect-[4/5] w-full max-w-[140px] rounded-2xl bg-slate-100 flex items-center justify-center overflow-hidden">
          {thumbnailUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={thumbnailUrl}
              alt={title}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="h-full w-full flex items-center justify-center text-[10px] text-slate-500 px-2 text-center leading-snug">
              {title}
            </div>
          )}
        </div>

        {typeof pageCount === "number" && pageCount > 0 && (
          <span className="absolute bottom-2 right-4 text-[11px] font-medium text-slate-500">
            {pageCount}
          </span>
        )}
      </div>

      {/* Info */}
      <div className="px-3 pb-3">
        <p className="text-sm font-semibold text-sky-600 leading-snug line-clamp-3">
          {title}
        </p>
        {subjectName && (
          <p className="mt-1 text-xs text-slate-400 line-clamp-1">
            {subjectName}
          </p>
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


