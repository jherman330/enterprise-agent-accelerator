import { fireEvent, render, screen } from '@testing-library/react';
import toast from 'react-hot-toast';
import type { ChatMessage } from '../../types/chatTypes';
import { ChatWorkbench } from './ChatWorkbench';
import { useChat } from './useChat';
import type { UseChatResult } from './useChat';

vi.mock('react-hot-toast', () => {
  const toastFn = Object.assign(vi.fn(), { error: vi.fn() });
  return { default: toastFn };
});

vi.mock('./useChat', () => ({
  useChat: vi.fn(),
}));

const useChatMock = vi.mocked(useChat);
const toastErrorMock = vi.mocked(toast.error);

beforeAll(() => {
  // jsdom does not implement scrollIntoView (used by the real ChatMessageList).
  Element.prototype.scrollIntoView = vi.fn();
});

afterEach(() => {
  vi.clearAllMocks();
});

function chatState(overrides: Partial<UseChatResult> = {}): UseChatResult {
  return {
    sessionId: 'session-abc',
    messages: [],
    isLoading: false,
    error: null,
    networkError: false,
    model: null,
    sendMessage: vi.fn().mockResolvedValue(undefined),
    ...overrides,
  };
}

function assistantMessage(): ChatMessage {
  return {
    id: '1',
    role: 'assistant',
    content: 'hi there',
    timestamp: '2026-06-09T00:00:00.000Z',
  };
}

test('shows a loading indicator while a request is in flight', () => {
  useChatMock.mockReturnValue(chatState({ isLoading: true }));

  render(<ChatWorkbench />);

  expect(screen.getByRole('status')).toBeTruthy();
});

test('hides the loading indicator when idle', () => {
  useChatMock.mockReturnValue(chatState({ isLoading: false }));

  render(<ChatWorkbench />);

  expect(screen.queryByRole('status')).toBeNull();
});

test('renders an inline API error while keeping the input available', () => {
  useChatMock.mockReturnValue(chatState({ error: 'The request failed.' }));

  render(<ChatWorkbench />);

  expect(screen.getByText('The request failed.')).toBeTruthy();
  expect(screen.getByLabelText('Chat message')).toBeTruthy();
});

test('fires the network-error toast on the false->true transition and not again while it stays true', () => {
  useChatMock.mockReturnValue(chatState({ networkError: false }));
  const { rerender } = render(<ChatWorkbench />);
  expect(toastErrorMock).not.toHaveBeenCalled();

  useChatMock.mockReturnValue(chatState({ networkError: true }));
  rerender(<ChatWorkbench />);
  expect(toastErrorMock).toHaveBeenCalledTimes(1);
  expect(toastErrorMock).toHaveBeenCalledWith(
    'Network error — could not reach the server.',
  );

  useChatMock.mockReturnValue(chatState({ networkError: true }));
  rerender(<ChatWorkbench />);
  expect(toastErrorMock).toHaveBeenCalledTimes(1);
});

test('does not display session metadata before any messages exist', () => {
  useChatMock.mockReturnValue(chatState({ messages: [] }));

  render(<ChatWorkbench />);

  expect(screen.queryByText('Session')).toBeNull();
  expect(screen.queryByText('session-abc')).toBeNull();
});

test('displays the session ID and model after the first response', () => {
  useChatMock.mockReturnValue(
    chatState({ messages: [assistantMessage()], model: 'gpt-deployment' }),
  );

  render(<ChatWorkbench />);

  expect(screen.getByText('session-abc')).toBeTruthy();
  expect(screen.getByText('gpt-deployment')).toBeTruthy();
});

test('submits the typed message through useChat.sendMessage', () => {
  const sendMessage = vi.fn().mockResolvedValue(undefined);
  useChatMock.mockReturnValue(chatState({ sendMessage }));

  render(<ChatWorkbench />);
  fireEvent.change(screen.getByLabelText('Chat message'), {
    target: { value: 'hello' },
  });
  fireEvent.click(screen.getByRole('button', { name: /send/i }));

  expect(sendMessage).toHaveBeenCalledWith('hello');
});
