import numpy as np

DIM = 384

def cosine(a: np.ndarray, b: np.ndarray) -> float:
    # Tính cosine similarity giữa hai vector
    norm = np.linalg.norm(a) * np.linalg.norm(b)
    return float(np.dot(a, b) / norm) if norm > 0 else 0.0

def weighted_avg(vectors: np.ndarray, weights: list[float]) -> np.ndarray:
    # Tính trung bình trọng số của các vector
    if len(vectors) == 0:
        return np.zeros(DIM)
    w = np.array(weights).reshape(-1, 1)
    return np.sum(vectors * w, axis=0) / np.sum(w)