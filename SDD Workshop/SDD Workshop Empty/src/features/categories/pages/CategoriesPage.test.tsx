import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '../../../App'

describe('CategoriesPage', () => {
  beforeEach(() => {
    window.localStorage.clear()
    window.history.pushState({}, '', '/categories')
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

  it('keeps the add button disabled until the category name is valid and unique', async () => {
    const user = userEvent.setup()

    render(<App />)

    const addButton = screen.getByRole('button', { name: /^add$/i })
    const input = screen.getByLabelText(/category name/i)

    expect(addButton).toBeDisabled()

    await user.type(input, 'Groceries')
    expect(addButton).toBeDisabled()

    await user.clear(input)
    await user.type(input, 'Travel')
    expect(addButton).toBeEnabled()
  })

  it('adds a category, clears the input, and disables add again', async () => {
    const user = userEvent.setup()

    render(<App />)

    const addButton = screen.getByRole('button', { name: /^add$/i })
    const input = screen.getByLabelText(/category name/i)

    await user.type(input, 'Travel')
    await user.click(addButton)

    expect(screen.getByText('Travel')).toBeInTheDocument()
    expect(input).toHaveValue('')
    expect(addButton).toBeDisabled()
  })

  it('shows a blocking dialog when trying to delete a used category', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /delete groceries/i }))

    expect(
      screen.getByRole('dialog', {
        name: /the category can not be deleted, it's used by at least one expense\./i,
      }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /ok/i })).toBeInTheDocument()
  })

  it('closes the blocking dialog on escape', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.click(screen.getByRole('button', { name: /delete groceries/i }))
    expect(
      screen.getByRole('dialog', {
        name: /the category can not be deleted, it's used by at least one expense\./i,
      }),
    ).toBeInTheDocument()

    await user.keyboard('{Escape}')

    expect(
      screen.queryByRole('dialog', {
        name: /the category can not be deleted, it's used by at least one expense\./i,
      }),
    ).not.toBeInTheDocument()
  })

  it('allows deleting an unused category after confirmation', async () => {
    const user = userEvent.setup()

    render(<App />)

    await user.type(screen.getByLabelText(/category name/i), 'Travel')
    await user.click(screen.getByRole('button', { name: /^add$/i }))

    const travelPill = screen.getByText('Travel').closest('div')
    expect(travelPill).not.toBeNull()

    await user.click(screen.getByRole('button', { name: /delete travel/i }))

    const dialog = screen.getByRole('dialog', { name: /delete the category\?/i })
    await user.click(within(dialog).getByRole('button', { name: /^yes$/i }))

    expect(screen.queryByText('Travel')).not.toBeInTheDocument()
  })
})
