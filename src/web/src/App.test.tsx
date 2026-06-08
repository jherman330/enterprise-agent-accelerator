import { render, screen } from '@testing-library/react'
import App from './App'

test('renders application heading', () => {
  render(<App />)

  expect(screen.getByRole('heading', { name: 'Enterprise Agent Accelerator' })).toBeTruthy()
})
