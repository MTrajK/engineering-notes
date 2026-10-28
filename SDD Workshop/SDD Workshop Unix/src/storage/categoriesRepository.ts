import type { Category } from '../domain'
import { readStorageItem, writeStorageItem } from './localStorage'
import { storageKeys } from './storageKeys'

export const categoriesRepository = {
  getAll() {
    return readStorageItem<Category[]>(storageKeys.categories, { fallback: [] })
  },
  saveAll(categories: Category[]) {
    writeStorageItem(storageKeys.categories, categories)
  },
} as const
