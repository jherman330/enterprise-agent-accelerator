import type { ChatRequest, ChatResponse } from './chatTypes';

test('ChatResponse matches the backend POST /api/chat wire shape (camelCase fields)', () => {
  const wirePayload = `{
    "sessionId": "session-1",
    "messageId": "11111111-1111-1111-1111-111111111111",
    "response": "hello there",
    "model": "gpt-test",
    "createdAt": "2026-06-09T00:00:00.0000000Z"
  }`;

  const parsed = JSON.parse(wirePayload) as ChatResponse;

  expect(parsed.sessionId).toBe('session-1');
  expect(parsed.messageId).toBe('11111111-1111-1111-1111-111111111111');
  expect(parsed.response).toBe('hello there');
  expect(parsed.model).toBe('gpt-test');
  expect(parsed.createdAt).toBe('2026-06-09T00:00:00.0000000Z');
});

test('ChatRequest serializes to exactly the body the API expects', () => {
  const request: ChatRequest = { sessionId: 'session-1', message: 'hello' };

  expect(JSON.parse(JSON.stringify(request))).toEqual({
    sessionId: 'session-1',
    message: 'hello',
  });
});
