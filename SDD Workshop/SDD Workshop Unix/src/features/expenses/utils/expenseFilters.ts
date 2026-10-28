import { isAfter, isBefore, isEqual, parseISO } from 'date-fns'
import type { Expense, ExpenseFilters } from '../../../domain'

export const defaultExpenseFilters: ExpenseFilters = {
  amountFrom: null,
  amountTo: null,
  categoryId: null,
  dateFrom: null,
  dateTo: null,
  name: '',
}

function matchesName(expense: Expense, query: string) {
  const normalizedQuery = query.trim().toLowerCase()

  if (normalizedQuery.length === 0) {
    return true
  }

  return expense.name.toLowerCase().includes(normalizedQuery)
}

function matchesCategory(expense: Expense, categoryId: string | null) {
  if (categoryId === null || categoryId.length === 0) {
    return true
  }

  return expense.categoryId === categoryId
}

function matchesAmount(expense: Expense, amountFrom: number | null, amountTo: number | null) {
  if (amountFrom !== null && expense.amount < amountFrom) {
    return false
  }

  if (amountTo !== null && expense.amount > amountTo) {
    return false
  }

  return true
}

function matchesDate(expense: Expense, dateFrom: string | null, dateTo: string | null) {
  const expenseDate = parseISO(expense.date)

  if (dateFrom) {
    const startDate = parseISO(dateFrom)

    if (isBefore(expenseDate, startDate) && !isEqual(expenseDate, startDate)) {
      return false
    }
  }

  if (dateTo) {
    const endDate = parseISO(dateTo)

    if (isAfter(expenseDate, endDate) && !isEqual(expenseDate, endDate)) {
      return false
    }
  }

  return true
}

export function filterExpenses(expenses: Expense[], filters: ExpenseFilters) {
  return expenses.filter(
    (expense) =>
      matchesName(expense, filters.name) &&
      matchesCategory(expense, filters.categoryId) &&
      matchesAmount(expense, filters.amountFrom, filters.amountTo) &&
      matchesDate(expense, filters.dateFrom, filters.dateTo),
  )
}
