# GenericAuth Containerization

Complete Docker containerization solution for GenericAuth .NET 9.0 API with support for dynamic horizontal scaling.

## Overview

This containerization implementation provides:

- Multi-stage Docker build optimized for .NET 9.0
- Docker Compose configuration for local development
- Docker Swarm stack for production deployment
- Kubernetes manifests for cloud-native deployment
- NGINX load balancer with health checks
- PostgreSQL database with connection pooling
- Redis for distributed caching
- Seq for centralized logging
- Configuration-driven scaling (via environment variables)
- Zero-downtime deployments
- Automated database migrations
- Comprehensive health checks

## Quick Start

### Option 1: Automated Setup (Recommended)

```bash
# Run the setup script
./scripts/docker-setup.sh
```

This script will:
1. Check prerequisites
2. Create and configure .env file
3. Generate SSL certificates
4. Build Docker images
5. Initialize database
6. Start all services
7. Display access information

### Option 2: Manual Setup

```bash
# 1. Create environment file
cp .env.template .env

# 2. Edit .env and set INSTANCE_COUNT
nano .env

# 3. Start services
docker compose up -d

# 4. View status
docker compose ps
```

## Architecture Options

### 1. Docker Compose (Local Development)

**Best for:** Local development, testing, small deployments

**Features:**
- Simple configuration
- Easy to set up and use
- Environment variable-based scaling
- Good for 1-10 instances

**Scaling:**
```bash
# Scale to 5 instances
docker compose up -d --scale api=5

# Or set in .env
INSTANCE_COUNT=5
docker compose up -d
```

**Documentation:** [Docker Quick Start Guide](./DOCKER_QUICKSTART.md)

### 2. Docker Swarm (Production)

**Best for:** Production deployments, 10-100 instances

**Features:**
- Native Docker orchestration
- Service discovery and load balancing
- Rolling updates and rollbacks
- Secret management
- Resource constraints

**Scaling:**
```bash
# Initialize Swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-stack.yml genericauth

# Scale service
docker service scale genericauth_api=10
```

**Documentation:** See [docker-stack.yml](../docker-stack.yml)

### 3. Kubernetes (Enterprise Production)

**Best for:** Cloud deployments, 100+ instances, auto-scaling

**Features:**
- HorizontalPodAutoscaler (auto-scaling)
- Service mesh integration
- Advanced networking
- Cloud provider integration
- Monitoring and observability

**Scaling:**
```bash
# Deploy to Kubernetes
kubectl apply -f kubernetes/

# Manual scaling
kubectl scale deployment genericauth-api --replicas=10

# Auto-scaling (configured via HPA)
# Scales automatically based on CPU/memory
```

**Documentation:** See [kubernetes/](../kubernetes/)

## Directory Structure

```
GenericAuth/
├── Dockerfile                          # Multi-stage Docker build
├── .dockerignore                       # Exclude files from build
├── docker-compose.yml                  # Local development
├── docker-stack.yml                    # Docker Swarm production
├── .env.template                       # Environment template
├── nginx/
│   ├── nginx.conf                     # NGINX load balancer config
│   └── ssl/                           # SSL certificates (dev)
├── kubernetes/
│   ├── deployment.yaml                # K8s deployment & services
│   └── ingress.yaml                   # K8s ingress & TLS
├── scripts/
│   └── docker-setup.sh                # Automated setup script
└── docs/
    ├── CONTAINERIZATION_PLAN.md       # Detailed architecture plan
    ├── DOCKER_QUICKSTART.md           # Quick start guide
    └── CONTAINERIZATION_README.md     # This file
```

## Configuration

### Environment Variables

Key configuration options in `.env`:

```bash
# Scaling
INSTANCE_COUNT=3              # Number of API instances

# Database
DB_NAME=genericauth
DB_USER=genericauth
DB_PASSWORD=<strong-password>

# Security
JWT_SECRET=<32-char-secret>   # CHANGE IN PRODUCTION!

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### appsettings Configuration

The application supports configuration via:
1. Environment variables (highest priority)
2. appsettings.{Environment}.json
3. appsettings.json (lowest priority)

**Example appsettings.Docker.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=genericauth;...",
    "Redis": "redis:6379"
  },
  "JwtSettings": {
    "Secret": "PLACEHOLDER",
    "Issuer": "GenericAuth",
    "Audience": "GenericAuthClient"
  },
  "Scaling": {
    "DesiredInstanceCount": 3
  }
}
```

