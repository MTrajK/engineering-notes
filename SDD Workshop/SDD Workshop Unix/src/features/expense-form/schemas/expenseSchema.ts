import { z } from 'zod'

export const expenseSchema = z.object({
  amount: z.preprocess(
    (value) => (value === '' ? undefined : value),
    z.coerce
      .number({
        error: 'Amount is required.',
      })
      .positive('Amount must be greater than zero.'),
  ),
  categoryId: z.preprocess(
    (value) => (value === '' ? null : value),
    z.string().nullable(),
  ),
  date: z.preprocess(
    (value) => (value === '' ? undefined : value),
    z.string().optional(),
  ),
  name: z.string().trim().min(1, 'Name is required.'),
})

export type ExpenseFormInput = z.input<typeof expenseSchema>
export type ExpenseFormValues = z.output<typeof expenseSchema>
