import type { Table as TanStackTable } from '@tanstack/react-table'
import styles from './ExpensesPagination.module.css'

interface ExpensesPaginationProps<TData> {
  table: TanStackTable<TData>
  totalRows: number
}

export function ExpensesPagination<TData>({
  table,
  totalRows,
}: ExpensesPaginationProps<TData>) {
  return (
    <section className={styles.panel}>
      <p className={styles.summary}>
        Showing {table.getRowModel().rows.length} of {totalRows} matching expenses
      </p>
      <div className={styles.controls}>
        <label className={styles.pageSizeField}>
          <span className={styles.label}>Rows per page</span>
          <select
            className={styles.select}
            value={table.getState().pagination.pageSize}
            onChange={(event) => table.setPageSize(Number(event.currentTarget.value))}
          >
            {[10, 25, 50].map((pageSize) => (
              <option key={pageSize} value={pageSize}>
                {pageSize}
              </option>
            ))}
          </select>
        </label>
        <div className={styles.buttons}>
          <button
            type="button"
            className={styles.button}
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
          >
            Previous
          </button>
          <button
            type="button"
            className={styles.button}
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
          >
            Next
          </button>
        </div>
      </div>
    </section>
  )
}
