"""
Ví dụ test Python recommendation service

Chạy file này để hiểu cách service hoạt động
"""

import numpy as np
import requests
import json
from b import calculate_user_vector


def test_cosine_similarity_manual():
    """Test tính cosine similarity thủ công"""
    print("=" * 60)
    print("TEST 1: Tính Cosine Similarity Thủ Công")
    print("=" * 60)
    
    # User: Học Machine Learning và Python
    user_vector = np.array([0.0, 0.0, 0.8, 0.6, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0])
    print(f"\nUser Vector: {user_vector}")
    print("User quan tâm: Machine Learning (0.8), Python (0.6)")
    
    # Conversation 1: Nhóm ML với Python
    conv1_vector = np.array([0.0, 0.0, 0.9, 0.7, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0])
    print(f"\nConversation 1 Vector: {conv1_vector}")
    print("Nhóm về: Machine Learning (0.9), Python (0.7)")
    
    # Tính similarity
    dot_product = np.dot(user_vector, conv1_vector)
    user_norm = np.linalg.norm(user_vector)
    conv1_norm = np.linalg.norm(conv1_vector)
    similarity = dot_product / (user_norm * conv1_norm)
    
    print(f"\nTính toán:")
    print(f"  Dot product: {dot_product:.4f}")
    print(f"  User norm: {user_norm:.4f}")
    print(f"  Conv1 norm: {conv1_norm:.4f}")
    print(f"  Similarity: {similarity:.4f}")
    print(f"  → Rất giống! ✅")
    
    # Conversation 2: Nhóm C++
    conv2_vector = np.array([0.0, 0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0])
    print(f"\nConversation 2 Vector: {conv2_vector}")
    print("Nhóm về: C++ (0.9)")
    
    dot_product2 = np.dot(user_vector, conv2_vector)
    similarity2 = dot_product2 / (user_norm * np.linalg.norm(conv2_vector))
    
    print(f"  Similarity: {similarity2:.4f}")
    print(f"  → Không liên quan ❌")


def test_vector_calculation():
    """Test tính vector từ user data"""
    print("\n" + "=" * 60)
    print("TEST 2: Tính Vector từ User Data")
    print("=" * 60)
    
    # User có:
    # - Major: Computer Science
    # - Subjects: Machine Learning, AI
    # - Tags: Python, Deep Learning
    
    user_vector = calculate_user_vector(
        major_id="cs-001",
        subject_ids=["ml-001", "ai-002"],
        tag_ids=["python", "deep-learning"]
    )
    
    print(f"\nUser Vector (100 chiều):")
    print(f"  Dimension: {len(user_vector)}")
    print(f"  Non-zero values: {np.count_nonzero(user_vector)}")
    print(f"  Norm: {np.linalg.norm(user_vector):.4f}")
    print(f"  Top 5 values: {np.argsort(np.abs(user_vector))[-5:][::-1]}")


def test_api_recommend():
    """Test API recommend endpoint"""
    print("\n" + "=" * 60)
    print("TEST 3: Test API /recommend")
    print("=" * 60)
    
    # Tạo test data
    user_vector = [0.1, 0.2, 0.3, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
    
    conversation_vectors = [
        {
            "id": "conv-1",
            "vector": [0.15, 0.25, 0.28, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
        },
        {
            "id": "conv-2",
            "vector": [0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
        },
        {
            "id": "conv-3",
            "vector": [0.12, 0.22, 0.32, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
        }
    ]
    
    request_data = {
        "userVector": user_vector,
        "conversationVectors": conversation_vectors,
        "topK": 10,
        "minSimilarity": 0.3
    }
    
    print(f"\nRequest:")
    print(json.dumps(request_data, indent=2))
    
    try:
        response = requests.post(
            "http://localhost:8000/recommend",
            json=request_data,
            timeout=5
        )
        
        if response.status_code == 200:
            result = response.json()
            print(f"\nResponse:")
            print(json.dumps(result, indent=2))
            
            print(f"\nKết quả:")
            for rec in result["recommendations"]:
                print(f"  - {rec['conversationId']}: similarity = {rec['similarity']:.4f}")
        else:
            print(f"Error: {response.status_code}")
            print(response.text)
    except requests.exceptions.ConnectionError:
        print("\n⚠️  Service chưa chạy!")
        print("Chạy: uvicorn main:app --reload --port 8000")
    except Exception as e:
        print(f"\nError: {e}")


def test_batch_processing():
    """Test batch processing với nhiều user"""
    print("\n" + "=" * 60)
    print("TEST 4: Batch Processing (Nhiều User)")
    print("=" * 60)
    
    request_data = {
        "requests": [
            {
                "userId": "user-1",
                "userVector": [0.1, 0.2, 0.3, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
                "conversationVectors": [
                    {"id": "conv-1", "vector": [0.15, 0.25, 0.28, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]},
                    {"id": "conv-2", "vector": [0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]}
                ],
                "topK": 5
            },
            {
                "userId": "user-2",
                "userVector": [0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
                "conversationVectors": [
                    {"id": "conv-1", "vector": [0.15, 0.25, 0.28, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]},
                    {"id": "conv-2", "vector": [0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]}
                ],
                "topK": 5
            }
        ]
    }
    
    try:
        response = requests.post(
            "http://localhost:8000/batch-recommend",
            json=request_data,
            timeout=10
        )
        
        if response.status_code == 200:
            result = response.json()
            print(f"\nProcessed {result['totalUsers']} users in {result['processingTimeMs']:.2f}ms")
            
            for user_result in result["results"]:
                print(f"\nUser {user_result['userId']}:")
                for rec in user_result["recommendations"]:
                    print(f"  - {rec['conversationId']}: {rec['similarity']:.4f}")
        else:
            print(f"Error: {response.status_code}")
    except requests.exceptions.ConnectionError:
        print("\n⚠️  Service chưa chạy!")
    except Exception as e:
        print(f"\nError: {e}")


if __name__ == "__main__":
    print("\n" + "=" * 60)
    print("PYTHON RECOMMENDATION SERVICE - TEST EXAMPLES")
    print("=" * 60)
    
    # Test 1: Tính thủ công
    test_cosine_similarity_manual()
    
    # Test 2: Tính vector
    test_vector_calculation()
    
    # Test 3: API (cần service đang chạy)
    test_api_recommend()
    
    # Test 4: Batch (cần service đang chạy)
    test_batch_processing()
    
    print("\n" + "=" * 60)
    print("TEST COMPLETED")
    print("=" * 60)
