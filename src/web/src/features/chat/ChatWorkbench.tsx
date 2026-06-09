import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { ChatInput } from './ChatInput';
import { ChatMessageList } from './ChatMessageList';
import { useChat } from './useChat';

export function ChatWorkbench() {
  const {
    sessionId,
    messages,
    isLoading,
    error,
    networkError,
    model,
    sendMessage,
  } = useChat();
  const [input, setInput] = useState('');

  useEffect(() => {
    if (networkError) {
      toast.error('Network error — could not reach the server.');
    }
  }, [networkError]);

  const handleSubmit = () => {
    void sendMessage(input);
  };

  return (
    <div className="chat-workbench">
      {messages.length > 0 && (
        <dl className="chat-workbench__metadata">
          <div>
            <dt>Session</dt>
            <dd>{sessionId}</dd>
          </div>
          {model && (
            <div>
              <dt>Model</dt>
              <dd>{model}</dd>
            </div>
          )}
        </dl>
      )}

      <ChatMessageList messages={messages} />

      {isLoading && (
        <p role="status" className="chat-workbench__loading">
          Waiting for response…
        </p>
      )}

      {error && (
        <p role="alert" className="chat-workbench__error">
          {error}
        </p>
      )}

      <ChatInput
        value={input}
        onChange={setInput}
        onSubmit={handleSubmit}
        isLoading={isLoading}
      />
    </div>
  );
}
