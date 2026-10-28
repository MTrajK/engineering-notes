import { categoriesRepository } from './categoriesRepository'
import { expensesRepository } from './expensesRepository'
import { readStorageItem, writeStorageItem } from './localStorage'
import { seedCategories, seedExpenses } from './seed'
import { STORAGE_VERSION, storageKeys } from './storageKeys'

function hasAppData() {
  const categories = categoriesRepository.getAll()
  const expenses = expensesRepository.getAll()

  return categories.length > 0 || expenses.length > 0
}

export function initializeStorage() {
  const storedVersion = readStorageItem<string | null>(storageKeys.version, {
    fallback: null,
  })

  if (!hasAppData()) {
    categoriesRepository.saveAll(seedCategories)
    expensesRepository.saveAll(seedExpenses)
    writeStorageItem(storageKeys.version, STORAGE_VERSION)
    return
  }

  if (storedVersion !== STORAGE_VERSION) {
    writeStorageItem(storageKeys.version, STORAGE_VERSION)
  }
}
