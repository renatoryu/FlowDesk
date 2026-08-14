# Sprint 9 — Front-end operacional com React

## Objetivo

Disponibilizar uma interface web responsiva para operar o FlowDesk, integrada à API ASP.NET Core e alinhada às regras de autorização do backend.

## Tecnologias

- React 19
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form e Zod
- CSS Modules
- Lucide React
- Vitest e Testing Library

## Funcionalidades entregues

### Autenticação e sessão

- Login integrado à API.
- Rotas protegidas.
- Persistência da sessão no `sessionStorage`.
- Renovação automática e rotativa do access token.
- Encerramento seguro da sessão quando a renovação não é possível.
- Página de perfil integrada ao endpoint `GET /api/auth/me`.
- Tokens não são exibidos na interface.

### Dashboard

- Indicadores de chamados abertos, em andamento e finalizados.
- Dados carregados diretamente de `GET /api/dashboard/summary`.
- Indicadores respeitam o perfil autenticado.

### Chamados

- Listagem paginada com filtros por status e prioridade.
- Criação de chamados para Customer.
- Consulta de detalhes.
- Comentários em ordem cronológica.
- Upload, listagem e download de anexos.
- Mudança de status conforme as transições permitidas pelo backend.
- Fechamento por Customer somente após resolução.
- Atualização de cache para manter dashboard, listagem e detalhe consistentes.

### Empresas

- Diretório de empresas disponível para Agent e Admin.
- Busca por nome, CNPJ e e-mail.
- Inclusão opcional de empresas inativas.
- Customer não visualiza o menu e não acessa a rota.

## Rotas principais

| Rota | Finalidade |
|---|---|
| `/login` | Autenticação |
| `/dashboard` | Indicadores operacionais |
| `/tickets` | Listagem de chamados |
| `/tickets/new` | Abertura de chamado |
| `/tickets/:ticketId` | Detalhes, comentários, anexos e status |
| `/companies` | Diretório de empresas |
| `/profile` | Dados e segurança da sessão |

## Organização

```text
src/FlowDesk.Web/src
├── features
│   ├── auth
│   ├── attachments
│   ├── categories
│   ├── comments
│   ├── companies
│   ├── dashboard
│   └── tickets
└── shared
    ├── api
    ├── layout
    └── routes
```

Cada funcionalidade concentra seus tipos, serviços, componentes e páginas. A comunicação HTTP passa por um cliente compartilhado, e o TanStack Query gerencia cache, carregamento e sincronização dos dados.

## Execução local

Na pasta `src/FlowDesk.Web`:

```powershell
Copy-Item .env.example .env.local
npm.cmd install
npm.cmd run dev
```

O arquivo `.env.local` define a URL da API usada pelo proxy local:

```env
VITE_API_URL=/api
FLOWDESK_API_PROXY_TARGET=http://localhost:62560
```

Para usar a API em Docker, altere o destino para:

```env
FLOWDESK_API_PROXY_TARGET=http://localhost:8080
```

## Qualidade

Validações executadas:

- `npm.cmd run typecheck`
- `npm.cmd run lint`
- `npm.cmd run build`
- `npm.cmd run test`

A Sprint encerra com 6 testes automatizados no front-end e 244 testes unitários aprovados no backend.

## Resultado

O FlowDesk agora possui uma experiência web operacional, responsiva e integrada às regras reais de autenticação, autorização, chamados, comentários, anexos, empresas e dashboard. O foco permanece educacional e de portfólio, com uma estrutura preparada para a etapa de deploy.