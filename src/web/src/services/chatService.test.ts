import { post } from './apiClient';
import { sendMessage } from './chatService';
import type { ChatResponse } from '../types/chatTypes';

vi.mock('./apiClient', () => ({
  post: vi.fn(),
}));

const postMock = vi.mocked(post);

afterEach(() => {
  vi.resetAllMocks();
});

test('sendMessage posts the request to /api/chat and returns the typed response', async () => {
  const response: ChatResponse = {
    sessionId: 's1',
    messageId: 'm1',
    response: 'hello there',
    model: 'gpt-test',
    createdAt: '2026-06-09T00:00:00.0000000Z',
  };
  postMock.mockResolvedValue(response);

  const result = await sendMessage({ sessionId: 's1', message: 'hello' });

  expect(postMock).toHaveBeenCalledWith('/api/chat', {
    sessionId: 's1',
    message: 'hello',
  });
  expect(result).toBe(response);
});

test('sendMessage propagates errors from ApiClient without swallowing them', async () => {
  const error = new Error('boom');
  postMock.mockRejectedValue(error);

  await expect(sendMessage({ sessionId: 's1', message: 'hello' })).rejects.toBe(
    error,
  );
});