## Database Considerations

### Migration from SQLite to PostgreSQL

The application uses SQLite by default, which is NOT suitable for multi-instance deployment. You must migrate to PostgreSQL:

**Steps:**
1. PostgreSQL is included in docker-compose.yml
2. Update connection string in .env
3. Database migrations run automatically on first start
4. Default credentials created (admin@genericauth.com / Admin@123)

**Connection Pooling:**
- Configured for 5-20 connections per instance
- PostgreSQL max_connections set to 300
- Supports up to 10-15 instances safely

### Database Seeding

Database seeding runs ONCE via init container pattern:
- Migration service runs before API starts
- API instances skip seeding in Docker environment
- Prevents race conditions and duplicate data

## Load Balancing

### NGINX Configuration

The NGINX load balancer provides:
- **Least connections** algorithm (distributes to least busy instance)
- **Health checks** (removes unhealthy instances from rotation)
- **Rate limiting** (100 req/s general, 10 req/s auth endpoints)
- **SSL/TLS termination** (optional, for HTTPS)
- **Request timeout** (30s default)
- **Gzip compression** (reduces bandwidth)

**Load balancer endpoints:**
- Port 80: HTTP (redirects to HTTPS in production)
- Port 443: HTTPS (requires SSL certificates)

### Session Management

The application is **stateless** by design:
- JWT tokens (no server-side sessions)
- No session affinity required
- Requests can go to any instance

**Shared state (via Redis):**
- Data Protection keys
- Distributed cache
- Token blacklist (for logout feature)

## Monitoring and Logging

### Seq (Centralized Logging)

All API instances send logs to Seq:
- **URL:** http://localhost:5341
- **Features:** Search, filter, structured logging, alerts
- **Retention:** Configurable (default 7 days)

**Log correlation:**
- Each request has correlation ID
- Logs tagged with instance ID
- Easy to trace requests across instances

### Health Checks

Multiple health check endpoints:

```bash
# Overall health
curl http://localhost/health

# Liveness probe (is container alive?)
curl http://localhost/health/live

# Readiness probe (ready to serve traffic?)
curl http://localhost/health/ready
```

**Checks include:**
- Database connectivity
- Redis connectivity
- Application startup status

### Metrics (Optional)

For production monitoring, integrate:
- Prometheus (metrics collection)
- Grafana (visualization)
- Jaeger (distributed tracing)

See [CONTAINERIZATION_PLAN.md](./CONTAINERIZATION_PLAN.md) for details.

## Performance Optimization

### Resource Limits

Default limits per instance:
- **CPU:** 250m (reserved) to 500m (limit)
- **Memory:** 256Mi (reserved) to 512Mi (limit)

Adjust in docker-compose.yml:

```yaml
resources:
  limits:
    cpus: '1.0'
    memory: 1024M
  reservations:
    cpus: '0.5'
    memory: 512M
```

### Scaling Guidelines

**Vertical Scaling (per instance):**
- Increase CPU/memory limits
- Better for CPU-intensive operations
- Limited by single machine resources

**Horizontal Scaling (more instances):**
- Increase INSTANCE_COUNT
- Better for I/O operations
- Unlimited scalability (with load balancer)

**Recommended:**
- Start with 3 instances
- Monitor CPU/memory usage
- Scale horizontally first
- Scale vertically if instances are CPU-bound

### Connection Pooling

**Calculation:**
```
Max DB Connections Needed = (INSTANCE_COUNT × 20) + 50 buffer

Examples:
- 3 instances: (3 × 20) + 50 = 110
- 5 instances: (5 × 20) + 50 = 150
- 10 instances: (10 × 20) + 50 = 250
```

Update PostgreSQL `max_connections` accordingly.

## Security Best Practices

### Production Checklist

