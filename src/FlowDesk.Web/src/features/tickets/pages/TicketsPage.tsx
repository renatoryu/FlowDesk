import { useQuery } from '@tanstack/react-query'
import {
  ChevronLeft,
  ChevronRight,
  CircleAlert,
  Inbox,
  RefreshCw,
  TicketCheck,
  Plus,
  CircleCheck
} from 'lucide-react'
import { useState } from 'react'
import { useAuth } from '../../auth/context/useAuth'
import { listTickets } from '../services/ticketApi'
import type {
  TicketPriority,
  TicketStatus,
} from '../types/ticketTypes'
import styles from './TicketsPage.module.css'
import {
  Link,
  useLocation,
} from 'react-router'

const statusLabels: Record<TicketStatus, string> = {
  1: 'Aberto',
  2: 'Em andamento',
  3: 'Resolvido',
  4: 'Fechado',
}

const priorityLabels: Record<TicketPriority, string> = {
  1: 'Baixa',
  2: 'Média',
  3: 'Alta',
  4: 'Crítica',
}

const dateFormatter = new Intl.DateTimeFormat(
  'pt-BR',
  {
    dateStyle: 'short',
    timeStyle: 'short',
    timeZone: 'America/Sao_Paulo',
  },
)

interface TicketsLocationState {
  createdTicketTitle?: string
}

function TicketsPage() {
  const { session } = useAuth()

  const location = useLocation()

  const locationState =
    location.state as TicketsLocationState | null

  const [page, setPage] = useState(1)
  const [status, setStatus] =
    useState<TicketStatus | ''>('')
  const [priority, setPriority] =
    useState<TicketPriority | ''>('')

  const accessToken = session?.accessToken ?? ''

  const ticketsQuery = useQuery({
    queryKey: [
      'tickets',
      'list',
      session?.user.id,
      page,
      status,
      priority,
    ],
    queryFn: () =>
      listTickets(accessToken, {
        page,
        pageSize: 10,
        status: status || undefined,
        priority: priority || undefined,
      }),
    enabled: accessToken.length > 0,
    placeholderData: (previousData) =>
      previousData,
  })

  if (!session) {
    return null
  }

  const tickets = ticketsQuery.data?.items ?? []

  return (
    <main className={styles.content}>
      <header className={styles.header}>
        <div>
          <span>Central de atendimento</span>
          <h1>Chamados</h1>
          <p>
            Consulte e acompanhe as solicitações
            disponíveis para o seu perfil.
          </p>
        </div>

        <div className={styles.headerActions}>
          {session.user.role === 'Customer' && (
            <Link
              className={styles.newTicket}
              to="/tickets/new"
            >
              <Plus aria-hidden="true" />
              Novo chamado
            </Link>
          )}

          <span className={styles.total}>
            <TicketCheck aria-hidden="true" />
            {ticketsQuery.data?.totalCount ?? 0}
            {' '}
            chamado(s)
          </span>
        </div>
      </header>

      {locationState?.createdTicketTitle && (
        <div className={styles.success} role="status">
          <CircleCheck aria-hidden="true" />

          <div>
            <strong>Chamado aberto com sucesso.</strong>
            <span>{locationState.createdTicketTitle}</span>
          </div>
        </div>
      )}

      <section className={styles.filters}>
        <label>
          Status
          <select
            value={status}
            onChange={(event) => {
              const value = event.target.value

              setStatus(
                value
                  ? Number(value) as TicketStatus
                  : '',
              )
              setPage(1)
            }}
          >
            <option value="">Todos</option>
            <option value="1">Aberto</option>
            <option value="2">Em andamento</option>
            <option value="3">Resolvido</option>
            <option value="4">Fechado</option>
          </select>
        </label>

        <label>
          Prioridade
          <select
            value={priority}
            onChange={(event) => {
              const value = event.target.value

              setPriority(
                value
                  ? Number(value) as TicketPriority
                  : '',
              )
              setPage(1)
            }}
          >
            <option value="">Todas</option>
            <option value="1">Baixa</option>
            <option value="2">Média</option>
            <option value="3">Alta</option>
            <option value="4">Crítica</option>
          </select>
        </label>

        <button
          type="button"
          onClick={() => ticketsQuery.refetch()}
          disabled={ticketsQuery.isFetching}
        >
          <RefreshCw aria-hidden="true" />
          Atualizar
        </button>
      </section>

      {ticketsQuery.isPending && (
        <section className={styles.state}>
          <RefreshCw
            className={styles.spinning}
            aria-hidden="true"
          />
          <strong>Carregando chamados...</strong>
        </section>
      )}

      {ticketsQuery.isError && (
        <section className={styles.state}>
          <CircleAlert aria-hidden="true" />
          <strong>
            Não foi possível carregar os chamados.
          </strong>
          <button
            type="button"
            onClick={() => ticketsQuery.refetch()}
          >
            Tentar novamente
          </button>
        </section>
      )}

      {!ticketsQuery.isPending &&
        !ticketsQuery.isError &&
        tickets.length === 0 && (
          <section className={styles.state}>
            <Inbox aria-hidden="true" />
            <strong>Nenhum chamado encontrado.</strong>
            <p>
              Altere os filtros ou crie uma nova
              solicitação.
            </p>
          </section>
        )}

      {tickets.length > 0 && (
        <>
          <div className={styles.tableContainer}>
            <table>
              <thead>
                <tr>
                  <th>Chamado</th>
                  <th>Prioridade</th>
                  <th>Status</th>
                  <th>Atualizado em</th>
                </tr>
              </thead>

              <tbody>
                {tickets.map((ticket) => (
                  <tr key={ticket.id}>
                    <td>
                      <Link
                        className={styles.ticketLink}
                        to={`/tickets/${ticket.id}`}
                      >
                        <strong>{ticket.title}</strong>
                        <small>
                          #
                          {ticket.id
                            .slice(0, 8)
                            .toUpperCase()}
                        </small>
                      </Link>
                    </td>

                    <td>
                      <span
                        className={`${styles.badge} ${styles[
                          `priority${ticket.priority}`
                          ]
                          }`}
                      >
                        {priorityLabels[ticket.priority]}
                      </span>
                    </td>

                    <td>
                      <span
                        className={`${styles.badge} ${styles[
                          `status${ticket.status}`
                          ]
                          }`}
                      >
                        {statusLabels[ticket.status]}
                      </span>
                    </td>

                    <td>
                      {dateFormatter.format(
                        new Date(ticket.updatedAtUtc),
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <footer className={styles.pagination}>
            <span>
              Página {ticketsQuery.data?.page ?? page}
              {' '}
              de {ticketsQuery.data?.totalPages ?? 1}
            </span>

            <div>
              <button
                type="button"
                onClick={() =>
                  setPage((current) => current - 1)
                }
                disabled={page <= 1}
                aria-label="Página anterior"
              >
                <ChevronLeft aria-hidden="true" />
              </button>

              <button
                type="button"
                onClick={() =>
                  setPage((current) => current + 1)
                }
                disabled={
                  page >=
                  (ticketsQuery.data?.totalPages ?? 1)
                }
                aria-label="Próxima página"
              >
                <ChevronRight aria-hidden="true" />
              </button>
            </div>
          </footer>
        </>
      )}
    </main>
  )
}

export default TicketsPage
