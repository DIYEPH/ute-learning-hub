from fastapi import FastAPI
from sentence_transformers import SentenceTransformer
from sklearn.cluster import KMeans
import numpy as np
import time

from models import *
from utils import *

app = FastAPI(title="AI Recommendation", version="2.0")
model = SentenceTransformer('all-MiniLM-L6-v2')
DIM = 384 

@app.get("/health")
def health():
    return {"status": "ok", "model": "all-MiniLM-L6-v2", "dim": DIM}

@app.post("/vector/user")
def user_vector(req: UserVectorRequest):
    # Tính vector của user
    texts = req.subjects + req.tags
    weights = req.subjectWeights + req.tagWeights
    
    if not texts:
        return {"vector": [0.0] * DIM, "dim": DIM}
    
    vecs = model.encode(texts, normalize_embeddings=True)
    result = weighted_avg(vecs, weights)
    
    # Normalize final vector
    norm = np.linalg.norm(result)
    if norm > 0:
        result = result / norm
    
    return {"vector": result.tolist(), "dim": DIM}

@app.post("/vector/conv")
def conv_vector(req: ConvVectorRequest):
    # Tính vector của conversation
    parts = [req.name]
    if req.subject:
        parts.append(req.subject)
    parts.extend(req.tags)
    
    text = " ".join(parts)
    vec = model.encode(text, normalize_embeddings=True)
    
    return {"vector": vec.tolist(), "dim": DIM}

@app.post("/recommend")
def recommend(req: RecommendRequest):
    start = time.perf_counter()
    
    # Lấy top-K recommendations by cosine similarity
    user = np.array(req.UserVector)
    results = []
    
    for conv in req.ConversationVectors:
        vec = np.array(conv["vector"]) 
        score = cosine(user, vec)
        if score >= req.MinSimilarity:
            results.append({
                "conversationId": conv["id"], 
                "similarity": round(score, 4)
            })
    print(results)
    # Sắp xếp theo score giảm dần
    results.sort(key=lambda x: x["similarity"], reverse=True)
    top = results[:req.TopK]
    
    # Thêm rank
    for i, r in enumerate(top):
        r["rank"] = i + 1
    
    elapsed = (time.perf_counter() - start) * 1000  # ms
    
    return {
        "recommendations": top,
        "totalProcessed": len(req.ConversationVectors),
        "processingTimeMs": round(elapsed, 2)
    }

@app.post("/similar/users")
def similar_users(req: SimilarUsersRequest):
    start = time.perf_counter()
    
    conv = np.array(req.ConvVector)
    results = []
    
    for user in req.UserVectors:
        vec = np.array(user["vector"])
        score = cosine(conv, vec)
        if score >= req.MinScore:
            results.append({
                "userId": user["id"],
                "similarity": round(score, 4)
            })
    
    # Sắp xếp theo score giảm dần
    results.sort(key=lambda x: x["similarity"], reverse=True)
    top = results[:req.TopK]
    
    # Thêm rank
    for i, r in enumerate(top):
        r["rank"] = i + 1
    
    elapsed = (time.perf_counter() - start) * 1000
    
    return {
        "users": top,
        "totalProcessed": len(req.UserVectors),
        "processingTimeMs": round(elapsed, 2)
    }

@app.post("/cluster/users")
def cluster_users(req: ClusterUsersRequest):
    start = time.perf_counter()

    users = req.UserVectors
    n_users = len(users)

    if n_users < req.MinClusterSize:
        return {
            "clusters": [],
            "totalProcessed": n_users,
            "processingTimeMs": 0
        }

    vectors = np.array([u["vector"] for u in users])

    K = n_users // req.MinClusterSize
    K = max(1, K)

    kmeans = KMeans(
        n_clusters=K,
        init="k-means++",
        n_init=10,
        random_state=42
    )

    labels = kmeans.fit_predict(vectors)
    centroids = kmeans.cluster_centers_

    centroids = centroids / np.linalg.norm(centroids, axis=1, keepdims=True)

    clusters = []

    for i in range(K):
        members = []
        centroid = centroids[i]

        for idx, label in enumerate(labels):
            if label == i:
                sim = float(np.dot(vectors[idx], centroid))  # cosine
                members.append({
                    "userId": users[idx]["id"],
                    "similarityToCentroid": round(sim, 4)
                })

        if len(members) >= req.MinClusterSize:
            members.sort(
                key=lambda x: x["similarityToCentroid"],
                reverse=True
            )
            clusters.append({
                "centroid": centroid.tolist(),
                "users": members
            })

    elapsed = (time.perf_counter() - start) * 1000

    return {
        "clusters": clusters,
        "totalProcessed": n_users,
        "processingTimeMs": round(elapsed, 2)
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

