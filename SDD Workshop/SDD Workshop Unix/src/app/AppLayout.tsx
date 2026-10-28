import { Outlet } from 'react-router-dom'
import { AddExpenseButton } from '../features/expense-form/components/AddExpenseButton'
import { ExpenseModalContainer } from '../features/expense-form/components/ExpenseModalContainer'
import { Navbar } from '../shared/components/Navbar'
import { useAppDispatch, useAppState } from './useAppContext'
import styles from './AppLayout.module.css'

export function AppLayout() {
  const { themeMode } = useAppState()
  const dispatch = useAppDispatch()

  const handleToggleTheme = () => {
    dispatch({
      type: 'theme/set',
      payload: {
        themeMode: themeMode === 'light' ? 'dark' : 'light',
      },
    })
  }

  const handleOpenCreateExpense = () => {
    dispatch({
      type: 'expenseModal/openCreate',
    })
  }

  return (
    <div className={styles.shell}>
      <Navbar theme={themeMode} onToggleTheme={handleToggleTheme} />
      <main className={styles.main}>
        <Outlet />
      </main>
      <AddExpenseButton onClick={handleOpenCreateExpense} />
      <ExpenseModalContainer />
    </div>
  )
}
