import { DashboardBreakdown } from '../components/DashboardBreakdown'
import styles from './DashboardPage.module.css'
import { useDashboardData } from '../hooks/useDashboardData'

const monthOptions = [
  { label: 'January', value: 1 },
  { label: 'February', value: 2 },
  { label: 'March', value: 3 },
  { label: 'April', value: 4 },
  { label: 'May', value: 5 },
  { label: 'June', value: 6 },
  { label: 'July', value: 7 },
  { label: 'August', value: 8 },
  { label: 'September', value: 9 },
  { label: 'October', value: 10 },
  { label: 'November', value: 11 },
  { label: 'December', value: 12 },
] as const

export function DashboardPage() {
  const {
    availableYears,
    monthSummary,
    selectedDashboardMonth,
    selectedDashboardYear,
    setSelectedDashboardMonth,
    setSelectedDashboardYear,
  } = useDashboardData()

  return (
    <section className={styles.page}>
      <div className={styles.hero}>
        <div>
          <p className={styles.kicker}>Monthly overview</p>
          <h1 className={styles.title}>Dashboard</h1>
          <p className={styles.description}>
            Summary values below are driven by the selected month and year.
          </p>
        </div>
        <div className={styles.controls} aria-label="Dashboard filters">
          <label className={styles.field}>
            <span className={styles.fieldLabel}>Month</span>
            <select
              aria-label="Select month"
              className={styles.select}
              value={selectedDashboardMonth}
              onChange={(event) =>
                setSelectedDashboardMonth(Number(event.currentTarget.value))
              }
            >
              {monthOptions.map((monthOption) => (
                <option key={monthOption.value} value={monthOption.value}>
                  {monthOption.label}
                </option>
              ))}
            </select>
          </label>
          {availableYears.length > 1 ? (
            <label className={styles.field}>
              <span className={styles.fieldLabel}>Year</span>
              <select
                aria-label="Select year"
                className={styles.select}
                value={selectedDashboardYear}
                onChange={(event) =>
                  setSelectedDashboardYear(Number(event.currentTarget.value))
                }
              >
                {availableYears.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
            </label>
          ) : null}
        </div>
      </div>

      <div className={styles.summaryGrid}>
        <article className={styles.summaryCard}>
          <p className={styles.summaryLabel}>Total</p>
          <p className={styles.summaryValue} aria-label="Monthly total">
            ${monthSummary.total.toFixed(2)}
          </p>
        </article>
        <article className={styles.summaryCard}>
          <p className={styles.summaryLabel}>Day Average</p>
          <p className={styles.summaryValue} aria-label="Daily average">
            ${monthSummary.dayAverage.toFixed(2)}
          </p>
        </article>
      </div>

      <div className={styles.contentGrid}>
        <section className={styles.panel} aria-labelledby="chart-breakdown-title">
          <h2 id="chart-breakdown-title" className={styles.panelTitle}>
            Category Breakdown
          </h2>
          <DashboardBreakdown categorySummaries={monthSummary.categorySummaries} />
        </section>
      </div>
    </section>
  )
}
