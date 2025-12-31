"use client";

import { useState, useEffect, useCallback } from "react";
import { getApiProposalMy, postApiProposalByConversationIdRespond } from "@/src/api";
import type { ProposalDto } from "@/src/api/database/types.gen";

export function useProposals() {
    const [proposals, setProposals] = useState<ProposalDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [responding, setResponding] = useState<string | null>(null);

    const fetchProposals = useCallback(async () => {
        try {
            const response = await getApiProposalMy();
            const payload = (response.data ?? response) as any;
            // Only show pending proposals (myStatus === 1)
            const items = (payload?.proposals ?? []).filter((p: ProposalDto) => p.myStatus === 1);
            setProposals(items);
        } catch (err) {
            console.error("Error fetching proposals:", err);
            setProposals([]);
        } finally {
            setLoading(false);
        }
    }, []);

    const respond = useCallback(async (conversationId: string, accept: boolean) => {
        setResponding(conversationId);
        try {
            const response = await postApiProposalByConversationIdRespond({
                path: { conversationId },
                body: { accept },
            });
            const result = (response.data ?? response) as any;

            // Remove from local state
            setProposals((prev) => prev.filter((p) => p.conversationId !== conversationId));

            return {
                success: true,
                isActivated: result.isActivated,
                conversationId: result.conversation?.id,
            };
        } catch (err) {
            console.error("Error responding to proposal:", err);
            return { success: false };
        } finally {
            setResponding(null);
        }
    }, []);

    const dismiss = useCallback((conversationId: string) => {
        // Just hide locally (user can decline later in /conversations)
        setProposals((prev) => prev.filter((p) => p.conversationId !== conversationId));
    }, []);

    useEffect(() => {
        fetchProposals();
        // Refresh every 30 seconds to check for new proposals
        const interval = setInterval(fetchProposals, 30000);
        return () => clearInterval(interval);
    }, [fetchProposals]);

    return {
        proposals,
        loading,
        responding,
        respond,
        dismiss,
        refetch: fetchProposals,
        pendingCount: proposals.length,
    };
}
