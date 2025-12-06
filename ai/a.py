"""
Python Recommendation Service - Gợi ý nhóm học dựa trên Cosine Similarity

Service này NHẬN vectors từ C# backend và tính similarity
KHÔNG động tới database, chỉ tính toán thuần túy
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import numpy as np
from datetime import datetime
import logging

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Study Group Recommendation Service",
    description="Dịch vụ gợi ý nhóm học sử dụng Cosine Similarity",
    version="1.0.0"
)


# ==================== MODELS ====================

class RecommendationRequest(BaseModel):
    """
    Request từ C# backend
    C# đã query DB và tính vectors rồi, chỉ gửi vectors cho Python
    """
    userVector: List[float]  # Vector của user: [0.1, 0.2, 0.3, ...]
    conversationVectors: List[dict]  # List các conversation vectors
    # [
    #   {"id": "guid-1", "vector": [0.2, 0.1, 0.3, ...]},
    #   {"id": "guid-2", "vector": [0.0, 0.0, 0.0, ...]},
    #   ...
    # ]
    topK: int = 10  # Số lượng gợi ý tối đa
    minSimilarity: float = 0.3  # Ngưỡng similarity tối thiểu


class RecommendationResponse(BaseModel):
    """Response trả về cho C# backend"""
    recommendations: List[dict]
    # [
    #   {"conversationId": "guid-3", "similarity": 0.92, "rank": 1},
    #   {"conversationId": "guid-1", "similarity": 0.85, "rank": 2},
    #   ...
    # ]
    totalProcessed: int  # Tổng số conversations đã xử lý
    processingTimeMs: float  # Thời gian xử lý (milliseconds)


class BatchRecommendationRequest(BaseModel):
    """Xử lý nhiều user cùng lúc"""
    requests: List[dict]
    # [
    #   {
    #     "userId": "guid-1",
    #     "userVector": [0.1, 0.2, ...],
    #     "conversationVectors": [...],
    #     "topK": 10
    #   },
    #   ...
    # ]


# ==================== COSINE SIMILARITY FUNCTIONS ====================

def cosine_similarity_single(user_vector: np.ndarray, conversation_vector: np.ndarray) -> float:
    """
    Tính cosine similarity giữa 1 user vector và 1 conversation vector
    
    Công thức: cosine_similarity = (A · B) / (||A|| * ||B||)
    
    Args:
        user_vector: Vector của user, shape (n,)
        conversation_vector: Vector của conversation, shape (n,)
    
    Returns:
        Similarity score từ -1 đến 1 (1 = giống nhất, 0 = không liên quan, -1 = đối lập)
    
    Ví dụ:
        user_vector = [0.1, 0.2, 0.3]
        conversation_vector = [0.15, 0.25, 0.28]
        similarity ≈ 0.99 (rất giống)
    """
    # Tính dot product: A · B
    dot_product = np.dot(user_vector, conversation_vector)
    
    # Tính norm (độ dài) của mỗi vector
    user_norm = np.linalg.norm(user_vector)
    conversation_norm = np.linalg.norm(conversation_vector)
    
    # Tránh chia 0
    if user_norm == 0 or conversation_norm == 0:
        return 0.0
    
    # Cosine similarity
    similarity = dot_product / (user_norm * conversation_norm)
    
    return float(similarity)


def cosine_similarity_batch(user_vector: np.ndarray, conversation_vectors: np.ndarray) -> np.ndarray:
    """
    Tính cosine similarity giữa 1 user vector và NHIỀU conversation vectors cùng lúc
    Sử dụng vectorized operations để tính nhanh
    
    Args:
        user_vector: Vector của user, shape (n,)
        conversation_vectors: Vectors của conversations, shape (m, n)
            m = số lượng conversations
            n = số chiều của vector
    
    Returns:
        Array of similarities, shape (m,)
    
    Ví dụ:
        user_vector: shape (100,)
        conversation_vectors: shape (1000, 100)  # 1000 nhóm, mỗi nhóm 100 chiều
        Kết quả: shape (1000,)  # 1000 similarity scores
    """
    # Normalize user vector
    user_norm = np.linalg.norm(user_vector)
    if user_norm == 0:
        return np.zeros(len(conversation_vectors))
    user_normalized = user_vector / user_norm
    
    # Normalize tất cả conversation vectors cùng lúc
    # axis=1: tính norm theo từng hàng (từng conversation)
    # keepdims=True: giữ shape để broadcast được
    conversation_norms = np.linalg.norm(conversation_vectors, axis=1, keepdims=True)
    conversation_norms[conversation_norms == 0] = 1  # Tránh chia 0
    conversation_normalized = conversation_vectors / conversation_norms
    
    # Tính dot product cho tất cả cùng lúc
    # user_normalized: (100,)
    # conversation_normalized: (1000, 100)
    # Kết quả: (1000,) - mỗi phần tử là dot product với 1 conversation
    similarities = np.dot(conversation_normalized, user_normalized)
    
    return similarities


