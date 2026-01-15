import { useState } from "react";
import { postApiConversationsByConversationIdMessagesByIdPin } from "@/src/api";
import type { MessageDto2 as MessageDto } from "@/src/api/database/types.gen";

export function usePinMessage(conversationId: string, onUpdate?: (message: MessageDto) => void) {
  const [isPinning, setIsPinning] = useState(false);

  // Toggle pin/unpin
  const togglePin = async (message: MessageDto) => {
    if (!message.id || !conversationId) return;
    setIsPinning(true);
    try {
      await postApiConversationsByConversationIdMessagesByIdPin({
        path: { conversationId, id: message.id },
        body: { id: message.id, conversationId, isPined: !message.isPined },
      });
      const updatedMessage: MessageDto = { ...message, isPined: !message.isPined };
      onUpdate?.(updatedMessage);
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.message || "Không thể ghim/bỏ ghim tin nhắn";
      alert(msg);
    } finally { setIsPinning(false); }
  };

  return { togglePin, isPinning };
}