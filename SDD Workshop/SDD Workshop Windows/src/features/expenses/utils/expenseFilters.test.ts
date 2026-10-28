import type { Expense } from '../../../domain'
import { defaultExpenseFilters, filterExpenses } from './expenseFilters'

const expenses: Expense[] = [
  {
    id: 'expense-1',
    name: 'January rent',
    categoryId: 'housing',
    amount: 1200,
    date: '2026-01-02',
    createdAt: '2026-01-02T00:00:00.000Z',
    updatedAt: '2026-01-02T00:00:00.000Z',
  },
  {
    id: 'expense-2',
    name: 'Weekly groceries',
    categoryId: 'groceries',
    amount: 84.35,
    date: '2026-01-08',
    createdAt: '2026-01-08T00:00:00.000Z',
    updatedAt: '2026-01-08T00:00:00.000Z',
  },
  {
    id: 'expense-3',
    name: 'Airport rideshare',
    categoryId: 'transport',
    amount: 38.4,
    date: '2026-04-03',
    createdAt: '2026-04-03T00:00:00.000Z',
    updatedAt: '2026-04-03T00:00:00.000Z',
  },
]

describe('filterExpenses', () => {
  it('filters by name text', () => {
    expect(
      filterExpenses(expenses, { ...defaultExpenseFilters, name: 'rent' }),
    ).toEqual([expenses[0]])
  })

  it('filters by category, amount, and date range together', () => {
    expect(
      filterExpenses(expenses, {
        ...defaultExpenseFilters,
        amountFrom: 30,
        amountTo: 90,
        categoryId: 'transport',
        dateFrom: '2026-04-01',
        dateTo: '2026-04-30',
        name: '',
      }),
    ).toEqual([expenses[2]])
  })

  it('returns an empty array when nothing matches', () => {
    expect(
      filterExpenses(expenses, {
        ...defaultExpenseFilters,
        name: 'dentist',
      }),
    ).toEqual([])
  })
})
