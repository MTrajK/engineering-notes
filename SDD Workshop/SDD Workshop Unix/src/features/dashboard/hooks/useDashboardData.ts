import { useAppDispatch, useAppState } from '../../../app/useAppContext'
import {
  selectAvailableYears,
  selectDashboardMonthSummary,
  selectExpensesForMonth,
} from '../selectors/dashboardSelectors'

export function useDashboardData() {
  const {
    categories,
    expenses,
    selectedDashboardMonth,
    selectedDashboardYear,
  } = useAppState()
  const dispatch = useAppDispatch()

  const availableYears = selectAvailableYears(expenses)
  const monthlyExpenses = selectExpensesForMonth(
    expenses,
    selectedDashboardMonth,
    selectedDashboardYear,
  )
  const monthSummary = selectDashboardMonthSummary(
    expenses,
    categories,
    selectedDashboardMonth,
    selectedDashboardYear,
  )

  return {
    availableYears,
    monthSummary,
    monthlyExpenses,
    selectedDashboardMonth,
    selectedDashboardYear,
    setSelectedDashboardMonth(month: number) {
      dispatch({
        type: 'dashboard/setMonth',
        payload: { month },
      })
    },
    setSelectedDashboardYear(year: number) {
      dispatch({
        type: 'dashboard/setYear',
        payload: { year },
      })
    },
  }
}
