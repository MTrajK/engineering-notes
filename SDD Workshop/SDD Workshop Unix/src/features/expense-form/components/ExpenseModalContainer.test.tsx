import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '../../../App'
import { storageKeys } from '../../../storage'

describe('ExpenseModalContainer', () => {
  beforeEach(() => {
    window.localStorage.clear()
    window.history.pushState({}, '', '/')
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

  it('opens the modal from the floating button and validates required fields', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /add expense/i }))
    expect(
      screen.getByRole('dialog', { name: /add expense/i }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /^save$/i }))

    expect(screen.getByText(/name is required\./i)).toBeInTheDocument()
  })

  it('closes the modal when escape is pressed', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /add expense/i }))
    expect(
      screen.getByRole('dialog', { name: /add expense/i }),
    ).toBeInTheDocument()

    await user.keyboard('{Escape}')

    await waitFor(() => {
      expect(
        screen.queryByRole('dialog', { name: /add expense/i }),
      ).not.toBeInTheDocument()
    })
  })

  it('creates a new expense and persists it through the app flow', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /add expense/i }))
    await user.type(screen.getByLabelText(/^name$/i), 'Team lunch')
    await user.type(screen.getByLabelText(/^amount$/i), '42.5')
    await user.selectOptions(
      screen.getByLabelText(/^category$/i),
      'category-groceries',
    )
    await user.click(screen.getByRole('button', { name: /^save$/i }))

    await waitFor(() => {
      expect(
        screen.queryByRole('dialog', { name: /add expense/i }),
      ).not.toBeInTheDocument()
    })

    const storedExpenses = JSON.parse(
      window.localStorage.getItem(storageKeys.expenses) ?? '[]',
    )
    expect(
      storedExpenses.some((expense: { name: string }) => expense.name === 'Team lunch'),
    ).toBe(true)
  })

  it('opens edit mode with prepopulated values and persists updates', async () => {
    const user = userEvent.setup()

    window.history.pushState({}, '', '/expenses')
    render(<App />)

    await user.click(
      screen.getByRole('button', { name: /edit airport rideshare/i }),
    )

    const dialog = screen.getByRole('dialog', { name: /edit expense/i })

    expect(dialog).toBeInTheDocument()
    expect(within(dialog).getByLabelText(/^name$/i)).toHaveValue(
      'Airport rideshare',
    )
    expect(within(dialog).getByLabelText(/^category$/i)).toHaveValue(
      'category-transport',
    )
    expect(within(dialog).getByLabelText(/^amount$/i)).toHaveValue(38.4)
    expect(within(dialog).getByLabelText(/^date$/i)).toHaveValue('2026-04-03')

    await user.clear(within(dialog).getByLabelText(/^name$/i))
    await user.type(within(dialog).getByLabelText(/^name$/i), 'Airport transfer')
    await user.clear(within(dialog).getByLabelText(/^amount$/i))
    await user.type(within(dialog).getByLabelText(/^amount$/i), '50')
    await user.click(within(dialog).getByRole('button', { name: /^update$/i }))

    await waitFor(() => {
      expect(
        screen.queryByRole('dialog', { name: /edit expense/i }),
      ).not.toBeInTheDocument()
    })

    const storedExpenses = JSON.parse(
      window.localStorage.getItem(storageKeys.expenses) ?? '[]',
    )
    expect(
      storedExpenses.filter(
        (expense: { id: string }) => expense.id === 'expense-april-rideshare',
      ),
    ).toHaveLength(1)
    expect(
      storedExpenses.some(
        (expense: { name: string }) => expense.name === 'Airport rideshare',
      ),
    ).toBe(false)
    expect(
      storedExpenses.some(
        (expense: { amount: number; id: string; name: string }) =>
          expense.id === 'expense-april-rideshare' &&
          expense.name === 'Airport transfer' &&
          expense.amount === 50,
      ),
    ).toBe(true)
  })
})
