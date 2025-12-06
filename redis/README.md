# Redis Setup

## Chức năng:

Redis được dùng để **cache recommendations** cho recommendation system:
- Cache kết quả recommendations của từng user
- Giảm tải cho Python service khi có nhiều requests
- Tăng tốc độ response cho user

## Chạy Redis:

Redis được build từ `Dockerfile` và chạy cùng với các services khác từ `docker-compose.yml` ở root:

```bash
# Build và chạy tất cả services (SQL Server, Redis, Kafka, Zookeeper)
docker-compose up -d --build

# Hoặc chỉ build và chạy Redis
docker-compose up -d --build redis
```

## Kiểm tra:

```bash
# Xem logs
docker-compose logs -f redis

# Test connection
docker exec -it ute-learning-hub-redis-1 redis-cli ping
# Kết quả: PONG

# Xem keys
docker exec -it ute-learning-hub-redis-1 redis-cli KEYS "*"

# Xem info
docker exec -it ute-learning-hub-redis-1 redis-cli INFO
```

## Cấu hình trong Backend:

Thêm vào `appsettings.json`:

```json
"Redis": {
  "ConnectionString": "localhost:6379",
  "InstanceName": "ute-learninghub:"
}
```

## Cache Keys Format:

- Recommendations: `ute-learninghub:recommendations:{userId}`
- User Vector: `ute-learninghub:user-vector:{userId}` (optional)

