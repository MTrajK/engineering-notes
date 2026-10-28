interface ReadOptions<T> {
  fallback: T
}

function isStorageAvailable() {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined'
}

export function readStorageItem<T>(key: string, options: ReadOptions<T>): T {
  if (!isStorageAvailable()) {
    return options.fallback
  }

  const rawValue = window.localStorage.getItem(key)

  if (rawValue === null) {
    return options.fallback
  }

  try {
    return JSON.parse(rawValue) as T
  } catch {
    return options.fallback
  }
}

export function writeStorageItem<T>(key: string, value: T) {
  if (!isStorageAvailable()) {
    return
  }

  window.localStorage.setItem(key, JSON.stringify(value))
}

export function removeStorageItem(key: string) {
  if (!isStorageAvailable()) {
    return
  }

  window.localStorage.removeItem(key)
}
