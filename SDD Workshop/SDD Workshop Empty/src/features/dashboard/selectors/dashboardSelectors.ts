import { getDaysInMonth, parseISO } from 'date-fns'
import type {
  Category,
  DashboardCategorySummary,
  DashboardMonthSummary,
  Expense,
} from '../../../domain'

const uncategorizedFallback = {
  color: '#94A3B8',
  name: 'Uncategorized',
}

function isSameMonthYear(date: string, month: number, year: number) {
  const parsedDate = parseISO(date)

  return (
    parsedDate.getFullYear() === year && parsedDate.getMonth() + 1 === month
  )
}

export function selectExpensesForMonth(
  expenses: Expense[],
  month: number,
  year: number,
) {
  return expenses.filter((expense) => isSameMonthYear(expense.date, month, year))
}

export function selectMonthlyTotal(
  expenses: Expense[],
  month: number,
  year: number,
) {
  return selectExpensesForMonth(expenses, month, year).reduce(
    (total, expense) => total + expense.amount,
    0,
  )
}

export function selectAveragePerDay(
  expenses: Expense[],
  month: number,
  year: number,
) {
  const total = selectMonthlyTotal(expenses, month, year)
  const daysInMonth = getDaysInMonth(new Date(year, month - 1))

  return total / daysInMonth
}

export function groupMonthlyExpensesByCategory(
  expenses: Expense[],
  categories: Category[],
  month: number,
  year: number,
) {
  const categoriesById = new Map(
    categories.map((category) => [category.id, category] as const),
  )

  const grouped = selectExpensesForMonth(expenses, month, year).reduce(
    (summaryMap, expense) => {
      const category = expense.categoryId
        ? categoriesById.get(expense.categoryId) ?? null
        : null
      const groupKey = expense.categoryId ?? 'uncategorized'
      const existing = summaryMap.get(groupKey)

      const summary: DashboardCategorySummary = existing ?? {
        amount: 0,
        categoryColor: category?.color ?? uncategorizedFallback.color,
        categoryId: expense.categoryId,
        categoryName: category?.name ?? uncategorizedFallback.name,
      }

      summaryMap.set(groupKey, {
        ...summary,
        amount: summary.amount + expense.amount,
      })

      return summaryMap
    },
    new Map<string, DashboardCategorySummary>(),
  )

  return Array.from(grouped.values()).sort((left, right) => right.amount - left.amount)
}

export function selectAvailableYears(expenses: Expense[]) {
  return Array.from(
    new Set(expenses.map((expense) => parseISO(expense.date).getFullYear())),
  ).sort((left, right) => right - left)
}

export function selectDashboardMonthSummary(
  expenses: Expense[],
  categories: Category[],
  month: number,
  year: number,
): DashboardMonthSummary {
  return {
    categorySummaries: groupMonthlyExpensesByCategory(
      expenses,
      categories,
      month,
      year,
    ),
    dayAverage: selectAveragePerDay(expenses, month, year),
    month,
    total: selectMonthlyTotal(expenses, month, year),
    year,
  }
}
