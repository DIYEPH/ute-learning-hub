"use client";

import { useRouter } from "next/navigation";
import { X, Check, Loader2, Users } from "lucide-react";
import { useProposals } from "@/src/hooks/use-proposals";
import { Button } from "@/src/components/ui/button";
import { useAuthState } from "@/src/hooks/use-auth-state";

export function ProposalFloatingCards() {
    const router = useRouter();
    const { authenticated } = useAuthState();
    const { proposals, responding, respond, dismiss } = useProposals({ enabled: authenticated });

    if (!authenticated || proposals.length === 0) return null;

    const visibleProposals = proposals.slice(0, 3);

    // Accept proposal
    const handleAccept = async (conversationId: string) => {
        const result = await respond(conversationId, true);
        if (result.success && result.isActivated && result.conversationId) {
            router.push(`/chat?id=${result.conversationId}`);
        }
    };

    return (
        <div className="fixed bottom-4 right-4 z-90 flex flex-col gap-3 pointer-events-none max-w-sm">
            {visibleProposals.map((proposal, index) => (
                <div
                    key={proposal.conversationId}
                    className="pointer-events-auto bg-card border border-yellow-200 dark:border-yellow-800 rounded-xl shadow-xl overflow-hidden animate-in slide-in-from-right-full fade-in duration-300"
                    style={{ animationDelay: `${index * 100}ms` }}
                >
                    <div className="bg-linear-to-r from-yellow-50 to-orange-50 dark:from-yellow-950/50 dark:to-orange-950/50 px-4 py-2 flex items-center justify-between border-b border-yellow-100 dark:border-yellow-900">
                        <span className="text-xs font-medium text-yellow-700 dark:text-yellow-300">AI gợi ý tạo nhóm</span>
                        <button onClick={() => dismiss(proposal.conversationId!)} className="text-muted-foreground hover:text-foreground transition-colors">
                            <X className="h-4 w-4" />
                        </button>
                    </div>
                    <div className="p-4">
                        <h4 className="font-semibold text-foreground text-sm mb-1">{proposal.conversationName}</h4>
                        {proposal.subjectName && (
                            <p className="text-xs text-muted-foreground mb-2">{proposal.subjectName}</p>
                        )}
                        <div className="flex items-center gap-2 mb-3">
                            <div className="flex -space-x-1">
                                {proposal.members?.slice(0, 3).map(member => (
                                    <div
                                        key={member.userId}
                                        className="w-6 h-6 rounded-full bg-primary/20 border border-background flex items-center justify-center text-[10px] font-medium overflow-hidden"
                                        title={member.fullName}
                                    >
                                        {member.avatarUrl ? (
                                            <img src={member.avatarUrl} alt={member.fullName} className="w-full h-full object-cover" />
                                        ) : (
                                            member.fullName?.charAt(0) || "?"
                                        )}
                                    </div>
                                ))}
                            </div>
                            <span className="text-xs text-muted-foreground">
                                <Users className="h-3 w-3 inline mr-1" />{proposal.totalMembers} thành viên
                            </span>
                            {proposal.mySimilarityScore && (
                                <span className="text-xs text-primary font-medium ml-auto">
                                    {Math.round((proposal.mySimilarityScore ?? 0) * 100)}% phù hợp
                                </span>
                            )}
                        </div>
                        <div className="flex gap-2">
                            <Button
                                size="sm"
                                variant="outline"
                                className="flex-1 h-8 text-xs"
                                onClick={() => respond(proposal.conversationId!, false)}
                                disabled={responding === proposal.conversationId}
                            >
                                Từ chối
                            </Button>
                            <Button
                                size="sm"
                                className="flex-1 h-8 text-xs"
                                onClick={() => handleAccept(proposal.conversationId!)}
                                disabled={responding === proposal.conversationId}
                            >
                                {responding === proposal.conversationId ? (
                                    <Loader2 className="h-3 w-3 animate-spin" />
                                ) : (
                                    <><Check className="h-3 w-3 mr-1" />Tham gia</>
                                )}
                            </Button>
                        </div>
                    </div>
                </div>
            ))}

            {proposals.length > 3 && (
                <div className="pointer-events-auto text-center">
                    <button onClick={() => router.push("/conversations")} className="text-xs text-primary hover:underline">
                        +{proposals.length - 3} gợi ý khác
                    </button>
                </div>
            )}
        </div>
    );
}