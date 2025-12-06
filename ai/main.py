from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
import numpy as np
import time

app = FastAPI()

class RecommendationRequest(BaseModel):
    userVector: List[float]
    conversationVectors: List[dict]
    topK: int = 10
    minSimilarity: float = 0.3

class RecommendationResponse(BaseModel):
    recommendations: List[dict]
    totalProcessed: int
    processingTimeMs: float

def cosine_similarity_batch(user_vector: np.ndarray, conversation_vectors: np.ndarray) -> np.ndarray:
    user_norm = np.linalg.norm(user_vector)
    if user_norm == 0:
        return np.zeros(len(conversation_vectors))
    user_normalized = user_vector / user_norm
    
    conversation_norms = np.linalg.norm(conversation_vectors, axis=1, keepdims=True)
    conversation_norms[conversation_norms == 0] = 1
    conversation_normalized = conversation_vectors / conversation_norms
    
    similarities = np.dot(conversation_normalized, user_normalized)
    return similarities

@app.get("/health")
async def health():
    return {"status": "healthy"}

@app.post("/recommend", response_model=RecommendationResponse)
async def recommend_study_groups(request: RecommendationRequest):
    start_time = time.time()
    
    try:
        if not request.userVector:
            raise HTTPException(status_code=400, detail="userVector không được rỗng")
        
        if not request.conversationVectors:
            return RecommendationResponse(
                recommendations=[],
                totalProcessed=0,
                processingTimeMs=0.0
            )
        
        user_vector = np.array(request.userVector, dtype=np.float32)
        conversation_ids = [c["id"] for c in request.conversationVectors]
        conversation_vectors_list = [c["vector"] for c in request.conversationVectors]
        
        vector_dim = len(user_vector)
        for i, vec in enumerate(conversation_vectors_list):
            if len(vec) != vector_dim:
                raise HTTPException(
                    status_code=400,
                    detail=f"Conversation {i} có vector dimension khác với user vector"
                )
        
        conversation_vectors = np.array(conversation_vectors_list, dtype=np.float32)
        similarities = cosine_similarity_batch(user_vector, conversation_vectors)
        
        results = []
        for conv_id, sim in zip(conversation_ids, similarities):
            if sim >= request.minSimilarity:
                results.append({
                    "conversationId": conv_id,
                    "similarity": float(sim)
                })
        
        results.sort(key=lambda x: x["similarity"], reverse=True)
        top_results = results[:request.topK]
        
        for i, result in enumerate(top_results, 1):
            result["rank"] = i
        
        processing_time = (time.time() - start_time) * 1000
        
        return RecommendationResponse(
            recommendations=top_results,
            totalProcessed=len(conversation_vectors),
            processingTimeMs=processing_time
        )
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
