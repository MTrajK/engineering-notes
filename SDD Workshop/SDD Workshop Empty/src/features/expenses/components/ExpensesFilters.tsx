import type { Category, ExpenseFilters } from '../../../domain'
import styles from './ExpensesFilters.module.css'

interface ExpensesFiltersProps {
  categories: Category[]
  filters: ExpenseFilters
  onFilterChange: <K extends keyof ExpenseFilters>(
    key: K,
    value: ExpenseFilters[K],
  ) => void
}

export function ExpensesFilters({
  categories,
  filters,
  onFilterChange,
}: ExpensesFiltersProps) {
  return (
    <section className={styles.panel} aria-labelledby="expense-filters-title">
      <div className={styles.heading}>
        <h2 id="expense-filters-title" className={styles.title}>
          Filters
        </h2>
        <p className={styles.description}>
          Narrow the table by name, category, amount, or date range.
        </p>
      </div>

      <div className={styles.grid}>
        <label className={styles.field}>
          <span className={styles.label}>Name</span>
          <input
            className={styles.input}
            value={filters.name}
            onChange={(event) => onFilterChange('name', event.currentTarget.value)}
          />
        </label>

        <label className={styles.field}>
          <span className={styles.label}>Category</span>
          <select
            className={styles.input}
            value={filters.categoryId ?? ''}
            onChange={(event) =>
              onFilterChange(
                'categoryId',
                event.currentTarget.value ? event.currentTarget.value : null,
              )
            }
          >
            <option value="">All categories</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>

        <label className={styles.field}>
          <span className={styles.label}>Amount from</span>
          <input
            type="number"
            min="0"
            step="0.01"
            className={styles.input}
            value={filters.amountFrom ?? ''}
            onChange={(event) =>
              onFilterChange(
                'amountFrom',
                event.currentTarget.value ? Number(event.currentTarget.value) : null,
              )
            }
          />
        </label>

        <label className={styles.field}>
          <span className={styles.label}>Amount to</span>
          <input
            type="number"
            min="0"
            step="0.01"
            className={styles.input}
            value={filters.amountTo ?? ''}
            onChange={(event) =>
              onFilterChange(
                'amountTo',
                event.currentTarget.value ? Number(event.currentTarget.value) : null,
              )
            }
          />
        </label>

        <label className={styles.field}>
          <span className={styles.label}>Date from</span>
          <input
            type="date"
            className={styles.input}
            value={filters.dateFrom ?? ''}
            onChange={(event) =>
              onFilterChange(
                'dateFrom',
                event.currentTarget.value ? event.currentTarget.value : null,
              )
            }
          />
        </label>

        <label className={styles.field}>
          <span className={styles.label}>Date to</span>
          <input
            type="date"
            className={styles.input}
            value={filters.dateTo ?? ''}
            onChange={(event) =>
              onFilterChange(
                'dateTo',
                event.currentTarget.value ? event.currentTarget.value : null,
              )
            }
          />
        </label>
      </div>
    </section>
  )
}
