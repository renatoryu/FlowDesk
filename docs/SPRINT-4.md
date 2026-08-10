# Sprint 4 — Comentários e histórico de chamados

## Visão geral

A Sprint 4 adicionou interações registradas aos chamados do FlowDesk. Customers, Agents e Admins podem consultar o histórico conforme suas permissões, e novos comentários são registrados com autor, data e vínculo ao chamado.

## Funcionalidades entregues

- Modelagem da entidade `Comment`.
- Relacionamento entre comentários, chamados e autores.
- Migration `AddComments` com persistência no SQL Server.
- Criação segura de comentários.
- Consulta cronológica do histórico de um chamado.
- Autorização por perfil e isolamento de dados por empresa.
- Validações de domínio, aplicação e API.
- Documentação Swagger/OpenAPI.
- Testes unitários para regras, validação e casos de uso.

## Regras de negócio

### Comentários

- Um comentário possui `TicketId`, `AuthorId`, conteúdo e datas de auditoria.
- O conteúdo é obrigatório, normalizado com `Trim()` e limitado a 2.000 caracteres.
- O autor é sempre derivado do usuário autenticado; `AuthorId` não é recebido pelo corpo HTTP.
- Comentários são imutáveis nesta Sprint: não há edição nem exclusão de comentário.
- Chamados `Closed` não aceitam novos comentários e retornam `409 Conflict`.
- Chamados excluídos logicamente não aceitam nem exibem comentários e retornam `404 Not Found`.

### Autorização

- Customer cria e consulta comentários apenas nos próprios chamados da empresa ativa atual.
- Agent e Admin criam e consultam comentários em qualquer chamado não excluído.
- Usuário ausente, inativo ou com token de papel desatualizado recebe `401 Unauthorized`.
- Customer fora do escopo recebe `404 Not Found`, evitando enumeração de chamados.
- Customer sem empresa, ou vinculado a empresa indisponível/inativa, recebe `409 Conflict`.

### Histórico

- O histórico é retornado em ordem cronológica por `CreatedAtUtc` e, em caso de empate, por `Id`.
- Chamados fechados continuam exibindo os comentários já registrados.

## Organização por camada

| Camada | Responsabilidade |
|---|---|
| `FlowDesk.Domain` | Entidade `Comment` e regra de bloqueio para chamados fechados ou excluídos |
| `FlowDesk.Application` | Casos de uso de criação e consulta de histórico |
| `FlowDesk.Infrastructure` | Entity Framework Core, migration, configuração e repositório |
| `FlowDesk.Api` | Endpoints HTTP, políticas e documentação Swagger |
| `FlowDesk.UnitTests` | Testes de domínio, validação e autorização contextual |

## Persistência

A migration `AddComments` criou a tabela `Comments` com:

- chaves `Id`, `TicketId` e `AuthorId`;
- coluna `Content` com máximo de 2.000 caracteres;
- datas `CreatedAtUtc` e `UpdatedAtUtc`;
- chaves estrangeiras para `Tickets` e `Users`, sem exclusão em cascata;
- índice cronológico por `TicketId`, `CreatedAtUtc` e `Id`.

A empresa não é repetida em `Comments`: ela é obtida a partir do chamado, evitando inconsistência de dados.

## Endpoints

| Método | Endpoint | Acesso | Finalidade |
|---|---|---|---|
| `POST` | `/api/tickets/{ticketId}/comments` | Customer próprio, Agent ou Admin | Registrar comentário |
| `GET` | `/api/tickets/{ticketId}/comments` | Customer próprio, Agent ou Admin | Consultar histórico |

## Respostas HTTP

| Status | Situação |
|---:|---|
| `200 OK` | Histórico consultado com sucesso |
| `201 Created` | Comentário criado |
| `400 Bad Request` | Conteúdo ou identificador inválido |
| `401 Unauthorized` | JWT ausente, inválido ou sessão desatualizada |
| `403 Forbidden` | Perfil sem permissão |
| `404 Not Found` | Chamado inexistente, excluído ou fora do escopo |
| `409 Conflict` | Ticket fechado ou empresa indisponível |
| `500 Internal Server Error` | Falha inesperada |

## Validação realizada

- Solução compilada com 0 erros e 0 avisos.
- 191 testes unitários aprovados.
- Criação de comentário validada com `201 Created`.
- Histórico validado com `200 OK`.
- Conteúdo inválido validado com `400 Bad Request`.
- Acesso sem JWT validado com `401 Unauthorized`.
- Comentário em chamado fechado validado com `409 Conflict`.

## Rastreabilidade

- [Issue #14 — Sprint 4: comentários e histórico de chamados](https://github.com/renatoryu/FlowDesk/issues/14)

## Próximos aprimoramentos

- Iniciar a Sprint 5 com indicadores do dashboard.
- Criar testes de integração para API e Entity Framework Core.
- Automatizar build e testes com integração contínua.