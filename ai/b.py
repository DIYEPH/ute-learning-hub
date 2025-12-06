"""
Utility functions để tính vector đặc trưng cho user và conversation

Các hàm này có thể được dùng ở C# backend hoặc Python service
tùy theo nơi bạn muốn tính vector
"""

import numpy as np
from typing import List, Dict, Optional


def calculate_user_vector(
    major_id: Optional[str] = None,
    subject_ids: List[str] = None,
    tag_ids: List[str] = None,
    vector_dimension: int = 100
) -> np.ndarray:
    """
    Tính vector đặc trưng cho user dựa trên:
    - Major (chuyên ngành): weight 0.3
    - Subjects (môn học): weight 0.4
    - Tags (thẻ): weight 0.3
    
    Args:
        major_id: ID của chuyên ngành
        subject_ids: List ID các môn học user quan tâm
        tag_ids: List ID các thẻ user quan tâm
        vector_dimension: Số chiều của vector (mặc định 100)
    
    Returns:
        Vector đặc trưng, shape (vector_dimension,)
    
    Ví dụ:
        user_vector = calculate_user_vector(
            major_id="cs-001",
            subject_ids=["ml-001", "ai-002"],
            tag_ids=["python", "deep-learning"]
        )
        # Vector sẽ có giá trị cao ở các chiều tương ứng với
        # chuyên ngành CS, môn ML/AI, và tags Python/DL
    """
    if subject_ids is None:
        subject_ids = []
    if tag_ids is None:
        tag_ids = []
    
    vector = np.zeros(vector_dimension, dtype=np.float32)
    
    # Phân bổ không gian vector:
    # - Chiều 0-19: Major (20 chiều)
    # - Chiều 20-59: Subjects (40 chiều)
    # - Chiều 60-99: Tags (40 chiều)
    
    # 1. Encode Major (chiều 0-19)
    if major_id:
        major_idx = hash(major_id) % 20
        vector[major_idx] = 0.3
    
    # 2. Encode Subjects (chiều 20-59)
    if subject_ids:
        weight_per_subject = 0.4 / len(subject_ids)
        for subject_id in subject_ids[:40]:  # Giới hạn 40 subjects
            subject_idx = 20 + (hash(subject_id) % 40)
            vector[subject_idx] += weight_per_subject
    
    # 3. Encode Tags (chiều 60-99)
    if tag_ids:
        weight_per_tag = 0.3 / len(tag_ids)
        for tag_id in tag_ids[:40]:  # Giới hạn 40 tags
            tag_idx = 60 + (hash(tag_id) % 40)
            vector[tag_idx] += weight_per_tag
    
    # Normalize vector về unit vector
    norm = np.linalg.norm(vector)
    if norm > 0:
        vector = vector / norm
    
    return vector


def calculate_conversation_vector(
    subject_id: Optional[str] = None,
    tag_ids: List[str] = None,
    vector_dimension: int = 100
) -> np.ndarray:
    """
    Tính vector đặc trưng cho conversation (nhóm học)
    
    Args:
        subject_id: ID môn học của nhóm
        tag_ids: List ID các thẻ của nhóm
        vector_dimension: Số chiều của vector
    
    Returns:
        Vector đặc trưng, shape (vector_dimension,)
    """
    if tag_ids is None:
        tag_ids = []
    
    vector = np.zeros(vector_dimension, dtype=np.float32)
    
    # Phân bổ giống user vector:
    # - Chiều 20-59: Subjects
    # - Chiều 60-99: Tags
    
    # 1. Encode Subject (chiều 20-59)
    if subject_id:
        subject_idx = 20 + (hash(subject_id) % 40)
        vector[subject_idx] = 0.5
    
    # 2. Encode Tags (chiều 60-99)
    if tag_ids:
        weight_per_tag = 0.5 / len(tag_ids) if tag_ids else 0
        for tag_id in tag_ids[:40]:
            tag_idx = 60 + (hash(tag_id) % 40)
            vector[tag_idx] += weight_per_tag
    
    # Normalize
    norm = np.linalg.norm(vector)
    if norm > 0:
        vector = vector / norm
    
    return vector


def update_vector_incremental(
    old_vector: np.ndarray,
    new_major_id: Optional[str] = None,
    new_subject_ids: List[str] = None,
    new_tag_ids: List[str] = None,
    learning_rate: float = 0.2
) -> np.ndarray:
    """
    Cập nhật vector incrementally (không tính lại từ đầu)
    
    Khi user thích/tham gia nhóm mới, chỉ cập nhật phần thay đổi
    thay vì tính lại toàn bộ vector
    
    Args:
        old_vector: Vector cũ
        new_major_id: Major mới (nếu có)
        new_subject_ids: Subjects mới thêm vào
        new_tag_ids: Tags mới thêm vào
        learning_rate: Tỷ lệ học (0.1-0.3), càng cao càng thay đổi nhanh
    
    Returns:
        Vector mới sau khi cập nhật
    """
    # Tính vector cho phần mới
    delta_vector = calculate_user_vector(
        major_id=new_major_id,
        subject_ids=new_subject_ids,
        tag_ids=new_tag_ids,
        vector_dimension=len(old_vector)
    )
    
    # Cập nhật: new = old + learning_rate * delta
    new_vector = old_vector + learning_rate * delta_vector
    
    # Normalize lại
    norm = np.linalg.norm(new_vector)
    if norm > 0:
        new_vector = new_vector / norm
    
    return new_vector


def explain_vector(vector: np.ndarray) -> Dict:
    """
    Giải thích vector để debug
    
    Trả về thông tin về các chiều có giá trị cao
    """
    # Tìm các chiều có giá trị cao nhất
    top_indices = np.argsort(np.abs(vector))[-10:][::-1]
    
    explanation = {
        "vector_dimension": len(vector),
        "non_zero_count": np.count_nonzero(vector),
        "top_10_dimensions": [
            {
                "index": int(idx),
                "value": float(vector[idx]),
                "category": _get_dimension_category(int(idx))
            }
            for idx in top_indices
        ],
        "vector_norm": float(np.linalg.norm(vector))
    }
    
    return explanation


def _get_dimension_category(index: int) -> str:
    """Xác định category của một chiều vector"""
    if 0 <= index < 20:
        return "Major"
    elif 20 <= index < 60:
        return "Subject"
    elif 60 <= index < 100:
        return "Tag"
    else:
        return "Unknown"
