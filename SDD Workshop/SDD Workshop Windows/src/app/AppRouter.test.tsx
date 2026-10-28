import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '../App'
import { storageKeys } from '../storage'

describe('App shell navigation', () => {
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

  it('renders the persistent navbar and switches routes', async () => {
    const user = userEvent.setup()

    render(<App />)

    expect(screen.getByRole('navigation', { name: /primary/i })).toBeVisible()
    expect(
      screen.getByRole('heading', { name: 'Dashboard' }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText(/monthly total/i)).toHaveTextContent('$70.25')

    const expensesLink = screen.getByRole('link', { name: /expenses/i })
    await user.click(expensesLink)

    expect(screen.getByRole('heading', { name: 'Expenses' })).toBeInTheDocument()
    expect(expensesLink).toHaveAttribute('aria-current', 'page')

    const categoriesLink = screen.getByRole('link', { name: /categories/i })
    await user.click(categoriesLink)

    expect(
      screen.getByRole('heading', { name: 'Categories' }),
    ).toBeInTheDocument()
    expect(categoriesLink).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('button', { name: /add expense/i })).toBeVisible()
  })

  it('persists theme changes through the provider', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /activate dark mode/i }))

    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(JSON.parse(window.localStorage.getItem(storageKeys.theme) ?? 'null')).toBe(
      'dark',
    )
  })

  it('uses system preference when no stored theme exists', () => {
    window.matchMedia = vi.fn().mockImplementation(() => ({
      matches: true,
      media: '(prefers-color-scheme: dark)',
      onchange: null,
      addEventListener: vi.fn(),
      addListener: vi.fn(),
      dispatchEvent: vi.fn(),
      removeEventListener: vi.fn(),
      removeListener: vi.fn(),
    }))

    render(<App />)

    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(JSON.parse(window.localStorage.getItem(storageKeys.theme) ?? 'null')).toBe(
      'dark',
    )
  })

  it('updates dashboard summaries when month and year change', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.selectOptions(screen.getByLabelText(/select month/i), '1')
    expect(screen.getByLabelText(/monthly total/i)).toHaveTextContent('$1356.85')
    expect(screen.getByLabelText(/daily average/i)).toHaveTextContent('$43.77')

    await user.selectOptions(screen.getByLabelText(/select year/i), '2025')
    await user.selectOptions(screen.getByLabelText(/select month/i), '12')

    expect(screen.getByLabelText(/monthly total/i)).toHaveTextContent('$1390.00')
    expect(screen.getByLabelText(/daily average/i)).toHaveTextContent('$44.84')
  })
})
