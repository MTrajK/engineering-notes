import type { Category, Expense } from '../../../domain'
import {
  groupMonthlyExpensesByCategory,
  selectAvailableYears,
  selectAveragePerDay,
  selectDashboardMonthSummary,
  selectExpensesForMonth,
  selectMonthlyTotal,
} from './dashboardSelectors'

const categories: Category[] = [
  {
    id: 'housing',
    name: 'Housing',
    color: '#B45309',
    createdAt: '2026-01-01T00:00:00.000Z',
  },
  {
    id: 'groceries',
    name: 'Groceries',
    color: '#15803D',
    createdAt: '2026-01-01T00:00:00.000Z',
  },
]

const expenses: Expense[] = [
  {
    id: 'expense-1',
    name: 'Rent',
    categoryId: 'housing',
    amount: 1000,
    date: '2026-02-03',
    createdAt: '2026-02-03T00:00:00.000Z',
    updatedAt: '2026-02-03T00:00:00.000Z',
  },
  {
    id: 'expense-2',
    name: 'Groceries',
    categoryId: 'groceries',
    amount: 100,
    date: '2026-02-10',
    createdAt: '2026-02-10T00:00:00.000Z',
    updatedAt: '2026-02-10T00:00:00.000Z',
  },
  {
    id: 'expense-3',
    name: 'Snacks',
    categoryId: 'groceries',
    amount: 50,
    date: '2026-02-18',
    createdAt: '2026-02-18T00:00:00.000Z',
    updatedAt: '2026-02-18T00:00:00.000Z',
  },
  {
    id: 'expense-4',
    name: 'Uncategorized',
    categoryId: null,
    amount: 25,
    date: '2025-12-04',
    createdAt: '2025-12-04T00:00:00.000Z',
    updatedAt: '2025-12-04T00:00:00.000Z',
  },
]

describe('dashboard selectors', () => {
  it('selects expenses for the active month and year', () => {
    expect(selectExpensesForMonth(expenses, 2, 2026)).toEqual(expenses.slice(0, 3))
  })

  it('computes the monthly total', () => {
    expect(selectMonthlyTotal(expenses, 2, 2026)).toBe(1150)
  })

  it('computes the average per day for the selected month', () => {
    expect(selectAveragePerDay(expenses, 2, 2026)).toBeCloseTo(41.07, 2)
  })

  it('groups the monthly expenses by category', () => {
    expect(groupMonthlyExpensesByCategory(expenses, categories, 2, 2026)).toEqual([
      {
        amount: 1000,
        categoryColor: '#B45309',
        categoryId: 'housing',
        categoryName: 'Housing',
      },
      {
        amount: 150,
        categoryColor: '#15803D',
        categoryId: 'groceries',
        categoryName: 'Groceries',
      },
    ])
  })

  it('derives available years from the expense data', () => {
    expect(selectAvailableYears(expenses)).toEqual([2026, 2025])
  })

  it('builds a full dashboard month summary', () => {
    expect(selectDashboardMonthSummary(expenses, categories, 2, 2026)).toEqual({
      categorySummaries: [
        {
          amount: 1000,
          categoryColor: '#B45309',
          categoryId: 'housing',
          categoryName: 'Housing',
        },
        {
          amount: 150,
          categoryColor: '#15803D',
          categoryId: 'groceries',
          categoryName: 'Groceries',
        },
      ],
      dayAverage: 1150 / 28,
      month: 2,
      total: 1150,
      year: 2026,
    })
  })
})
