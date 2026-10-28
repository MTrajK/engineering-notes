import {
  readStorageItem,
  removeStorageItem,
  storageKeys,
  writeStorageItem,
} from './index'

describe('localStorage helpers', () => {
  beforeEach(() => {
    window.localStorage.clear()
  })

  it('writes and reads JSON values by key', () => {
    writeStorageItem(storageKeys.theme, 'dark')

    expect(readStorageItem(storageKeys.theme, { fallback: 'light' })).toBe('dark')
  })

  it('returns the fallback when a key is missing', () => {
    expect(readStorageItem(storageKeys.expenses, { fallback: [] })).toEqual([])
  })

  it('returns the fallback when stored JSON is invalid', () => {
    window.localStorage.setItem(storageKeys.version, '{invalid-json')

    expect(
      readStorageItem(storageKeys.version, { fallback: '1' }),
    ).toBe('1')
  })

  it('removes stored values', () => {
    writeStorageItem(storageKeys.theme, 'dark')
    removeStorageItem(storageKeys.theme)

    expect(window.localStorage.getItem(storageKeys.theme)).toBeNull()
  })
})
