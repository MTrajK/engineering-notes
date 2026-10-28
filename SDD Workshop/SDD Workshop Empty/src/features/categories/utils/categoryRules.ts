import type { Category, Expense } from '../../../domain'

const categoryColorPalette = [
  '#EA580C',
  '#0F766E',
  '#2563EB',
  '#DB2777',
  '#7C3AED',
  '#CA8A04',
] as const

export function canDeleteCategory(categoryId: string, expenses: Expense[]) {
  return !expenses.some((expense) => expense.categoryId === categoryId)
}

export function isCategoryNameUnique(name: string, categories: Category[]) {
  const normalizedName = name.trim().toLowerCase()

  if (normalizedName.length === 0) {
    return false
  }

  return !categories.some(
    (category) => category.name.trim().toLowerCase() === normalizedName,
  )
}

export function getNextCategoryColor(categories: Category[]) {
  return categoryColorPalette[categories.length % categoryColorPalette.length]
}
