import { expect, test } from '@playwright/test'

test('loads the app shell and supports route navigation', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('navigation', { name: /primary/i })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()

  const expensesLink = page.getByRole('link', { name: /expenses/i })
  await expensesLink.click()
  await expect(page).toHaveURL(/\/expenses$/)
  await expect(page.getByRole('heading', { name: 'Expenses' })).toBeVisible()
  await expect(expensesLink).toHaveAttribute('aria-current', 'page')

  const categoriesLink = page.getByRole('link', { name: /categories/i })
  await categoriesLink.click()
  await expect(page).toHaveURL(/\/categories$/)
  await expect(page.getByRole('heading', { name: 'Categories' })).toBeVisible()
  await expect(categoriesLink).toHaveAttribute('aria-current', 'page')

  const dashboardLink = page.getByRole('link', { name: /dashboard/i })
  await dashboardLink.click()
  await expect(page).toHaveURL(/\/$/)
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
  await expect(dashboardLink).toHaveAttribute('aria-current', 'page')
})

test('toggles theme and persists it after reload', async ({ page }) => {
  await page.goto('/')

  const themeToggle = page.getByRole('button', { name: /activate dark mode/i })

  await themeToggle.click()
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark')

  await page.reload()

  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark')
  await expect(
    page.getByRole('button', { name: /activate light mode/i }),
  ).toBeVisible()
})

test('switches dashboard month and year and updates summary values', async ({
  page,
}) => {
  await page.goto('/')

  await expect(page.getByTestId('dashboard-chart')).toBeVisible()
  await expect(page.getByTestId('dashboard-legend')).toBeVisible()
  await expect(page.getByTestId('dashboard-legend').getByText('Transport')).toBeVisible()
  await expect(page.getByTestId('dashboard-legend').getByText('Utilities')).toBeVisible()

  await expect(page.getByLabel('Monthly total')).toHaveText('$70.25')
  await expect(page.getByLabel('Daily average')).toHaveText('$2.34')

  await page.getByLabel('Select month').selectOption('1')
  await expect(page.getByTestId('dashboard-legend').getByText('Housing')).toBeVisible()
  await expect(
    page.getByTestId('dashboard-legend').getByText('Groceries'),
  ).toBeVisible()
  await expect(page.getByLabel('Monthly total')).toHaveText('$1356.85')
  await expect(page.getByLabel('Daily average')).toHaveText('$43.77')

  await page.getByLabel('Select year').selectOption('2025')
  await page.getByLabel('Select month').selectOption('12')

  await expect(page.getByLabel('Monthly total')).toHaveText('$1390.00')
  await expect(page.getByLabel('Daily average')).toHaveText('$44.84')

  await page.getByLabel('Select year').selectOption('2026')
  await page.getByLabel('Select month').selectOption('5')

  await expect(
    page.getByText(/no expense data for the selected month/i),
  ).toBeVisible()
})

test('adds a category, deletes an unused one, and blocks deleting a used one', async ({
  page,
}) => {
  await page.goto('/categories')

  const addButton = page.getByRole('button', { name: /^add$/i })
  const input = page.getByLabel('Category name')

  await expect(addButton).toBeDisabled()

  await input.fill('Travel')
  await expect(addButton).toBeEnabled()
  await addButton.click()

  await expect(page.getByText('Travel')).toBeVisible()
  await expect(input).toHaveValue('')
  await expect(addButton).toBeDisabled()

  await page.getByRole('button', { name: /delete travel/i }).click()
  await expect(
    page.getByRole('dialog', { name: /delete the category\?/i }),
  ).toBeVisible()
  await page.getByRole('button', { name: /^yes$/i }).click()
  await expect(page.getByText('Travel')).not.toBeVisible()

  await page.getByRole('button', { name: /delete groceries/i }).click()
  await expect(
    page.getByRole('dialog', {
      name: /the category can not be deleted, it's used by at least one expense\./i,
    }),
  ).toBeVisible()
  await page.getByRole('button', { name: /^ok$/i }).click()
})

