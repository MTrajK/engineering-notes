import type { Category, Expense } from '../../../domain'
import {
  canDeleteCategory,
  getNextCategoryColor,
  isCategoryNameUnique,
} from './categoryRules'

const categories: Category[] = [
  {
    id: 'category-groceries',
    name: 'Groceries',
    color: '#15803D',
    createdAt: '2026-01-01T00:00:00.000Z',
  },
  {
    id: 'category-health',
    name: 'Health',
    color: '#DB2777',
    createdAt: '2026-01-01T00:00:00.000Z',
  },
]

const expenses: Expense[] = [
  {
    id: 'expense-1',
    name: 'Market',
    categoryId: 'category-groceries',
    amount: 30,
    date: '2026-04-10',
    createdAt: '2026-04-10T00:00:00.000Z',
    updatedAt: '2026-04-10T00:00:00.000Z',
  },
]

describe('categoryRules', () => {
  it('enforces case-insensitive category uniqueness on trimmed input', () => {
    expect(isCategoryNameUnique('  groceries  ', categories)).toBe(false)
    expect(isCategoryNameUnique('Travel', categories)).toBe(true)
  })

  it('blocks deleting categories used by at least one expense', () => {
    expect(canDeleteCategory('category-groceries', expenses)).toBe(false)
    expect(canDeleteCategory('category-health', expenses)).toBe(true)
  })

  it('derives the next category color deterministically', () => {
    expect(getNextCategoryColor(categories)).toBe('#2563EB')
  })
})
