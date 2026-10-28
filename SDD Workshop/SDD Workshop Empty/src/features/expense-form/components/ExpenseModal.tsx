import type { ReactNode } from 'react'
import { useEffect, useRef } from 'react'
import { X } from 'lucide-react'
import styles from './ExpenseModal.module.css'

interface ExpenseModalProps {
  children: ReactNode
  mode: 'create' | 'edit'
  onClose: () => void
}

export function ExpenseModal({ children, mode, onClose }: ExpenseModalProps) {
  const modalRef = useRef<HTMLDivElement>(null)
  const closeButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose()
        return
      }

      if (event.key !== 'Tab' || !modalRef.current) {
        return
      }

      const focusableElements = modalRef.current.querySelectorAll<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',
      )

      if (focusableElements.length === 0) {
        return
      }

      const firstElement = focusableElements[0]
      const lastElement = focusableElements[focusableElements.length - 1]

      if (event.shiftKey && document.activeElement === firstElement) {
        event.preventDefault()
        lastElement.focus()
      } else if (!event.shiftKey && document.activeElement === lastElement) {
        event.preventDefault()
        firstElement.focus()
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    closeButtonRef.current?.focus()

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [onClose])

  return (
    <div className={styles.backdrop} onMouseDown={onClose}>
      <div
        ref={modalRef}
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="expense-modal-title"
        tabIndex={-1}
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className={styles.header}>
          <h2 id="expense-modal-title" className={styles.title}>
            {mode === 'create' ? 'Add Expense' : 'Edit Expense'}
          </h2>
          <button
            ref={closeButtonRef}
            type="button"
            className={styles.closeButton}
            aria-label="Close expense form"
            onClick={onClose}
          >
            <X aria-hidden="true" className={styles.closeIcon} />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}
