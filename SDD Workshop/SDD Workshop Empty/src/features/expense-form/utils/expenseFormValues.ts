import { format } from 'date-fns'
import type { Expense } from '../../../domain'
import type { ExpenseFormValues } from '../schemas/expenseSchema'

export function getDefaultExpenseDate() {
  return format(new Date(), 'yyyy-MM-dd')
}

export function buildExpenseFromValues(
  values: ExpenseFormValues,
  existingExpense?: Expense,
): Expense {
  const now = new Date().toISOString()

  return {
    amount: values.amount,
    categoryId: values.categoryId,
    createdAt: existingExpense?.createdAt ?? now,
    date: values.date ?? getDefaultExpenseDate(),
    id: existingExpense?.id ?? crypto.randomUUID(),
    name: values.name.trim(),
    updatedAt: now,
  }
}
