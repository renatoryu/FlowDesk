import {
  Building2,
  LayoutDashboard,
  LifeBuoy,
  LogOut,
  TicketCheck,
} from 'lucide-react'
import { NavLink, Outlet } from 'react-router'
import { useAuth } from '../../features/auth/context/useAuth'
import type { UserRole } from '../../features/auth/types/authTypes'
import styles from './AppLayout.module.css'

const roleLabels: Record<UserRole, string> = {
  Customer: 'Cliente',
  Agent: 'Agente',
  Admin: 'Administrador',
}

function AppLayout() {
  const { session, signOut } = useAuth()

  if (!session) {
    return null
  }

  const firstName =
    session.user.fullName.split(' ')[0]

  return (
    <div className={styles.layout}>
      <aside className={styles.sidebar}>
        <div className={styles.brand}>
          <span className={styles.brandMark}>
            <LifeBuoy aria-hidden="true" />
          </span>

          <div className={styles.brandCopy}>
            <strong>FlowDesk</strong>
            <small>Service management</small>
          </div>
        </div>

        <nav className={styles.navigation}>
          <NavLink
            to="/dashboard"
            end
            className={({ isActive }) =>
              isActive ? styles.active : undefined
            }
          >
            <LayoutDashboard aria-hidden="true" />
            Visão geral
          </NavLink>

          <NavLink
            to="/tickets"
            className={({ isActive }) =>
              isActive ? styles.active : undefined
            }
          >
            <TicketCheck aria-hidden="true" />
            Chamados
          </NavLink>

          {session.user.role !== 'Customer' && (
            <NavLink
              to="/companies"
              className={({ isActive }) =>
                isActive ? styles.active : undefined
              }
            >
              <Building2 aria-hidden="true" />
              Empresas
            </NavLink>
          )}
        </nav>

        <div className={styles.user}>
          <span className={styles.avatar}>
            {firstName.charAt(0)}
          </span>

          <div className={styles.userCopy}>
            <strong>{session.user.fullName}</strong>
            <small>
              {roleLabels[session.user.role]}
            </small>
          </div>

          <button
            type="button"
            onClick={signOut}
            title="Sair"
            aria-label="Sair"
          >
            <LogOut aria-hidden="true" />
          </button>
        </div>
      </aside>

      <Outlet />
    </div>
  )
}

export default AppLayout
