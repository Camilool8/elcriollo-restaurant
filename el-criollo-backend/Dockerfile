FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY src/ElCriollo.API/*.csproj ./src/ElCriollo.API/
COPY src/ElCriollo.API/NuGet.config ./src/ElCriollo.API/
RUN dotnet restore src/ElCriollo.API/ElCriollo.Api.csproj --runtime linux-x64

COPY src/ElCriollo.API/ ./src/ElCriollo.API/

WORKDIR /app/src/ElCriollo.API
RUN dotnet publish -c Release -o /app/publish --no-restore --runtime linux-x64 --self-contained false

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN groupadd -r elcriollo && useradd -r -g elcriollo elcriollo

RUN apt-get update && apt-get install -y \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build-env /app/publish .

RUN mkdir -p /app/logs /app/logs/emails && \
    chown -R elcriollo:elcriollo /app

USER elcriollo

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ElCriollo.Api.dll"] 