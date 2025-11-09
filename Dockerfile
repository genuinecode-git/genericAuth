# =================================================================
# GenericAuth API - Multi-Stage Dockerfile
# Optimized for .NET 9.0 with Alpine Linux
# =================================================================

# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

# Install wget for health checks
RUN apk add --no-cache wget

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files (for layer caching)
COPY ["GenericAuth.sln", "."]
COPY ["src/GenericAuth.API/GenericAuth.API.csproj", "src/GenericAuth.API/"]
COPY ["src/GenericAuth.Application/GenericAuth.Application.csproj", "src/GenericAuth.Application/"]
COPY ["src/GenericAuth.Infrastructure/GenericAuth.Infrastructure.csproj", "src/GenericAuth.Infrastructure/"]
COPY ["src/GenericAuth.Domain/GenericAuth.Domain.csproj", "src/GenericAuth.Domain/"]

# Restore dependencies (cached layer if no project file changes)
RUN dotnet restore "GenericAuth.sln"

# Copy all source code
COPY . .

# Build the solution
WORKDIR "/src/src/GenericAuth.API"
RUN dotnet build "GenericAuth.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GenericAuth.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Copy published artifacts from publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install required globalization support
RUN apk add --no-cache icu-libs icu-data-full

# Switch to non-root user
USER appuser

# Health check (Docker-level)
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GenericAuth.API.dll"]
