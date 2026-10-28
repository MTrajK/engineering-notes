import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useRef } from 'react'
import { useForm } from 'react-hook-form'
import type { Category, Expense } from '../../../domain'
import {
  expenseSchema,
  type ExpenseFormInput,
  type ExpenseFormValues,
} from '../schemas/expenseSchema'
import { getDefaultExpenseDate } from '../utils/expenseFormValues'
import styles from './ExpenseForm.module.css'

interface ExpenseFormProps {
  categories: Category[]
  expense?: Expense
  mode: 'create' | 'edit'
  onCancel: () => void
  onSubmit: (values: ExpenseFormValues) => void
}

export function ExpenseForm({
  categories,
  expense,
  mode,
  onCancel,
  onSubmit,
}: ExpenseFormProps) {
  const defaultValues = useMemo<ExpenseFormInput>(
    () => ({
      amount: expense?.amount ?? '',
      categoryId: expense?.categoryId ?? '',
      date: expense?.date ?? getDefaultExpenseDate(),
      name: expense?.name ?? '',
    }),
    [expense],
  )

  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<ExpenseFormInput, undefined, ExpenseFormValues>({
    defaultValues,
    resolver: zodResolver(expenseSchema),
  })

  const nameInputRef = useRef<HTMLInputElement | null>(null)
  const nameField = register('name')

  useEffect(() => {
    reset(defaultValues)
    nameInputRef.current?.focus()
  }, [defaultValues, reset])

  return (
    <form className={styles.form} onSubmit={handleSubmit(onSubmit)}>
      <label className={styles.field}>
        <span className={styles.label}>Name</span>
        <input
          {...nameField}
          ref={(element) => {
            nameField.ref(element)
            nameInputRef.current = element
          }}
          className={styles.input}
          aria-invalid={errors.name ? 'true' : 'false'}
        />
        {errors.name ? <span className={styles.error}>{errors.name.message}</span> : null}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>Category</span>
        <select {...register('categoryId')} className={styles.input}>
          <option value="">No category</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </label>

      <label className={styles.field}>
        <span className={styles.label}>Amount</span>
        <input
          {...register('amount')}
          type="number"
          min="0"
          step="0.01"
          className={styles.input}
          aria-invalid={errors.amount ? 'true' : 'false'}
        />
        {errors.amount ? (
          <span className={styles.error}>{errors.amount.message}</span>
        ) : null}
      </label>

      <label className={styles.field}>
        <span className={styles.label}>Date</span>
        <input {...register('date')} type="date" className={styles.input} />
      </label>

      <div className={styles.actions}>
        <button type="button" className={styles.secondaryButton} onClick={onCancel}>
          Cancel
        </button>
        <button type="submit" className={styles.primaryButton}>
          {mode === 'create' ? 'Save' : 'Update'}
        </button>
      </div>
    </form>
  )
}
