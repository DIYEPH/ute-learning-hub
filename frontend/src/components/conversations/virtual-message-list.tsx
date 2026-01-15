"use client";

import { useRef, useEffect, useCallback } from "react";
import { useVirtualizer } from "@tanstack/react-virtual";
import type { MessageDto2 as MessageDto } from "@/src/api/database/types.gen";
import { MessageItem } from "@/src/components/conversations/message-item";

interface VirtualMessageListProps {
    messages: MessageDto[];
    conversationId: string;
    currentUserId?: string;
    onReply?: (message: MessageDto) => void;
    onScrollToMessage?: (messageId: string) => void;
    onUpdate?: (message: MessageDto) => void;
    onDelete?: (messageId: string) => void;
    onLoadMore?: () => void;
    hasMore?: boolean;
    isLoadingMore?: boolean;
    compact?: boolean;
    typingIndicator?: React.ReactNode;
}

export function VirtualMessageList({
    messages,
    conversationId,
    currentUserId,
    onReply,
    onScrollToMessage,
    onUpdate,
    onDelete,
    onLoadMore,
    hasMore = false,
    isLoadingMore = false,
    compact = false,
    typingIndicator,
}: VirtualMessageListProps) {
    const parentRef = useRef<HTMLDivElement>(null);
    const wasAtBottomRef = useRef(true);
    const prevMessageCountRef = useRef(messages.length);

    // Estimate row height based on message content
    const estimateSize = useCallback((index: number) => {
        const message = messages[index];
        if (!message) return compact ? 60 : 80;

        // System messages are smaller
        if (message.type === 1) return compact ? 32 : 40;

        // Base height
        let height = compact ? 48 : 64;

        // Add height for content length
        const contentLength = message.content?.length || 0;
        if (contentLength > 100) height += compact ? 16 : 24;
        if (contentLength > 200) height += compact ? 16 : 24;

        // Add height for files
        const fileCount = message.files?.length || 0;
        if (fileCount > 0) height += compact ? 40 : 60;

        // Add height for reply
        if (message.parentId) height += compact ? 24 : 32;

        return height;
    }, [messages, compact]);

    const virtualizer = useVirtualizer({
        count: messages.length + (typingIndicator ? 1 : 0),
        getScrollElement: () => parentRef.current,
        estimateSize,
        overscan: 5,
    });

    // Check if user is at bottom
    const isAtBottom = useCallback(() => {
        const parent = parentRef.current;
        if (!parent) return true;
        const threshold = 100;
        return parent.scrollHeight - parent.scrollTop - parent.clientHeight < threshold;
    }, []);

    // Scroll to bottom
    const scrollToBottom = useCallback(() => {
        if (messages.length > 0) {
            virtualizer.scrollToIndex(messages.length - 1, { align: "end" });
        }
    }, [virtualizer, messages.length]);

    // Auto-scroll when new messages arrive (if user was at bottom)
    useEffect(() => {
        if (messages.length > prevMessageCountRef.current) {
            if (wasAtBottomRef.current) {
                // Small delay to let virtualizer update
                requestAnimationFrame(() => {
                    scrollToBottom();
                });
            }
        }
        prevMessageCountRef.current = messages.length;
    }, [messages.length, scrollToBottom]);

    // Track scroll position and detect scroll to top for loading more
    useEffect(() => {
        const parent = parentRef.current;
        if (!parent) return;

        const handleScroll = () => {
            wasAtBottomRef.current = isAtBottom();

            // Detect scroll to top for infinite scroll
            if (parent.scrollTop < 100 && hasMore && !isLoadingMore && onLoadMore) {
                onLoadMore();
            }
        };

        parent.addEventListener("scroll", handleScroll, { passive: true });
        return () => parent.removeEventListener("scroll", handleScroll);
    }, [isAtBottom, hasMore, isLoadingMore, onLoadMore]);

    // Initial scroll to bottom
    useEffect(() => {
        const timer = setTimeout(() => {
            scrollToBottom();
        }, 100);
        return () => clearTimeout(timer);
    }, []); // eslint-disable-line react-hooks/exhaustive-deps

    // Scroll to specific message
    const handleScrollToMessage = useCallback((messageId: string) => {
        const index = messages.findIndex((m) => m.id === messageId);
        if (index !== -1) {
            virtualizer.scrollToIndex(index, { align: "center" });
        }
        onScrollToMessage?.(messageId);
    }, [messages, virtualizer, onScrollToMessage]);

    // Compute showDate and showSender for each message
    const getMessageProps = (index: number) => {
        const message = messages[index];
        const previousMessage = index > 0 ? messages[index - 1] : null;

        const currentDate = message?.createdAt ? new Date(message.createdAt) : null;
        const previousDate = previousMessage?.createdAt ? new Date(previousMessage.createdAt) : null;

        const showDate =
            index === 0 ||
            !currentDate ||
            !previousDate ||
            currentDate.toDateString() !== previousDate.toDateString();

        const isSameSender = previousMessage?.createdById === message?.createdById;
        const isWithin5Min =
            currentDate &&
            previousDate &&
            currentDate.getTime() - previousDate.getTime() < 5 * 60 * 1000;
        const showSender = index === 0 || showDate || !isSameSender || !isWithin5Min;

        return { showDate, showSender };
    };

    const virtualItems = virtualizer.getVirtualItems();

    return (
        <div
            ref={parentRef}
            className="h-full overflow-y-auto"
            style={{ contain: "strict" }}
        >
            <div
                style={{
                    height: virtualizer.getTotalSize(),
                    width: "100%",
                    position: "relative",
                }}
            >
                <div
                    style={{
                        position: "absolute",
                        top: 0,
                        left: 0,
                        width: "100%",
                        transform: `translateY(${virtualItems[0]?.start ?? 0}px)`,
                    }}
                >
                    {virtualItems.map((virtualRow) => {
                        const isTypingIndicator = virtualRow.index === messages.length;

                        if (isTypingIndicator && typingIndicator) {
                            return (
                                <div
                                    key="typing-indicator"
                                    data-index={virtualRow.index}
                                    ref={virtualizer.measureElement}
                                >
                                    {typingIndicator}
                                </div>
                            );
                        }

                        const message = messages[virtualRow.index];
                        if (!message) return null;

                        const { showDate, showSender } = getMessageProps(virtualRow.index);

                        return (
                            <div
                                key={message.id}
                                data-index={virtualRow.index}
                                data-message-id={message.id}
                                ref={virtualizer.measureElement}
                            >
                                <MessageItem
                                    message={message}
                                    conversationId={conversationId}
                                    currentUserId={currentUserId}
                                    showDate={showDate}
                                    showSender={showSender}
                                    allMessages={messages}
                                    onReply={onReply}
                                    onScrollToMessage={handleScrollToMessage}
                                    onUpdate={onUpdate}
                                    onDelete={onDelete}
                                    compact={compact}
                                />
                            </div>
                        );
                    })}
                </div>
            </div>
        </div>
    );
}
