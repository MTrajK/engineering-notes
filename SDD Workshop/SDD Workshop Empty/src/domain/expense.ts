export interface Expense {
  id: string
  name: string
  categoryId: string | null
  amount: number
  date: string
  createdAt: string
  updatedAt: string
}
