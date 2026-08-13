import { zodResolver } from '@hookform/resolvers/zod'
import {
  ArrowRight,
  Gauge,
  LifeBuoy,
  KeyRound,
  Mail,
  MessagesSquare,
  ShieldCheck,
  Sparkles,
} from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import styles from './LoginPage.module.css'

const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Informe seu e-mail.')
    .email('Informe um e-mail válido.'),
  password: z
    .string()
    .min(1, 'Informe sua senha.')
    .min(8, 'A senha deve possuir pelo menos 8 caracteres.'),
})

type LoginFormValues = z.infer<typeof loginSchema>

function LoginPage() {
  const [feedback, setFeedback] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: 'onTouched',
    defaultValues: {
      email: '',
      password: '',
    },
  })

  function handleValidSubmit() {
    setFeedback(
      'Formulário validado. A autenticação será conectada à API no próximo bloco.',
    )
  }

  return (
    <div className={styles.page}>
      <section className={styles.showcase}>
        <div className={styles.brand}>
          <span className={styles.brandMark}>
            <LifeBuoy aria-hidden="true" />
          </span>

          <span className={styles.brandCopy}>
            <strong>FlowDesk</strong>
            <small>Service management</small>
          </span>
        </div>

        <div className={styles.hero}>
          <span className={styles.eyebrow}>
            <Sparkles size={16} aria-hidden="true" />
            Atendimento que realmente flui
          </span>

          <h1>Uma experiência simples para resolver o que importa.</h1>

          <p className={styles.heroDescription}>
            Centralize chamados, acompanhe prioridades e mantenha sua equipe
            conectada em uma única plataforma.
          </p>

          <div className={styles.benefits}>
            <article>
              <span>
                <MessagesSquare aria-hidden="true" />
              </span>
              <div>
                <strong>Chamados centralizados</strong>
                <small>Histórico completo em um só lugar</small>
              </div>
            </article>

            <article>
              <span>
                <ShieldCheck aria-hidden="true" />
              </span>
              <div>
                <strong>Acesso seguro</strong>
                <small>Permissões adequadas a cada perfil</small>
              </div>
            </article>

            <article>
              <span>
                <Gauge aria-hidden="true" />
              </span>
              <div>
                <strong>Visão operacional</strong>
                <small>Indicadores para decisões mais rápidas</small>
              </div>
            </article>
          </div>

          <article className={styles.ticketPreview}>
            <header>
              <div>
                <small>Chamado em destaque</small>
                <strong>#FD-2048</strong>
              </div>
              <span className={styles.priority}>Alta prioridade</span>
            </header>

            <h2>Falha de acesso ao sistema financeiro</h2>
            <p>
              A equipe já recebeu a solicitação e iniciou o atendimento.
            </p>

            <footer>
              <span className={styles.status}>
                <i />
                Em andamento
              </span>

              <span className={styles.assignee}>
                <i>RC</i>
                <span>
                  <small>Responsável</small>
                  <strong>Renato Caetité</strong>
                </span>
              </span>
            </footer>
          </article>
        </div>

        <p className={styles.technology}>
          API .NET 10 <span>•</span> React <span>•</span> TypeScript
        </p>
      </section>

      <main className={styles.formArea}>
        <section className={styles.formCard}>
          <span className={styles.securityBadge}>
            <ShieldCheck size={16} aria-hidden="true" />
            Área segura
          </span>

          <div className={styles.formHeading}>
            <h2>Acesse sua conta</h2>
            <p>Entre com suas credenciais para continuar no FlowDesk.</p>
          </div>

          <form
            className={styles.form}
            onSubmit={handleSubmit(handleValidSubmit)}
            noValidate
          >
            <div className={styles.field}>
              <label htmlFor="email">E-mail</label>

              <div
                className={`${styles.inputShell} ${errors.email ? styles.inputInvalid : ''
                  }`}
              >
                <Mail size={19} aria-hidden="true" />
                <input
                  id="email"
                  type="email"
                  placeholder="voce@empresa.com"
                  autoComplete="email"
                  aria-invalid={Boolean(errors.email)}
                  aria-describedby={errors.email ? 'email-error' : undefined}
                  {...register('email')}
                />
              </div>

              {errors.email && (
                <small id="email-error" className={styles.error}>
                  {errors.email.message}
                </small>
              )}
            </div>

            <div className={styles.field}>
              <label htmlFor="password">Senha</label>

              <div
                className={`${styles.inputShell} ${errors.password ? styles.inputInvalid : ''
                  }`}
              >
                <KeyRound size={19} aria-hidden="true" />
                <input
                  id="password"
                  type="password"
                  placeholder="Digite sua senha"
                  autoComplete="current-password"
                  aria-invalid={Boolean(errors.password)}
                  aria-describedby={
                    errors.password ? 'password-error' : undefined
                  }
                  {...register('password')}
                />
              </div>

              {errors.password && (
                <small id="password-error" className={styles.error}>
                  {errors.password.message}
                </small>
              )}
            </div>

            <button className={styles.submitButton} type="submit">
              Entrar no FlowDesk
              <ArrowRight size={19} aria-hidden="true" />
            </button>

            {feedback && (
              <p className={styles.feedback} role="status">
                {feedback}
              </p>
            )}
          </form>

          <div className={styles.formFooter}>
            <ShieldCheck size={17} aria-hidden="true" />
            <span>Autenticação protegida por JWT e refresh token.</span>
          </div>
        </section>

        <p className={styles.projectNote}>
          Projeto educacional desenvolvido para estudo e portfólio.
        </p>
      </main>
    </div>
  )
}

export default LoginPage
