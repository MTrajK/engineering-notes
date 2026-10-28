import type { ReactNode } from 'react'
import { useEffect, useReducer } from 'react'
import { applyThemeMode } from '../features/theme/utils/themeMode'
import { categoriesRepository, expensesRepository, themeRepository } from '../storage'
import { AppDispatchContext, AppStateContext } from './AppContext'
import { appReducer } from './appReducer'
import { createInitialAppState } from './createInitialAppState'

interface AppProviderProps {
  children: ReactNode
}

export function AppProvider({ children }: AppProviderProps) {
  const [state, dispatch] = useReducer(appReducer, undefined, createInitialAppState)

  useEffect(() => {
    expensesRepository.saveAll(state.expenses)
  }, [state.expenses])

  useEffect(() => {
    categoriesRepository.saveAll(state.categories)
  }, [state.categories])

  useEffect(() => {
    themeRepository.save(state.themeMode)
    applyThemeMode(state.themeMode)
  }, [state.themeMode])

  return (
    <AppStateContext.Provider value={state}>
      <AppDispatchContext.Provider value={dispatch}>
        {children}
      </AppDispatchContext.Provider>
    </AppStateContext.Provider>
  )
}
