# Kafka Setup

## Topics được sử dụng:

1. **ute-learninghub-messages** - Chat messages
2. **ute-learninghub-user-interactions** - User interactions (join group, like document, etc.) cho recommendation system

## Chạy Kafka:

Kafka được chạy cùng với các services khác từ `docker-compose.yml` ở root:

```bash
# Chạy tất cả services (SQL Server, Redis, Kafka, Zookeeper)
docker-compose up -d

# Hoặc chỉ chạy Kafka và Zookeeper
docker-compose up -d zookeeper kafka
```

## Tạo topics (optional):

Topics sẽ được tự động tạo khi có message đầu tiên (KAFKA_AUTO_CREATE_TOPICS_ENABLE=true).

Hoặc tạo thủ công:

```bash
# Vào container
docker exec -it ute-learning-hub-kafka-1 bash

# Tạo topics
kafka-topics --create --bootstrap-server localhost:9092 --topic ute-learninghub-messages --partitions 3 --replication-factor 1
kafka-topics --create --bootstrap-server localhost:9092 --topic ute-learninghub-user-interactions --partitions 3 --replication-factor 1

# List topics
kafka-topics --list --bootstrap-server localhost:9092
```

## Kiểm tra:

```bash
# Xem logs
docker-compose logs -f kafka

# Kiểm tra topics
docker exec -it ute-learning-hub-kafka-1 kafka-topics --list --bootstrap-server localhost:9092
```

