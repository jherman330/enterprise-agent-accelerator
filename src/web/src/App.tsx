import { ErrorBoundary } from 'react-error-boundary';
import { Toaster } from 'react-hot-toast';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ChatPage } from './features/chat/ChatPage';

export function ErrorFallback() {
  return <div role="alert">Something went wrong. Please refresh the page.</div>;
}

function App() {
  return (
    <ErrorBoundary FallbackComponent={ErrorFallback}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/chat" replace />} />
          <Route path="/chat" element={<ChatPage />} />
        </Routes>
      </BrowserRouter>
      <Toaster position="top-right" />
    </ErrorBoundary>
  );
}

export default App;
