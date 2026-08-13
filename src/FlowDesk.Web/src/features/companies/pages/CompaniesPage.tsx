import { useQuery } from '@tanstack/react-query'
import {
  BadgeCheck,
  Ban,
  Building2,
  CircleAlert,
  LoaderCircle,
  Mail,
  RefreshCw,
  Search,
} from 'lucide-react'
import { useState } from 'react'
import { Navigate } from 'react-router'
import { useAuth } from '../../auth/context/useAuth'
import { listCompanies } from '../services/companyApi'
import styles from './CompaniesPage.module.css'

const dateFormatter = new Intl.DateTimeFormat(
  'pt-BR',
  {
    dateStyle: 'short',
    timeZone: 'America/Sao_Paulo',
  },
)

function formatTaxId(taxId: string) {
  const digits = taxId.replace(/\D/g, '')

  if (digits.length !== 14) {
    return taxId
  }

  return digits.replace(
    /^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/,
    '$1.$2.$3/$4-$5',
  )
}

function CompaniesPage() {
  const { session } = useAuth()
  const [search, setSearch] = useState('')
  const [includeInactive, setIncludeInactive] =
    useState(false)

  const accessToken = session?.accessToken ?? ''

  const companiesQuery = useQuery({
    queryKey: [
      'companies',
      'list',
      includeInactive,
    ],
    queryFn: () =>
      listCompanies(
        accessToken,
        includeInactive,
      ),
    enabled:
      accessToken.length > 0 &&
      session?.user.role !== 'Customer',
  })

  if (!session) {
    return null
  }

  if (session.user.role === 'Customer') {
    return <Navigate to="/dashboard" replace />
  }

  const companies = companiesQuery.data ?? []

  const normalizedSearch =
    search.trim().toLocaleLowerCase('pt-BR')

  const searchDigits =
    normalizedSearch.replace(/\D/g, '')

  const filteredCompanies = companies
    .filter((company) => {
      if (!normalizedSearch) {
        return true
      }

      const matchesName = company.name
        .toLocaleLowerCase('pt-BR')
        .includes(normalizedSearch)

      const matchesEmail = company.contactEmail
        .toLocaleLowerCase('pt-BR')
        .includes(normalizedSearch)

      const matchesTaxId =
        searchDigits.length > 0 &&
        company.taxId
          .replace(/\D/g, '')
          .includes(searchDigits)

      return (
        matchesName ||
        matchesEmail ||
        matchesTaxId
      )
    })
    .sort((first, second) =>
      first.name.localeCompare(
        second.name,
        'pt-BR',
      ),
    )

  const activeCompanies = companies.filter(
    (company) => company.isActive,
  ).length

  const inactiveCompanies =
    companies.length - activeCompanies

  return (
    <main className={styles.content}>
      <header className={styles.header}>
        <div>
          <span>Gestão organizacional</span>
          <h1>Empresas</h1>
          <p>
            Consulte as organizações atendidas pelo
            FlowDesk e acompanhe sua situação cadastral.
          </p>
        </div>

        <span className={styles.total}>
          <Building2 aria-hidden="true" />
          {companies.length} empresa(s)
        </span>
      </header>

      <section
        className={styles.indicators}
        aria-label="Indicadores de empresas"
      >
        <article>
          <span className={styles.activeIcon}>
            <BadgeCheck aria-hidden="true" />
          </span>

          <div>
            <strong>{activeCompanies}</strong>
            <span>Empresas ativas</span>
          </div>
        </article>

        <article>
          <span className={styles.inactiveIcon}>
            <Ban aria-hidden="true" />
          </span>

          <div>
            <strong>{inactiveCompanies}</strong>
            <span>Empresas inativas</span>
          </div>
        </article>
      </section>

      <section className={styles.filters}>
        <label className={styles.search}>
          <Search aria-hidden="true" />
          <span className={styles.visuallyHidden}>
            Pesquisar empresas
          </span>
          <input
            type="search"
            value={search}
            placeholder="Pesquisar por nome, CNPJ ou e-mail"
            onChange={(event) =>
              setSearch(event.target.value)
            }
          />
        </label>

        <label className={styles.inactiveFilter}>
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(event) =>
              setIncludeInactive(
                event.target.checked,
              )
            }
          />
          Incluir empresas inativas
        </label>
      </section>

      {companiesQuery.isPending && (
        <section className={styles.state}>
          <LoaderCircle
            className={styles.spinning}
            aria-hidden="true"
          />
          <strong>Carregando empresas...</strong>
        </section>
      )}

      {companiesQuery.isError && (
        <section className={styles.state}>
          <CircleAlert aria-hidden="true" />

          <strong>
            Não foi possível carregar as empresas.
          </strong>

          <button
            type="button"
            onClick={() => companiesQuery.refetch()}
          >
            <RefreshCw aria-hidden="true" />
            Tentar novamente
          </button>
        </section>
      )}

      {!companiesQuery.isPending &&
        !companiesQuery.isError &&
        filteredCompanies.length === 0 && (
          <section className={styles.state}>
            <Building2 aria-hidden="true" />

            <strong>
              Nenhuma empresa encontrada.
            </strong>

            <p>
              Altere a pesquisa ou os filtros para
              consultar outros registros.
            </p>
          </section>
        )}

      {filteredCompanies.length > 0 && (
        <section className={styles.companyList}>
          <header>
            <span>Empresa</span>
            <span>Contato</span>
            <span>Situação</span>
            <span>Cadastro</span>
          </header>

          {filteredCompanies.map((company) => (
            <article key={company.id}>
              <div className={styles.company}>
                <span>
                  <Building2 aria-hidden="true" />
                </span>

                <div>
                  <strong>{company.name}</strong>
                  <small>
                    {formatTaxId(company.taxId)}
                  </small>
                </div>
              </div>

              <div className={styles.contact}>
                <Mail aria-hidden="true" />
                <span>{company.contactEmail}</span>
              </div>

              <span
                className={
                  company.isActive
                    ? styles.activeStatus
                    : styles.inactiveStatus
                }
              >
                {company.isActive
                  ? 'Ativa'
                  : 'Inativa'}
              </span>

              <time>
                {dateFormatter.format(
                  new Date(company.createdAtUtc),
                )}
              </time>
            </article>
          ))}
        </section>
      )}
    </main>
  )
}

export default CompaniesPage
