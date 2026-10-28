import type { Expense } from '../domain'
import { readStorageItem, writeStorageItem } from './localStorage'
import { storageKeys } from './storageKeys'

export const expensesRepository = {
  getAll() {
    return readStorageItem<Expense[]>(storageKeys.expenses, { fallback: [] })
  },
  saveAll(expenses: Expense[]) {
    writeStorageItem(storageKeys.expenses, expenses)
  },
} as const
