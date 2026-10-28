import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { DashboardPage } from '../features/dashboard/pages/DashboardPage'
import { AppLayout } from './AppLayout'
import { CategoriesPage } from '../features/categories/pages/CategoriesPage'
import { ExpensesPage } from '../features/expenses/pages/ExpensesPage'

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/expenses" element={<ExpensesPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
