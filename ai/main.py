from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
import numpy as np
import time
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

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
        logger.info("=" * 50)
        logger.info("Received recommendation request")
        logger.info(f"topK={request.topK}, minSimilarity={request.minSimilarity}")
        logger.info(f"User vector length: {len(request.userVector)}")
        logger.info(f"Number of conversations: {len(request.conversationVectors)}")
        
        if not request.userVector:
            raise HTTPException(status_code=400, detail="userVector không được rỗng")
        
        if not request.conversationVectors:
            logger.info("No conversation vectors, returning empty")
            return RecommendationResponse(
                recommendations=[],
                totalProcessed=0,
                processingTimeMs=0.0
            )
        
        user_vector = np.array(request.userVector, dtype=np.float32)
        logger.info(f"User vector (first 10 values): {user_vector[:10].tolist()}")
        logger.info(f"User vector non-zero count: {np.count_nonzero(user_vector)}")
        
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
        
        # Log each conversation vector
        for i, (conv_id, vec) in enumerate(zip(conversation_ids, conversation_vectors)):
            logger.info(f"Conversation {conv_id[:8]}... vector (first 10): {vec[:10].tolist()}, non-zero: {np.count_nonzero(vec)}")
        
        similarities = cosine_similarity_batch(user_vector, conversation_vectors)
        
        # Log similarities
        logger.info("Similarity scores:")
        for conv_id, sim in zip(conversation_ids, similarities):
            status = "PASS" if sim >= request.minSimilarity else "FAIL"
            logger.info(f"  {conv_id[:8]}... : {sim:.4f} ({status}, threshold={request.minSimilarity})")
        
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
        
        logger.info(f"Returning {len(top_results)} recommendations")
        logger.info("=" * 50)
        
        return RecommendationResponse(
            recommendations=top_results,
            totalProcessed=len(conversation_vectors),
            processingTimeMs=processing_time
        )
    
    except Exception as e:
        logger.error(f"Error: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

