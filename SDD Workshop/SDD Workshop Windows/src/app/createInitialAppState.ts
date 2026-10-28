import type { AppState } from './appState'
import {
  categoriesRepository,
  expensesRepository,
  initializeStorage,
  themeRepository,
} from '../storage'
import {
  applyThemeMode,
  resolveInitialThemeMode,
} from '../features/theme/utils/themeMode'

export function createInitialAppState(): AppState {
  initializeStorage()

  const currentDate = new Date()
  const themeMode = resolveInitialThemeMode(themeRepository.get())

  applyThemeMode(themeMode)

  return {
    categories: categoriesRepository.getAll(),
    expenses: expensesRepository.getAll(),
    expenseModal: {
      expenseId: null,
      mode: null,
      isOpen: false,
    },
    selectedDashboardMonth: currentDate.getMonth() + 1,
    selectedDashboardYear: currentDate.getFullYear(),
    themeMode,
  }
}
