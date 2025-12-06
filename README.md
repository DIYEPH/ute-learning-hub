# UTE Learning Hub

## Docker Setup

Tất cả services được quản lý trong `docker-compose.yml` ở root.

### Services:

#### Infrastructure:
1. **SQL Server** - Database (port 1433)
2. **Redis** - Cache cho recommendations (port 6379)
3. **Kafka** - Message broker (port 9092)
4. **Zookeeper** - Quản lý Kafka (port 2181)

#### Application Services:
5. **Backend** - .NET 9.0 API (internal: 7080)
6. **Frontend** - Next.js 16 (internal: 3000)
7. **AI** - Python FastAPI Recommendation Service (internal: 8000)
8. **Nginx** - Reverse Proxy & Load Balancer (ports 80, 443)

### Chạy tất cả services:

```bash
# Build và start tất cả services
docker-compose up -d --build

# Xem logs
docker-compose logs -f

# Xem logs của service cụ thể
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f ai

# Dừng tất cả
docker-compose down

# Dừng và xóa volumes
docker-compose down -v
```

### Chạy từng service riêng:

```bash
# Chỉ chạy infrastructure (database, redis, kafka)
docker-compose up -d sqlserver redis zookeeper kafka

# Chỉ chạy backend
docker-compose up -d backend

# Chỉ chạy frontend
docker-compose up -d frontend

# Chỉ chạy AI service
docker-compose up -d ai
```

### Kiểm tra services:

```bash
# SQL Server
docker exec -it ute-learning-hub-sqlserver-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT @@VERSION"

# Redis
docker exec -it ute-learning-hub-redis-1 redis-cli ping

# Kafka
docker exec -it ute-learning-hub-kafka-1 kafka-topics --list --bootstrap-server localhost:9092

# Nginx (main entry point)
curl http://localhost/health

# Backend API (qua Nginx)
curl http://localhost/api/health

# Frontend (qua Nginx)
curl http://localhost

# AI Service (internal only - backend calls directly via http://ai:8000)

# Kiểm tra Nginx logs
docker-compose logs -f nginx
```

### URLs (Production - qua Nginx):

- **Frontend**: http://localhost
- **Backend API**: http://localhost/api
- **Backend OpenAPI**: http://localhost/openapi/v1.json
- **Scalar API Docs**: http://localhost/scalar
- **Health Check**: http://localhost/health
- **AI Service**: Internal only (backend calls via `http://ai:8000`)

**Lưu ý**: Tất cả services chỉ expose qua Nginx (port 80). Backend, Frontend, và AI chỉ accessible trong Docker network.

### Connection Strings:

- **SQL Server**: `Data Source=localhost;Initial Catalog=ute-learning-hub;User Id=sa;Password=YourStrong@Passw0rd;Trust Server Certificate=True`
- **Redis**: `localhost:6379`
- **Kafka**: `localhost:9092`

### Environment Variables:

Backend tự động nhận các biến môi trường từ `docker-compose.yml`:
- Database connection string
- Kafka configuration
- Redis (nếu cần)

Frontend:
- `API_URL`: Backend API URL (internal: `http://backend:7080`)
- `NEXT_PUBLIC_API_URL`: Backend API URL (public: `/api` - relative path qua Nginx)

### Nginx Configuration:

Nginx hoạt động như reverse proxy:
- **/** → Frontend (Next.js)
- **/api/** → Backend API
- **/images/** → Backend static files (cached 30 days)
- **/hubs/** → SignalR WebSocket connections
- **/ai/** → AI Recommendation Service
- **/openapi, /scalar** → API Documentation

### Production Features:

✅ **Single Entry Point** - Tất cả traffic qua port 80  
✅ **Compression** - Gzip enabled  
✅ **Caching** - Static files cached  
✅ **Security Headers** - XSS, Frame Options, etc.  
✅ **WebSocket Support** - SignalR ready  
✅ **SSL Ready** - Port 443 exposed (cần cấu hình SSL certificate)

