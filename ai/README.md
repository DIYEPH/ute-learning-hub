# AI Service API - Sentence Transformer

## Endpoints

### 1. Health Check
```
GET /health
→ {"status": "healthy", "model": "all-MiniLM-L6-v2", "dimension": 384}
```

### 2. Single Text Embedding
```
POST /embed
Body: {"text": "Lập trình Python"}
→ {"vector": [0.12, -0.34, ...], "dimension": 384}
```

### 3. Batch Embeddings
```
POST /embed/batch
Body: {"texts": ["Python", "Machine Learning", "AI"]}
→ {"vectors": [[...], [...], [...]], "dimension": 384, "count": 3}
```

### 4. User Vector (từ hành vi)
```
POST /vector/user
Body: {
  "subjectNames": ["Lập trình Python", "Machine Learning"],
  "subjectWeights": [10, 5],  // số lần đọc
  "tagNames": ["AI", "Data Science"],
  "tagWeights": [12, 8]
}
→ {"vector": [...], "dimension": 384}
```

### 5. Conversation Vector
```
POST /vector/conversation
Body: {
  "name": "Nhóm học AI/ML",
  "subjectName": "Machine Learning",
  "tagNames": ["AI", "Python"],
  "memberVectors": [[...], [...]],  // optional
  "memberWeight": 0.6
}
→ {"vector": [...], "dimension": 384}
```

### 6. Recommendations
```
POST /recommend
Body: {
  "userVector": [...],
  "conversationVectors": [{"id": "xxx", "vector": [...]}],
  "topK": 10,
  "minSimilarity": 0.3
}
→ {"recommendations": [{"conversationId": "xxx", "similarity": 0.85, "rank": 1}]}
```

## Run Locally
```bash
cd ai
pip install -r requirements.txt
python main.py
# → http://localhost:8000/docs
```

## Run with Docker
```bash
docker-compose up ai -d
```
