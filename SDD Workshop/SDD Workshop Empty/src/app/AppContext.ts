import type { Dispatch } from 'react'
import { createContext } from 'react'
import type { AppAction, AppState } from './appState'

export const AppStateContext = createContext<AppState | undefined>(undefined)
export const AppDispatchContext = createContext<Dispatch<AppAction> | undefined>(
  undefined,
)
