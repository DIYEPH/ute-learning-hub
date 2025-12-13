"use client";

import { MessageCircle, Users, Sparkles } from "lucide-react";
import { useRouter } from "next/navigation";
import type { ConversationRecommendationDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Badge } from "@/src/components/ui/badge";

interface RecommendationCardProps {
    recommendation: ConversationRecommendationDto;
}

export function RecommendationCard({ recommendation }: RecommendationCardProps) {
    const router = useRouter();
    const isMember = recommendation.isCurrentUserMember ?? false;

    const handleClick = () => {
        if (recommendation.conversationId) {
            if (isMember) {
                router.push(`/chat?id=${recommendation.conversationId}`);
            } else {
                // Navigate to conversation detail or show join modal
                router.push(`/conversations`);
            }
        }
    };

    const similarityPercent = recommendation.similarity
        ? Math.round(recommendation.similarity * 100)
        : null;

    return (
        <div
            className="min-w-[240px] max-w-[280px] flex-shrink-0  border border-slate-200 bg-gradient-to-br from-slate-50 to-white p-4 shadow-sm transition-all hover:shadow-md hover:border-sky-300 cursor-pointer dark:border-slate-700 dark:from-slate-900 dark:to-slate-800"
            onClick={handleClick}
        >
            <div className="flex items-start gap-3">
                <Avatar className="h-10 w-10 flex-shrink-0 ring-2 ring-sky-100 dark:ring-sky-900">
                    <AvatarImage
                        src={recommendation.avatarUrl || undefined}
                        alt={recommendation.conversationName || "Avatar"}
                    />
                    <AvatarFallback className="bg-sky-100 text-sky-700 dark:bg-sky-900 dark:text-sky-300">
                        <MessageCircle className="h-4 w-4" />
                    </AvatarFallback>
                </Avatar>

                <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-semibold text-foreground truncate">
                        {recommendation.conversationName || "Nhóm"}
                    </h4>

                    {recommendation.subject && (
                        <p className="text-xs text-slate-500 dark:text-slate-400 truncate">
                            {recommendation.subject.subjectName}
                        </p>
                    )}
                </div>
            </div>

            {/* Tags */}
            {recommendation.tags && recommendation.tags.length > 0 && (
                <div className="flex flex-wrap gap-1 mt-2">
                    {recommendation.tags.slice(0, 2).map((tag) => (
                        <Badge
                            key={tag.id}
                            variant="secondary"
                            className="text-[10px] bg-slate-100 dark:bg-slate-800"
                        >
                            {tag.tagName}
                        </Badge>
                    ))}
                    {recommendation.tags.length > 2 && (
                        <span className="text-[10px] text-slate-500">
                            +{recommendation.tags.length - 2}
                        </span>
                    )}
                </div>
            )}

            {/* Footer */}
            <div className="flex items-center justify-between mt-3 pt-2 border-t border-slate-100 dark:border-slate-700">
                <div className="flex items-center gap-1 text-xs text-slate-500 dark:text-slate-400">
                    <Users className="h-3 w-3" />
                    <span>{recommendation.memberCount ?? 0}</span>
                </div>

                {similarityPercent !== null && (
                    <div className="flex items-center gap-1 text-xs font-medium text-sky-600 dark:text-sky-400">
                        <Sparkles className="h-3 w-3" />
                        <span>{similarityPercent}% phù hợp</span>
                    </div>
                )}
            </div>
        </div>
    );
}


