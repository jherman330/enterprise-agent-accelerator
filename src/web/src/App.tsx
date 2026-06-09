import { ErrorBoundary } from 'react-error-boundary';
import { Toaster } from 'react-hot-toast';

export function ErrorFallback() {
  return <div role="alert">Something went wrong. Please refresh the page.</div>;
}

function App() {
  return (
    <ErrorBoundary FallbackComponent={ErrorFallback}>
      <h1>Enterprise Agent Accelerator</h1>
      <Toaster position="top-right" />
    </ErrorBoundary>
  );
}

export default App;