# ==================== API ENDPOINTS ====================

@app.get("/")
async def root():
    """Health check endpoint"""
    return {
        "service": "Study Group Recommendation Service",
        "status": "running",
        "version": "1.0.0"
    }


@app.get("/health")
async def health():
    """Health check cho monitoring"""
    return {"status": "healthy", "timestamp": datetime.utcnow().isoformat()}


@app.post("/recommend", response_model=RecommendationResponse)
async def recommend_study_groups(request: RecommendationRequest):
    """
    Gợi ý nhóm học dựa trên cosine similarity
    
    Flow:
    1. Nhận user vector và conversation vectors từ C# backend
    2. Tính cosine similarity cho tất cả conversations cùng lúc (vectorized)
    3. Lọc và sắp xếp theo similarity
    4. Trả về top K recommendations
    
    Ví dụ request:
    {
        "userVector": [0.1, 0.2, 0.3, 0.0, ...],
        "conversationVectors": [
            {"id": "guid-1", "vector": [0.15, 0.25, 0.28, 0.0, ...]},
            {"id": "guid-2", "vector": [0.0, 0.0, 0.0, 0.0, ...]},
            {"id": "guid-3", "vector": [0.12, 0.22, 0.32, 0.0, ...]}
        ],
        "topK": 10,
        "minSimilarity": 0.3
    }
    """
    import time
    start_time = time.time()
    
    try:
        # Validate input
        if not request.userVector:
            raise HTTPException(status_code=400, detail="userVector không được rỗng")
        
        if not request.conversationVectors:
            return RecommendationResponse(
                recommendations=[],
                totalProcessed=0,
                processingTimeMs=0.0
            )
        
        # Convert sang numpy arrays
        user_vector = np.array(request.userVector, dtype=np.float32)
        
        # Tách IDs và vectors
        conversation_ids = [c["id"] for c in request.conversationVectors]
        conversation_vectors_list = [c["vector"] for c in request.conversationVectors]
        
        # Validate vectors có cùng dimension
        vector_dim = len(user_vector)
        for i, vec in enumerate(conversation_vectors_list):
            if len(vec) != vector_dim:
                raise HTTPException(
                    status_code=400,
                    detail=f"Conversation {i} có vector dimension khác với user vector"
                )
        
        # Convert sang numpy array: shape (m, n)
        # m = số conversations, n = vector dimension
        conversation_vectors = np.array(conversation_vectors_list, dtype=np.float32)
        
        logger.info(
            f"Processing {len(conversation_vectors)} conversations "
            f"with vector dimension {vector_dim}"
        )
        
        # Tính similarity cho TẤT CẢ conversations cùng lúc
        # Đây là điểm mạnh của vectorized operations - tính 1000 conversations
        # cũng nhanh như tính 1 conversation
        similarities = cosine_similarity_batch(user_vector, conversation_vectors)
        
        # Tạo list kết quả với similarity scores
        results = []
        for conv_id, similarity in zip(conversation_ids, similarities):
            if similarity >= request.minSimilarity:
                results.append({
                    "conversationId": conv_id,
                    "similarity": float(similarity)
                })
        
        # Sắp xếp theo similarity giảm dần
        results.sort(key=lambda x: x["similarity"], reverse=True)
        
        # Lấy top K
        top_results = results[:request.topK]
        
        # Thêm rank
        for i, result in enumerate(top_results, 1):
            result["rank"] = i
        
        processing_time = (time.time() - start_time) * 1000  # Convert to ms
        
        logger.info(
            f"Found {len(top_results)} recommendations "
            f"in {processing_time:.2f}ms"
        )
        
        return RecommendationResponse(
            recommendations=top_results,
            totalProcessed=len(conversation_vectors),
            processingTimeMs=processing_time
        )
    
    except Exception as e:
        logger.error(f"Error in recommend: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/batch-recommend")
