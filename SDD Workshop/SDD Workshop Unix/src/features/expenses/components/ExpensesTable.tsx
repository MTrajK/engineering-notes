import type { Table as TanStackTable } from '@tanstack/react-table'
import { flexRender } from '@tanstack/react-table'
import styles from './ExpensesTable.module.css'

interface ExpensesTableProps<TData> {
  hasRows: boolean
  table: TanStackTable<TData>
}

export function ExpensesTable<TData>({
  hasRows,
  table,
}: ExpensesTableProps<TData>) {
  if (!hasRows) {
    return (
      <section className={styles.emptyState} aria-live="polite">
        <h2 className={styles.emptyTitle}>No expenses match the current filters.</h2>
        <p className={styles.emptyDescription}>
          Adjust the filters above to widen the result set.
        </p>
      </section>
    )
  }

  return (
    <section className={styles.panel}>
      <div className={styles.tableWrapper}>
        <table className={styles.table}>
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th key={header.id} scope="col" className={styles.headerCell}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.map((row) => (
              <tr key={row.id} className={styles.row}>
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id} className={styles.bodyCell}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
