# GenericAuth - Docker Quick Start Guide

This guide will help you get GenericAuth running in Docker with multiple instances in under 10 minutes.

## Prerequisites

- Docker Desktop installed (version 20.10 or higher)
- Docker Compose V2 installed (comes with Docker Desktop)
- At least 4GB of available RAM
- 10GB of available disk space

## Quick Start (3 Steps)

### Step 1: Clone and Navigate

```bash
cd /Users/prabathsl/Projects/GenericAuth/genericAuth
```

### Step 2: Create Environment File

```bash
# Copy the template
cp .env.template .env

# Edit .env and set your values (or use defaults for local testing)
# Minimum required: Set INSTANCE_COUNT to desired number of API instances
```

### Step 3: Start All Services

```bash
# Start with 3 API instances (default)
docker-compose up -d

# Or specify instance count
INSTANCE_COUNT=5 docker-compose up -d
```

That's it! The application is now running at:
- API: http://localhost (load balanced)
- Swagger UI: http://localhost/swagger
- Seq Logs: http://localhost:5341
- PostgreSQL: localhost:5432
- Redis: localhost:6379

## Detailed Configuration

### Environment Variables (.env file)

```bash
# Number of API instances to run (1-20 recommended)
INSTANCE_COUNT=3

# Database configuration
DB_NAME=genericauth
DB_USER=genericauth
DB_PASSWORD=Dev@Pass123

# JWT Secret (CHANGE IN PRODUCTION!)
JWT_SECRET=YourSuperSecretKeyForProductionMustBeAtLeast32CharactersLong!

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### Scaling Instances

#### Scale Up/Down Dynamically

```bash
# Scale to 5 instances
docker-compose up -d --scale api=5

# Scale to 10 instances
docker-compose up -d --scale api=10

# Scale down to 2 instances
docker-compose up -d --scale api=2
```

#### Using Environment Variable

```bash
# Set in .env file
echo "INSTANCE_COUNT=7" >> .env

# Apply changes
docker-compose up -d
```

### Viewing Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f nginx
docker-compose logs -f postgres

# Last 100 lines
docker-compose logs --tail=100 api

# Centralized logs (all instances)
# Open browser to http://localhost:5341
```

### Service Management

```bash
# View running services
docker-compose ps

# Check service health
docker-compose ps api

# Restart a service
docker-compose restart api

# Stop all services
docker-compose stop

# Stop and remove containers (keeps data)
docker-compose down

# Stop and remove everything (including volumes)
docker-compose down -v
```

### Health Checks

```bash
# Check API health
curl http://localhost/health

# Check specific instance
curl http://localhost:8080/health

# Check NGINX status
curl http://localhost/health

# View health in browser
open http://localhost/health
```

## Accessing Services

### Swagger UI (API Documentation)

```
http://localhost/swagger
```

Default credentials:
- Email: `admin@genericauth.com`
- Password: `Admin@123`

### Seq (Centralized Logging)

```
http://localhost:5341
```

View logs from all API instances in one place. Search, filter, and analyze structured logs.

### PostgreSQL Database

```bash
# Using psql
docker exec -it genericauth-postgres psql -U genericauth -d genericauth

# Using pgAdmin or other tools
Host: localhost
Port: 5432
Database: genericauth
Username: genericauth
Password: Dev@Pass123
```

### Redis Cache

```bash
# Redis CLI
docker exec -it genericauth-redis redis-cli

# Monitor cache activity
docker exec -it genericauth-redis redis-cli MONITOR

# Check keys
docker exec -it genericauth-redis redis-cli KEYS "*"
```

## Testing Multi-Instance Load Balancing

### Test Load Distribution

```bash
# Send 100 requests and see distribution across instances
for i in {1..100}; do
  curl -s http://localhost/health | grep -o '"status":"[^"]*"' &
done | sort | uniq -c
```

### Monitor NGINX Load Balancing

```bash
# View NGINX access logs (shows which upstream instance handled each request)
docker-compose logs -f nginx | grep upstream
```

### Test Instance Failure Recovery

```bash
# Get list of API container IDs
docker ps | grep genericauth-api

# Stop one instance
docker stop <container-id>

# Verify application still works (other instances handle traffic)
curl http://localhost/health

# NGINX should automatically route to healthy instances
# Stopped instance will be removed from rotation
```

## Database Management

### Apply Migrations Manually

```bash
# Access API container
docker exec -it <api-container-id> /bin/sh

# Run migrations
dotnet ef database update
```

### Backup Database

```bash
# Backup to file
docker exec genericauth-postgres pg_dump -U genericauth genericauth > backup.sql

# Restore from file
docker exec -i genericauth-postgres psql -U genericauth genericauth < backup.sql
```

