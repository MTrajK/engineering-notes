export interface DashboardCategorySummary {
  amount: number
  categoryColor: string
  categoryId: string | null
  categoryName: string
}

export interface DashboardMonthSummary {
  categorySummaries: DashboardCategorySummary[]
  dayAverage: number
  total: number
  year: number
  month: number
}
