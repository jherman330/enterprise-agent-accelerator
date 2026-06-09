import { act, renderHook, waitFor } from '@testing-library/react';
import { ApiError, NetworkError } from '../../services/apiClient';
import { sendMessage as sendChatMessage } from '../../services/chatService';
import type { ChatResponse } from '../../types/chatTypes';
import { useChat } from './useChat';

vi.mock('../../services/chatService', () => ({
  sendMessage: vi.fn(),
}));

const sendChatMessageMock = vi.mocked(sendChatMessage);

afterEach(() => {
  vi.resetAllMocks();
});

function makeResponse(overrides: Partial<ChatResponse> = {}): ChatResponse {
  return {
    sessionId: 'srv-session',
    messageId: 'srv-message-id',
    response: 'assistant reply',
    model: 'gpt-test',
    createdAt: '2026-06-09T00:00:00.0000000Z',
    ...overrides,
  };
}

test('initializes with a generated session ID and empty state', () => {
  const { result } = renderHook(() => useChat());

  expect(result.current.sessionId).toMatch(
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i,
  );
  expect(result.current.messages).toEqual([]);
  expect(result.current.isLoading).toBe(false);
  expect(result.current.error).toBeNull();
  expect(result.current.networkError).toBe(false);
});

test('appends the user message and assistant response on success', async () => {
  sendChatMessageMock.mockResolvedValue(makeResponse({ response: 'hi back' }));
  const { result } = renderHook(() => useChat());

  await act(async () => {
    await result.current.sendMessage('hello');
  });

  expect(result.current.isLoading).toBe(false);
  expect(result.current.messages).toHaveLength(2);
  expect(result.current.messages[0]).toMatchObject({
    role: 'user',
    content: 'hello',
  });
  expect(result.current.messages[1]).toMatchObject({
    role: 'assistant',
    content: 'hi back',
  });
  expect(sendChatMessageMock).toHaveBeenCalledWith({
    sessionId: result.current.sessionId,
    message: 'hello',
  });
});

test('locks into a loading state while the request is in flight', async () => {
  let resolvePromise!: (value: ChatResponse) => void;
  sendChatMessageMock.mockReturnValue(
    new Promise<ChatResponse>((resolve) => {
      resolvePromise = resolve;
    }),
  );
  const { result } = renderHook(() => useChat());

  let sendPromise!: Promise<void>;
  act(() => {
    sendPromise = result.current.sendMessage('hello');
  });

  await waitFor(() => expect(result.current.isLoading).toBe(true));
  expect(result.current.messages).toHaveLength(1);

  await act(async () => {
    resolvePromise(makeResponse());
    await sendPromise;
  });

  expect(result.current.isLoading).toBe(false);
});

test('sets networkError (not inline error) when ChatService throws NetworkError', async () => {
  sendChatMessageMock.mockRejectedValue(new NetworkError());
  const { result } = renderHook(() => useChat());

  await act(async () => {
    await result.current.sendMessage('hello');
  });

  expect(result.current.networkError).toBe(true);
  expect(result.current.error).toBeNull();
  expect(result.current.isLoading).toBe(false);
});

test('sets an inline error (not networkError) when ChatService throws ApiError', async () => {
  sendChatMessageMock.mockRejectedValue(new ApiError(500, { detail: 'boom' }));
  const { result } = renderHook(() => useChat());

  await act(async () => {
    await result.current.sendMessage('hello');
  });

  expect(result.current.error).not.toBeNull();
  expect(result.current.networkError).toBe(false);
  expect(result.current.isLoading).toBe(false);
});

test('clears a previous error and networkError when a new message is submitted', async () => {
  sendChatMessageMock.mockRejectedValueOnce(new ApiError(500, null));
  const { result } = renderHook(() => useChat());

  await act(async () => {
    await result.current.sendMessage('first');
  });
  expect(result.current.error).not.toBeNull();

  sendChatMessageMock.mockResolvedValueOnce(makeResponse());
  await act(async () => {
    await result.current.sendMessage('second');
  });

  expect(result.current.error).toBeNull();
  expect(result.current.networkError).toBe(false);
});

test('exposes the model from the response (null until the first successful response)', async () => {
  sendChatMessageMock.mockResolvedValue(
    makeResponse({ model: 'gpt-deployment' }),
  );
  const { result } = renderHook(() => useChat());

  expect(result.current.model).toBeNull();

  await act(async () => {
    await result.current.sendMessage('hello');
  });

  expect(result.current.model).toBe('gpt-deployment');
});

test('reuses the same session ID across requests', async () => {
  sendChatMessageMock.mockResolvedValue(makeResponse());
  const { result } = renderHook(() => useChat());
  const { sessionId } = result.current;

  await act(async () => {
    await result.current.sendMessage('a');
  });
  await act(async () => {
    await result.current.sendMessage('b');
  });

  expect(result.current.sessionId).toBe(sessionId);
  expect(sendChatMessageMock).toHaveBeenNthCalledWith(1, {
    sessionId,
    message: 'a',
  });
  expect(sendChatMessageMock).toHaveBeenNthCalledWith(2, {
    sessionId,
    message: 'b',
  });
});
