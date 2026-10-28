import { useEffect, useRef } from 'react'
import styles from './ExpenseDeleteDialog.module.css'

interface ExpenseDeleteDialogProps {
  expenseName: string
  onCancel: () => void
  onConfirm: () => void
}

export function ExpenseDeleteDialog({
  expenseName,
  onCancel,
  onConfirm,
}: ExpenseDeleteDialogProps) {
  const cancelButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    cancelButtonRef.current?.focus()
  }, [])

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onCancel()
      }
    }

    window.addEventListener('keydown', handleKeyDown)

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [onCancel])

  return (
    <div className={styles.backdrop} onMouseDown={onCancel}>
      <div
        className={styles.dialog}
        role="dialog"
        aria-modal="true"
        aria-labelledby="expense-delete-title"
        aria-describedby="expense-delete-description"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h2 id="expense-delete-title" className={styles.title}>
          Delete the expense?
        </h2>
        <p id="expense-delete-description" className={styles.description}>
          This will remove <strong>{expenseName}</strong> from the tracker.
        </p>
        <div className={styles.actions}>
          <button
            ref={cancelButtonRef}
            type="button"
            className={styles.secondaryButton}
            onClick={onCancel}
          >
            No
          </button>
          <button
            type="button"
            className={styles.primaryButton}
            onClick={onConfirm}
          >
            Yes
          </button>
        </div>
      </div>
    </div>
  )
}
