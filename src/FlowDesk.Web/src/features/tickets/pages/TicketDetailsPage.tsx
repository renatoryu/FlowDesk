import { useQuery } from '@tanstack/react-query'
import {
  ArrowLeft,
  CalendarClock,
  CircleAlert,
  FileText,
  LoaderCircle,
  Paperclip,
  Tag,
  UserRound,
} from 'lucide-react'
import {
  Link,
  useParams,
} from 'react-router'
import { ApiError } from '../../../shared/api/apiClient'
import { useAuth } from '../../auth/context/useAuth'
import { listCategories } from '../../categories/services/categoryApi'
import { getTicketById } from '../services/ticketApi'
import type {
  TicketPriority,
  TicketStatus,
} from '../types/ticketTypes'
import styles from './TicketDetailsPage.module.css'
import TicketCommentsSection from '../../comments/components/TicketCommentsSection'

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
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'America/Sao_Paulo',
  },
)

function TicketDetailsPage() {
  const { session } = useAuth()
  const { ticketId } = useParams<{
    ticketId: string
  }>()

  const accessToken = session?.accessToken ?? ''

  const ticketQuery = useQuery({
    queryKey: ['tickets', 'detail', ticketId],
    queryFn: () =>
      getTicketById(accessToken, ticketId ?? ''),
    enabled:
      accessToken.length > 0 &&
      Boolean(ticketId),
  })

  const categoriesQuery = useQuery({
    queryKey: ['categories', 'list'],
    queryFn: () => listCategories(accessToken),
    enabled: accessToken.length > 0,
  })

  if (!session) {
    return null
  }

  if (ticketQuery.isPending) {
    return (
      <main className={styles.content}>
        <section className={styles.state}>
          <LoaderCircle
            className={styles.spinning}
            aria-hidden="true"
          />
          <strong>Carregando chamado...</strong>
        </section>
      </main>
    )
  }

  if (ticketQuery.isError || !ticketQuery.data) {
    const notFound =
      ticketQuery.error instanceof ApiError &&
      ticketQuery.error.status === 404

    return (
      <main className={styles.content}>
        <section className={styles.state}>
          <CircleAlert aria-hidden="true" />

          <strong>
            {notFound
              ? 'Chamado não encontrado.'
              : 'Não foi possível carregar o chamado.'}
          </strong>

          <p>
            O chamado pode não existir ou não estar
            disponível para o seu perfil.
          </p>

          <Link to="/tickets">
            Voltar para chamados
          </Link>
        </section>
      </main>
    )
  }

  const ticket = ticketQuery.data

  const categoryName =
    categoriesQuery.data?.find(
      (category) => category.id === ticket.categoryId,
    )?.name ?? 'Categoria indisponível'

  return (
    <main className={styles.content}>
      <Link className={styles.back} to="/tickets">
        <ArrowLeft aria-hidden="true" />
        Voltar para chamados
      </Link>

      <header className={styles.header}>
        <div>
          <span>
            Chamado #
            {ticket.id.slice(0, 8).toUpperCase()}
          </span>
          <h1>{ticket.title}</h1>
        </div>

        <div className={styles.badges}>
          <span
            className={`${styles.badge} ${styles[`priority${ticket.priority}`]
              }`}
          >
            {priorityLabels[ticket.priority]}
          </span>

          <span
            className={`${styles.badge} ${styles[`status${ticket.status}`]
              }`}
          >
            {statusLabels[ticket.status]}
          </span>
        </div>
      </header>

      <div className={styles.layout}>
        <div className={styles.mainColumn}>
          <section className={styles.card}>
            <header>
              <FileText aria-hidden="true" />
              <h2>Descrição</h2>
            </header>

            <p className={styles.description}>
              {ticket.description}
            </p>
          </section>

          <TicketCommentsSection
            ticketId={ticket.id}
            ticketStatus={ticket.status}
          />

          <section className={styles.card}>
            <header>
              <Paperclip aria-hidden="true" />
              <h2>Anexos</h2>
            </header>

            <p className={styles.placeholder}>
              Os arquivos deste chamado serão exibidos
              aqui.
            </p>
          </section>
        </div>

        <aside className={styles.metadata}>
          <h2>Informações</h2>

          <dl>
            <div>
              <dt>
                <Tag aria-hidden="true" />
                Categoria
              </dt>
              <dd>{categoryName}</dd>
            </div>

            <div>
              <dt>
                <UserRound aria-hidden="true" />
                Solicitante
              </dt>
              <dd>
                {ticket.requesterId
                  .slice(0, 8)
                  .toUpperCase()}
              </dd>
            </div>

            <div>
              <dt>
                <CalendarClock aria-hidden="true" />
                Criado em
              </dt>
              <dd>
                {dateFormatter.format(
                  new Date(ticket.createdAtUtc),
                )}
              </dd>
            </div>

            <div>
              <dt>
                <CalendarClock aria-hidden="true" />
                Atualizado em
              </dt>
              <dd>
                {dateFormatter.format(
                  new Date(ticket.updatedAtUtc),
                )}
              </dd>
            </div>

            <div>
              <dt>
                <CalendarClock aria-hidden="true" />
                Última mudança de status
              </dt>
              <dd>
                {dateFormatter.format(
                  new Date(ticket.statusChangedAtUtc),
                )}
              </dd>
            </div>
          </dl>
        </aside>
      </div>
    </main>
  )
}

export default TicketDetailsPage
