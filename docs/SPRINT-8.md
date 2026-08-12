# Sprint 8 — Docker e execução em contêineres

## Objetivo

Containerizar a API do FlowDesk e o SQL Server, permitindo executar o ambiente completo de desenvolvimento com Docker Compose.

## Entregas

- Dockerfile multi-stage para a API.
- Imagem final baseada somente no runtime do ASP.NET Core.
- Execução da API com usuário não privilegiado.
- Orquestração da API e do SQL Server com Docker Compose.
- Health checks para os dois serviços.
- Volumes persistentes para o banco de dados e os anexos.
- Aplicação automática e controlada das migrations.
- Configuração por variáveis de ambiente.
- Validação automática da imagem e do Compose no GitHub Actions.

## Arquitetura dos contêineres

| Serviço | Porta local | Responsabilidade |
| --- | ---: | --- |
| `api` | `8080` | Executar a API ASP.NET Core |
| `sqlserver` | `14330` | Persistir os dados do FlowDesk |

Os serviços se comunicam por uma rede Docker privada. A API utiliza o nome `sqlserver` para acessar o banco dentro dessa rede.

## Configuração local

Copie o arquivo de exemplo:

```powershell
Copy-Item .env.example .env
```

Substitua os valores de exemplo no `.env` por segredos locais fortes.

O arquivo `.env` não deve ser enviado ao Git. Apenas o `.env.example`, sem credenciais reais, é versionado.

## Executando o ambiente

Na raiz da solução:

```powershell
docker compose up --build --detach
```

Verifique o estado:

```powershell
docker compose ps
```

Os serviços devem aparecer como `healthy`.

Acessos locais:

- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- SQL Server: `localhost,14330`

## Migrations

No ambiente Docker, a variável `Database__ApplyMigrations` habilita a aplicação automática das migrations durante a inicialização da API.

Esse comportamento é controlado por configuração e permanece desabilitado por padrão fora do Compose.

Foram validadas as migrations de usuários, refresh tokens, empresas, chamados, comentários e anexos.

## Persistência

O Compose utiliza dois volumes nomeados:

- `sqlserver-data`: armazena os dados do SQL Server.
- `attachments`: armazena os anexos enviados à API.

Os dados permanecem disponíveis após:

```powershell
docker compose down
docker compose up --detach
```

Para interromper o ambiente preservando os dados:

```powershell
docker compose down
```

> Não utilize `docker compose down --volumes` sem a intenção de apagar o banco e os anexos locais.

## Segurança da imagem

A imagem da API adota:

- build multi-stage;
- imagem final sem o SDK do .NET;
- execução com usuário `app`, sem privilégios de root;
- segredos fornecidos somente em tempo de execução;
- arquivos locais e sensíveis excluídos do contexto pelo `.dockerignore`;
- health check da API.

## Integração contínua

O GitHub Actions valida automaticamente:

- a configuração do Docker Compose;
- a construção da imagem da API;
- a execução da imagem com usuário não privilegiado;
- a ausência do SDK do .NET na imagem final.

## Validações realizadas

- API e SQL Server iniciados como contêineres saudáveis.
- Banco criado e atualizado pelas migrations.
- Cadastro e login executados com sucesso.
- Dados do banco preservados após reinicialização.
- Anexos preservados após reinicialização.
- API executada com usuário não privilegiado.
- Swagger acessível pelo contêiner.
- Solução compilada sem erros ou avisos.
- 244 testes unitários aprovados.

## Resultado

O FlowDesk pode ser executado localmente de forma reproduzível com um único comando, mantendo banco e anexos persistentes e sem armazenar segredos no repositório.