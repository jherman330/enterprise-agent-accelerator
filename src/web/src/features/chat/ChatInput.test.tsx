import { fireEvent, render, screen } from '@testing-library/react';
import toast from 'react-hot-toast';
import { ChatInput } from './ChatInput';

vi.mock('react-hot-toast', () => ({
  default: vi.fn(),
}));

const toastMock = vi.mocked(toast);

afterEach(() => {
  vi.clearAllMocks();
});

interface RenderOverrides {
  value?: string;
  isLoading?: boolean;
}

function setup(overrides: RenderOverrides = {}) {
  const props = {
    value: overrides.value ?? '',
    onChange: vi.fn(),
    onSubmit: vi.fn(),
    isLoading: overrides.isLoading ?? false,
  };
  render(<ChatInput {...props} />);
  return props;
}

function textarea(): HTMLTextAreaElement {
  return screen.getByLabelText('Chat message') as HTMLTextAreaElement;
}

function sendButton(): HTMLButtonElement {
  return screen.getByRole('button', { name: /send/i }) as HTMLButtonElement;
}

test('reports typed text through onChange', () => {
  const props = setup();

  fireEvent.change(textarea(), { target: { value: 'hello' } });

  expect(props.onChange).toHaveBeenCalledWith('hello');
});

test('submits and clears the input when the send button is clicked with a non-empty value', () => {
  const props = setup({ value: 'hello' });

  fireEvent.click(sendButton());

  expect(props.onSubmit).toHaveBeenCalledTimes(1);
  expect(props.onChange).toHaveBeenCalledWith('');
  expect(toastMock).not.toHaveBeenCalled();
});

test('submits when Enter is pressed without Shift', () => {
  const props = setup({ value: 'hello' });

  fireEvent.keyDown(textarea(), { key: 'Enter' });

  expect(props.onSubmit).toHaveBeenCalledTimes(1);
  expect(props.onChange).toHaveBeenCalledWith('');
});

test('does not submit on Shift+Enter (allows a newline)', () => {
  const props = setup({ value: 'hello' });

  fireEvent.keyDown(textarea(), { key: 'Enter', shiftKey: true });

  expect(props.onSubmit).not.toHaveBeenCalled();
});

test('shows a toast and does not submit when the value is empty or whitespace', () => {
  const props = setup({ value: '   ' });

  fireEvent.click(sendButton());

  expect(toastMock).toHaveBeenCalledWith('Prompt cannot be empty');
  expect(props.onSubmit).not.toHaveBeenCalled();
  expect(props.onChange).not.toHaveBeenCalled();
});

test('disables the input and send button while a request is in flight', () => {
  setup({ value: 'hello', isLoading: true });

  expect(textarea().disabled).toBe(true);
  expect(sendButton().disabled).toBe(true);
});
