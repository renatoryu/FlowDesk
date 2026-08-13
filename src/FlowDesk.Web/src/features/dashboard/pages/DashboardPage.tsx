import { useQuery } from '@tanstack/react-query'
import { getDashboardSummary } from '../services/dashboardApi'
import {
  BarChart3,
  CircleUserRound,
  LayoutDashboard,
  LifeBuoy,
  TicketCheck,
} from 'lucide-react'
import { useAuth } from '../../auth/context/useAuth'
import styles from './DashboardPage.module.css'

function DashboardPage() {

  const { session } = useAuth()
  const accessToken = session?.accessToken ?? ''

  const summaryQuery = useQuery({
    queryKey: [
      'dashboard',
      'summary',
      session?.user.id,
    ],
    queryFn: () =>
      getDashboardSummary(accessToken),
    enabled: accessToken.length > 0,
    staleTime: 30_000,
    retry: 1,
  })

  const formatMetric = (value?: number) => {
    if (summaryQuery.isPending) {
      return '...'
    }

    if (summaryQuery.isError) {
      return '—'
    }

    return value ?? 0
  }


  if (!session) {
    return null
  }

  const firstName =
    session.user.fullName.split(' ')[0]

  return (
      <main className={styles.content}>
        <header className={styles.header}>
          <div>
            <span>Central de atendimento</span>
            <h1>Olá, {firstName}.</h1>
            <p>
              Acompanhe a operação do FlowDesk em um
              só lugar.
            </p>
          </div>

          <span className={styles.sessionBadge}>
            <CircleUserRound aria-hidden="true" />
            Sessão conectada
          </span>
        </header>

        {summaryQuery.isError && (
          <div className={styles.queryError} role="alert">
            <span>
              Não foi possível carregar os indicadores.
            </span>

            <button
              type="button"
              onClick={() => summaryQuery.refetch()}
            >
              Tentar novamente
            </button>
          </div>
        )}

        <section className={styles.cards}>
          <article>
            <span className={styles.openIcon}>
              <LifeBuoy aria-hidden="true" />
            </span>
            <div>
              <small>Chamados abertos</small>
              <strong>
                {formatMetric(
                  summaryQuery.data?.openTickets,
                )}
              </strong>
              <p>Aguardando atendimento</p>
            </div>
          </article>

          <article>
            <span className={styles.progressIcon}>
              <BarChart3 aria-hidden="true" />
            </span>
            <div>
              <small>Em andamento</small>
              <strong>
                {formatMetric(
                  summaryQuery.data?.inProgressTickets,
                )}
              </strong>
              <p>Em atendimento pela equipe</p>
            </div>
          </article>

          <article>
            <span className={styles.doneIcon}>
              <TicketCheck aria-hidden="true" />
            </span>
            <div>
              <small>Finalizados</small>
              <strong>
                {formatMetric(
                  summaryQuery.data?.completedTickets,
                )}
              </strong>
              <p>Resolvidos ou encerrados</p>
            </div>
          </article>
        </section>

        <section className={styles.welcome}>
          <div>
            <span>Dados em tempo real</span>
            <h2>Visão operacional atualizada.</h2>
            <p>
              Os indicadores são carregados diretamente da API
              e respeitam o perfil do usuário conectado.
            </p>
          </div>

          <span className={styles.welcomeIcon}>
            <LayoutDashboard aria-hidden="true" />
          </span>
        </section>
      </main>
  )
}

export default DashboardPage
