import { act, render, screen } from '@testing-library/react';
import toast from 'react-hot-toast';
import App from './App';

// jsdom does not implement matchMedia, which react-hot-toast reads for
// prefers-reduced-motion. Stub it so the Toaster can render under test.
beforeAll(() => {
  vi.stubGlobal(
    'matchMedia',
    vi.fn().mockReturnValue({
      matches: false,
      media: '',
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }),
  );
});

afterEach(() => {
  toast.remove();
});

afterAll(() => {
  vi.unstubAllGlobals();
});

test('mounts the Toaster provider so toasts triggered from anywhere are rendered', async () => {
  render(<App />);

  await act(async () => {
    toast('network toast probe');
  });

  expect(await screen.findByText('network toast probe')).toBeTruthy();
});
