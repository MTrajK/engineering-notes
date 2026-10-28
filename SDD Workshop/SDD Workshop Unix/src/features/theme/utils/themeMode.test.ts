import { applyThemeMode, resolveInitialThemeMode } from './themeMode'

describe('themeMode utilities', () => {
  it('prefers a stored theme over the system preference', () => {
    window.matchMedia = vi.fn().mockImplementation(() => ({
      matches: true,
      media: '(prefers-color-scheme: dark)',
      onchange: null,
      addEventListener: vi.fn(),
      addListener: vi.fn(),
      dispatchEvent: vi.fn(),
      removeEventListener: vi.fn(),
      removeListener: vi.fn(),
    }))

    expect(resolveInitialThemeMode('light')).toBe('light')
  })

  it('uses the system preference when no stored theme exists', () => {
    window.matchMedia = vi.fn().mockImplementation(() => ({
      matches: true,
      media: '(prefers-color-scheme: dark)',
      onchange: null,
      addEventListener: vi.fn(),
      addListener: vi.fn(),
      dispatchEvent: vi.fn(),
      removeEventListener: vi.fn(),
      removeListener: vi.fn(),
    }))

    expect(resolveInitialThemeMode(null)).toBe('dark')
  })

  it('applies the theme mode to the root document element', () => {
    applyThemeMode('dark')

    expect(document.documentElement.dataset.theme).toBe('dark')
  })
})
