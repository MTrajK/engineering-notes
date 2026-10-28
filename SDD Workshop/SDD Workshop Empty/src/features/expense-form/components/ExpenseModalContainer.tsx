import { useCallback, useEffect, useMemo } from 'react'
import { useAppDispatch, useAppState } from '../../../app/useAppContext'
import { ExpenseForm } from './ExpenseForm'
import { ExpenseModal } from './ExpenseModal'
import { buildExpenseFromValues } from '../utils/expenseFormValues'
import type { ExpenseFormValues } from '../schemas/expenseSchema'

export function ExpenseModalContainer() {
  const { categories, expenseModal, expenses } = useAppState()
  const dispatch = useAppDispatch()

  const editingExpense = useMemo(
    () =>
      expenseModal.expenseId
        ? expenses.find((expense) => expense.id === expenseModal.expenseId)
        : undefined,
    [expenseModal.expenseId, expenses],
  )

  const handleClose = useCallback(() => {
    dispatch({ type: 'expenseModal/close' })
  }, [dispatch])

  const handleSubmit = (values: ExpenseFormValues) => {
    const nextExpense = buildExpenseFromValues(values, editingExpense)

    dispatch({
      type: editingExpense ? 'expense/update' : 'expense/add',
      payload: nextExpense,
    })
    handleClose()
  }

  useEffect(() => {
    if (
      expenseModal.isOpen &&
      expenseModal.mode === 'edit' &&
      expenseModal.expenseId &&
      !editingExpense
    ) {
      handleClose()
    }
  }, [
    editingExpense,
    expenseModal.expenseId,
    expenseModal.isOpen,
    expenseModal.mode,
    handleClose,
  ])

  if (!expenseModal.isOpen || expenseModal.mode === null) {
    return null
  }

  return (
    <ExpenseModal mode={expenseModal.mode} onClose={handleClose}>
      <ExpenseForm
        categories={categories}
        expense={editingExpense}
        mode={expenseModal.mode}
        onCancel={handleClose}
        onSubmit={handleSubmit}
      />
    </ExpenseModal>
  )
}
