import {
  initializeStorage,
  seedCategories,
  seedExpenses,
  STORAGE_VERSION,
  storageKeys,
} from './index'

describe('initializeStorage', () => {
  beforeEach(() => {
    window.localStorage.clear()
  })

  it('seeds categories, expenses, and version on first run without forcing theme', () => {
    initializeStorage()

    expect(JSON.parse(window.localStorage.getItem(storageKeys.categories) ?? '[]')).toEqual(
      seedCategories,
    )
    expect(JSON.parse(window.localStorage.getItem(storageKeys.expenses) ?? '[]')).toEqual(
      seedExpenses,
    )
    expect(window.localStorage.getItem(storageKeys.theme)).toBeNull()
    expect(JSON.parse(window.localStorage.getItem(storageKeys.version) ?? 'null')).toBe(
      STORAGE_VERSION,
    )
  })

  it('does not overwrite existing app data on subsequent runs', () => {
    window.localStorage.setItem(
      storageKeys.categories,
      JSON.stringify([
        {
          id: 'custom-category',
          name: 'Custom',
          color: '#111827',
          createdAt: '2026-04-22T10:00:00.000Z',
        },
      ]),
    )
    window.localStorage.setItem(
      storageKeys.expenses,
      JSON.stringify([
        {
          id: 'custom-expense',
          name: 'Coffee',
          categoryId: null,
          amount: 4.5,
          date: '2026-04-22',
          createdAt: '2026-04-22T10:10:00.000Z',
          updatedAt: '2026-04-22T10:10:00.000Z',
        },
      ]),
    )
    window.localStorage.setItem(storageKeys.theme, JSON.stringify('dark'))

    initializeStorage()

    expect(JSON.parse(window.localStorage.getItem(storageKeys.categories) ?? '[]')).toEqual([
      {
        id: 'custom-category',
        name: 'Custom',
        color: '#111827',
        createdAt: '2026-04-22T10:00:00.000Z',
      },
    ])
    expect(JSON.parse(window.localStorage.getItem(storageKeys.expenses) ?? '[]')).toEqual([
      {
        id: 'custom-expense',
        name: 'Coffee',
        categoryId: null,
        amount: 4.5,
        date: '2026-04-22',
        createdAt: '2026-04-22T10:10:00.000Z',
        updatedAt: '2026-04-22T10:10:00.000Z',
      },
    ])
    expect(JSON.parse(window.localStorage.getItem(storageKeys.theme) ?? 'null')).toBe('dark')
    expect(JSON.parse(window.localStorage.getItem(storageKeys.version) ?? 'null')).toBe(
      STORAGE_VERSION,
    )
  })
})
