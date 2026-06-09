import { render, screen } from '@testing-library/react';
import type { ChatMessage } from '../../types/chatTypes';
import { ChatMessageList } from './ChatMessageList';

const scrollIntoViewMock = vi.fn();

beforeAll(() => {
  // jsdom does not implement scrollIntoView.
  Element.prototype.scrollIntoView = scrollIntoViewMock;
});

afterEach(() => {
  vi.clearAllMocks();
});

function msg(
  id: string,
  role: ChatMessage['role'],
  content: string,
): ChatMessage {
  return { id, role, content, timestamp: '2026-06-09T00:00:00.000Z' };
}

test('shows an empty-state placeholder when there are no messages', () => {
  render(<ChatMessageList messages={[]} />);

  expect(screen.getByText(/no messages yet/i)).toBeTruthy();
});

test('renders user and assistant messages in chronological order with author labels', () => {
  const messages = [
    msg('1', 'user', 'first question'),
    msg('2', 'assistant', 'first answer'),
    msg('3', 'user', 'second question'),
  ];

  render(<ChatMessageList messages={messages} />);

  const items = screen.getAllByRole('listitem');
  expect(items).toHaveLength(3);
  expect(items[0].textContent).toContain('first question');
  expect(items[1].textContent).toContain('first answer');
  expect(items[2].textContent).toContain('second question');
  expect(screen.getAllByText('You')).toHaveLength(2);
  expect(screen.getByText('Assistant')).toBeTruthy();
});

test('visually distinguishes user messages from assistant messages', () => {
  render(
    <ChatMessageList
      messages={[msg('1', 'user', 'hi'), msg('2', 'assistant', 'hello')]}
    />,
  );

  const items = screen.getAllByRole('listitem');
  expect(items[0].getAttribute('data-role')).toBe('user');
  expect(items[1].getAttribute('data-role')).toBe('assistant');
  expect(items[0].className).not.toBe(items[1].className);
});

test('auto-scrolls to the latest message as new messages arrive', () => {
  const { rerender } = render(
    <ChatMessageList messages={[msg('1', 'user', 'hi')]} />,
  );
  expect(scrollIntoViewMock).toHaveBeenCalled();

  scrollIntoViewMock.mockClear();
  rerender(
    <ChatMessageList
      messages={[msg('1', 'user', 'hi'), msg('2', 'assistant', 'hello')]}
    />,
  );
  expect(scrollIntoViewMock).toHaveBeenCalled();
});
