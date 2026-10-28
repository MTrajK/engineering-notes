import { LayoutDashboard, Moon, ReceiptText, Sun, Tags } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { ThemeToggle } from '../../features/theme/components/ThemeToggle'
import styles from './Navbar.module.css'

interface NavbarProps {
  onToggleTheme: () => void
  theme: 'light' | 'dark'
}

interface NavigationItem {
  end?: boolean
  icon: typeof LayoutDashboard
  label: string
  to: string
}

const navigationItems: NavigationItem[] = [
  {
    to: '/',
    label: 'Dashboard',
    icon: LayoutDashboard,
    end: true,
  },
  {
    to: '/expenses',
    label: 'Expenses',
    icon: ReceiptText,
  },
  {
    to: '/categories',
    label: 'Categories',
    icon: Tags,
  },
]

export function Navbar({ onToggleTheme, theme }: NavbarProps) {
  return (
    <header className={styles.header}>
      <div className={styles.inner}>
        <div className={styles.brandBlock}>
          <p className={styles.eyebrow}>Personal finance</p>
          <span className={styles.brand}>Expense Tracker</span>
        </div>
        <nav aria-label="Primary" className={styles.nav}>
          {navigationItems.map(({ end, icon: Icon, label, to }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              aria-label={label}
              className={({ isActive }) =>
                `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`
              }
            >
              <Icon aria-hidden="true" className={styles.icon} />
              <span>{label}</span>
            </NavLink>
          ))}
        </nav>
        <ThemeToggle onToggle={onToggleTheme} theme={theme}>
          {theme === 'light' ? (
            <Moon aria-hidden="true" className={styles.toggleIcon} />
          ) : (
            <Sun aria-hidden="true" className={styles.toggleIcon} />
          )}
        </ThemeToggle>
      </div>
    </header>
  )
}
