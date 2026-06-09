import { useEffect, useRef } from 'react';
import type { ChatMessage } from '../../types/chatTypes';

interface ChatMessageListProps {
  messages: ChatMessage[];
}

const EMPTY_STATE_TEXT = 'No messages yet. Type a prompt to begin.';

export function ChatMessageList({ messages }: ChatMessageListProps) {
  const endRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  if (messages.length === 0) {
    return <p className="chat-message-list__empty">{EMPTY_STATE_TEXT}</p>;
  }

  return (
    <div className="chat-message-list">
      <ul className="chat-message-list__items">
        {messages.map((message) => (
          <li
            key={message.id}
            className={`chat-message chat-message--${message.role}`}
            data-role={message.role}
          >
            <span className="chat-message__author">
              {message.role === 'user' ? 'You' : 'Assistant'}
            </span>
            <span className="chat-message__content">{message.content}</span>
          </li>
        ))}
      </ul>
      <div ref={endRef} />
    </div>
  );
}
