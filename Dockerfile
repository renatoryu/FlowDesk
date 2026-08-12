# syntax=docker/dockerfile:1

ARG SDK_VERSION=10.0.302
ARG RUNTIME_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build

WORKDIR /src

COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY NuGet.Config ./

COPY src/FlowDesk.Domain/FlowDesk.Domain.csproj \
    src/FlowDesk.Domain/

COPY src/FlowDesk.Application/FlowDesk.Application.csproj \
    src/FlowDesk.Application/

COPY src/FlowDesk.Infrastructure/FlowDesk.Infrastructure.csproj \
    src/FlowDesk.Infrastructure/

COPY src/FlowDesk.Api/FlowDesk.Api.csproj \
    src/FlowDesk.Api/

RUN dotnet restore \
    src/FlowDesk.Api/FlowDesk.Api.csproj

COPY src/ src/

RUN dotnet publish \
    src/FlowDesk.Api/FlowDesk.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_VERSION} AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build \
    --chown=app:app \
    /app/publish ./

RUN mkdir -p /app/uploads/attachments \
    && chown -R app:app /app/uploads

USER app

HEALTHCHECK \
    --interval=10s \
    --timeout=5s \
    --start-period=10s \
    --retries=5 \
    CMD curl --fail --silent http://127.0.0.1:8080/ > /dev/null || exit 1

ENTRYPOINT ["dotnet", "FlowDesk.Api.dll"]
