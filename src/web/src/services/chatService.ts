import { post } from './apiClient';
import type { ChatRequest, ChatResponse } from '../types/chatTypes';

export const sendMessage = (request: ChatRequest): Promise<ChatResponse> =>
  post<ChatResponse>('/api/chat', request);
