import { AppProvider } from './app/AppProvider'
import { AppRouter } from './app/AppRouter'

function App() {
  return (
    <AppProvider>
      <AppRouter />
    </AppProvider>
  )
}

export default App
