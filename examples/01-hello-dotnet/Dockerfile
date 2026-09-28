# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# 1. deps first: this layer stays cached until the .csproj changes
COPY HelloApi.csproj .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore

# 2. source last: changes on every commit
COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release --no-restore -o /app

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app .
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=3s --retries=3 \
  CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "HelloApi.dll"]
