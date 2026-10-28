import type { ReactNode } from 'react'
import styles from './ThemeToggle.module.css'

interface ThemeToggleProps {
  children: ReactNode
  onToggle: () => void
  theme: 'light' | 'dark'
}

export function ThemeToggle({
  children,
  onToggle,
  theme,
}: ThemeToggleProps) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className={styles.button}
      aria-label={theme === 'light' ? 'Activate dark mode' : 'Activate light mode'}
      aria-pressed={theme === 'dark'}
      title={theme === 'light' ? 'Activate dark mode' : 'Activate light mode'}
    >
      <span className={styles.visual}>{children}</span>
      <span className={styles.label}>{theme === 'light' ? 'Dark mode' : 'Light mode'}</span>
      <span className={styles.state}>{theme === 'light' ? 'Off' : 'On'}</span>
    </button>
  )
}