- [ ] Change all default passwords
- [ ] Generate strong JWT secret (32+ characters)
- [ ] Set ASPNETCORE_ENVIRONMENT=Production
- [ ] Enable HTTPS (configure SSL certificates)
- [ ] Configure proper CORS origins
- [ ] Review rate limiting settings
- [ ] Use Docker secrets (not environment variables)
- [ ] Enable audit logging
- [ ] Set up firewall rules
- [ ] Regular security updates

### Secrets Management

**Development:**
- Environment variables in .env
- Self-signed SSL certificates

**Production:**
- Docker Swarm: Use Docker secrets
- Kubernetes: Use Kubernetes secrets
- Cloud: Use managed secret services (Azure Key Vault, AWS Secrets Manager)

**Example (Docker Swarm):**

```bash
# Create secrets
echo "strong-password" | docker secret create db_password -
echo "32-char-jwt-secret" | docker secret create jwt_secret -

# Reference in docker-stack.yml
secrets:
  - db_password
  - jwt_secret
```

## Troubleshooting

### Common Issues

**1. Services won't start**
```bash
docker compose logs  # Check logs
docker compose ps    # Check status
docker system prune  # Clean up if needed
```

**2. Database connection errors**
```bash
docker compose logs postgres  # Check PostgreSQL logs
docker exec -it genericauth-postgres pg_isready -U genericauth
```

**3. API instances not responding**
```bash
docker compose logs api  # Check API logs
docker compose restart api  # Restart instances
```

**4. Load balancer errors**
```bash
docker exec genericauth-nginx nginx -t  # Test NGINX config
docker compose logs nginx  # Check NGINX logs
```

**5. Out of memory**
- Increase Docker Desktop memory allocation
- Reduce INSTANCE_COUNT
- Adjust container memory limits

### Debug Mode

Enable detailed logging:

```bash
# Set in .env
ASPNETCORE_ENVIRONMENT=Development
Logging__LogLevel__Default=Debug

# Restart services
docker compose up -d
```

## Deployment Strategies

### Rolling Updates (Zero Downtime)

**Docker Swarm:**
```yaml
update_config:
  parallelism: 1        # Update 1 at a time
  delay: 10s            # Wait 10s between updates
  order: start-first    # Start new before stopping old
```

**Kubernetes:**
```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

### Blue-Green Deployment

1. Deploy new version alongside old
2. Test new version
3. Switch traffic to new version
4. Remove old version

### Canary Deployment

1. Deploy new version to subset of instances
2. Monitor metrics
3. Gradually increase traffic to new version
4. Rollback if issues detected

## Cost Optimization

### Resource Optimization

**Right-sizing:**
- Monitor actual CPU/memory usage
- Adjust limits based on actual needs
- Use HPA to auto-scale based on load

**Spot Instances (Cloud):**
- Use spot/preemptible instances for non-critical workloads
- Configure node affinity for cost optimization

**Reserved Capacity:**
- Reserve capacity for baseline load
- Use auto-scaling for burst traffic

## Support and Documentation

### Documentation

- [Containerization Plan](./CONTAINERIZATION_PLAN.md) - Detailed architecture
- [Quick Start Guide](./DOCKER_QUICKSTART.md) - Get started in 5 minutes
- [Testing Guide](../TESTING_GUIDE.md) - End-to-end testing
- [Architecture](../ARCHITECTURE.md) - Overall application architecture

### Commands Reference

```bash
# Build
docker compose build

# Start
docker compose up -d

# Scale
docker compose up -d --scale api=5

# Logs
docker compose logs -f

# Stop
docker compose down

# Clean up
docker compose down -v

# Status
docker compose ps

# Health check
curl http://localhost/health
```

### Getting Help

1. Check logs: `docker compose logs`
2. Review troubleshooting section
3. Consult official documentation
4. Check GitHub issues
5. Contact support team

## Next Steps

1. **Local Development:** Start with Docker Compose
2. **Testing:** Test scaling with different instance counts
3. **Production Preparation:** Review security checklist
4. **Deploy to Swarm:** For production with Docker Swarm
5. **Migrate to Kubernetes:** For cloud-native deployment
6. **Set up CI/CD:** Automate builds and deployments
7. **Configure Monitoring:** Add Prometheus and Grafana
8. **Load Testing:** Test performance under load

---

**Version:** 1.0
**Last Updated:** 2025-11-09
**Maintainer:** GenericAuth Team
