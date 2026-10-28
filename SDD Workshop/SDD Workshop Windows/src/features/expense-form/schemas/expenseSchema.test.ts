import { expenseSchema } from './expenseSchema'

describe('expenseSchema', () => {
  it('requires a non-empty trimmed name', () => {
    const result = expenseSchema.safeParse({
      amount: 10,
      categoryId: null,
      date: '2026-04-22',
      name: '   ',
    })

    expect(result.success).toBe(false)
  })

  it('requires a positive amount', () => {
    const result = expenseSchema.safeParse({
      amount: 0,
      categoryId: null,
      date: '2026-04-22',
      name: 'Coffee',
    })

    expect(result.success).toBe(false)
  })

  it('accepts a valid expense payload', () => {
    const result = expenseSchema.safeParse({
      amount: 12.5,
      categoryId: 'category-groceries',
      date: '2026-04-22',
      name: 'Coffee',
    })

    expect(result.success).toBe(true)
  })
})
