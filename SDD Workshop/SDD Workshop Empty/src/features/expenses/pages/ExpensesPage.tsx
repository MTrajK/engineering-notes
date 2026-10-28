import { ExpenseDeleteDialog } from '../components/ExpenseDeleteDialog'
import { ExpensesFilters } from '../components/ExpensesFilters'
import { ExpensesPagination } from '../components/ExpensesPagination'
import { ExpensesTable } from '../components/ExpensesTable'
import { useExpensesTable } from '../hooks/useExpensesTable'
import styles from './ExpensesPage.module.css'

export function ExpensesPage() {
  const {
    cancelDeleteExpense,
    categories,
    confirmDeleteExpense,
    filters,
    hasRows,
    pendingDeleteExpense,
    rowsCount,
    setFilter,
    table,
  } = useExpensesTable()

  return (
    <>
      <section className={styles.page}>
        <div className={styles.hero}>
          <p className={styles.kicker}>Expense history</p>
          <h1 className={styles.title}>Expenses</h1>
          <p className={styles.description}>
            Review the recorded expenses, narrow the table with filters, and page
            through the matching results.
          </p>
        </div>

        <ExpensesFilters
          categories={categories}
          filters={filters}
          onFilterChange={setFilter}
        />
        <ExpensesTable hasRows={hasRows} table={table} />
        <ExpensesPagination table={table} totalRows={rowsCount} />
      </section>

      {pendingDeleteExpense ? (
        <ExpenseDeleteDialog
          expenseName={pendingDeleteExpense.name}
          onCancel={cancelDeleteExpense}
          onConfirm={confirmDeleteExpense}
        />
      ) : null}
    </>
  )
}