async def batch_recommend(request: BatchRecommendationRequest):
    """
    Xử lý nhiều user cùng lúc (batch processing)
    
    Hữu ích khi:
    - Background job cần tính recommendations cho nhiều user
    - Có thể tối ưu nếu tất cả user so sánh với cùng 1 bộ conversations
    """
    import time
    start_time = time.time()
    
    try:
        results = []
        
        for req in request.requests:
            user_id = req.get("userId")
            user_vector = np.array(req.get("userVector", []), dtype=np.float32)
            conversation_vectors_data = req.get("conversationVectors", [])
            top_k = req.get("topK", 10)
            min_sim = req.get("minSimilarity", 0.3)
            
            if len(user_vector) == 0 or len(conversation_vectors_data) == 0:
                results.append({
                    "userId": user_id,
                    "recommendations": [],
                    "error": "Missing vector data"
                })
                continue
            
            # Tính similarity
            conversation_ids = [c["id"] for c in conversation_vectors_data]
            conversation_vectors = np.array(
                [c["vector"] for c in conversation_vectors_data],
                dtype=np.float32
            )
            
            similarities = cosine_similarity_batch(user_vector, conversation_vectors)
            
            # Lọc và sắp xếp
            user_results = []
            for conv_id, sim in zip(conversation_ids, similarities):
                if sim >= min_sim:
                    user_results.append({
                        "conversationId": conv_id,
                        "similarity": float(sim)
                    })
            
            user_results.sort(key=lambda x: x["similarity"], reverse=True)
            user_results = user_results[:top_k]
            
            results.append({
                "userId": user_id,
                "recommendations": user_results
            })
        
        processing_time = (time.time() - start_time) * 1000
        
        return {
            "results": results,
            "totalUsers": len(request.requests),
            "processingTimeMs": processing_time
        }
    
    except Exception as e:
        logger.error(f"Error in batch_recommend: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))


# ==================== DEMO/EXAMPLE ENDPOINTS ====================

@app.get("/demo/example")
async def demo_example():
    """
    Demo endpoint để hiểu cách tính cosine similarity
    
    Trả về ví dụ cụ thể với giải thích từng bước
    """
    # Ví dụ: User học về "Machine Learning" và "Python"
    user_vector = np.array([0.0, 0.0, 0.8, 0.6, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0])
    # Giải thích:
    # - Chiều 2 (index 2): Machine Learning = 0.8
    # - Chiều 3 (index 3): Python = 0.6
    # - Các chiều khác = 0 (không liên quan)
    
    # Conversation 1: Nhóm "Machine Learning với Python"
    conv1_vector = np.array([0.0, 0.0, 0.9, 0.7, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0])
    
    # Conversation 2: Nhóm "Lập trình C++"
    conv2_vector = np.array([0.0, 0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0])
    
    # Conversation 3: Nhóm "Toán học"
    conv3_vector = np.array([0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0])
    
    # Tính similarities
    sim1 = cosine_similarity_single(user_vector, conv1_vector)
    sim2 = cosine_similarity_single(user_vector, conv2_vector)
    sim3 = cosine_similarity_single(user_vector, conv3_vector)
    
    return {
        "explanation": "Ví dụ tính cosine similarity",
        "userVector": user_vector.tolist(),
        "userInterests": {
            "Machine Learning": 0.8,
            "Python": 0.6
        },
        "conversations": [
            {
                "name": "Nhóm Machine Learning với Python",
                "vector": conv1_vector.tolist(),
                "similarity": float(sim1),
                "explanation": "Rất giống với user (cùng ML và Python)"
            },
            {
                "name": "Nhóm Lập trình C++",
                "vector": conv2_vector.tolist(),
                "similarity": float(sim2),
                "explanation": "Không liên quan (user không học C++)"
            },
            {
                "name": "Nhóm Toán học",
                "vector": conv3_vector.tolist(),
                "similarity": float(sim3),
                "explanation": "Hoàn toàn không liên quan"
            }
        ],
        "formula": {
            "cosine_similarity": "(A · B) / (||A|| * ||B||)",
            "dot_product": "A · B = sum(A[i] * B[i])",
            "norm": "||A|| = sqrt(sum(A[i]^2))"
        },
        "calculation": {
            "conv1": {
                "dot_product": float(np.dot(user_vector, conv1_vector)),
                "user_norm": float(np.linalg.norm(user_vector)),
                "conv1_norm": float(np.linalg.norm(conv1_vector)),
                "similarity": float(sim1)
            }
        }
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
