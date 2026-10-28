import {
  createColumnHelper,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
} from '@tanstack/react-table'
import { format, parseISO } from 'date-fns'
import { Pencil, Trash2 } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useAppDispatch, useAppState } from '../../../app/useAppContext'
import type { Expense, ExpenseFilters } from '../../../domain'
import { defaultExpenseFilters, filterExpenses } from '../utils/expenseFilters'
import styles from '../components/ExpensesTable.module.css'

interface ExpenseTableRow {
  amount: string
  category: string
  date: string
  id: string
  name: string
}

interface PendingDeleteExpense {
  id: string
  name: string
}

const columnHelper = createColumnHelper<ExpenseTableRow>()

export function useExpensesTable() {
  const dispatch = useAppDispatch()
  const { categories, expenses } = useAppState()
  const [filters, setFilters] = useState<ExpenseFilters>(defaultExpenseFilters)
  const [pendingDeleteExpense, setPendingDeleteExpense] =
    useState<PendingDeleteExpense | null>(null)

  const categoryNames = useMemo(
    () =>
      new Map(categories.map((category) => [category.id, category.name] as const)),
    [categories],
  )

  const filteredExpenses = useMemo(
    () => filterExpenses(expenses, filters),
    [expenses, filters],
  )

  const rows = useMemo(
    () =>
      filteredExpenses
        .slice()
        .sort((left, right) => right.date.localeCompare(left.date))
        .map((expense) => ({
          amount: `$${expense.amount.toFixed(2)}`,
          category: expense.categoryId
            ? categoryNames.get(expense.categoryId) ?? 'Unknown'
            : 'Uncategorized',
          date: format(parseISO(expense.date), 'MMM d, yyyy'),
          id: expense.id,
          name: expense.name,
        })),
    [categoryNames, filteredExpenses],
  )

  const handleEditExpense = useCallback((expenseId: Expense['id']) => {
    dispatch({
      type: 'expenseModal/openEdit',
      payload: { expenseId },
    })
  }, [dispatch])

  const handleRequestDelete = useCallback((expense: PendingDeleteExpense) => {
    setPendingDeleteExpense(expense)
  }, [])

  const columns = useMemo(
    () => [
      columnHelper.accessor('name', {
        cell: (info) => info.getValue(),
        header: 'Name',
      }),
      columnHelper.accessor('category', {
        cell: (info) => info.getValue(),
        header: 'Category',
      }),
      columnHelper.accessor('date', {
        cell: (info) => info.getValue(),
        header: 'Date',
      }),
      columnHelper.accessor('amount', {
        cell: (info) => info.getValue(),
        header: 'Amount',
      }),
      columnHelper.display({
        cell: ({ row }) => (
          <button
            type="button"
            className={styles.actionButton}
            aria-label={`Edit ${row.original.name}`}
            onClick={() => handleEditExpense(row.original.id)}
            title={`Edit ${row.original.name}`}
          >
            <Pencil aria-hidden="true" className={styles.actionIcon} />
          </button>
        ),
        header: 'Edit',
        id: 'edit',
      }),
      columnHelper.display({
        cell: ({ row }) => (
          <button
            type="button"
            className={styles.actionButton}
            aria-label={`Delete ${row.original.name}`}
            onClick={() =>
              handleRequestDelete({
                id: row.original.id,
                name: row.original.name,
              })
            }
            title={`Delete ${row.original.name}`}
          >
            <Trash2 aria-hidden="true" className={styles.actionIcon} />
          </button>
        ),
        header: 'Delete',
        id: 'delete',
      }),
    ],
    [handleEditExpense, handleRequestDelete],
  )

  // TanStack Table manages imperative table APIs internally; React Compiler
  // compatibility is intentionally handled at this boundary.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    columns,
    data: rows,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: {
        pageIndex: 0,
        pageSize: 10,
      },
    },
  })

  const { pageIndex, pageSize } = table.getState().pagination

  useEffect(() => {
    const lastPageIndex = Math.max(Math.ceil(rows.length / pageSize) - 1, 0)

    if (pageIndex > lastPageIndex) {
      table.setPageIndex(lastPageIndex)
    }
  }, [pageIndex, pageSize, rows.length, table])

  const setFilter = <K extends keyof ExpenseFilters>(
    key: K,
    value: ExpenseFilters[K],
  ) => {
    setFilters((currentFilters) => ({
      ...currentFilters,
      [key]: value,
    }))
    table.setPageIndex(0)
  }

  const cancelDeleteExpense = useCallback(() => {
    setPendingDeleteExpense(null)
  }, [])

  const confirmDeleteExpense = useCallback(() => {
    if (!pendingDeleteExpense) {
      return
    }

    dispatch({
      type: 'expense/delete',
      payload: { expenseId: pendingDeleteExpense.id },
    })
    setPendingDeleteExpense(null)
  }, [dispatch, pendingDeleteExpense])

  return {
    cancelDeleteExpense,
    categories,
    confirmDeleteExpense,
    filters,
    hasRows: rows.length > 0,
    pendingDeleteExpense,
    rowsCount: rows.length,
    setFilter,
    table,
  }
}
