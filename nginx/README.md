# Nginx Configuration

## Chức năng:

Nginx hoạt động như **reverse proxy** và **load balancer** cho production:

- **Single Entry Point**: Tất cả traffic vào qua port 80/443
- **Reverse Proxy**: Route requests đến đúng service
- **Compression**: Gzip compression cho text files
- **Caching**: Cache static files (images, CSS, JS)
- **Security Headers**: XSS protection, frame options, etc.
- **WebSocket Support**: SignalR connections

## Routing:

| Path | Destination | Description |
|------|-------------|-------------|
| `/` | Frontend (Next.js) | Main application |
| `/api/*` | Backend API | REST API endpoints |
| `/images/*` | Backend | Static images (cached 30 days) |
| `/hubs/*` | Backend | SignalR WebSocket hub |
| `/openapi` | Backend | OpenAPI documentation |
| `/scalar` | Backend | Scalar API documentation |
| `/health` | Nginx | Health check endpoint |

**Note**: AI Service (`/ai/*`) is **internal only** - accessed directly by backend via Docker network (`http://ai:8000`), not exposed through Nginx.

## SSL/HTTPS Setup (Production):

Để enable HTTPS, cần:

1. **Tạo SSL certificates** (Let's Encrypt hoặc self-signed):
```bash
# Let's Encrypt (recommended)
certbot certonly --standalone -d yourdomain.com

# Self-signed (development only)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /path/to/nginx/ssl/nginx.key \
  -out /path/to/nginx/ssl/nginx.crt
```

2. **Cập nhật nginx.conf** để thêm SSL server block:
```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    ssl_certificate /etc/nginx/ssl/nginx.crt;
    ssl_certificate_key /etc/nginx/ssl/nginx.key;
    
    # SSL configuration...
}
```

3. **Mount SSL certificates** trong docker-compose.yml:
```yaml
volumes:
  - ./nginx/ssl:/etc/nginx/ssl:ro
```

## Performance Tuning:

- **Client max body size**: 100MB (cho file uploads)
- **Gzip compression**: Level 6
- **Proxy timeouts**: 300s (5 minutes)
- **WebSocket timeout**: 86400s (24 hours)

## Health Check:

```bash
curl http://localhost/health
# Returns: "healthy"
```

## Logs:

```bash
# View Nginx access logs
docker-compose logs -f nginx

# View Nginx error logs
docker exec -it ute-learning-hub-nginx-1 tail -f /var/log/nginx/error.log
```

