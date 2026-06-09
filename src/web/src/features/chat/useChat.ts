import { useCallback, useState } from 'react';
import { NetworkError } from '../../services/apiClient';
import { sendMessage as sendChatMessage } from '../../services/chatService';
import type { ChatMessage } from '../../types/chatTypes';

const GENERIC_ERROR_MESSAGE =
  'The assistant could not process your request. Please try again.';

export interface UseChatResult {
  sessionId: string;
  messages: ChatMessage[];
  isLoading: boolean;
  error: string | null;
  networkError: boolean;
  sendMessage: (content: string) => Promise<void>;
}

export function useChat(): UseChatResult {
  const [sessionId] = useState(() => crypto.randomUUID());
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [networkError, setNetworkError] = useState(false);

  const sendMessage = useCallback(
    async (content: string): Promise<void> => {
      setError(null);
      setNetworkError(false);

      const userMessage: ChatMessage = {
        id: crypto.randomUUID(),
        role: 'user',
        content,
        timestamp: new Date().toISOString(),
      };
      setMessages((current) => [...current, userMessage]);
      setIsLoading(true);

      try {
        const response = await sendChatMessage({ sessionId, message: content });
        const assistantMessage: ChatMessage = {
          id: response.messageId,
          role: 'assistant',
          content: response.response,
          timestamp: response.createdAt,
        };
        setMessages((current) => [...current, assistantMessage]);
      } catch (caught) {
        if (caught instanceof NetworkError) {
          setNetworkError(true);
        } else {
          setError(GENERIC_ERROR_MESSAGE);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [sessionId],
  );

  return { sessionId, messages, isLoading, error, networkError, sendMessage };
}
