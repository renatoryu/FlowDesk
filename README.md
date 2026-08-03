# FlowDesk

O FlowDesk é uma plataforma de gerenciamento de chamados criada para simular um produto de Help Desk utilizado por empresas.

O projeto está sendo desenvolvido com foco em aprendizado, boas práticas, portfólio e preparação profissional para oportunidades na área .NET.

## Estado do projeto

🚧 Em desenvolvimento — Sprint 0: Preparação.

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

## Executando o estado atual

Pré-requisitos:

- Visual Studio com suporte ao .NET 10.
- SDK do .NET 10.

Passos:

1. Abra `FlowDesk.slnx`.
2. Defina `FlowDesk.Api` como projeto de inicialização.
3. Execute com `Ctrl+F5`.
4. Acesse a URL apresentada pelo Visual Studio.

A rota inicial deve retornar:

```json
{
  "application": "FlowDesk.Api",
  "status": "running"
}
```

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
