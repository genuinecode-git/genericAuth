# GenericAuth Docker Containerization Plan

## Executive Summary

This document provides a comprehensive plan for containerizing the GenericAuth .NET 9.0 application with support for dynamic horizontal scaling. The architecture enables running multiple instances of authentication pods/containers with configuration-driven scaling, load balancing, and proper state management.

---

## Table of Contents

1. [Application Analysis](#1-application-analysis)
2. [Docker Containerization Strategy](#2-docker-containerization-strategy)
3. [Dynamic Pod Scaling Architecture](#3-dynamic-pod-scaling-architecture)
4. [Database and Infrastructure Considerations](#4-database-and-infrastructure-considerations)
5. [Best Practices Implementation](#5-best-practices-implementation)
6. [Implementation Roadmap](#6-implementation-roadmap)
7. [Appendix: Configuration Files](#7-appendix-configuration-files)

---

## 1. Application Analysis

### 1.1 Current Application Structure

**Technology Stack:**
- .NET 9.0 (Latest version)
- Clean Architecture with 4 layers
- EF Core 9.0 with SQLite (Commands/Writes)
- Dapper (Queries/Reads)
- MediatR for CQRS
- JWT Bearer Authentication
- Multi-tenant architecture with application-scoped roles

**Project Structure:**
```
GenericAuth/
├── src/
│   ├── GenericAuth.Domain/          # Core domain logic (no dependencies)
│   ├── GenericAuth.Application/     # CQRS, MediatR, FluentValidation
│   ├── GenericAuth.Infrastructure/  # EF Core, Dapper, JWT, Identity
│   └── GenericAuth.API/             # Controllers, Middleware, Swagger
└── tests/                           # Unit & Integration Tests
```

### 1.2 Key Dependencies

**API Layer (GenericAuth.API.csproj):**
- Microsoft.AspNetCore.OpenApi 9.0.1
- Asp.Versioning.Mvc 8.0.0
- Swashbuckle.AspNetCore 9.0.6
- Microsoft.EntityFrameworkCore.Design 9.0.10

**Infrastructure Layer:**
- Microsoft.EntityFrameworkCore.Sqlite 9.0.10
- Microsoft.EntityFrameworkCore.SqlServer 9.0.10 (for production)
- Dapper 2.1.66
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.10
- System.IdentityModel.Tokens.Jwt 8.14.0

### 1.3 Current Configuration Analysis

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=GenericAuth.db"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "GenericAuth",
    "Audience": "GenericAuthClient",
    "ExpirationInMinutes": "60"
  }
}
```

### 1.4 Stateful vs Stateless Analysis

**Stateless Components (Container-Ready):**
- JWT Authentication (token-based, no server-side sessions)
- API Controllers (request/response only)
- MediatR Handlers (process and return)
- Application middleware

**Stateful Components (Require Consideration):**
- SQLite database file (GenericAuth.db) - needs migration to shared storage
- Database migrations (run once, not per instance)
- Database seeding (Program.cs lines 142-155) - needs coordination

**Critical Finding:** The application uses SQLite with a local file (`GenericAuth.db`). For multi-instance deployment, this must be migrated to a centralized database (SQL Server, PostgreSQL, or MySQL).

---

## 2. Docker Containerization Strategy

### 2.1 Multi-Stage Dockerfile Design

**Optimized Dockerfile with .NET 9.0:**

```dockerfile
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files
COPY ["GenericAuth.sln", "."]
COPY ["src/GenericAuth.API/GenericAuth.API.csproj", "src/GenericAuth.API/"]
COPY ["src/GenericAuth.Application/GenericAuth.Application.csproj", "src/GenericAuth.Application/"]
COPY ["src/GenericAuth.Infrastructure/GenericAuth.Infrastructure.csproj", "src/GenericAuth.Infrastructure/"]
COPY ["src/GenericAuth.Domain/GenericAuth.Domain.csproj", "src/GenericAuth.Domain/"]

# Restore dependencies (cached layer)
RUN dotnet restore "GenericAuth.sln"

# Copy source code
COPY . .

# Build solution
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

# Copy published artifacts
COPY --from=publish --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GenericAuth.API.dll"]
```

**Key Features:**
- Multi-stage build reduces final image size by ~80%
- Alpine-based images for minimal footprint (~100MB vs ~500MB)
- Non-root user for security compliance
- Cached layer optimization for faster rebuilds
- Integrated health check for container orchestration
- .NET 9.0 compatibility

### 2.2 .dockerignore Configuration

```dockerignore
# Build artifacts
**/bin/
**/obj/
**/out/
**/.vs/
**/.vscode/

# Test artifacts
**/TestResults/
**/coverage/
**/*.db
**/*.db-shm
**/*.db-wal

# Git and CI/CD
.git/
.gitignore
.github/
**/.DS_Store

# Documentation and configs
**/*.md
!README.md
.editorconfig

# Node modules (if any)
**/node_modules/

# Temporary files
**/tmp/
**/logs/
**/*.log
```

### 2.3 Docker Compose for Local Development

**docker-compose.yml:**

```yaml
version: '3.8'

services:
  # PostgreSQL Database (Shared across instances)
  postgres:
    image: postgres:16-alpine
    container_name: genericauth-postgres
    environment:
      POSTGRES_DB: genericauth
      POSTGRES_USER: ${DB_USER:-genericauth}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-Dev@Pass123}
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U genericauth"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - genericauth-network

  # Database Migration Service (Runs once)
  migration:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: genericauth-migration
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=genericauth;Username=${DB_USER:-genericauth};Password=${DB_PASSWORD:-Dev@Pass123}
      - RUN_MIGRATIONS_ONLY=true
    command: ["dotnet", "ef", "database", "update", "--project", "/app/GenericAuth.API.dll"]
    networks:
      - genericauth-network

  # GenericAuth API Instances
  api:
    build:
      context: .
      dockerfile: Dockerfile
    depends_on:
      postgres:
        condition: service_healthy
      migration:
        condition: service_completed_successfully
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ASPNETCORE_HTTP_PORTS=8080
      - ASPNETCORE_HTTPS_PORTS=8081
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=genericauth;Username=${DB_USER:-genericauth};Password=${DB_PASSWORD:-Dev@Pass123}
      - JwtSettings__Secret=${JWT_SECRET:-YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!}
      - JwtSettings__Issuer=GenericAuth
      - JwtSettings__Audience=GenericAuthClient
      - JwtSettings__ExpirationInMinutes=60
      - Logging__LogLevel__Default=Information
    healthcheck:
      test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    networks:
      - genericauth-network
    deploy:
      replicas: ${INSTANCE_COUNT:-3}  # Configuration-driven scaling
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M

  # NGINX Load Balancer
  nginx:
    image: nginx:alpine
    container_name: genericauth-nginx
    depends_on:
      - api
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    healthcheck:
      test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - genericauth-network

  # Redis for Distributed Caching (Optional but recommended)
  redis:
    image: redis:7-alpine
    container_name: genericauth-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - genericauth-network

  # Seq for Centralized Logging
  seq:
    image: datalust/seq:latest
    container_name: genericauth-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seq-data:/data
    networks:
      - genericauth-network

volumes:
  postgres-data:
    driver: local
  redis-data:
    driver: local
  seq-data:
    driver: local

networks:
  genericauth-network:
    driver: bridge
```

### 2.4 Environment Configuration Strategy

**appsettings.Docker.json** (Container-specific overrides):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "GenericAuth": "Information"
    },
    "Seq": {
      "ServerUrl": "http://seq:5341",
      "ApiKey": "",
      "MinimumLevel": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=genericauth;Username=genericauth;Password=PLACEHOLDER"
  },
  "JwtSettings": {
    "Secret": "PLACEHOLDER",
    "Issuer": "GenericAuth",
    "Audience": "GenericAuthClient",
    "ExpirationInMinutes": "60"
  },
  "Redis": {
    "Configuration": "redis:6379",
    "InstanceName": "GenericAuth:"
  },
  "HealthChecks": {
    "Enabled": true,
    "Database": {
      "Enabled": true,
      "Timeout": 5
    },
    "Redis": {
      "Enabled": true,
      "Timeout": 3
    }
  },
  "Scaling": {
    "InstanceCount": 3,
    "MaxConcurrentRequests": 100,
    "RequestTimeout": 30
  }
}
```

**.env file** (Local development secrets):

```bash
# Database Configuration
DB_USER=genericauth
DB_PASSWORD=Dev@Pass123
POSTGRES_DB=genericauth

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
JWT_SECRET=YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!

# Scaling Configuration
INSTANCE_COUNT=3

# Redis Configuration
REDIS_PASSWORD=

# Seq Configuration
SEQ_API_KEY=
```

---

## 3. Dynamic Pod Scaling Architecture

### 3.1 Configuration-Driven Scaling Approaches

#### Option 1: Docker Compose Scale (Simple - Local Development)

**Pros:**
- Simple to implement
- Good for local development and testing
- No additional infrastructure required
- Easy to understand

**Cons:**
- Limited production capabilities
- Manual scaling only
- No auto-scaling based on load
- Limited health monitoring

**Implementation:**

```bash
# Scale to 5 instances
docker-compose up --scale api=5 -d

# Scale from environment variable
INSTANCE_COUNT=5 docker-compose up -d
```

**Configuration in appsettings:**
```json
{
  "Scaling": {
    "DesiredInstanceCount": 5,
    "Note": "Set INSTANCE_COUNT environment variable for Docker Compose"
  }
}
```

#### Option 2: Docker Swarm (Intermediate - Production Ready)

**Pros:**
- Built into Docker (no additional tools)
- Native Docker scaling
- Service discovery
- Load balancing
- Rolling updates
- Health-based orchestration

**Cons:**
- Less feature-rich than Kubernetes
- Smaller ecosystem
- Limited advanced features

**Implementation:**

**docker-stack.yml:**

```yaml
version: '3.8'

services:
  api:
    image: genericauth-api:latest
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
      rollback_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
    networks:
      - overlay-network

networks:
  overlay-network:
    driver: overlay
    attachable: true
```

**Deployment Commands:**

```bash
# Initialize Swarm
docker swarm init

# Deploy stack with 3 replicas
docker stack deploy -c docker-stack.yml genericauth

# Scale to 5 replicas
docker service scale genericauth_api=5

# From environment variable
REPLICAS=5 docker service scale genericauth_api=$REPLICAS
```

#### Option 3: Kubernetes (Advanced - Enterprise Production)

**Pros:**
- Industry standard orchestration
- Auto-scaling (HPA, VPA, Cluster Autoscaler)
- Advanced networking and service mesh
- Rich ecosystem
- Cloud-native integration (AKS, EKS, GKE)
- Self-healing and automated rollouts

**Cons:**
- Complex to set up and manage
- Steeper learning curve
- Requires infrastructure investment

**Implementation:**

**kubernetes/deployment.yaml:**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: genericauth

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: genericauth-config
  namespace: genericauth
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  JwtSettings__Issuer: "GenericAuth"
  JwtSettings__Audience: "GenericAuthClient"
  JwtSettings__ExpirationInMinutes: "60"
  # Instance count is managed by HPA, not ConfigMap
  Scaling__MaxConcurrentRequests: "100"

---
apiVersion: v1
kind: Secret
metadata:
  name: genericauth-secrets
  namespace: genericauth
type: Opaque
stringData:
  DB_CONNECTION_STRING: "Host=postgres;Database=genericauth;Username=genericauth;Password=SecurePassword123!"
  JWT_SECRET: "YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!"

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: genericauth-api
  namespace: genericauth
  labels:
    app: genericauth-api
spec:
  replicas: 3  # Initial replica count (overridden by HPA)
  selector:
    matchLabels:
      app: genericauth-api
  template:
    metadata:
      labels:
        app: genericauth-api
    spec:
      containers:
      - name: api
        image: genericauth-api:latest
        ports:
        - containerPort: 8080
          name: http
        - containerPort: 8081
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: genericauth-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: genericauth-secrets
              key: DB_CONNECTION_STRING
        - name: JwtSettings__Secret
          valueFrom:
            secretKeyRef:
              name: genericauth-secrets
              key: JWT_SECRET
        - name: JwtSettings__Issuer
          valueFrom:
            configMapKeyRef:
              name: genericauth-config
              key: JwtSettings__Issuer
        - name: POD_NAME
          valueFrom:
            fieldRef:
              fieldPath: metadata.name
        - name: POD_IP
          valueFrom:
            fieldRef:
              fieldPath: status.podIP
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        startupProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 0
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 30

---
apiVersion: v1
kind: Service
metadata:
  name: genericauth-api-service
  namespace: genericauth
spec:
  selector:
    app: genericauth-api
  ports:
  - name: http
    protocol: TCP
    port: 80
    targetPort: 8080
  - name: https
    protocol: TCP
    port: 443
    targetPort: 8081
  type: LoadBalancer

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: genericauth-api-hpa
  namespace: genericauth
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: genericauth-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 30
      - type: Pods
        value: 2
        periodSeconds: 30
      selectPolicy: Max
```

**Custom Metrics HPA (Configuration-Driven):**

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: genericauth-api-hpa-custom
  namespace: genericauth
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: genericauth-api
  minReplicas: 2
  maxReplicas: 20
  metrics:
  - type: External
    external:
      metric:
        name: appsettings_desired_instance_count
      target:
        type: Value
        value: "5"  # Read from ConfigMap or external source
```

### 3.2 Load Balancing Strategy

#### NGINX Configuration

**nginx/nginx.conf:**

```nginx
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
    use epoll;
    multi_accept on;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Logging format with instance tracking
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for" '
                    'upstream: $upstream_addr';

    access_log /var/log/nginx/access.log main;

    # Performance optimizations
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    client_max_body_size 20M;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css text/xml text/javascript
               application/json application/javascript application/xml+rss
               application/rss+xml font/truetype font/opentype
               application/vnd.ms-fontobject image/svg+xml;

    # Upstream API instances (Docker Compose service discovery)
    upstream genericauth_api {
        least_conn;  # Least connections load balancing

        # Docker Compose DNS resolution
        server api:8080 max_fails=3 fail_timeout=30s;

        # Keepalive connections
        keepalive 32;
    }

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/s;
    limit_conn_zone $binary_remote_addr zone=addr:10m;

    server {
        listen 80;
        server_name localhost;

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Referrer-Policy "no-referrer-when-downgrade" always;

        # Health check endpoint (direct)
        location /health {
            access_log off;
            proxy_pass http://genericauth_api/health;
            proxy_set_header Host $host;
        }

        # API endpoints
        location /api/ {
            # Rate limiting
            limit_req zone=api_limit burst=20 nodelay;
            limit_conn addr 10;

            # Proxy settings
            proxy_pass http://genericauth_api;
            proxy_http_version 1.1;

            # Headers
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header Connection "";

            # Timeouts
            proxy_connect_timeout 5s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;

            # Buffering
            proxy_buffering on;
            proxy_buffer_size 4k;
            proxy_buffers 8 4k;
            proxy_busy_buffers_size 8k;
        }

        # Swagger UI
        location / {
            proxy_pass http://genericauth_api;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    # HTTPS server (optional)
    server {
        listen 443 ssl http2;
        server_name localhost;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;

        location / {
            proxy_pass http://genericauth_api;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
```

### 3.3 Session Management and State Considerations

Since the application uses JWT tokens, it's inherently stateless. However, for optimal multi-instance deployment:

**Required Changes to Application:**

1. **Distributed Caching for Token Blacklisting** (if implementing logout)
2. **Shared Data Protection Keys** (for cookie encryption across instances)
3. **Centralized Logging** (aggregate logs from all instances)

**Implementation in Program.cs additions:**

```csharp
// Add distributed caching (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "GenericAuth:";
});

// Configure Data Protection for multi-instance deployment
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")),
        "GenericAuth:DataProtection-Keys")
    .SetApplicationName("GenericAuth");

// Add distributed session (if needed)
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".GenericAuth.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Serilog for structured logging to Seq
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "GenericAuth")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .Enrich.WithProperty("InstanceId", Environment.GetEnvironmentVariable("HOSTNAME") ?? "local")
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Logging:Seq:ServerUrl"] ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## 4. Database and Infrastructure Considerations

### 4.1 Database Migration from SQLite to PostgreSQL

**Current State:** SQLite (single-file, not suitable for multi-instance)

**Target State:** PostgreSQL (centralized, connection pooling, ACID compliant)

**Migration Steps:**

1. **Update ConnectionStrings:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=genericauth;Username=genericauth;Password=SecurePassword123!;Pooling=true;MinPoolSize=5;MaxPoolSize=20;ConnectionLifetime=300;"
  }
}
```

2. **Update Infrastructure DependencyInjection.cs:**

```csharp
// Change from SQLite to PostgreSQL
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        b => {
            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            b.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));
```

3. **Add Npgsql Package:**

```bash
cd src/GenericAuth.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
```

4. **Generate New Migrations:**

```bash
# Remove old SQLite migrations
rm -rf src/GenericAuth.Infrastructure/Migrations/*

# Add new PostgreSQL migration
dotnet ef migrations add InitialPostgreSQL \
  --project src/GenericAuth.Infrastructure \
  --startup-project src/GenericAuth.API \
  --context ApplicationDbContext
```

### 4.2 Connection Pooling Configuration

**Best Practices for Multi-Instance Deployment:**

**Connection String Configuration:**

```
Host=postgres;
Database=genericauth;
Username=genericauth;
Password=SecurePassword123!;
Pooling=true;
MinPoolSize=5;
MaxPoolSize=20;
ConnectionLifetime=300;
ConnectionIdleLifetime=60;
CommandTimeout=30;
```

**Calculation for Connection Pool Sizing:**

```
Total DB Connections = (Instance Count × MaxPoolSize) + Buffer
Example: (10 instances × 20 connections) + 50 buffer = 250 connections

PostgreSQL max_connections should be set to at least 250-300
```

**PostgreSQL Configuration (docker-compose.yml):**

```yaml
postgres:
  image: postgres:16-alpine
  command:
    - "postgres"
    - "-c"
    - "max_connections=300"
    - "-c"
    - "shared_buffers=256MB"
    - "-c"
    - "effective_cache_size=1GB"
    - "-c"
    - "maintenance_work_mem=64MB"
    - "-c"
    - "checkpoint_completion_target=0.9"
    - "-c"
    - "wal_buffers=16MB"
    - "-c"
    - "default_statistics_target=100"
    - "-c"
    - "random_page_cost=1.1"
    - "-c"
    - "effective_io_concurrency=200"
    - "-c"
    - "work_mem=1310kB"
    - "-c"
    - "min_wal_size=1GB"
    - "-c"
    - "max_wal_size=4GB"
```

### 4.3 Database Migration Strategy

**Challenge:** Database seeding runs in Program.cs for every instance, causing race conditions.

**Solution: Init Container Pattern**

**docker-compose.yml migration service:**

```yaml
migration:
  build:
    context: .
    dockerfile: Dockerfile.Migration
  container_name: genericauth-migration
  depends_on:
    postgres:
      condition: service_healthy
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Host=postgres;Database=genericauth;Username=genericauth;Password=Dev@Pass123
  networks:
    - genericauth-network
```

**Dockerfile.Migration:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy and restore
COPY ["GenericAuth.sln", "."]
COPY ["src/GenericAuth.API/GenericAuth.API.csproj", "src/GenericAuth.API/"]
COPY ["src/GenericAuth.Application/GenericAuth.Application.csproj", "src/GenericAuth.Application/"]
COPY ["src/GenericAuth.Infrastructure/GenericAuth.Infrastructure.csproj", "src/GenericAuth.Infrastructure/"]
COPY ["src/GenericAuth.Domain/GenericAuth.Domain.csproj", "src/GenericAuth.Domain/"]
RUN dotnet restore

# Copy source
COPY . .

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set working directory
WORKDIR /src/src/GenericAuth.API

# Entry point runs migrations and seeding
ENTRYPOINT ["sh", "-c", "dotnet ef database update && dotnet run --no-build -- --seed-only"]
```

**Program.cs modification:**

```csharp
var app = builder.Build();

// Only seed if --seed-only argument is present OR if not running in Docker
var seedOnly = args.Contains("--seed-only");
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (seedOnly || !isDocker)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var seeder = services.GetRequiredService<GenericAuth.Infrastructure.Persistence.DatabaseSeeder>();
            await seeder.SeedAsync();

            if (seedOnly)
            {
                Console.WriteLine("Database seeding completed. Exiting...");
                return; // Exit after seeding
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
```

### 4.4 Shared Resources Management

**Redis Configuration for Distributed Caching:**

```csharp
// IDistributedCache for token blacklisting (logout)
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public async Task BlacklistTokenAsync(string tokenJti, DateTime expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };
        await _cache.SetStringAsync($"blacklist:{tokenJti}", "true", options);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string tokenJti)
    {
        var value = await _cache.GetStringAsync($"blacklist:{tokenJti}");
        return value != null;
    }
}
```

---

## 5. Best Practices Implementation

### 5.1 Enhanced Health Checks

**Update Program.cs:**

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "postgresql" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "cache", "redis" })
    .AddUrlGroup(
        new Uri("http://seq:5341/api/healthcheck"),
        name: "seq",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "logging", "seq" });

// Map health check endpoints with details
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

**Required Package:**

```bash
dotnet add package AspNetCore.HealthChecks.NpgSql
dotnet add package AspNetCore.HealthChecks.Redis
dotnet add package AspNetCore.HealthChecks.Uris
dotnet add package AspNetCore.HealthChecks.UI.Client
```

### 5.2 Structured Logging with Serilog

**appsettings.Docker.json:**

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341",
          "apiKey": "",
          "compact": true
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithEnvironmentName"
    ],
    "Properties": {
      "Application": "GenericAuth",
      "Environment": "Docker"
    }
  }
}
```

**Required Packages:**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
```

### 5.3 Secrets Management

#### Development: Docker Secrets

**docker-compose.yml with secrets:**

```yaml
version: '3.8'

secrets:
  db_password:
    file: ./secrets/db_password.txt
  jwt_secret:
    file: ./secrets/jwt_secret.txt

services:
  api:
    secrets:
      - db_password
      - jwt_secret
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=genericauth;Username=genericauth;Password_File=/run/secrets/db_password
      - JwtSettings__Secret_File=/run/secrets/jwt_secret
```

**Reading secrets in code:**

```csharp
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtSecretFile = builder.Configuration["JwtSettings:Secret_File"];

if (!string.IsNullOrEmpty(jwtSecretFile) && File.Exists(jwtSecretFile))
{
    jwtSecret = await File.ReadAllTextAsync(jwtSecretFile);
}
```

#### Production: Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: genericauth-secrets
  namespace: genericauth
type: Opaque
stringData:
  db-connection: "Host=postgres;Database=genericauth;Username=genericauth;Password=SecurePassword123!"
  jwt-secret: "YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!"
```

#### Production: Azure Key Vault / AWS Secrets Manager

```csharp
// Azure Key Vault integration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// AWS Secrets Manager integration
builder.Configuration.AddSecretsManager(region: RegionEndpoint.USEast1);
```

### 5.4 Performance Optimization

#### Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
```

#### Output Caching (ASP.NET Core 9.0)

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(1)));
    options.AddPolicy("ApiCache", builder =>
        builder.Expire(TimeSpan.FromMinutes(5))
               .Tag("api"));
});

app.UseOutputCache();

// In controllers
[OutputCache(PolicyName = "ApiCache")]
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    // ...
}
```

#### HTTP/2 and HTTP/3 Support

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});
```

### 5.5 Monitoring and Observability

#### OpenTelemetry Integration

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("GenericAuth")
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "jaeger";
                options.AgentPort = 6831;
            });
    });

app.UseOpenTelemetryPrometheusScrapingEndpoint();
```

**Required Packages:**

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.Runtime
dotnet add package OpenTelemetry.Instrumentation.Process
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
dotnet add package OpenTelemetry.Exporter.Jaeger
```

**Prometheus & Grafana in docker-compose.yml:**

```yaml
prometheus:
  image: prom/prometheus:latest
  container_name: genericauth-prometheus
  volumes:
    - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
    - prometheus-data:/prometheus
  ports:
    - "9090:9090"
  networks:
    - genericauth-network

grafana:
  image: grafana/grafana:latest
  container_name: genericauth-grafana
  ports:
    - "3000:3000"
  environment:
    - GF_SECURITY_ADMIN_PASSWORD=admin
  volumes:
    - grafana-data:/var/lib/grafana
    - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
    - ./grafana/datasources:/etc/grafana/provisioning/datasources
  networks:
    - genericauth-network
```

---

## 6. Implementation Roadmap

### Phase 1: Foundation (Week 1)

**Day 1-2: Database Migration**
- [ ] Add Npgsql.EntityFrameworkCore.PostgreSQL package
- [ ] Update DependencyInjection.cs to use PostgreSQL
- [ ] Generate new migrations for PostgreSQL
- [ ] Test local PostgreSQL connection
- [ ] Update connection string configuration

**Day 3-4: Docker Basics**
- [ ] Create Dockerfile with multi-stage build
- [ ] Create .dockerignore file
- [ ] Build and test Docker image locally
- [ ] Create basic docker-compose.yml with PostgreSQL
- [ ] Test single-instance deployment

**Day 5-7: Multi-Instance Setup**
- [ ] Configure NGINX load balancer
- [ ] Update docker-compose.yml for multiple replicas
- [ ] Implement database migration init container
- [ ] Remove seeding from API startup for multi-instance
- [ ] Test multi-instance deployment locally

### Phase 2: State Management (Week 2)

**Day 1-2: Distributed Caching**
- [ ] Add Redis service to docker-compose.yml
- [ ] Configure StackExchangeRedisCache in Program.cs
- [ ] Implement token blacklist service
- [ ] Configure Data Protection with Redis
- [ ] Test cache across multiple instances

**Day 3-4: Centralized Logging**
- [ ] Add Seq service to docker-compose.yml
- [ ] Configure Serilog with Seq sink
- [ ] Add structured logging to controllers
- [ ] Add correlation IDs for request tracking
- [ ] Test log aggregation across instances

**Day 5-7: Health Checks**
- [ ] Add comprehensive health check packages
- [ ] Implement database health check
- [ ] Implement Redis health check
- [ ] Configure liveness and readiness probes
- [ ] Test health endpoints

### Phase 3: Configuration & Scaling (Week 3)

**Day 1-3: Environment Configuration**
- [ ] Create appsettings.Docker.json
- [ ] Configure environment variable overrides
- [ ] Implement secrets management
- [ ] Create .env template file
- [ ] Document configuration options

**Day 4-5: Docker Compose Scaling**
- [ ] Implement INSTANCE_COUNT environment variable
- [ ] Test scaling from docker-compose
- [ ] Create scaling documentation
- [ ] Test load distribution

**Day 6-7: Docker Swarm Preparation**
- [ ] Create docker-stack.yml
- [ ] Configure service replicas
- [ ] Implement rolling update strategy
- [ ] Test Swarm deployment locally

### Phase 4: Kubernetes (Week 4)

**Day 1-2: Kubernetes Manifests**
- [ ] Create Deployment manifest
- [ ] Create Service manifest
- [ ] Create ConfigMap and Secrets
- [ ] Create Namespace

**Day 3-4: Auto-Scaling**
- [ ] Configure HorizontalPodAutoscaler
- [ ] Set resource requests/limits
- [ ] Configure scaling policies
- [ ] Test HPA behavior

**Day 5-7: Production Readiness**
- [ ] Implement Ingress controller
- [ ] Configure TLS/SSL certificates
- [ ] Set up persistent volumes for PostgreSQL
- [ ] Test full Kubernetes deployment

### Phase 5: Monitoring & Optimization (Week 5)

**Day 1-2: OpenTelemetry**
- [ ] Add OpenTelemetry packages
- [ ] Configure metrics collection
- [ ] Configure distributed tracing
- [ ] Integrate with Jaeger

**Day 3-4: Prometheus & Grafana**
- [ ] Add Prometheus to docker-compose
- [ ] Add Grafana to docker-compose
- [ ] Create custom dashboards
- [ ] Configure alerts

**Day 5-7: Performance Tuning**
- [ ] Implement response compression
- [ ] Configure output caching
- [ ] Optimize database connection pooling
- [ ] Load testing and benchmarking

### Phase 6: Documentation & Deployment (Week 6)

**Day 1-2: Documentation**
- [ ] Update README.md with Docker instructions
- [ ] Create deployment guide
- [ ] Document configuration options
- [ ] Create troubleshooting guide

**Day 3-4: CI/CD Integration**
- [ ] Update GitHub Actions for Docker builds
- [ ] Add Docker image publishing
- [ ] Configure automated deployment
- [ ] Test CI/CD pipeline

**Day 5-7: Production Deployment**
- [ ] Deploy to staging environment
- [ ] Perform load testing
- [ ] Security audit
- [ ] Production deployment

---

## 7. Appendix: Configuration Files

### A. Complete .env Template

```bash
# =================================================================
# GenericAuth Environment Configuration
# =================================================================

# Environment
ASPNETCORE_ENVIRONMENT=Development

# =================================================================
# Database Configuration
# =================================================================
DB_HOST=postgres
DB_PORT=5432
DB_NAME=genericauth
DB_USER=genericauth
DB_PASSWORD=Dev@Pass123
DB_CONNECTION_STRING=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Pooling=true;MinPoolSize=5;MaxPoolSize=20;

# =================================================================
# JWT Configuration
# =================================================================
JWT_SECRET=YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!
JWT_ISSUER=GenericAuth
JWT_AUDIENCE=GenericAuthClient
JWT_EXPIRATION_MINUTES=60

# =================================================================
# Redis Configuration
# =================================================================
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=
REDIS_CONNECTION_STRING=${REDIS_HOST}:${REDIS_PORT},password=${REDIS_PASSWORD}

# =================================================================
# Scaling Configuration
# =================================================================
INSTANCE_COUNT=3
MIN_REPLICAS=2
MAX_REPLICAS=10
CPU_THRESHOLD=70
MEMORY_THRESHOLD=80

# =================================================================
# Logging Configuration
# =================================================================
SEQ_URL=http://seq:5341
SEQ_API_KEY=

# =================================================================
# Health Check Configuration
# =================================================================
HEALTH_CHECK_ENABLED=true
HEALTH_CHECK_INTERVAL=30
HEALTH_CHECK_TIMEOUT=5

# =================================================================
# NGINX Configuration
# =================================================================
NGINX_WORKER_PROCESSES=auto
NGINX_WORKER_CONNECTIONS=1024
NGINX_CLIENT_MAX_BODY_SIZE=20M

# =================================================================
# Performance Configuration
# =================================================================
ENABLE_RESPONSE_COMPRESSION=true
ENABLE_OUTPUT_CACHING=true
ENABLE_HTTP2=true
ENABLE_HTTP3=true

# =================================================================
# Security Configuration
# =================================================================
CORS_ALLOWED_ORIGINS=http://localhost:3000,https://app.example.com
RATE_LIMIT_REQUESTS_PER_SECOND=100
RATE_LIMIT_BURST=20
```

### B. docker-compose.override.yml (Local Development)

```yaml
version: '3.8'

services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5000:8080"  # Expose for direct access during dev
    volumes:
      - ./src:/src:ro  # Mount source for hot reload (if using watch)

  postgres:
    ports:
      - "5432:5432"  # Expose for local database tools
    environment:
      - POSTGRES_HOST_AUTH_METHOD=trust  # Dev only!

  redis:
    ports:
      - "6379:6379"  # Expose for Redis CLI

  seq:
    ports:
      - "5341:80"  # Expose Seq UI
```

### C. Kubernetes Values Template (Helm)

```yaml
# values.yaml for Helm chart
replicaCount: 3

image:
  repository: genericauth-api
  tag: latest
  pullPolicy: IfNotPresent

service:
  type: LoadBalancer
  port: 80
  targetPort: 8080

ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
  hosts:
    - host: api.genericauth.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: genericauth-tls
      hosts:
        - api.genericauth.com

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 250m
    memory: 256Mi

postgresql:
  enabled: true
  auth:
    username: genericauth
    password: ""  # Use external secret
    database: genericauth
  primary:
    persistence:
      enabled: true
      size: 10Gi

redis:
  enabled: true
  auth:
    enabled: false
  master:
    persistence:
      enabled: true
      size: 1Gi
```

---

## Summary and Recommendations

### Recommended Approach

**For your use case (configuration-driven scaling via appsettings.json), I recommend:**

1. **Short-term (Development & Testing):**
   - Use Docker Compose with environment variable scaling
   - Configure `INSTANCE_COUNT` in `.env` file
   - Simple, fast to implement, good for local development

2. **Medium-term (Staging & Small Production):**
   - Use Docker Swarm with declarative service scaling
   - Configuration stored in `docker-stack.yml`
   - Better orchestration, still relatively simple

3. **Long-term (Production at Scale):**
   - Migrate to Kubernetes with HorizontalPodAutoscaler
   - Auto-scaling based on CPU/memory metrics
   - Optional: Custom metrics from appsettings via ConfigMap

### Critical Migrations Required

1. **Database:** SQLite → PostgreSQL (mandatory for multi-instance)
2. **Caching:** Add Redis for distributed state management
3. **Logging:** Add Seq or ELK for centralized log aggregation
4. **Seeding:** Move database seeding from application startup to init container

### Key Benefits

- **Horizontal Scalability:** Scale from 1 to 100+ instances
- **High Availability:** Zero-downtime deployments with rolling updates
- **Performance:** Load balancing distributes traffic efficiently
- **Resilience:** Health checks ensure only healthy instances serve traffic
- **Observability:** Centralized logging and monitoring across all instances
- **Security:** Non-root containers, secret management, TLS termination

### Next Steps

1. Review this plan with your team
2. Start with Phase 1 (Foundation) - Database migration and basic Docker setup
3. Proceed through phases sequentially
4. Test thoroughly at each phase before proceeding
5. Document any deviations or customizations

---

**Document Version:** 1.0
**Last Updated:** 2025-11-09
**Author:** GenericAuth Development Team
**Status:** Draft for Review
