import {
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'
import {
  BrowserRouter,
  Navigate,
  Route,
  Routes,
} from 'react-router'
import AuthProvider from './features/auth/context/AuthProvider'
import { useAuth } from './features/auth/context/useAuth'
import LoginPage from './features/auth/pages/LoginPage'
import DashboardPage from './features/dashboard/pages/DashboardPage'
import ProtectedRoute from './shared/routes/ProtectedRoute'
import AppLayout from './shared/layout/AppLayout'
import TicketsPage from './features/tickets/pages/TicketsPage'
import CreateTicketPage from './features/tickets/pages/CreateTicketPage'
import TicketDetailsPage from './features/tickets/pages/TicketDetailsPage'
import CompaniesPage from './features/companies/pages/CompaniesPage'
import ProfilePage from './features/auth/pages/ProfilePage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
})

function ApplicationRoutes() {
  const { session } = useAuth()

  return (
    <Routes>
      <Route
        path="/login"
        element={
          session ? (
            <Navigate to="/dashboard" replace />
          ) : (
            <LoginPage />
          )
        }
      />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route
            path="/dashboard"
            element={<DashboardPage />}
          />

          <Route
            path="/tickets"
            element={<TicketsPage />}
          />

          <Route
            path="/tickets/new"
            element={<CreateTicketPage />}
          />

          <Route
            path="/tickets/:ticketId"
            element={<TicketDetailsPage />}
          />

          <Route
            path="/companies"
            element={<CompaniesPage />}
          />

          <Route
            path="/profile"
            element={<ProfilePage />}
          />

        </Route>
      </Route>

      <Route
        path="*"
        element={
          <Navigate
            to={session ? '/dashboard' : '/login'}
            replace
          />
        }
      />
    </Routes>
  )
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <ApplicationRoutes />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
