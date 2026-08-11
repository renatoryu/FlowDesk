# Sprint 6 — Anexos de arquivos nos chamados

## Visão geral

A Sprint 6 adicionou o gerenciamento seguro de anexos aos chamados do FlowDesk. Os arquivos são armazenados localmente, enquanto seus metadados são persistidos no SQL Server.

A implementação utiliza uma abstração de armazenamento, permitindo substituir o disco local por um serviço externo futuramente sem alterar as regras da aplicação.

## Funcionalidades entregues

- Upload de arquivos PDF, PNG, JPG e JPEG.
- Limite máximo de 10 MiB por arquivo.
- Validação de extensão, MIME type e assinatura binária.
- Armazenamento físico com nomes internos aleatórios.
- Persistência dos metadados no SQL Server.
- Listagem cronológica dos anexos.
- Download autenticado com suporte a requisições parciais.
- Isolamento dos anexos conforme o perfil autenticado.
- Remoção compensatória do arquivo quando a persistência falha.
- Documentação Swagger/OpenAPI.
- Testes unitários de domínio, validação, autorização e casos de uso.

## Regras de negócio

- São permitidos apenas arquivos `.pdf`, `.png`, `.jpg` e `.jpeg`.
- O tamanho deve estar entre 1 byte e 10 MiB.
- A extensão deve corresponder ao MIME type informado.
- A assinatura binária deve corresponder ao formato real do arquivo.
- Nomes contendo caminhos ou tentativas de path traversal são rejeitados.
- Chamados fechados não recebem novos anexos.
- Anexos existentes podem ser listados e baixados após o fechamento.
- Chamados excluídos logicamente não permitem acesso aos anexos.
- O nome físico interno nunca é exposto pela API.

## Autorização e escopo

- Customer pode acessar somente anexos dos próprios chamados na empresa ativa atualmente vinculada.
- Agent e Admin podem acessar anexos de qualquer chamado não excluído.
- Usuário ausente, inativo ou com papel desatualizado recebe `401 Unauthorized`.
- Customer sem empresa válida ou com empresa inativa recebe `409 Conflict`.
- Tentativas de acessar chamados de outro Customer retornam `404 Not Found`, evitando revelar sua existência.

## Armazenamento

Os arquivos são gravados em:

```text
src/FlowDesk.Api/uploads/attachments/{ticketId}
```

A pasta `uploads` é ignorada pelo Git.

Cada arquivo recebe um nome interno aleatório, enquanto o nome original permanece apenas nos metadados:

```text
7b10b177920549309253644a3455c7ab.png
```

A interface `IAttachmentStorage` mantém a aplicação independente da implementação física. Em um ambiente comercial, o armazenamento local poderá ser substituído por Azure Blob Storage, Amazon S3 ou outro serviço compatível.

## Persistência

A migration `AddAttachments` criou a tabela `Attachments` com:

- relacionamento com `Tickets`;
- relacionamento com o usuário responsável pelo upload;
- nome original e nome interno;
- MIME type;
- tamanho em bytes;
- datas de auditoria;
- índices para consulta por chamado e usuário;
- constraints para tamanho e formatos permitidos.

## Endpoints

| Método | Endpoint | Acesso | Finalidade |
|---|---|---|---|
| `POST` | `/api/tickets/{ticketId}/attachments` | Customer próprio, Agent ou Admin | Enviar um anexo |
| `GET` | `/api/tickets/{ticketId}/attachments` | Customer próprio, Agent ou Admin | Listar os anexos |
| `GET` | `/api/tickets/{ticketId}/attachments/{attachmentId}/download` | Customer próprio, Agent ou Admin | Baixar um anexo |

## Respostas HTTP

| Status | Situação |
|---:|---|
| `200 OK` | Listagem ou download realizado |
| `201 Created` | Anexo enviado com sucesso |
| `400 Bad Request` | Arquivo ou identificador inválido |
| `401 Unauthorized` | JWT ausente, inválido ou desatualizado |
| `403 Forbidden` | Perfil sem permissão |
| `404 Not Found` | Chamado, anexo ou arquivo indisponível |
| `409 Conflict` | Empresa inválida ou chamado fechado |
| `413 Payload Too Large` | Corpo da requisição excede o limite |
| `500 Internal Server Error` | Falha inesperada |

## Segurança

A validação não depende somente do nome informado pelo usuário. O conteúdo inicial do arquivo é inspecionado para confirmar sua assinatura:

- PDF: `%PDF-`
- PNG: assinatura de oito bytes do formato PNG
- JPEG: bytes iniciais `FF D8 FF`

O caminho físico é construído exclusivamente pelo servidor e validado para permanecer dentro da raiz configurada.

## Consistência entre arquivo e banco

O upload segue esta sequência:

1. validar arquivo e autorização;
2. salvar o conteúdo no armazenamento;
3. criar os metadados;
4. persistir no SQL Server;
5. apagar o arquivo automaticamente caso o banco falhe.

Isso evita arquivos órfãos quando uma operação não é concluída.

## Validação realizada

- Upload PNG validado com `201 Created`.
- Arquivo não permitido validado com `400 Bad Request`.
- Listagem de anexos validada com `200 OK`.
- Download validado com `200 OK`.
- Arquivo físico e metadados verificados.
- Pasta de uploads confirmada como ignorada pelo Git.
- 244 testes unitários aprovados.

## Rastreabilidade

- [Issue #20 — Sprint 6: anexos de arquivos nos chamados](https://github.com/renatoryu/FlowDesk/issues/20)

## Limitações do MVP

- Armazenamento local, adequado ao ambiente de estudos.
- Não há endpoint para exclusão individual de anexos.
- Não há integração com armazenamento em nuvem.
- Não há antivírus externo; a proteção atual valida formato, tamanho e assinatura binária.

## Próxima etapa

Iniciar a Sprint 7 com ampliação da estratégia de testes, automação e relatório de cobertura.