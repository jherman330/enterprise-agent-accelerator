import { ApiError, NetworkError, post } from './apiClient';

const BASE_URL = 'http://localhost:5099';

beforeEach(() => {
  vi.stubEnv('VITE_API_BASE_URL', BASE_URL);
});

afterEach(() => {
  vi.unstubAllEnvs();
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
});

test('post sends a JSON POST to the configured base URL and returns the parsed body', async () => {
  const requestBody = { sessionId: 's1', message: 'hi' };
  const responseBody = { sessionId: 's1', response: 'hello' };
  const fetchMock = vi
    .fn()
    .mockResolvedValue(
      new Response(JSON.stringify(responseBody), { status: 200 }),
    );
  vi.stubGlobal('fetch', fetchMock);

  const result = await post<typeof responseBody>('/api/chat', requestBody);

  expect(result).toEqual(responseBody);
  expect(fetchMock).toHaveBeenCalledWith(`${BASE_URL}/api/chat`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(requestBody),
  });
});

test('post throws NetworkError when fetch rejects (server unreachable)', async () => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockRejectedValue(new TypeError('Failed to fetch')),
  );

  await expect(post('/api/chat', {})).rejects.toBeInstanceOf(NetworkError);
});

test('post throws ApiError with the status and parsed body on a non-2xx response', async () => {
  const errorBody = { error: 'bad request' };
  vi.stubGlobal(
    'fetch',
    vi
      .fn()
      .mockResolvedValue(
        new Response(JSON.stringify(errorBody), { status: 400 }),
      ),
  );

  const error = await post('/api/chat', {}).catch((caught: unknown) => caught);

  expect(error).toBeInstanceOf(ApiError);
  expect((error as ApiError).status).toBe(400);
  expect((error as ApiError).body).toEqual(errorBody);
});

test('post throws ApiError with a null body when the error response is not JSON', async () => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue(new Response('not json', { status: 500 })),
  );

  const error = await post('/api/chat', {}).catch((caught: unknown) => caught);

  expect(error).toBeInstanceOf(ApiError);
  expect((error as ApiError).status).toBe(500);
  expect((error as ApiError).body).toBeNull();
});
