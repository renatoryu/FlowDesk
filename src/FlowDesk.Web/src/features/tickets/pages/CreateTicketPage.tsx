import { zodResolver } from '@hookform/resolvers/zod'
import {
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query'
import {
  ArrowLeft,
  CircleAlert,
  LoaderCircle,
  Send,
  TicketPlus,
} from 'lucide-react'
import {
  useForm,
  useWatch,
} from 'react-hook-form'
import {
  Link,
  Navigate,
  useNavigate,
} from 'react-router'
import { z } from 'zod'
import { ApiError } from '../../../shared/api/apiClient'
import { useAuth } from '../../auth/context/useAuth'
import { listCategories } from '../../categories/services/categoryApi'
import { createTicket } from '../services/ticketApi'
import type { TicketPriority } from '../types/ticketTypes'
import styles from './CreateTicketPage.module.css'

const createTicketSchema = z.object({
  categoryId: z
    .string()
    .uuid('Selecione uma categoria.'),
  title: z
    .string()
    .trim()
    .min(1, 'Informe o título.')
    .max(200, 'Use no máximo 200 caracteres.'),
  description: z
    .string()
    .trim()
    .min(1, 'Informe a descrição.')
    .max(4000, 'Use no máximo 4000 caracteres.'),
  priority: z.enum(['1', '2', '3', '4']),
})

type CreateTicketFormValues =
  z.infer<typeof createTicketSchema>

function CreateTicketPage() {
  const { session } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const accessToken = session?.accessToken ?? ''

  const categoriesQuery = useQuery({
    queryKey: ['categories', 'list'],
    queryFn: () => listCategories(accessToken),
    enabled:
      accessToken.length > 0 &&
      session?.user.role === 'Customer',
  })

  const createMutation = useMutation({
    mutationFn: (
      values: CreateTicketFormValues,
    ) =>
      createTicket(accessToken, {
        categoryId: values.categoryId,
        title: values.title,
        description: values.description,
        priority:
          Number(values.priority) as TicketPriority,
      }),
    onSuccess: (ticket) => {
      void queryClient.invalidateQueries({
        queryKey: ['tickets'],
      })

      void queryClient.invalidateQueries({
        queryKey: ['dashboard'],
      })

      navigate('/tickets', {
        replace: true,
        state: {
          createdTicketTitle: ticket.title,
        },
      })
    },
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<CreateTicketFormValues>({
    resolver: zodResolver(createTicketSchema),
    mode: 'onTouched',
    defaultValues: {
      categoryId: '',
      title: '',
      description: '',
      priority: '2',
    },
  })

  const title = useWatch({
    control,
    name: 'title',
  })

  const description = useWatch({
    control,
    name: 'description',
  })


  if (!session) {
    return null
  }

  if (session.user.role !== 'Customer') {
    return <Navigate to="/tickets" replace />
  }

  const titleLength = title.length
  const descriptionLength = description.length

  const mutationError =
    createMutation.error instanceof ApiError
      ? createMutation.error.message
      : createMutation.error
        ? 'Não foi possível abrir o chamado.'
        : null

  return (
    <main className={styles.content}>
      <Link className={styles.back} to="/tickets">
        <ArrowLeft aria-hidden="true" />
        Voltar para chamados
      </Link>

      <header className={styles.header}>
        <span className={styles.icon}>
          <TicketPlus aria-hidden="true" />
        </span>

        <div>
          <span>Nova solicitação</span>
          <h1>Abrir chamado</h1>
          <p>
            Descreva o problema com clareza para
            agilizar o atendimento.
          </p>
        </div>
      </header>

      <form
        className={styles.form}
        onSubmit={handleSubmit((values) =>
          createMutation.mutate(values),
        )}
        noValidate
      >
        <label>
          Categoria
          <select
            {...register('categoryId')}
            disabled={categoriesQuery.isPending}
            aria-invalid={Boolean(errors.categoryId)}
          >
            <option value="">
              Selecione uma categoria
            </option>

            {categoriesQuery.data?.map((category) => (
              <option
                key={category.id}
                value={category.id}
              >
                {category.name}
              </option>
            ))}
          </select>

          {errors.categoryId && (
            <small className={styles.error}>
              {errors.categoryId.message}
            </small>
          )}
        </label>

        {categoriesQuery.isError && (
          <div className={styles.apiError} role="alert">
            <CircleAlert aria-hidden="true" />
            Não foi possível carregar as categorias.
          </div>
        )}

        <label>
          Título
          <input
            type="text"
            placeholder="Ex.: Não consigo acessar o sistema"
            maxLength={200}
            {...register('title')}
            aria-invalid={Boolean(errors.title)}
          />

          <span className={styles.fieldFooter}>
            <small className={styles.error}>
              {errors.title?.message}
            </small>
            <small>{titleLength}/200</small>
          </span>
        </label>

        <label>
          Descrição
          <textarea
            rows={8}
            placeholder="Explique o que aconteceu, quando começou e qual impacto está causando."
            maxLength={4000}
            {...register('description')}
            aria-invalid={Boolean(errors.description)}
          />

          <span className={styles.fieldFooter}>
            <small className={styles.error}>
              {errors.description?.message}
            </small>
            <small>{descriptionLength}/4000</small>
          </span>
        </label>

        <label>
          Prioridade
          <select {...register('priority')}>
            <option value="1">Baixa</option>
            <option value="2">Média</option>
            <option value="3">Alta</option>
            <option value="4">Crítica</option>
          </select>
        </label>

        {mutationError && (
          <div className={styles.apiError} role="alert">
            <CircleAlert aria-hidden="true" />
            {mutationError}
          </div>
        )}

        <footer className={styles.actions}>
          <Link to="/tickets">Cancelar</Link>

          <button
            type="submit"
            disabled={
              createMutation.isPending ||
              categoriesQuery.isPending ||
              categoriesQuery.isError
            }
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
              ? 'Abrindo chamado...'
              : 'Abrir chamado'}
          </button>
        </footer>
      </form>
    </main>
  )
}

export default CreateTicketPage
