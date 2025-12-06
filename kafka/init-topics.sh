#!/bin/bash

# Script để tạo Kafka topics
# Chạy sau khi Kafka đã start

KAFKA_BROKER="localhost:9092"

echo "Creating Kafka topics..."

# Topic cho chat messages
kafka-topics --create \
  --bootstrap-server $KAFKA_BROKER \
  --topic ute-learninghub-messages \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists

# Topic cho user interactions (recommendation system)
kafka-topics --create \
  --bootstrap-server $KAFKA_BROKER \
  --topic ute-learninghub-user-interactions \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists

echo "Topics created successfully!"

