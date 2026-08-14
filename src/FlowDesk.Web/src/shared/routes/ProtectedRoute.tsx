import { Navigate, Outlet } from 'react-router'
import { useAuth } from '../../features/auth/context/useAuth'

function ProtectedRoute() {
  const { session } = useAuth()

  if (!session) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}

export default ProtectedRoute
