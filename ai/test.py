import requests
import json

BASE_URL = "http://localhost:8000"

def test_health():
    """Test health endpoint"""
    print("Testing /health...")
    response = requests.get(f"{BASE_URL}/health")
    print(f"Status: {response.status_code}")
    print(f"Response: {response.json()}")
    print()

def test_recommend():
    """Test recommend endpoint"""
    print("Testing /recommend...")
    
    request_data = {
        "userVector": [0.1, 0.2, 0.3, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],
        "conversationVectors": [
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
        ],
        "topK": 10,
        "minSimilarity": 0.3
    }
    
    response = requests.post(
        f"{BASE_URL}/recommend",
        json=request_data
    )
    
    print(f"Status: {response.status_code}")
    result = response.json()
    print(f"Total processed: {result['totalProcessed']}")
    print(f"Processing time: {result['processingTimeMs']:.2f}ms")
    print(f"Recommendations: {len(result['recommendations'])}")
    print()
    
    for rec in result['recommendations']:
        print(f"  - {rec['conversationId']}: similarity = {rec['similarity']:.4f}, rank = {rec['rank']}")
    print()

if __name__ == "__main__":
    try:
        test_health()
        test_recommend()
        print("✅ All tests passed!")
    except requests.exceptions.ConnectionError:
        print("❌ Error: Service chưa chạy!")
        print("Chạy: uvicorn main:app --reload --port 8000")
    except Exception as e:
        print(f"❌ Error: {e}")
