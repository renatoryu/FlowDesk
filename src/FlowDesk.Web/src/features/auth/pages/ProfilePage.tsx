import { useQuery } from '@tanstack/react-query'
import {
  BadgeCheck,
  CircleAlert,
  Clock3,
  KeyRound,
  LoaderCircle,
  Mail,
  RefreshCw,
  ShieldCheck,
  UserRound,
} from 'lucide-react'
import { useAuth } from '../context/useAuth'
import { getCurrentUser } from '../services/authApi'
import type { UserRole } from '../types/authTypes'
import styles from './ProfilePage.module.css'

const roleLabels: Record<UserRole, string> = {
  Customer: 'Cliente',
  Agent: 'Agente de atendimento',
  Admin: 'Administrador',
}

const dateFormatter = new Intl.DateTimeFormat(
  'pt-BR',
  {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'America/Sao_Paulo',
  },
)

function ProfilePage() {
  const { session } = useAuth()
  const accessToken = session?.accessToken ?? ''

  const currentUserQuery = useQuery({
    queryKey: [
      'auth',
      'current-user',
      session?.user.id,
    ],
    queryFn: () =>
      getCurrentUser(accessToken),
    enabled: accessToken.length > 0,
  })

  if (!session) {
    return null
  }

  const user =
    currentUserQuery.data ?? session.user

  const accessTokenExpiresAt = new Date(
    session.accessTokenExpiresAtUtc,
  )

  const refreshTokenExpiresAt = new Date(
    session.refreshTokenExpiresAtUtc,
  )

  const initials = user.fullName
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0))
    .join('')
    .toUpperCase()

  return (
    <main className={styles.content}>
      <header className={styles.header}>
        <div>
          <span>Conta e segurança</span>
          <h1>Meu perfil</h1>
          <p>
            Consulte sua identidade, permissões e
            informações da sessão conectada.
          </p>
        </div>

        <span className={styles.secureBadge}>
          <ShieldCheck aria-hidden="true" />
          Sessão protegida
        </span>
      </header>

      {currentUserQuery.isPending && (
        <section className={styles.state}>
          <LoaderCircle
            className={styles.spinning}
            aria-hidden="true"
          />
          Validando sua identidade...
        </section>
      )}

      {currentUserQuery.isError && (
        <section className={styles.error} role="alert">
          <CircleAlert aria-hidden="true" />

          <div>
            <strong>
              Não foi possível validar novamente o
              perfil.
            </strong>
            <span>
              Os dados armazenados na sessão continuam
              sendo exibidos.
            </span>
          </div>

          <button
            type="button"
            onClick={() =>
              currentUserQuery.refetch()
            }
          >
            <RefreshCw aria-hidden="true" />
            Tentar novamente
          </button>
        </section>
      )}

      <div className={styles.layout}>
        <section className={styles.identityCard}>
          <div className={styles.avatar}>
            {initials}
          </div>

          <div className={styles.identity}>
            <span>Usuário autenticado</span>
            <h2>{user.fullName}</h2>
            <p>{roleLabels[user.role]}</p>
          </div>

          <span className={styles.active}>
            <BadgeCheck aria-hidden="true" />
            Conta ativa
          </span>
        </section>

        <section className={styles.detailsCard}>
          <header>
            <UserRound aria-hidden="true" />

            <div>
              <h2>Dados da conta</h2>
              <span>
                Informações fornecidas pela API
              </span>
            </div>
          </header>

          <dl>
            <div>
              <dt>
                <UserRound aria-hidden="true" />
                Nome completo
              </dt>
              <dd>{user.fullName}</dd>
            </div>

            <div>
              <dt>
                <Mail aria-hidden="true" />
                E-mail
              </dt>
              <dd>{user.email}</dd>
            </div>

            <div>
              <dt>
                <ShieldCheck aria-hidden="true" />
                Perfil de acesso
              </dt>
              <dd>{roleLabels[user.role]}</dd>
            </div>

            <div>
              <dt>
                <KeyRound aria-hidden="true" />
                Identificador
              </dt>
              <dd className={styles.identifier}>
                {user.id}
              </dd>
            </div>
          </dl>
        </section>

        <section className={styles.securityCard}>
          <header>
            <KeyRound aria-hidden="true" />

            <div>
              <h2>Segurança da sessão</h2>
              <span>
                Autenticação baseada em JWT
              </span>
            </div>
          </header>

          <div className={styles.securityItems}>
            <article>
              <span className={styles.validStatus}>
                Ativo
              </span>

              <div>
                <strong>Token de acesso</strong>
                <span>
                  Renovado automaticamente antes de
                  expirar.
                </span>
              </div>

              <time>
                <Clock3 aria-hidden="true" />
                {dateFormatter.format(
                  accessTokenExpiresAt,
                )}
              </time>
            </article>

            <article>
              <span className={styles.validStatus}>
                Protegido
              </span>

              <div>
                <strong>Sessão renovável</strong>
                <span>
                  O refresh token mantém a experiência
                  segura e contínua.
                </span>
              </div>

              <time>
                <Clock3 aria-hidden="true" />
                {dateFormatter.format(
                  refreshTokenExpiresAt,
                )}
              </time>
            </article>
          </div>

          <footer>
            <ShieldCheck aria-hidden="true" />

            <p>
              Os tokens não são exibidos nesta página.
              Apenas metadados seguros da sessão são
              apresentados.
            </p>
          </footer>
        </section>
      </div>
    </main>
  )
}

export default ProfilePage
