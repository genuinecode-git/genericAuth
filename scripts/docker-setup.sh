#!/bin/bash

# =================================================================
# GenericAuth Docker Setup Script
# Automated setup for Docker containerization
# =================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================================${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"

    # Check Docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker Desktop."
        exit 1
    fi
    print_success "Docker is installed: $(docker --version)"

    # Check Docker Compose
    if ! docker compose version &> /dev/null; then
        print_error "Docker Compose V2 is not available. Please update Docker Desktop."
        exit 1
    fi
    print_success "Docker Compose is installed: $(docker compose version)"

    # Check if Docker is running
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running. Please start Docker Desktop."
        exit 1
    fi
    print_success "Docker daemon is running"

    echo ""
}

# Create .env file
setup_env_file() {
    print_header "Setting Up Environment Configuration"

    if [ -f ".env" ]; then
        print_warning ".env file already exists"
        read -p "Do you want to overwrite it? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "Skipping .env setup"
            return
        fi
    fi

    # Copy template
    if [ -f ".env.template" ]; then
        cp .env.template .env
        print_success "Created .env from template"
    else
        print_error ".env.template not found"
        exit 1
    fi

    # Prompt for configuration
    print_info "Please configure the following settings:"
    echo ""

    # Instance count
    read -p "Number of API instances to run (default: 3): " INSTANCE_COUNT
    INSTANCE_COUNT=${INSTANCE_COUNT:-3}
    sed -i.bak "s/INSTANCE_COUNT=.*/INSTANCE_COUNT=$INSTANCE_COUNT/" .env

    # Environment
    read -p "Environment (Development/Production, default: Development): " ENVIRONMENT
    ENVIRONMENT=${ENVIRONMENT:-Development}
    sed -i.bak "s/ASPNETCORE_ENVIRONMENT=.*/ASPNETCORE_ENVIRONMENT=$ENVIRONMENT/" .env

    # Database password
    print_warning "Database password (leave empty to use default 'Dev@Pass123'):"
    read -sp "Password: " DB_PASSWORD
    echo ""
    if [ ! -z "$DB_PASSWORD" ]; then
        sed -i.bak "s/DB_PASSWORD=.*/DB_PASSWORD=$DB_PASSWORD/" .env
    fi

    # JWT Secret
    if [[ "$ENVIRONMENT" == "Production" ]]; then
        print_warning "Generating strong JWT secret for production..."
        JWT_SECRET=$(openssl rand -base64 48)
        sed -i.bak "s|JWT_SECRET=.*|JWT_SECRET=$JWT_SECRET|" .env
        print_success "Generated JWT secret"
    fi

    # Clean up backup files
    rm -f .env.bak

    print_success "Environment configuration completed"
    echo ""
}

# Create required directories
create_directories() {
    print_header "Creating Required Directories"

    mkdir -p nginx/ssl
    print_success "Created nginx/ssl directory"

    mkdir -p secrets
    print_success "Created secrets directory"

    echo ""
}

# Generate self-signed SSL certificate for development
generate_ssl_cert() {
    print_header "Generating SSL Certificate"

    if [ -f "nginx/ssl/cert.pem" ] && [ -f "nginx/ssl/key.pem" ]; then
        print_warning "SSL certificates already exist"
        return
    fi

    print_info "Generating self-signed certificate for development..."

    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout nginx/ssl/key.pem \
        -out nginx/ssl/cert.pem \
        -subj "/C=US/ST=State/L=City/O=GenericAuth/CN=localhost" \
        &> /dev/null

    print_success "SSL certificate generated"
    print_warning "Note: This is a self-signed certificate for development only"
    echo ""
}

# Build Docker images
build_images() {
    print_header "Building Docker Images"

    print_info "Building GenericAuth API image..."
    docker compose build --no-cache

    print_success "Docker images built successfully"
    echo ""
}

# Initialize database
init_database() {
    print_header "Initializing Database"

    print_info "Starting PostgreSQL..."
    docker compose up -d postgres

    print_info "Waiting for PostgreSQL to be ready..."
    sleep 10

    # Check if database is ready
    until docker compose exec postgres pg_isready -U genericauth &> /dev/null; do
        echo -n "."
        sleep 1
    done
    echo ""

    print_success "PostgreSQL is ready"
    echo ""
}

# Start services
start_services() {
    print_header "Starting All Services"

    print_info "Starting all services with Docker Compose..."
    docker compose up -d

    print_info "Waiting for services to be healthy..."
    sleep 15

    print_success "All services started"
    echo ""
}

# Display service status
show_status() {
    print_header "Service Status"

    docker compose ps
    echo ""

    print_header "Access Information"
    echo -e "${GREEN}API (Load Balanced):${NC} http://localhost"
    echo -e "${GREEN}Swagger UI:${NC} http://localhost/swagger"
    echo -e "${GREEN}Health Check:${NC} http://localhost/health"
    echo -e "${GREEN}Seq Logs:${NC} http://localhost:5341"
    echo -e "${GREEN}PostgreSQL:${NC} localhost:5432"
    echo -e "${GREEN}Redis:${NC} localhost:6379"
    echo ""

    print_header "Default Credentials"
    echo -e "${YELLOW}Email:${NC} admin@genericauth.com"
    echo -e "${YELLOW}Password:${NC} Admin@123"
    echo ""

    print_header "Useful Commands"
    echo "View logs:           docker compose logs -f"
    echo "View API logs:       docker compose logs -f api"
    echo "Scale instances:     docker compose up -d --scale api=5"
    echo "Stop services:       docker compose down"
    echo "Restart services:    docker compose restart"
    echo ""
}

# Main execution
main() {
    clear
    print_header "GenericAuth Docker Setup"
    echo ""

    check_prerequisites
    setup_env_file
    create_directories
    generate_ssl_cert
    build_images
    init_database
    start_services
    show_status

    print_header "Setup Complete!"
    print_success "GenericAuth is now running in Docker"
    echo ""
    print_info "Open http://localhost/swagger to get started"
    echo ""
}

# Run main function
main
