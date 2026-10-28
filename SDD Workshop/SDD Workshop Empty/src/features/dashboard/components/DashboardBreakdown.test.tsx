import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { DashboardCategorySummary } from '../../../domain'
import { DashboardBreakdown } from './DashboardBreakdown'

const categorySummaries: DashboardCategorySummary[] = [
  {
    amount: 100,
    categoryColor: '#15803D',
    categoryId: 'groceries',
    categoryName: 'Groceries',
  },
  {
    amount: 50,
    categoryColor: '#2563EB',
    categoryId: 'transport',
    categoryName: 'Transport',
  },
]

describe('DashboardBreakdown', () => {
  it('renders an empty state when there is no data for the selected month', () => {
    render(<DashboardBreakdown categorySummaries={[]} />)

    expect(
      screen.getByText(/no expense data for the selected month/i),
    ).toBeInTheDocument()
  })

  it('renders the chart and synchronized legend items', async () => {
    const user = userEvent.setup()

    render(<DashboardBreakdown categorySummaries={categorySummaries} />)

    expect(screen.getByTestId('dashboard-chart')).toBeInTheDocument()
    expect(screen.getByTestId('dashboard-legend')).toBeInTheDocument()
    expect(screen.getByTestId('legend-item-groceries')).toBeVisible()

    const groceriesLegendItem = screen.getByTestId('legend-item-groceries')
    const transportLegendItem = screen.getByTestId('legend-item-transport')

    await user.hover(groceriesLegendItem)

    expect(groceriesLegendItem.className).toContain('legendItemActive')
    expect(transportLegendItem.className).toContain('legendItemInactive')
  })
})
