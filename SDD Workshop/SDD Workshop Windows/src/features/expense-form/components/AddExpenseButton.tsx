import { Plus } from 'lucide-react'
import styles from './AddExpenseButton.module.css'

interface AddExpenseButtonProps {
  onClick: () => void
}

export function AddExpenseButton({ onClick }: AddExpenseButtonProps) {
  return (
    <button
      type="button"
      className={styles.button}
      aria-label="Add expense"
      title="Add expense"
      onClick={onClick}
    >
      <Plus aria-hidden="true" className={styles.icon} />
      <span>Add Expense</span>
    </button>
  )
}
