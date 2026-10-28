import type { ThemeMode } from '../domain'
import { readStorageItem, writeStorageItem } from './localStorage'
import { storageKeys } from './storageKeys'

export const themeRepository = {
  get() {
    return readStorageItem<ThemeMode | null>(storageKeys.theme, { fallback: null })
  },
  save(theme: ThemeMode) {
    writeStorageItem(storageKeys.theme, theme)
  },
} as const
