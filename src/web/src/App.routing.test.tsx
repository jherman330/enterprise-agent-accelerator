import { render, screen } from '@testing-library/react';
import App from './App';

test('redirects the root path to the chat page and renders the workbench', () => {
  window.history.pushState({}, '', '/');

  render(<App />);

  expect(
    screen.getByRole('heading', { name: /enterprise agent accelerator/i }),
  ).toBeTruthy();
  expect(screen.getByText(/no messages yet/i)).toBeTruthy();
});

test('renders the chat page directly at /chat', () => {
  window.history.pushState({}, '', '/chat');

  render(<App />);

  expect(screen.getByLabelText('Chat message')).toBeTruthy();
  expect(screen.getByRole('button', { name: /send/i })).toBeTruthy();
});
