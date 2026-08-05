# FlowDesk

O FlowDesk é uma plataforma de gerenciamento de chamados criada para simular um produto de Help Desk utilizado por empresas.

O projeto está sendo desenvolvido com foco em aprendizado, boas práticas, portfólio e preparação profissional para oportunidades na área .NET.

## Estado do projeto

✅ Sprint 1 concluída — autenticação de usuários com cadastro, login, JWT, refresh token rotativo e Swagger/OpenAPI.

Próxima etapa: Sprint 2 — Empresas.

A solução possui atualmente 20 testes unitários aprovados.

[Veja a documentação técnica da Sprint 1](docs/SPRINT-1.md).

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
| `POST` | `/api/auth/refresh` | Não | Renovar e rotacionar os tokens |
| `GET` | `/api/auth/me` | Bearer JWT | Consultar o usuário autenticado |

### Testes

Abra o Gerenciador de Testes do Visual Studio e execute todos os testes. O estado atual possui 20 testes unitários.

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

O projeto será desenvolvido incrementalmente, da preparação inicial ao deploy, conforme o roadmap disponível na pasta `docs`.
