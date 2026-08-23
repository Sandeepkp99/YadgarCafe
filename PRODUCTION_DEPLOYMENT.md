# Yadgar Cafe - Production Deployment Guide

This guide covers deploying both the backend (.NET 8 API) and frontend (React) to production.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Backend Deployment](#backend-deployment)
3. [Frontend Deployment](#frontend-deployment)
4. [Environment Configuration](#environment-configuration)
5. [Database Setup](#database-setup)
6. [Security Considerations](#security-considerations)
7. [Monitoring & Maintenance](#monitoring--maintenance)

---

## Prerequisites

### Backend Requirements
- .NET 8 SDK installed
- SQL Server or compatible database
- Docker (optional, for containerization)

### Frontend Requirements
- Node.js v16+ and npm/yarn
- Git for version control

### Infrastructure
- Production server or cloud provider (Azure, AWS, DigitalOcean, etc.)
- Domain name
- SSL certificate
- Reverse proxy (Nginx/IIS)

---

## Backend Deployment

### 1. Build the Backend

```bash
cd Backend
dotnet publish -c Release -o ./publish
```

This creates an optimized production build in the `publish` folder.

### 2. Configure Connection Strings

Update `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_DB_SERVER;Database=YadgarCafe;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "YOUR_VERY_LONG_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com",
    "ExpirationMinutes": 60
  },
  "AllowedHosts": ["yourdomain.com", "api.yourdomain.com"],
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com", "https://www.yourdomain.com"]
  }
}
```

### 3. Database Migrations

```bash
cd Backend
dotnet ef database update --configuration Release
```

### 4. Deploy to Server

#### Option A: Direct Server Deployment

```bash
# On production server
cd /opt/yadgar-cafe-api
# Copy published files
scp -r ./publish/* user@server:/opt/yadgar-cafe-api/

# Run the application
cd /opt/yadgar-cafe-api
dotnet YadgarCafe.API.dll
```

#### Option B: Docker Deployment

Create a `Dockerfile` in the Backend directory:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YadgarCafe.API/YadgarCafe.API.csproj", "YadgarCafe.API/"]
COPY ["YadgarCafe.Application/YadgarCafe.Application.csproj", "YadgarCafe.Application/"]
COPY ["YadgarCafe.Domain/YadgarCafe.Domain.csproj", "YadgarCafe.Domain/"]
COPY ["YadgarCafe.Infrastructure/YadgarCafe.Infrastructure.csproj", "YadgarCafe.Infrastructure/"]
COPY ["YadgarCafe.Common/YadgarCafe.Common.csproj", "YadgarCafe.Common/"]
RUN dotnet restore "YadgarCafe.API/YadgarCafe.API.csproj"
COPY . .
RUN dotnet build "YadgarCafe.API/YadgarCafe.API.csproj" -c Release -o /app/build
RUN dotnet publish "YadgarCafe.API/YadgarCafe.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "YadgarCafe.API.dll"]
```

Build and run:

```bash
docker build -t yadgar-cafe-api .
docker run -d \
  -e ConnectionStrings__DefaultConnection="Server=db;Database=YadgarCafe;User Id=sa;Password=YourPassword;" \
  -e Jwt__Key="YOUR_SECRET_KEY" \
  -p 5000:5000 \
  yadgar-cafe-api
```

#### Option C: Azure App Service

```bash
# Install Azure CLI
# Login
az login

# Create resource group
az group create --name yadgar-cafe-rg --location eastus

# Create App Service Plan
az appservice plan create \
  --name yadgar-cafe-plan \
  --resource-group yadgar-cafe-rg \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group yadgar-cafe-rg \
  --plan yadgar-cafe-plan \
  --name yadgar-cafe-api \
  --runtime "DOTNET|8.0"

# Deploy from git
az webapp up \
  --name yadgar-cafe-api \
  --resource-group yadgar-cafe-rg \
  --runtime "DOTNET|8.0"
```

### 5. Nginx Configuration

```nginx
upstream backend {
    server localhost:5000;
}

server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Frontend Deployment

### 1. Build Frontend

```bash
cd Frontend/yadgar-cafe-web
npm install
npm run build
```

This creates optimized production files in the `dist` folder.

### 2. Configure Production Environment

Create `.env.production`:

```env
VITE_API_BASE_URL=https://api.yourdomain.com/api
```

### 3. Deploy Options

#### Option A: Static Hosting (Netlify)

```bash
npm install -g netlify-cli

# Login to Netlify
netlify login

# Deploy
netlify deploy --prod --dir=dist
```

#### Option B: Vercel

```bash
npm install -g vercel
vercel --prod
```

#### Option C: GitHub Pages

Update `vite.config.ts`:

```typescript
export default {
  base: '/yadgar-cafe/',
}
```

Then:

```bash
npm run build
# Commit and push dist folder to gh-pages branch
```

#### Option D: Self-Hosted (Nginx)

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;

    root /var/www/yadgar-cafe-frontend;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://api.yourdomain.com;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Copy build files:

```bash
scp -r dist/* user@server:/var/www/yadgar-cafe-frontend/
```

#### Option E: Docker (Self-Hosted)

Create `Dockerfile`:

```dockerfile
FROM node:18-alpine as builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

Create `nginx.conf`:

```nginx
server {
    listen 80;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

Build and run:

```bash
docker build -t yadgar-cafe-frontend .
docker run -d -p 80:80 yadgar-cafe-frontend
```

---

## Environment Configuration

### Production Environment Variables

**.NET Backend (appsettings.Production.json)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Production connection string"
  },
  "Jwt": {
    "Key": "Long secret key (min 32 chars)",
    "Issuer": "https://api.yourdomain.com",
    "Audience": "https://yourdomain.com",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**React Frontend (.env.production)**
```env
VITE_API_BASE_URL=https://api.yourdomain.com/api
```

---

## Database Setup

### SQL Server Setup

1. **Create Database**
```sql
CREATE DATABASE YadgarCafe;
```

2. **Run Migrations**
```bash
dotnet ef database update --configuration Release
```

3. **Backup Strategy**
```sql
BACKUP DATABASE YadgarCafe 
TO DISK = '/var/opt/mssql/backup/YadgarCafe.bak'
WITH INIT, COMPRESSION;
```

### Azure SQL Database

```bash
az sql server create \
  --name yadgar-cafe-server \
  --resource-group yadgar-cafe-rg \
  --admin-user sqladmin \
  --admin-password YourPassword123!

az sql db create \
  --resource-group yadgar-cafe-rg \
  --server yadgar-cafe-server \
  --name YadgarCafe
```

---

## Security Considerations

### 1. HTTPS/SSL

```bash
# Using Let's Encrypt with Certbot
sudo certbot certonly --nginx -d yourdomain.com -d api.yourdomain.com

# Nginx config update
server {
    listen 443 ssl http2;
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    # Security headers
    add_header Strict-Transport-Security "max-age=31536000" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
}
```

### 2. API Security

- Never expose sensitive keys in code
- Use environment variables for secrets
- Implement rate limiting
- Enable CORS only for your domain
- Use HTTPS/TLS for all communications
- Implement proper authentication/authorization

### 3. Database Security

- Use strong passwords
- Enable SQL Server authentication
- Restrict database access
- Regular backups
- Enable encryption at rest

### 4. Frontend Security

- Sanitize user input
- Never store sensitive data in localStorage (use httpOnly cookies)
- Implement CSP headers
- Regular security audits

---

## Monitoring & Maintenance

### Application Monitoring

```bash
# Check API health
curl https://api.yourdomain.com/health

# Check frontend
curl https://yourdomain.com

# View logs
docker logs <container-id>
```

### Database Maintenance

```sql
-- Check database size
SELECT 
    DB_NAME() as DatabaseName,
    SUM(size) * 8.0 / 1024 as SizeInMB
FROM sys.master_files
GROUP BY DB_NAME();

-- Rebuild indexes
ALTER INDEX ALL ON table_name REBUILD;

-- Check index fragmentation
SELECT * FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED');
```

### Backup Automation

```bash
#!/bin/bash
# backup.sh
BACKUP_DIR="/backups"
DB_NAME="YadgarCafe"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Backup database
sqlcmd -S SERVER_NAME -U USERNAME -P PASSWORD \
  -Q "BACKUP DATABASE $DB_NAME TO DISK='$BACKUP_DIR/$DB_NAME\_$TIMESTAMP.bak' WITH COMPRESSION;"

# Keep only last 30 days
find $BACKUP_DIR -name "*.bak" -mtime +30 -delete
```

### Performance Monitoring

Add Application Insights to backend:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

---

## Troubleshooting

### Backend Issues

```bash
# Check .NET version
dotnet --version

# Verify database connection
dotnet YadgarCafe.API.dll

# View detailed logs
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_LOG_LEVEL=Debug
```

### Frontend Issues

```bash
# Check build output
npm run build

# Test in production mode
npm run preview

# Check for console errors in browser DevTools
```

### CORS Errors

Update `appsettings.Production.json`:

```json
"Cors": {
  "AllowedOrigins": ["https://yourdomain.com", "https://www.yourdomain.com"]
}
```

---

## Rollback Procedure

```bash
# Backend rollback
docker stop yadgar-cafe-api
docker run -d --name yadgar-cafe-api:previous ...

# Frontend rollback (if using git-based deployment)
git checkout <previous-commit>
npm run build
```

---

## Performance Optimization

### Backend
- Enable response compression
- Configure caching policies
- Optimize database queries
- Use connection pooling

### Frontend
- Enable gzip compression
- Minify CSS/JS
- Optimize images
- Cache static assets

---

## Support & Escalation

For issues:
1. Check logs
2. Verify configuration
3. Test locally
4. Check status page
5. Contact support team

---

## Checklist Before Production Deployment

- [ ] Environment variables configured
- [ ] Database migrated and backed up
- [ ] SSL certificate installed
- [ ] CORS configured correctly
- [ ] API keys and secrets secured
- [ ] Logging configured
- [ ] Backup strategy implemented
- [ ] Monitoring set up
- [ ] Performance tested
- [ ] Security audit completed
- [ ] Documentation updated
- [ ] Team trained on deployment

---

Last Updated: 2024