### Reset Database

```bash
# Stop services
docker-compose down

# Remove database volume
docker volume rm genericauth-postgres-data

# Restart (will create fresh database)
docker-compose up -d
```

## Performance Tuning

### Adjust Resource Limits

Edit `docker-compose.yml`:

```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '1.0'      # Increase CPU limit
        memory: 1024M    # Increase memory limit
      reservations:
        cpus: '0.5'      # Increase CPU reservation
        memory: 512M     # Increase memory reservation
```

### Monitor Resource Usage

```bash
# Real-time stats for all containers
docker stats

# Stats for API instances only
docker stats $(docker ps --filter name=genericauth-api -q)

# One-time resource usage
docker-compose top
```

### Connection Pool Tuning

Edit `.env`:

```bash
# Increase max connections if you have many instances
# Formula: (INSTANCE_COUNT * 20) + 50 buffer
# Example: (10 instances * 20) + 50 = 250

# Update PostgreSQL max_connections in docker-compose.yml
# "-c" "max_connections=300"
```

## Troubleshooting

### Services Won't Start

```bash
# Check logs for errors
docker-compose logs

# Verify port availability
lsof -i :80,443,5432,6379,5341

# Check Docker resources
docker system df
docker system prune  # Clean up if needed
```

### Database Connection Errors

```bash
# Check if PostgreSQL is healthy
docker-compose ps postgres

# View PostgreSQL logs
docker-compose logs postgres

# Test connection
docker exec -it genericauth-postgres pg_isready -U genericauth
```

### API Instances Not Responding

```bash
# Check health status
docker-compose ps api

# View API logs
docker-compose logs api

# Restart unhealthy instances
docker-compose restart api
```

### NGINX Load Balancer Issues

```bash
# Check NGINX configuration syntax
docker exec genericauth-nginx nginx -t

# Reload NGINX configuration
docker exec genericauth-nginx nginx -s reload

# View NGINX error logs
docker-compose logs nginx | grep error
```

### Out of Memory Errors

```bash
# Check Docker Desktop memory allocation
# Increase in Docker Desktop Settings > Resources > Memory

# Reduce instance count
docker-compose up -d --scale api=2

# Or adjust container limits in docker-compose.yml
```

## Development Workflow

### Hot Reload (Development)

```bash
# Use docker-compose.override.yml for development
# Volume mount source code
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Watch logs during development
docker-compose logs -f api
```

### Rebuild After Code Changes

```bash
# Rebuild and restart
docker-compose up -d --build

# Force rebuild (no cache)
docker-compose build --no-cache
docker-compose up -d
```

### Run Tests in Docker

```bash
# Build test image
docker build -t genericauth-tests -f Dockerfile.Tests .

# Run tests
docker run --rm genericauth-tests
```

## Production Deployment

### Before Going to Production

1. Change all default passwords in `.env`
2. Set strong JWT secret (32+ characters)
3. Set `ASPNETCORE_ENVIRONMENT=Production`
4. Enable HTTPS in NGINX (configure SSL certificates)
5. Configure proper CORS origins
6. Review and adjust rate limiting
7. Set up monitoring and alerting
8. Configure backup strategy for PostgreSQL
9. Review security headers in NGINX
10. Load test your configuration

### Example Production .env

```bash
ASPNETCORE_ENVIRONMENT=Production
INSTANCE_COUNT=5

# Strong passwords
DB_PASSWORD=<generated-strong-password>
JWT_SECRET=<generated-strong-secret-minimum-32-chars>

# Production database
DB_NAME=genericauth_prod
DB_USER=genericauth_prod

# CORS
CORS_ALLOWED_ORIGINS=https://app.example.com,https://admin.example.com

# Rate limiting
RATE_LIMIT_REQUESTS_PER_SECOND=50
```

### Deploy to Production Server

```bash
# SSH to server
ssh user@production-server

# Clone repository
git clone <repository-url>
cd genericauth

# Create .env with production values
nano .env

# Start services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Verify health
curl https://api.example.com/health
```

## Next Steps

- [Read the full Containerization Plan](./CONTAINERIZATION_PLAN.md)
- [Deploy to Kubernetes](../kubernetes/README.md)
- [Set up CI/CD for automated deployments](../.github/workflows/)
- [Configure monitoring and alerting](./MONITORING.md)

## Support

For issues and questions:
- Check logs: `docker-compose logs`
- Review troubleshooting section above
- Consult [Docker documentation](https://docs.docker.com/)
- Check [GenericAuth documentation](../README.md)
