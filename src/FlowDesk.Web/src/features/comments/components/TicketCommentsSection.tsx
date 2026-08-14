import { zodResolver } from '@hookform/resolvers/zod'
import {
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query'
import {
  CircleAlert,
  LoaderCircle,
  MessageSquare,
  Send,
} from 'lucide-react'
import {
  useForm,
  useWatch,
} from 'react-hook-form'
import { z } from 'zod'
import { ApiError } from '../../../shared/api/apiClient'
import { useAuth } from '../../auth/context/useAuth'
import type { TicketStatus } from '../../tickets/types/ticketTypes'
import {
  createTicketComment,
  listTicketComments,
} from '../services/commentApi'
import styles from './TicketCommentsSection.module.css'

const commentSchema = z.object({
  content: z
    .string()
    .trim()
    .min(1, 'Escreva um comentário.')
    .max(2000, 'Use no máximo 2000 caracteres.'),
})

type CommentFormValues =
  z.infer<typeof commentSchema>

interface TicketCommentsSectionProps {
  ticketId: string
  ticketStatus: TicketStatus
}

const dateFormatter = new Intl.DateTimeFormat(
  'pt-BR',
  {
    dateStyle: 'short',
    timeStyle: 'short',
    timeZone: 'America/Sao_Paulo',
  },
)

function TicketCommentsSection({
  ticketId,
  ticketStatus,
}: TicketCommentsSectionProps) {
  const { session } = useAuth()
  const queryClient = useQueryClient()
  const accessToken = session?.accessToken ?? ''

  const commentsQuery = useQuery({
    queryKey: ['tickets', 'comments', ticketId],
    queryFn: () =>
      listTicketComments(accessToken, ticketId),
    enabled: accessToken.length > 0,
  })

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<CommentFormValues>({
    resolver: zodResolver(commentSchema),
    mode: 'onTouched',
    defaultValues: {
      content: '',
    },
  })

  const content = useWatch({
    control,
    name: 'content',
  })

  const createMutation = useMutation({
    mutationFn: (values: CommentFormValues) =>
      createTicketComment(
        accessToken,
        ticketId,
        values,
      ),
    onSuccess: () => {
      reset()

      void queryClient.invalidateQueries({
        queryKey: [
          'tickets',
          'comments',
          ticketId,
        ],
      })
    },
  })

  if (!session) {
    return null
  }

  const comments = commentsQuery.data?.items ?? []
  const isClosed = ticketStatus === 4

  const mutationError =
    createMutation.error instanceof ApiError
      ? createMutation.error.status === 409
        ? 'Este chamado não aceita novos comentários.'
        : createMutation.error.message
      : createMutation.error
        ? 'Não foi possível enviar o comentário.'
        : null

  return (
    <section className={styles.section}>
      <header>
        <MessageSquare aria-hidden="true" />

        <div>
          <h2>Comentários</h2>
          <span>{comments.length} registro(s)</span>
        </div>
      </header>

      {commentsQuery.isPending && (
        <div className={styles.state}>
          <LoaderCircle
            className={styles.spinning}
            aria-hidden="true"
          />
          Carregando histórico...
        </div>
      )}

      {commentsQuery.isError && (
        <div className={styles.state}>
          <CircleAlert aria-hidden="true" />
          <span>
            Não foi possível carregar os comentários.
          </span>
          <button
            type="button"
            onClick={() => commentsQuery.refetch()}
          >
            Tentar novamente
          </button>
        </div>
      )}

      {!commentsQuery.isPending &&
        !commentsQuery.isError &&
        comments.length === 0 && (
          <div className={styles.empty}>
            Nenhum comentário registrado.
          </div>
        )}

      {comments.length > 0 && (
        <div className={styles.history}>
          {comments.map((comment) => {
            const isCurrentUser =
              comment.authorId === session.user.id

            return (
              <article key={comment.id}>
                <span className={styles.avatar}>
                  {isCurrentUser
                    ? session.user.fullName
                      .charAt(0)
                      .toUpperCase()
                    : 'A'}
                </span>

                <div>
                  <header>
                    <strong>
                      {isCurrentUser
                        ? 'Você'
                        : `Autor #${comment.authorId
                          .slice(0, 8)
                          .toUpperCase()}`}
                    </strong>

                    <time>
                      {dateFormatter.format(
                        new Date(
                          comment.createdAtUtc,
                        ),
                      )}
                    </time>
                  </header>

                  <p>{comment.content}</p>
                </div>
              </article>
            )
          })}
        </div>
      )}

      {isClosed ? (
        <div className={styles.closedNotice}>
          Este chamado está fechado e não aceita
          novos comentários.
        </div>
      ) : (
        <form
          className={styles.form}
          onSubmit={handleSubmit((values) =>
            createMutation.mutate(values),
          )}
          noValidate
        >
          <label htmlFor="comment">
            Adicionar comentário
          </label>

          <textarea
            id="comment"
            rows={4}
            maxLength={2000}
            placeholder="Compartilhe uma atualização ou informação relevante."
            aria-invalid={Boolean(errors.content)}
            {...register('content')}
          />

          <div className={styles.formFooter}>
            <span>
              <small className={styles.error}>
                {errors.content?.message}
              </small>
              <small>{content.length}/2000</small>
            </span>

            <button
              type="submit"
              disabled={createMutation.isPending}
            >
              {createMutation.isPending ? (
                <LoaderCircle
                  className={styles.spinning}
                  aria-hidden="true"
                />
              ) : (
                <Send aria-hidden="true" />
              )}

              {createMutation.isPending
                ? 'Enviando...'
                : 'Comentar'}
            </button>
          </div>

          {mutationError && (
            <div className={styles.apiError} role="alert">
              <CircleAlert aria-hidden="true" />
              {mutationError}
            </div>
          )}
        </form>
      )}
    </section>
  )
}

export default TicketCommentsSection
