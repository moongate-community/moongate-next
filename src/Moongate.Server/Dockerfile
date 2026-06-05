# syntax=docker/dockerfile:1.7

# ---------------------------------------------------------------------------
# Web build stage
# ---------------------------------------------------------------------------
FROM node:22-alpine AS web-build
WORKDIR /web

COPY ["web/package.json", "web/package-lock.json", "./"]
RUN npm ci

COPY web/ ./
RUN npm run build

# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy source before restore so every project referenced by the server project
# is present in the Docker context.
COPY ["Directory.Build.props", "global.json", "./"]
COPY src/ src/
RUN dotnet restore "src/Moongate.Server/Moongate.Server.csproj"

RUN dotnet publish "src/Moongate.Server/Moongate.Server.csproj" \
        -c "$BUILD_CONFIGURATION" \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# The server writes logs, plugins, scripts and save state under MOONGATE_ROOT.
# Defaulting to /data keeps runtime state outside /app so the container image
# stays immutable and a single volume mount preserves everything.
ENV MOONGATE_ROOT=/data \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true

WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web-build /web/dist ./wwwroot

# Pre-create the data dir owned by the non-root APP_UID baked into the
# aspnet base image, then mark it as a persistent volume.
RUN mkdir -p /data && chown -R $APP_UID:$APP_UID /data
VOLUME ["/data"]

USER $APP_UID
EXPOSE 8080
EXPOSE 2593/tcp
EXPOSE 12000/udp

ENTRYPOINT ["dotnet", "Moongate.Server.dll"]
