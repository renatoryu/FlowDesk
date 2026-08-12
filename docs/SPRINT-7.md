# Sprint 7 — Testes automatizados e cobertura

## Visão geral

A Sprint 7 consolidou a estratégia de qualidade do FlowDesk por meio de testes automatizados, medição reproduzível de cobertura e integração contínua no GitHub Actions.

A suíte existente foi auditada para confirmar a cobertura das regras de domínio e dos principais casos de uso. Não foram adicionados testes artificiais apenas para elevar percentuais.

## Funcionalidades entregues

- Execução automatizada de build, formatação e testes.
- Build em modo Release com warnings tratados como erros.
- Geração de cobertura com Coverlet.
- Relatório HTML, Cobertura XML e resumo textual.
- Ferramenta ReportGenerator fixada no repositório.
- Publicação dos resultados de testes como artifact.
- Publicação do relatório de cobertura como artifact.
- Resumo da cobertura na execução do GitHub Actions.
- Cancelamento de execuções antigas da mesma branch.
- Pipeline disponível para push, pull request e execução manual.

## Estratégia de testes

A suíte possui testes para:

- invariantes e transições das entidades de domínio;
- validators da camada Application;
- autenticação e renovação de sessão;
- gerenciamento de empresas;
- criação, consulta, atualização, status e exclusão de chamados;
- autorização e isolamento entre Customers;
- comentários e histórico;
- dashboard e indicadores;
- upload, listagem e download de anexos;
- rollback do armazenamento quando a persistência falha.

## Cobertura oficial

A medição considera as camadas `FlowDesk.Domain` e `FlowDesk.Application`, onde estão concentradas as regras de negócio e os casos de uso.

| Escopo | Linhas | Branches |
|---|---:|---:|
| Total | 84,18% | 78,13% |
| FlowDesk.Domain | 90,80% | 89,01% |
| FlowDesk.Application | 81,46% | 74,15% |

A linha de base foi gerada com 244 testes aprovados.

Controllers, infraestrutura, migrations, código gerado e propriedades automáticas não fazem parte dessa métrica unitária. Essas áreas poderão receber testes de integração em uma etapa futura.

## Configuração de cobertura

O arquivo `coverage.runsettings` define:

- assemblies incluídos e excluídos;
- exclusão de código gerado;
- exclusão de migrations;
- cobertura no formato Cobertura;
- remoção de propriedades automáticas da métrica.

Comando para gerar a cobertura:

```powershell
dotnet test FlowDesk.slnx `
  --no-restore `
  --settings coverage.runsettings `
  --collect:"XPlat Code Coverage" `
  --results-directory TestResults
```

## Relatório local

Restaurar a ferramenta:

```powershell
dotnet tool restore
```

Gerar o relatório:

```powershell
dotnet tool run reportgenerator `
  -reports:"TestResults/**/coverage.cobertura.xml" `
  -targetdir:"artifacts/coverage-report" `
  -reporttypes:"Html;Cobertura;TextSummary"
```

O relatório HTML é criado em:

```text
artifacts/coverage-report/index.html
```

As pastas `TestResults` e `artifacts` são ignoradas pelo Git.

## Integração contínua

O workflow `.github/workflows/ci.yml` executa:

1. checkout do repositório;
2. instalação do SDK definido em `global.json`;
3. restauração das dependências e ferramentas;
4. verificação de formatação;
5. build Release com warnings como erros;
6. execução dos testes com cobertura;
7. geração do relatório;
8. publicação dos artifacts.

O pipeline é disparado em:

- push para `main`;
- push para `develop`;
- pull request para `main` ou `develop`;
- execução manual.

Falhas de formatação, compilação ou testes interrompem o pipeline.

## Artifacts

Cada execução mantém por 14 dias:

- resultados dos testes em TRX;
- cobertura no formato Cobertura XML;
- relatório HTML completo;
- resumo textual da cobertura.

## Validação realizada

- Solução compilada em Release com 0 erros e 0 avisos.
- 244 testes unitários aprovados.
- Formatação verificada sem alterações.
- Cobertura de linhas de 84,18%.
- Cobertura de branches de 78,13%.
- Relatório HTML gerado com sucesso.
- Arquivos temporários ignorados pelo Git.
- Pipeline reproduzível configurado.

## Rastreabilidade

- [Issue #23 — Sprint 7: automação de testes e cobertura](https://github.com/renatoryu/FlowDesk/issues/23)

## Próxima etapa

Iniciar a Sprint 8 com Dockerfile, Docker Compose e SQL Server executado em contêineres.