test('opens the add expense modal from multiple pages and validates required fields', async ({
  page,
}) => {
  await page.goto('/')

  await page.getByRole('button', { name: /add expense/i }).click()
  await expect(
    page.getByRole('dialog', { name: /add expense/i }),
  ).toBeVisible()
  await page.getByRole('button', { name: /^save$/i }).click()
  await expect(page.getByText(/name is required\./i)).toBeVisible()
  await page.getByRole('button', { name: /^cancel$/i }).click()

  await page.goto('/categories')
  await page.getByRole('button', { name: /add expense/i }).click()
  await expect(
    page.getByRole('dialog', { name: /add expense/i }),
  ).toBeVisible()
  await page.getByRole('button', { name: /close expense form/i }).click()
})

test('creates a valid expense and keeps it after reload', async ({ page }) => {
  await page.goto('/')

  await page.getByRole('button', { name: /add expense/i }).click()
  const dialog = page.getByRole('dialog', { name: /add expense/i })

  await dialog.getByLabel('Name').fill('Team lunch')
  await dialog.getByLabel('Amount').fill('42.5')
  await dialog.getByLabel('Category').selectOption('category-groceries')
  await dialog.getByRole('button', { name: /^save$/i }).click()

  await expect(
    page.getByRole('dialog', { name: /add expense/i }),
  ).not.toBeVisible()
  await page.goto('/expenses')
  await page.getByLabel('Name').fill('Team lunch')
  await expect(page.getByText('Team lunch')).toBeVisible()

  await page.reload()

  await expect(page.getByText('Team lunch')).toBeVisible()
})

test('edits an expense and updates the dashboard totals', async ({ page }) => {
  await page.goto('/expenses')

  await page.getByRole('button', { name: /edit airport rideshare/i }).click()
  const dialog = page.getByRole('dialog', { name: /edit expense/i })

  await expect(dialog.getByLabel('Name')).toHaveValue('Airport rideshare')
  await expect(dialog.getByLabel('Amount')).toHaveValue('38.4')

  await dialog.getByLabel('Name').fill('Airport transfer')
  await dialog.getByLabel('Amount').fill('50')
  await dialog.getByRole('button', { name: /^update$/i }).click()

  await expect(dialog).not.toBeVisible()
  await expect(page.getByText('Airport transfer')).toBeVisible()

  await page.goto('/')

  await expect(page.getByLabel('Monthly total')).toHaveText('$81.85')
  await expect(page.getByLabel('Daily average')).toHaveText('$2.73')
})

test('deletes an expense and updates dashboard data and category usage', async ({
  page,
}) => {
  await page.goto('/expenses')

  await page.getByRole('button', { name: /delete dental cleaning/i }).click()
  await expect(
    page.getByRole('dialog', { name: /delete the expense\?/i }),
  ).toBeVisible()
  await page.getByRole('button', { name: /^yes$/i }).click()
  await expect(page.getByText('Dental cleaning')).not.toBeVisible()

  await page.goto('/')
  await page.getByLabel('Select month').selectOption('2')
  await expect(page.getByLabel('Monthly total')).toHaveText('$49.00')
  await expect(page.getByLabel('Daily average')).toHaveText('$1.75')

  await page.goto('/categories')
  await page.getByRole('button', { name: /delete health/i }).click()
  await expect(
    page.getByRole('dialog', { name: /delete the category\?/i }),
  ).toBeVisible()
  await page.getByRole('button', { name: /^no$/i }).click()
})

test('filters expenses, paginates results, and shows an empty filtered state', async ({
  page,
}) => {
  await page.goto('/expenses')

  await expect(page.getByText('Airport rideshare')).toBeVisible()
  await expect(page.getByText('Water bill')).toBeVisible()

  await page.getByRole('button', { name: /next/i }).click()
  await expect(page.getByText('Train tickets')).toBeVisible()

  await page.getByLabel('Name').fill('rent')
  await expect(page.getByText('January rent')).toBeVisible()
  await expect(page.getByText('December rent')).toBeVisible()

  await page.getByLabel('Category').selectOption('category-transport')
  await expect(
    page.getByText(/no expenses match the current filters/i),
  ).toBeVisible()
})
