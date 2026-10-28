import type { Category, Expense, ThemeMode } from '../domain'

export interface ExpenseModalState {
  expenseId: string | null
  mode: 'create' | 'edit' | null
  isOpen: boolean
}

export interface AppState {
  categories: Category[]
  expenses: Expense[]
  expenseModal: ExpenseModalState
  selectedDashboardMonth: number
  selectedDashboardYear: number
  themeMode: ThemeMode
}

export type AppAction =
  | { type: 'expense/add'; payload: Expense }
  | { type: 'expense/update'; payload: Expense }
  | { type: 'expense/delete'; payload: { expenseId: string } }
  | { type: 'category/add'; payload: Category }
  | { type: 'category/delete'; payload: { categoryId: string } }
  | { type: 'dashboard/setMonth'; payload: { month: number } }
  | { type: 'dashboard/setYear'; payload: { year: number } }
  | { type: 'theme/set'; payload: { themeMode: ThemeMode } }
  | { type: 'expenseModal/openCreate' }
  | { type: 'expenseModal/openEdit'; payload: { expenseId: string } }
  | { type: 'expenseModal/close' }
