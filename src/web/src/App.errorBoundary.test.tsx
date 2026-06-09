import { render, screen } from '@testing-library/react';
import { ErrorBoundary } from 'react-error-boundary';
import { ErrorFallback } from './App';

function Bomb(): never {
  throw new Error('boom');
}

afterEach(() => {
  vi.restoreAllMocks();
});

test('ErrorFallback shows a safe, non-crashing alert message', () => {
  render(<ErrorFallback />);

  expect(screen.getByRole('alert').textContent).toMatch(
    /something went wrong/i,
  );
});

test('the error boundary catches a child render exception and shows the fallback', () => {
  // React logs caught render errors to console.error; silence it to keep test output clean.
  vi.spyOn(console, 'error').mockImplementation(() => {});

  render(
    <ErrorBoundary FallbackComponent={ErrorFallback}>
      <Bomb />
    </ErrorBoundary>,
  );

  expect(screen.getByRole('alert').textContent).toMatch(
    /something went wrong/i,
  );
});
