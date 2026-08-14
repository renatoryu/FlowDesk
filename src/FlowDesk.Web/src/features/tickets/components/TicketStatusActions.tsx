import {
  useMutation,
  useQueryClient,
} from '@tanstack/react-query'
import {
  CircleAlert,
  CircleCheck,
  LoaderCircle,
  LockKeyhole,
  PlayCircle,
  RotateCcw,
  Workflow,
} from 'lucide-react'
import { ApiError } from '../../../shared/api/apiClient'
import { useAuth } from '../../auth/context/useAuth'
import type { UserRole } from '../../auth/types/authTypes'
import { changeTicketStatus } from '../services/ticketApi'
import type {
  ChangeTicketStatusResponse,
  TicketStatus,
} from '../types/ticketTypes'
import styles from './TicketStatusActions.module.css'

interface TicketStatusActionsProps {
  ticketId: string
  currentStatus: TicketStatus
}

const statusLabels: Record<TicketStatus, string> = {
  1: 'Aberto',
  2: 'Em andamento',
  3: 'Resolvido',
  4: 'Fechado',
}

const actionLabels: Record<TicketStatus, string> = {
  1: 'Reabrir como aberto',
  2: 'Iniciar atendimento',
  3: 'Marcar como resolvido',
  4: 'Fechar chamado',
}

function getAvailableTransitions(
  role: UserRole,
  currentStatus: TicketStatus,
): TicketStatus[] {
  if (role === 'Customer') {
    return currentStatus === 3 ? [4] : []
  }

  switch (currentStatus) {
    case 1:
      return [2]
    case 2:
      return [1, 3]
    case 3:
      return [2, 4]
    case 4:
      return []
  }
}

function StatusActionIcon({
  status,
}: {
  status: TicketStatus
}) {
  switch (status) {
    case 1:
      return <RotateCcw aria-hidden="true" />
    case 2:
      return <PlayCircle aria-hidden="true" />
    case 3:
      return <CircleCheck aria-hidden="true" />
    case 4:
      return <LockKeyhole aria-hidden="true" />
  }
}

function TicketStatusActions({
  ticketId,
  currentStatus,
}: TicketStatusActionsProps) {
  const { session } = useAuth()
  const queryClient = useQueryClient()

  const accessToken = session?.accessToken ?? ''

  const statusMutation = useMutation({
    mutationFn: (status: TicketStatus) =>
      changeTicketStatus(
        accessToken,
        ticketId,
        { status },
      ),
    onSuccess: (
      updatedTicket: ChangeTicketStatusResponse,
    ) => {
      queryClient.setQueryData(
        ['tickets', 'detail', ticketId],
        updatedTicket,
      )

      void queryClient.invalidateQueries({
        queryKey: ['tickets'],
      })

      void queryClient.invalidateQueries({
        queryKey: ['dashboard'],
      })
    },
  })

  if (!session) {
    return null
  }

  const availableTransitions =
    getAvailableTransitions(
      session.user.role,
      currentStatus,
    )

  function requestStatusChange(
    nextStatus: TicketStatus,
  ) {
    if (
      nextStatus === 4 &&
      !window.confirm(
        'Deseja realmente fechar este chamado? Essa ação encerra o fluxo de atendimento.',
      )
    ) {
      return
    }

    statusMutation.mutate(nextStatus)
  }

  const mutationError =
    statusMutation.error instanceof ApiError
      ? statusMutation.error.status === 409
        ? 'Essa transição de status não é permitida.'
        : statusMutation.error.message
      : statusMutation.error
        ? 'Não foi possível alterar o status.'
        : null

  return (
    <section className={styles.section}>
      <header>
        <Workflow aria-hidden="true" />

        <div>
          <h2>Fluxo do chamado</h2>
          <span>
            Status atual:
            <strong>
              {statusLabels[currentStatus]}
            </strong>
          </span>
        </div>
      </header>

      {availableTransitions.length > 0 ? (
        <div className={styles.actions}>
          <p>
            Selecione a próxima etapa do atendimento.
          </p>

          <div>
            {availableTransitions.map(
              (nextStatus) => (
                <button
                  key={nextStatus}
                  type="button"
                  className={
                    nextStatus === 4
                      ? styles.closeButton
                      : styles.actionButton
                  }
                  disabled={statusMutation.isPending}
                  onClick={() =>
                    requestStatusChange(nextStatus)
                  }
                >
                  {statusMutation.isPending &&
                    statusMutation.variables ===
                    nextStatus ? (
                    <LoaderCircle
                      className={styles.spinning}
                      aria-hidden="true"
                    />
                  ) : (
                    <StatusActionIcon
                      status={nextStatus}
                    />
                  )}

                  {actionLabels[nextStatus]}
                </button>
              ),
            )}
          </div>
        </div>
      ) : (
        <div className={styles.notice}>
          {currentStatus === 4
            ? 'Este chamado está fechado e seu fluxo foi concluído.'
            : session.user.role === 'Customer'
              ? 'A equipe de atendimento realizará a próxima movimentação.'
              : 'Nenhuma transição está disponível para o status atual.'}
        </div>
      )}

      {mutationError && (
        <div className={styles.error} role="alert">
          <CircleAlert aria-hidden="true" />
          {mutationError}
        </div>
      )}
    </section>
  )
}

export default TicketStatusActions
