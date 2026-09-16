# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# build: restaura em uma camada própria (cache enquanto os .csproj não mudam),
# compila e publica a API em Release.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY .config/dotnet-tools.json .config/
COPY src/CarrinhoCompras.Domain/CarrinhoCompras.Domain.csproj src/CarrinhoCompras.Domain/
COPY src/CarrinhoCompras.Application/CarrinhoCompras.Application.csproj src/CarrinhoCompras.Application/
COPY src/CarrinhoCompras.Infrastructure/CarrinhoCompras.Infrastructure.csproj src/CarrinhoCompras.Infrastructure/
COPY src/CarrinhoCompras.Api/CarrinhoCompras.Api.csproj src/CarrinhoCompras.Api/
RUN dotnet restore src/CarrinhoCompras.Api/CarrinhoCompras.Api.csproj \
    && dotnet tool restore

COPY src/ src/
RUN dotnet publish src/CarrinhoCompras.Api/CarrinhoCompras.Api.csproj \
    --configuration Release --no-restore --output /app/publish -p:UseAppHost=false

# ---------------------------------------------------------------------------
# bundle: empacota as migrations do EF Core em um executável único.
# ---------------------------------------------------------------------------
FROM build AS bundle
RUN dotnet ef migrations bundle \
    --project src/CarrinhoCompras.Infrastructure \
    --startup-project src/CarrinhoCompras.Api \
    --configuration Release \
    --output /app/efbundle \
    --force

# ---------------------------------------------------------------------------
# migrations: aplica as migrations (e o seed do catálogo) e termina.
# Roda antes da API no docker-compose; em produção, seria um passo do pipeline de deploy.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS migrations
WORKDIR /app
ENV DOTNET_ROOT=/usr/share/dotnet \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
COPY --from=bundle /app/efbundle ./efbundle
USER $APP_UID
ENTRYPOINT ["./efbundle"]

# ---------------------------------------------------------------------------
# api: imagem final enxuta (chiseled: sem shell nem gerenciador de pacotes) e sem root.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS api
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CarrinhoCompras.Api.dll"]
