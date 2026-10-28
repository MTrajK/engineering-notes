import { useEffect, useRef } from 'react'
import styles from './CategoryDialog.module.css'

interface CategoryDialogProps {
  confirmLabel: string
  description: string
  onClose: () => void
  onConfirm?: () => void
  title: string
}

export function CategoryDialog({
  confirmLabel,
  description,
  onClose,
  onConfirm,
  title,
}: CategoryDialogProps) {
  const primaryButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    primaryButtonRef.current?.focus()
  }, [])

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose()
      }
    }

    window.addEventListener('keydown', handleKeyDown)

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [onClose])

  return (
    <div className={styles.backdrop} onMouseDown={onClose}>
      <div
        className={styles.dialog}
        role="dialog"
        aria-modal="true"
        aria-labelledby="category-dialog-title"
        aria-describedby="category-dialog-description"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h2 id="category-dialog-title" className={styles.title}>
          {title}
        </h2>
        <p id="category-dialog-description" className={styles.description}>
          {description}
        </p>
        <div className={styles.actions}>
          {onConfirm ? (
            <>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={onClose}
              >
                No
              </button>
              <button
                ref={primaryButtonRef}
                type="button"
                className={styles.primaryButton}
                onClick={onConfirm}
              >
                {confirmLabel}
              </button>
            </>
          ) : (
            <button
              ref={primaryButtonRef}
              type="button"
              className={styles.primaryButton}
              onClick={onClose}
            >
              {confirmLabel}
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
