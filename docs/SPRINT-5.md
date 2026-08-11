# Sprint 5 — Dashboard e indicadores operacionais

## Visão geral

A Sprint 5 adicionou indicadores operacionais para o dashboard do FlowDesk. O resumo é calculado em tempo real a partir dos chamados visíveis ao usuário autenticado, preservando as regras de autorização e isolamento já aplicadas no sistema.

## Funcionalidades entregues

- Endpoint `GET /api/dashboard/summary`.
- Quantidade de chamados abertos.
- Quantidade de chamados em andamento.
- Quantidade de chamados finalizados.
- Isolamento dos indicadores conforme o perfil autenticado.
- Consulta agregada e eficiente com Entity Framework Core.
- Documentação Swagger/OpenAPI.
- Testes unitários para escopo, segurança e resultado do resumo.

## Regras de negócio

### Indicadores

- `openTickets` contabiliza chamados com status `Open`.
- `inProgressTickets` contabiliza chamados com status `InProgress`.
- `completedTickets` contabiliza chamados com status `Resolved` ou `Closed`.
- Chamados excluídos logicamente não entram em nenhum indicador.
- Os três indicadores são mutuamente exclusivos e representam todos os chamados visíveis não excluídos.

### Autorização e escopo

- Customer consulta somente os próprios chamados da empresa ativa atualmente vinculada.
- Agent e Admin consultam os indicadores globais de chamados não excluídos.
- Usuário ausente, inativo ou com papel diferente do registrado no token recebe `401 Unauthorized`.
- Customer sem empresa, ou vinculado a empresa indisponível ou inativa, recebe `409 Conflict`.
- O endpoint não aceita `companyId` ou `requesterId` por query string; o escopo é sempre derivado do usuário autenticado.

## Organização por camada

| Camada | Responsabilidade |
|---|---|
| `FlowDesk.Application` | Contratos, caso de uso e montagem do filtro de acesso |
| `FlowDesk.Infrastructure` | Consulta agregada, leitura com `AsNoTracking()` e filtros no SQL Server |
| `FlowDesk.Api` | Endpoint HTTP, policy de autorização e Swagger/OpenAPI |
| `FlowDesk.UnitTests` | Testes de isolamento, perfis, usuário e empresa |

## Persistência e desempenho

Esta Sprint não exige nova entidade, migration ou pacote.

O `DashboardRepository` aplica primeiro os filtros de escopo e de exclusão lógica, depois executa uma única consulta agregada para calcular os três indicadores. Assim, a API não materializa chamados em memória nem executa consultas independentes para cada total.

## Endpoint

| Método | Endpoint | Acesso | Finalidade |
|---|---|---|---|
| `GET` | `/api/dashboard/summary` | Customer próprio, Agent ou Admin | Consultar os indicadores operacionais |

Exemplo de resposta:

```json
{
  "openTickets": 2,
  "inProgressTickets": 0,
  "completedTickets": 0
}
```

## Respostas HTTP

| Status | Situação |
|---:|---|
| `200 OK` | Indicadores consultados com sucesso |
| `401 Unauthorized` | JWT ausente, inválido ou sessão desatualizada |
| `403 Forbidden` | Perfil sem permissão |
| `409 Conflict` | Customer sem empresa válida ou empresa inativa |
| `500 Internal Server Error` | Falha inesperada |

## Validação realizada

- Solução compilada sem erros ou avisos.
- 200 testes unitários aprovados.
- Acesso sem JWT validado com `401 Unauthorized`.
- Dashboard do Customer validado com `200 OK` e escopo próprio.
- Dashboard global de Agent/Admin validado com `200 OK`.
- Indicadores comparados com a listagem filtrada de chamados.

## Rastreabilidade

- [Issue #17 — Sprint 5: dashboard e indicadores operacionais](https://github.com/renatoryu/FlowDesk/issues/17)

## Próximos aprimoramentos

- Iniciar a Sprint 6 com anexos de arquivos aos chamados.
- Criar testes de integração para a consulta agregada.
- Avaliar cache apenas após existir volume e métricas de uso reais.