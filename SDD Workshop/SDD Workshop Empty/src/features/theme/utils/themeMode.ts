import type { ThemeMode } from '../../../domain'

export function getSystemThemePreference(): ThemeMode {
  if (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-color-scheme: dark)').matches
  ) {
    return 'dark'
  }

  return 'light'
}

export function resolveInitialThemeMode(
  storedThemeMode: ThemeMode | null,
): ThemeMode {
  if (storedThemeMode !== null) {
    return storedThemeMode
  }

  return getSystemThemePreference()
}

export function applyThemeMode(themeMode: ThemeMode) {
  if (typeof document === 'undefined') {
    return
  }

  document.documentElement.dataset.theme = themeMode
}
