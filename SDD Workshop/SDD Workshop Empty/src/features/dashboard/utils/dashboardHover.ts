export type DashboardHoverState = 'active' | 'inactive' | 'idle'

export function getDashboardCategoryKey(categoryId: string | null) {
  return categoryId ?? 'uncategorized'
}

export function getDashboardHoverState(
  activeCategoryKey: string | null,
  categoryKey: string,
): DashboardHoverState {
  if (activeCategoryKey === null) {
    return 'idle'
  }

  return activeCategoryKey === categoryKey ? 'active' : 'inactive'
}
