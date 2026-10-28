import {
  getDashboardCategoryKey,
  getDashboardHoverState,
} from './dashboardHover'

describe('dashboard hover helpers', () => {
  it('normalizes nullable category ids for hover synchronization', () => {
    expect(getDashboardCategoryKey('category-1')).toBe('category-1')
    expect(getDashboardCategoryKey(null)).toBe('uncategorized')
  })

  it('returns idle when nothing is hovered', () => {
    expect(getDashboardHoverState(null, 'category-1')).toBe('idle')
  })

  it('returns active for the hovered category', () => {
    expect(getDashboardHoverState('category-1', 'category-1')).toBe('active')
  })

  it('returns inactive for non-hovered categories when one is active', () => {
    expect(getDashboardHoverState('category-1', 'category-2')).toBe('inactive')
  })
})
