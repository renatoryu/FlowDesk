# Sprint 3 — Gerenciamento de chamados

## Visão geral

A Sprint 3 implementou o gerenciamento completo de chamados do FlowDesk: criação, consulta, listagem paginada, atualização, alteração de status e exclusão lógica.

A implementação preserva a separação entre regras de domínio, casos de uso, persistência e API, além de aplicar controle de acesso por perfil e isolamento dos chamados de cada Customer.

## Funcionalidades entregues

- Modelagem das entidades `Ticket` e `Category`.
- Categorias iniciais cadastradas automaticamente.
- Prioridades e status representados por enums.
- Criação de chamados por Customers vinculados a empresas ativas.
- Consulta de chamado por identificador.
- Listagem paginada com filtros.
- Atualização de título, descrição, categoria e prioridade.
- Alteração de status com transições controladas.
- Exclusão lógica e idempotente.
- Autorização baseada em `Customer`, `Agent` e `Admin`.
- Respostas de erro padronizadas com `ProblemDetails`.
- Documentação interativa no Swagger/OpenAPI.

## Regras de negócio

### Categorias, prioridades e status

- Somente categorias ativas podem ser usadas em novos chamados ou atualizações.
- Prioridades: `Low = 1`, `Medium = 2`, `High = 3` e `Critical = 4`.
- Status: `Open = 1`, `InProgress = 2`, `Resolved = 3` e `Closed = 4`.
- Transições permitidas:

```text
Open → InProgress
InProgress → Open ou Resolved
Resolved → InProgress ou Closed
Closed → nenhuma transição
```

### Acesso aos chamados

- Customer cria chamados apenas para a própria empresa vinculada e ativa.
- Customer consulta, lista, edita e exclui somente os próprios chamados da empresa atual.
- Customer só pode solicitar o fechamento de um chamado próprio; o domínio permite isso somente quando ele já está `Resolved`.
- Agent e Admin podem listar, consultar, editar, alterar status e excluir qualquer chamado ativo.
- Agent e Admin não criam chamados pelo endpoint de criação.
- Acesso de Customer a um chamado de outro usuário ou empresa retorna `404`, evitando enumeração de IDs.

### Exclusão lógica

- A exclusão não remove o registro do banco.
- O chamado recebe informações de auditoria: data e usuário responsável pela exclusão.
- Chamados excluídos não aparecem em consultas e listagens normais.
- Repetir o mesmo `DELETE` retorna `204 No Content`, sem persistir uma nova alteração.

## Listagem e filtros

A listagem utiliza os parâmetros:

| Parâmetro | Padrão | Regra |
|---|---:|---|
| `page` | `1` | Deve ser maior ou igual a 1 |
| `pageSize` | `20` | Entre 1 e 100 |
| `status` | — | Filtro opcional por status |
| `priority` | — | Filtro opcional por prioridade |
| `categoryId` | — | Filtro opcional por categoria |

O Customer recebe somente os próprios chamados da empresa atual. Agent e Admin possuem visão global dos chamados não excluídos.

## Organização por camada

| Camada | Responsabilidade |
|---|---|
| `FlowDesk.Domain` | Entidades `Ticket` e `Category`, enums e regras de transição |
| `FlowDesk.Application` | Casos de uso de criação, leitura, listagem, atualização, status e exclusão |
| `FlowDesk.Infrastructure` | Entity Framework Core, SQL Server, configurações e repositórios |
| `FlowDesk.Api` | Endpoints HTTP, políticas de autorização e respostas |
| `FlowDesk.UnitTests` | Testes das regras de domínio e dos casos de uso |

## Persistência

A migration `AddTicketing` criou as tabelas `Categories` e `Tickets`.

A persistência inclui:

- categorias iniciais para Access, General, Hardware, Network e Software;
- relacionamentos entre chamado, empresa, categoria, solicitante e usuário que excluiu;
- índices para listagem por empresa, solicitante, status e categoria;
- índices filtrados para chamados não excluídos;
- `RowVersion` para concorrência;
- constraints para prioridade, status, datas de status e integridade da exclusão lógica.

## Endpoints

| Método | Endpoint | Acesso | Finalidade |
|---|---|---|---|
| `GET` | `/api/categories` | Usuário autenticado | Listar categorias ativas |
| `POST` | `/api/tickets` | Customer | Criar chamado |
| `GET` | `/api/tickets` | Customer próprio, Agent ou Admin | Listar chamados |
| `GET` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Consultar chamado |
| `PUT` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Atualizar detalhes |
| `PATCH` | `/api/tickets/{id}/status` | Customer com restrição, Agent ou Admin | Alterar status |
| `DELETE` | `/api/tickets/{id}` | Customer próprio, Agent ou Admin | Excluir logicamente |

## Respostas HTTP

| Status | Situação |
|---:|---|
| `200 OK` | Consulta, listagem, atualização ou mudança de status concluída |
| `201 Created` | Chamado criado |
| `204 No Content` | Chamado excluído logicamente |
| `400 Bad Request` | Dados de entrada inválidos |
| `401 Unauthorized` | JWT ausente, inválido ou sessão desatualizada |
| `403 Forbidden` | Perfil sem permissão para a operação |
| `404 Not Found` | Chamado inexistente, excluído ou fora do escopo do Customer |
| `409 Conflict` | Categoria/empresa inativa, transição inválida ou conflito de concorrência |
| `500 Internal Server Error` | Falha inesperada |

## Validação realizada

- Solução compilada com 0 erros e 0 avisos.
- Formatação validada com `dotnet format`.
- 157 testes unitários aprovados.
- Criação, consulta, listagem, filtros e paginação validados pelo Swagger.
- Atualização validada com respostas `200`, `400` e `401`.
- Fluxo de status validado com `403 → 401 → 200 → 200 → 200 → 409`.
- Soft delete validado com `401 → 204 → 204 → 404`.
- Chamados excluídos confirmados como ausentes da listagem.

## Rastreabilidade

- [Issue #11 — Sprint 3: gerenciamento de chamados](https://github.com/renatoryu/FlowDesk/issues/11)
- [Pull Request #12 — gerenciamento de chamados para develop](https://github.com/renatoryu/FlowDesk/pull/12)

## Próximos aprimoramentos

- Iniciar a Sprint 4, responsável por comentários e histórico dos chamados.
- Adicionar testes de integração da API e do Entity Framework Core.
- Automatizar build e testes com integração contínua.
- Evoluir o controle de concorrência HTTP com ETag e `If-Match`.