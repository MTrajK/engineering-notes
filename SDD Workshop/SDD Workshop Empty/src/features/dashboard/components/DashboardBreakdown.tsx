import { useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import type { DashboardCategorySummary } from '../../../domain'
import {
  getDashboardCategoryKey,
  getDashboardHoverState,
} from '../utils/dashboardHover'
import styles from './DashboardBreakdown.module.css'

interface DashboardBreakdownProps {
  categorySummaries: DashboardCategorySummary[]
}

interface DashboardTooltipProps {
  active?: boolean
  payload?: Array<{
    payload: DashboardCategorySummary
    value: number
  }>
}

function DashboardTooltip({ active, payload }: DashboardTooltipProps) {
  if (!active || !payload || payload.length === 0) {
    return null
  }

  const [entry] = payload

  return (
    <div className={styles.tooltip}>
      <p className={styles.tooltipLabel}>{entry.payload.categoryName}</p>
      <p className={styles.tooltipValue}>${entry.value.toFixed(2)}</p>
    </div>
  )
}

export function DashboardBreakdown({
  categorySummaries,
}: DashboardBreakdownProps) {
  const [activeCategoryKey, setActiveCategoryKey] = useState<string | null>(null)

  if (categorySummaries.length === 0) {
    return (
      <div className={styles.emptyState} data-testid="dashboard-breakdown-empty">
        <p className={styles.emptyTitle}>No expense data for the selected month.</p>
        <p className={styles.emptyDescription}>
          Switch month or year to inspect a period with recorded expenses.
        </p>
      </div>
    )
  }

  return (
    <div className={styles.breakdown}>
      <div
        className={styles.chartArea}
        data-testid="dashboard-chart"
        onMouseLeave={() => setActiveCategoryKey(null)}
      >
        <ResponsiveContainer width="100%" height={320}>
          <BarChart
            data={categorySummaries}
            margin={{ top: 20, right: 16, left: 0, bottom: 12 }}
          >
            <CartesianGrid
              vertical={false}
              stroke="var(--color-border)"
              strokeDasharray="4 4"
            />
            <XAxis
              dataKey="categoryName"
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              tickLine={false}
              axisLine={false}
            />
            <YAxis
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              tickFormatter={(value: number) => `$${value}`}
              tickLine={false}
              axisLine={false}
              width={52}
            />
            <Tooltip
              content={<DashboardTooltip />}
              cursor={{ fill: 'var(--color-accent-soft)' }}
              wrapperStyle={{ outline: 'none' }}
            />
            <Bar
              dataKey="amount"
              radius={[10, 10, 0, 0]}
              isAnimationActive={false}
              onMouseEnter={(_, index) => {
                const summary = categorySummaries[index]
                setActiveCategoryKey(getDashboardCategoryKey(summary.categoryId))
              }}
              onMouseLeave={() => setActiveCategoryKey(null)}
            >
              {categorySummaries.map((summary) => {
                const categoryKey = getDashboardCategoryKey(summary.categoryId)
                const hoverState = getDashboardHoverState(
                  activeCategoryKey,
                  categoryKey,
                )

                return (
                  <Cell
                    key={categoryKey}
                    fill={summary.categoryColor}
                    fillOpacity={hoverState === 'inactive' ? 0.3 : 1}
                    stroke={summary.categoryColor}
                    strokeWidth={hoverState === 'active' ? 2 : 0}
                  />
                )
              })}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
        <div className={styles.chartBadge}>
          <span className={styles.chartBadgeLabel}>Categories</span>
          <strong className={styles.chartBadgeValue}>
            {categorySummaries.length}
          </strong>
        </div>
      </div>

      <div className={styles.legendList} data-testid="dashboard-legend">
        {categorySummaries.map((summary) => {
          const categoryKey = getDashboardCategoryKey(summary.categoryId)
          const hoverState = getDashboardHoverState(activeCategoryKey, categoryKey)

          return (
            <button
              key={categoryKey}
              type="button"
              className={`${styles.legendItem} ${
                hoverState === 'active' ? styles.legendItemActive : ''
              } ${hoverState === 'inactive' ? styles.legendItemInactive : ''}`}
              data-testid={`legend-item-${categoryKey}`}
              onMouseEnter={() => setActiveCategoryKey(categoryKey)}
              onMouseLeave={() => setActiveCategoryKey(null)}
              onFocus={() => setActiveCategoryKey(categoryKey)}
              onBlur={() => setActiveCategoryKey(null)}
            >
              <span
                className={styles.legendSwatch}
                style={{ backgroundColor: summary.categoryColor }}
                aria-hidden="true"
              />
              <span className={styles.legendName}>{summary.categoryName}</span>
              <span className={styles.legendAmount}>${summary.amount.toFixed(2)}</span>
            </button>
          )
        })}
      </div>
    </div>
  )
}
