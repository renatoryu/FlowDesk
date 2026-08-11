# FlowDesk

O FlowDesk é uma plataforma de gerenciamento de chamados criada para simular um produto de Help Desk utilizado por empresas.

O projeto está sendo desenvolvido com foco em aprendizado, boas práticas, portfólio e preparação profissional para oportunidades na área .NET.

## Estado do projeto

✅ Sprint 1 concluída — autenticação de usuários com cadastro, login, JWT, refresh token rotativo e Swagger/OpenAPI.

✅ Sprint 2 concluída — gerenciamento de empresas com validação de CNPJ, CRUD protegido por perfis e exclusão lógica.

✅ Sprint 3 concluída — gerenciamento completo de chamados, com categorias, paginação, filtros, atualização, status controlado, exclusão lógica e autorização por perfil.

✅ Sprint 4 concluída — comentários imutáveis, histórico cronológico, persistência no SQL Server e autorização contextual por chamado e empresa.

✅ Sprint 5 concluída — dashboard seguro com indicadores de chamados abertos, em andamento e finalizados, calculados conforme o escopo de acesso do usuário.

Próxima etapa: Sprint 6 — Uploads e anexos de chamados.

A solução possui atualmente 200 testes unitários aprovados.

- [Documentação técnica da Sprint 1](docs/SPRINT-1.md)
- [Documentação técnica da Sprint 2](docs/SPRINT-2.md)
- [Documentação técnica da Sprint 3](docs/SPRINT-3.md)
- [Documentação técnica da Sprint 4](docs/SPRINT-4.md)
- [Documentação técnica da Sprint 5](docs/SPRINT-5.md)

## Objetivos

- Aplicar conceitos de C# e ASP.NET Core.
- Utilizar uma arquitetura inspirada na Clean Architecture.
- Implementar autenticação com JWT e refresh token.
- Persistir dados com Entity Framework Core e SQL Server.
- Criar testes automatizados.
- Construir um front-end com React e TypeScript.
- Containerizar a aplicação com Docker.
- Preparar a publicação em nuvem.

## Tecnologias

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

### Front-end

- React
- TypeScript

### Qualidade e infraestrutura

- xUnit
- FluentValidation
- Swagger/OpenAPI
- Docker
- Git e GitHub
- Azure

## Arquitetura

A solução segue uma arquitetura inspirada na Clean Architecture:

```text
FlowDesk.Api
├── FlowDesk.Application
└── FlowDesk.Infrastructure
    ├── FlowDesk.Application
    └── FlowDesk.Domain

FlowDesk.Application
└── FlowDesk.Domain
```

Responsabilidades:

- **Domain:** entidades e regras de negócio.
- **Application:** casos de uso e contratos.
- **Infrastructure:** banco de dados e serviços externos.
- **Api:** entrada HTTP, autenticação, autorização e respostas.
- **UnitTests:** testes das regras de negócio e serviços.

## Estrutura

```text
FlowDesk
├── src
│   ├── FlowDesk.Api
│   ├── FlowDesk.Application
│   ├── FlowDesk.Domain
│   └── FlowDesk.Infrastructure
├── tests
│   └── FlowDesk.UnitTests
├── docs
└── README.md
```

## Executando localmente

### Pré-requisitos

- Visual Studio com suporte ao .NET 10.
- SDK do .NET 10.
- SQL Server Express LocalDB.

### 1. Configurar a chave JWT

A chave de assinatura é armazenada com User Secrets e não deve ser adicionada ao Git.

No terminal PowerShell, na raiz da solução:

```powershell
$jwtKeyBytes = New-Object byte[] 64
$jwtRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$jwtRng.GetBytes($jwtKeyBytes)
$jwtSigningKey = [Convert]::ToBase64String($jwtKeyBytes)

dotnet user-secrets set "Jwt:SigningKey" $jwtSigningKey --project ".\src\FlowDesk.Api\FlowDesk.Api.csproj"

$jwtRng.Dispose()
Remove-Variable jwtKeyBytes, jwtSigningKey, jwtRng
```

### 2. Atualizar o banco de dados

No Console do Gerenciador de Pacotes do Visual Studio:

```powershell
Update-Database -Project FlowDesk.Infrastructure -StartupProject FlowDesk.Api
```

### 3. Executar a API

1. Abra `FlowDesk.slnx`.
2. Defina `FlowDesk.Api` como projeto de inicialização.
3. Execute com `Ctrl + F5`.
4. Acesse `/swagger`.

### Endpoints atuais

| Método | Endpoint | Autenticação | Finalidade |
|---|---|---|---|
| `GET` | `/` | Não | Verificar o estado da API |
| `POST` | `/api/auth/register` | Não | Cadastrar usuário |
| `POST` | `/api/auth/login` | Não | Obter access token e refresh token |
| `POST` | `/api/auth/refresh` | Refresh token | Renovar e rotacionar os tokens |
| `GET` | `/api/auth/me` | Bearer JWT | Consultar o usuário autenticado |
| `POST` | `/api/companies` | Admin JWT | Cadastrar empresa |
| `GET` | `/api/companies` | Admin/Agent JWT | Listar empresas e filtrar inativas |
| `GET` | `/api/companies/{id}` | Admin/Agent JWT | Consultar empresa por identificador |
| `PUT` | `/api/companies/{id}` | Admin JWT | Atualizar nome e e-mail da empresa |
| `DELETE` | `/api/companies/{id}` | Admin JWT | Desativar logicamente a empresa |
| `PUT` | `/api/users/{id}/company` | Admin JWT | Vincular Customer a uma empresa |
| `GET` | `/api/categories` | Bearer JWT | Listar categorias ativas |
| `POST` | `/api/tickets` | Customer JWT | Criar chamado |
| `GET` | `/api/tickets` | Customer próprio, Agent ou Admin | Listar chamados |
| `GET` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Consultar chamado |
| `PUT` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Atualizar detalhes |
| `PATCH` | `/api/tickets/{id}/status` | Customer com restrição, Agent ou Admin | Alterar status |
| `DELETE` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Excluir logicamente |
| `POST` | `/api/tickets/{ticketId}/comments` | Customer próprio, Agent ou Admin | Registrar comentário em um chamado |
| `GET` | `/api/tickets/{ticketId}/comments` | Customer próprio, Agent ou Admin | Consultar histórico cronológico |
| `GET` | `/api/dashboard/summary` | Customer próprio, Agent ou Admin | Consultar indicadores operacionais |

### Testes

Abra o Gerenciador de Testes do Visual Studio e execute todos os testes. O estado atual possui 200 testes unitários aprovados.

## Fluxo de desenvolvimento

- `main`: versões estáveis.
- `develop`: integração das funcionalidades.
- `feature/*`: desenvolvimento de funcionalidades.
- `bugfix/*`: correções em desenvolvimento.
- `hotfix/*`: correções urgentes em produção.

Os commits seguem o padrão Conventional Commits:

```text
feat:
fix:
docs:
refactor:
test:
style:
chore:
```

## Roadmap

O projeto será desenvolvido incrementalmente, da preparação inicial ao deploy, conforme o [roadmap detalhado](docs/ROADMAP.md).
