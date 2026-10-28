import type { Category, Expense } from '../domain'
import { appReducer } from './appReducer'
import type { AppState } from './appState'

const expense: Expense = {
  id: 'expense-1',
  name: 'Groceries',
  categoryId: 'category-1',
  amount: 25,
  date: '2026-04-22',
  createdAt: '2026-04-22T10:00:00.000Z',
  updatedAt: '2026-04-22T10:00:00.000Z',
}

const category: Category = {
  id: 'category-1',
  name: 'Food',
  color: '#22C55E',
  createdAt: '2026-04-22T10:00:00.000Z',
}

function createState(overrides: Partial<AppState> = {}): AppState {
  return {
    categories: [category],
    expenses: [expense],
    expenseModal: {
      expenseId: null,
      mode: null,
      isOpen: false,
    },
    selectedDashboardMonth: 4,
    selectedDashboardYear: 2026,
    themeMode: 'light',
    ...overrides,
  }
}

describe('appReducer', () => {
  it('adds an expense', () => {
    const nextExpense: Expense = {
      ...expense,
      id: 'expense-2',
      name: 'Bus ticket',
    }

    const result = appReducer(createState(), {
      type: 'expense/add',
      payload: nextExpense,
    })

    expect(result.expenses).toHaveLength(2)
    expect(result.expenses[1]).toEqual(nextExpense)
  })

  it('updates an expense', () => {
    const updatedExpense: Expense = {
      ...expense,
      amount: 30,
      updatedAt: '2026-04-22T11:00:00.000Z',
    }

    const result = appReducer(createState(), {
      type: 'expense/update',
      payload: updatedExpense,
    })

    expect(result.expenses[0]).toEqual(updatedExpense)
  })

  it('deletes an expense', () => {
    const secondExpense: Expense = {
      ...expense,
      id: 'expense-2',
      name: 'Internet bill',
    }

    const result = appReducer(createState({ expenses: [expense, secondExpense] }), {
      type: 'expense/delete',
      payload: { expenseId: expense.id },
    })

    expect(result.expenses).toEqual([secondExpense])
  })

  it('adds and deletes categories', () => {
    const nextCategory: Category = {
      id: 'category-2',
      name: 'Transport',
      color: '#2563EB',
      createdAt: '2026-04-22T12:00:00.000Z',
    }

    const added = appReducer(createState(), {
      type: 'category/add',
      payload: nextCategory,
    })
    const deleted = appReducer(added, {
      type: 'category/delete',
      payload: { categoryId: category.id },
    })

    expect(added.categories).toHaveLength(2)
    expect(deleted.categories).toEqual([nextCategory])
  })

  it('updates selected dashboard month and year', () => {
    const withMonth = appReducer(createState(), {
      type: 'dashboard/setMonth',
      payload: { month: 7 },
    })
    const withYear = appReducer(withMonth, {
      type: 'dashboard/setYear',
      payload: { year: 2027 },
    })

    expect(withYear.selectedDashboardMonth).toBe(7)
    expect(withYear.selectedDashboardYear).toBe(2027)
  })

  it('updates theme mode', () => {
    const result = appReducer(createState(), {
      type: 'theme/set',
      payload: { themeMode: 'dark' },
    })

    expect(result.themeMode).toBe('dark')
  })

  it('opens and closes the expense modal', () => {
    const openCreate = appReducer(createState(), {
      type: 'expenseModal/openCreate',
    })
    const openEdit = appReducer(createState(), {
      type: 'expenseModal/openEdit',
      payload: { expenseId: expense.id },
    })
    const closed = appReducer(openEdit, {
      type: 'expenseModal/close',
    })

    expect(openCreate.expenseModal).toEqual({
      expenseId: null,
      mode: 'create',
      isOpen: true,
    })
    expect(openEdit.expenseModal).toEqual({
      expenseId: expense.id,
      mode: 'edit',
      isOpen: true,
    })
    expect(closed.expenseModal).toEqual({
      expenseId: null,
      mode: null,
      isOpen: false,
    })
  })
})
