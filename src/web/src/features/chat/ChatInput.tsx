import toast from 'react-hot-toast';

interface ChatInputProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit: () => void;
  isLoading: boolean;
}

export function ChatInput({
  value,
  onChange,
  onSubmit,
  isLoading,
}: ChatInputProps) {
  const handleSubmit = () => {
    if (!value.trim()) {
      toast('Prompt cannot be empty');
      return;
    }

    onSubmit();
    onChange('');
  };

  return (
    <div>
      <textarea
        value={value}
        onChange={(event) => onChange(event.target.value)}
        onKeyDown={(event) => {
          if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            handleSubmit();
          }
        }}
        disabled={isLoading}
        aria-label="Chat message"
      />
      <button type="button" onClick={handleSubmit} disabled={isLoading}>
        Send
      </button>
    </div>
  );
}
