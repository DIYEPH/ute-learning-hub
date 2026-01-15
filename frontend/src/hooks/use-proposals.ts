"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { getApiProposalMy, postApiProposalByConversationIdRespond } from "@/src/api";
import type { ProposalDto } from "@/src/api/database/types.gen";

export function useProposals({ enabled = true } = {}) {
    const [proposals, setProposals] = useState<ProposalDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [responding, setResponding] = useState<string | null>(null);
    const intervalRef = useRef<NodeJS.Timeout | null>(null);

    // Fetch proposals
    const fetchProposals = useCallback(async () => {
        if (!enabled) return;
        try {
            const response = await getApiProposalMy();
            const payload = (response.data ?? response) as any;
            setProposals((payload?.proposals ?? []).filter((p: ProposalDto) => p.myStatus === 1));
        } catch { setProposals([]); }
        finally { setLoading(false); }
    }, [enabled]);

    // Respond to proposal
    const respond = useCallback(async (conversationId: string, accept: boolean) => {
        if (!enabled) return { success: false };
        setResponding(conversationId);
        try {
            const response = await postApiProposalByConversationIdRespond({
                path: { conversationId },
                body: { accept },
            });
            const result = (response.data ?? response) as any;
            setProposals(prev => prev.filter(p => p.conversationId !== conversationId));
            return { success: true, isActivated: result.isActivated, conversationId: result.conversation?.id };
        } catch { return { success: false }; }
        finally { setResponding(null); }
    }, [enabled]);

    const dismiss = useCallback((conversationId: string) => {
        setProposals(prev => prev.filter(p => p.conversationId !== conversationId));
    }, []);

    useEffect(() => {
        if (intervalRef.current) { clearInterval(intervalRef.current); intervalRef.current = null; }
        if (!enabled) { setProposals([]); setLoading(false); return; }
        fetchProposals();
        intervalRef.current = setInterval(fetchProposals, 30000);
        return () => { if (intervalRef.current) { clearInterval(intervalRef.current); intervalRef.current = null; } };
    }, [enabled, fetchProposals]);

    return { proposals, loading, responding, respond, dismiss, refetch: fetchProposals, pendingCount: proposals.length };
}