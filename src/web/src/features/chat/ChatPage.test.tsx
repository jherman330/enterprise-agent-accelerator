import { render, screen } from '@testing-library/react';
import { ChatPage } from './ChatPage';

test('presents the initial page state: title, empty placeholder, input, and send button', () => {
  render(<ChatPage />);

  expect(
    screen.getByRole('heading', { name: /enterprise agent accelerator/i }),
  ).toBeTruthy();
  expect(screen.getByText(/no messages yet/i)).toBeTruthy();
  expect(screen.getByLabelText('Chat message')).toBeTruthy();
  expect(screen.getByRole('button', { name: /send/i })).toBeTruthy();
});

test('shows no session ID or model identifier before the first response', () => {
  render(<ChatPage />);

  expect(screen.queryByText('Session')).toBeNull();
  expect(screen.queryByText('Model')).toBeNull();
});
