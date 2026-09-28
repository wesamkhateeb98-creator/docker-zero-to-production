# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY src/Api/Api.csproj src/Api/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/Api/Api.csproj
COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/Api/Api.csproj -c Release --no-restore -o /app

# ---------- migrations bundle (one self-contained executable) ----------
FROM build AS migrator-build
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet tool restore \
 && dotnet ef migrations bundle -p src/Api -r linux-musl-x64 --self-contained -o /efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS migrator
COPY --from=migrator-build /efbundle /efbundle
USER $APP_UID
ENTRYPOINT ["/efbundle"]

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
# Folder for the Data Protection key volume, owned by the non-root user
RUN mkdir -p /app/keys && chown $APP_UID:$APP_UID /app/keys
COPY --from=build --chown=$APP_UID:$APP_UID /app .
ARG APP_VERSION=dev
ENV APP_VERSION=$APP_VERSION \
    DataProtection__KeysPath=/app/keys
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Api.dll"]
