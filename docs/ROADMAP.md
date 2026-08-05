# Roadmap do FlowDesk

Este documento acompanha o desenvolvimento incremental do FlowDesk. Uma sprint só será considerada concluída depois que seu checklist for validado.

## Sprint 0 — Preparação

- [x] Criar a Solution.
- [x] Criar os projetos da aplicação.
- [x] Organizar as pastas `src`, `tests` e `docs`.
- [x] Configurar referências entre os projetos.
- [x] Criar o projeto de testes unitários.
- [x] Configurar o Git.
- [x] Criar as branches `main` e `develop`.
- [x] Criar o README.
- [x] Configurar a documentação inicial.
- [x] Executar a API localmente.
- [x] Realizar o primeiro commit.
- [x] Criar o commit de conclusão da Sprint 0.

## Sprint 1 — Autenticação

- [x] Modelar a entidade `User`.
- [x] Configurar o Entity Framework Core.
- [x] Configurar o SQL Server.
- [x] Criar a migration inicial.
- [x] Implementar cadastro.
- [x] Implementar login.
- [x] Implementar JWT.
- [x] Implementar refresh token.
- [x] Configurar Swagger/OpenAPI.

## Sprint 2 — Empresas

- [x] Modelar a entidade `Company`.
- [x] Criar empresa.
- [x] Editar empresa.
- [x] Desativar empresa por exclusão lógica.
- [x] Listar empresas.
- [x] Consultar empresa por identificador.

## Sprint 3 — Chamados

- [ ] Modelar `Ticket`.
- [ ] Modelar `Category`.
- [ ] Modelar prioridades.
- [ ] Modelar status.
- [ ] Criar chamado.
- [ ] Editar chamado.
- [ ] Alterar status.
- [ ] Excluir chamado.
- [ ] Listar chamados.
- [ ] Consultar chamado por identificador.

## Sprint 4 — Comentários

- [ ] Modelar `Comment`.
- [ ] Relacionar comentários aos chamados.
- [ ] Criar comentário.
- [ ] Consultar histórico do chamado.

## Sprint 5 — Dashboard

- [ ] Exibir quantidade de chamados abertos.
- [ ] Exibir quantidade de chamados em andamento.
- [ ] Exibir quantidade de chamados finalizados.

## Sprint 6 — Uploads

- [ ] Implementar upload de PDF.
- [ ] Implementar upload de PNG.
- [ ] Implementar upload de JPG.
- [ ] Validar tipo e tamanho dos arquivos.

## Sprint 7 — Testes

- [ ] Criar testes das regras de negócio.
- [ ] Criar testes dos serviços.
- [ ] Executar testes automaticamente.
- [ ] Gerar relatório de cobertura.

## Sprint 8 — Docker

- [ ] Criar o Dockerfile da API.
- [ ] Criar o Docker Compose.
- [ ] Configurar o SQL Server no Docker.
- [ ] Executar a solução em contêineres.

## Sprint 9 — Front-end

- [ ] Criar o projeto React com TypeScript.
- [ ] Criar a página de login.
- [ ] Criar o dashboard.
- [ ] Criar a área de empresas.
- [ ] Criar a área de chamados.
- [ ] Criar a página de perfil.
- [ ] Integrar o front-end com a API.

## Sprint 10 — Deploy

- [ ] Publicar a API.
- [ ] Publicar o banco de dados.
- [ ] Publicar o front-end.
- [ ] Configurar variáveis e segredos de produção.
- [ ] Validar a aplicação em produção.

## Checklist final

- [ ] Código limpo.
- [ ] Arquitetura organizada.
- [ ] Documentação completa.
- [ ] Testes automatizados.
- [ ] Docker configurado.
- [ ] Deploy em produção.
- [ ] README profissional.
- [ ] Histórico consistente de commits.
- [ ] Releases versionadas.
- [ ] Conteúdo técnico preparado para publicação.
