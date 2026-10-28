import type { AppAction, AppState } from './appState'

const closedExpenseModalState: AppState['expenseModal'] = {
  expenseId: null,
  mode: null,
  isOpen: false,
}

export function appReducer(state: AppState, action: AppAction): AppState {
  switch (action.type) {
    case 'expense/add':
      return {
        ...state,
        expenses: [...state.expenses, action.payload],
      }

    case 'expense/update':
      return {
        ...state,
        expenses: state.expenses.map((expense) =>
          expense.id === action.payload.id ? action.payload : expense,
        ),
      }

    case 'expense/delete':
      return {
        ...state,
        expenses: state.expenses.filter(
          (expense) => expense.id !== action.payload.expenseId,
        ),
      }

    case 'category/add':
      return {
        ...state,
        categories: [...state.categories, action.payload],
      }

    case 'category/delete':
      return {
        ...state,
        categories: state.categories.filter(
          (category) => category.id !== action.payload.categoryId,
        ),
      }

    case 'dashboard/setMonth':
      return {
        ...state,
        selectedDashboardMonth: action.payload.month,
      }

    case 'dashboard/setYear':
      return {
        ...state,
        selectedDashboardYear: action.payload.year,
      }

    case 'theme/set':
      return {
        ...state,
        themeMode: action.payload.themeMode,
      }

    case 'expenseModal/openCreate':
      return {
        ...state,
        expenseModal: {
          expenseId: null,
          mode: 'create',
          isOpen: true,
        },
      }

    case 'expenseModal/openEdit':
      return {
        ...state,
        expenseModal: {
          expenseId: action.payload.expenseId,
          mode: 'edit',
          isOpen: true,
        },
      }

    case 'expenseModal/close':
      return {
        ...state,
        expenseModal: closedExpenseModalState,
      }

    default:
      return state
  }
}
