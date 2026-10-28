import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '../../../App'

describe('ExpensesPage', () => {
  beforeEach(() => {
    window.localStorage.clear()
    window.history.pushState({}, '', '/expenses')
    window.matchMedia = vi.fn().mockImplementation(() => ({
      matches: false,
      media: '(prefers-color-scheme: dark)',
      onchange: null,
      addEventListener: vi.fn(),
      addListener: vi.fn(),
      dispatchEvent: vi.fn(),
      removeEventListener: vi.fn(),
      removeListener: vi.fn(),
    }))
  })

  it('shows an empty state when filters remove every row', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.type(screen.getByLabelText(/^name$/i), 'missing value')

    expect(
      screen.getByText(/no expenses match the current filters/i),
    ).toBeInTheDocument()
  })

  it('supports pagination across the seeded dataset', async () => {
    const user = userEvent.setup()

    render(<App />)

    expect(screen.getByText('Airport rideshare')).toBeInTheDocument()
    expect(screen.queryByText('Train tickets')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /next/i }))

    expect(screen.getByText('Train tickets')).toBeInTheDocument()
  })

  it('requires confirmation before deleting an expense', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(
      screen.getByRole('button', { name: /delete airport rideshare/i }),
    )
    expect(
      screen.getByRole('dialog', { name: /delete the expense\?/i }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /^no$/i }))
    expect(
      screen.queryByRole('dialog', { name: /delete the expense\?/i }),
    ).not.toBeInTheDocument()
    expect(screen.getByText('Airport rideshare')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', { name: /delete airport rideshare/i }),
    )
    await user.click(screen.getByRole('button', { name: /^yes$/i }))

    await waitFor(() => {
      expect(screen.queryByText('Airport rideshare')).not.toBeInTheDocument()
    })
  })

  it('closes the delete dialog when escape is pressed', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(
      screen.getByRole('button', { name: /delete airport rideshare/i }),
    )
    expect(
      screen.getByRole('dialog', { name: /delete the expense\?/i }),
    ).toBeInTheDocument()

    await user.keyboard('{Escape}')

    expect(
      screen.queryByRole('dialog', { name: /delete the expense\?/i }),
    ).not.toBeInTheDocument()
  })
})
