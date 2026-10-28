import '@testing-library/jest-dom/vitest'
import { vi } from 'vitest'

class LocalStorageMock implements Storage {
  private store = new Map<string, string>()

  get length() {
    return this.store.size
  }

  clear() {
    this.store.clear()
  }

  getItem(key: string) {
    return this.store.get(key) ?? null
  }

  key(index: number) {
    return Array.from(this.store.keys())[index] ?? null
  }

  removeItem(key: string) {
    this.store.delete(key)
  }

  setItem(key: string, value: string) {
    this.store.set(key, value)
  }
}

Object.defineProperty(window, 'localStorage', {
  value: new LocalStorageMock(),
  writable: true,
})

Object.defineProperty(window, 'matchMedia', {
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addEventListener: vi.fn(),
    addListener: vi.fn(),
    dispatchEvent: vi.fn(),
    removeEventListener: vi.fn(),
    removeListener: vi.fn(),
  })),
  writable: true,
})
