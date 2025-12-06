namespace UteLearningHub.Application.Services.Recommendation;

public interface IVectorCalculationService
{
    /// <summary>
    /// Tính vector đặc trưng cho user dựa trên hành vi (behavior-based)
    /// Sử dụng aggregate scores từ Faculty, Type, Tags
    /// </summary>
    /// <param name="facultyScores">Điểm Faculty tổng hợp từ Documents, Conversations, Votes</param>
    /// <param name="typeScores">Điểm Type tổng hợp từ Documents, Votes</param>
    /// <param name="tagScores">Điểm Tag tổng hợp từ Documents, Conversations, Votes</param>
    /// <param name="vectorDimension">Số chiều của vector (mặc định 100)</param>
    /// <returns>Vector đặc trưng dưới dạng mảng float</returns>
    float[] CalculateUserVectorFromBehavior(
        IReadOnlyList<ScoreItem> facultyScores,
        IReadOnlyList<ScoreItem> typeScores,
        IReadOnlyList<ScoreItem> tagScores,
        int vectorDimension = 100);

    /// <summary>
    /// Tính vector đặc trưng cho conversation (nhóm học) dựa trên Faculty và Tags
    /// </summary>
    /// <param name="facultyIds">Danh sách FacultyId (từ Subject → SubjectMajor → Major → Faculty)</param>
    /// <param name="tagIds">Danh sách ID các thẻ của nhóm</param>
    /// <param name="vectorDimension">Số chiều của vector (mặc định 100)</param>
    /// <returns>Vector đặc trưng dưới dạng mảng float</returns>
    float[] CalculateConversationVector(
        IReadOnlyList<Guid>? facultyIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        int vectorDimension = 100);

    // Legacy methods - kept for backwards compatibility
    [Obsolete("Use CalculateUserVectorFromBehavior instead")]
    float[] CalculateUserVector(
        Guid? majorId = null,
        IReadOnlyList<Guid>? subjectIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        int vectorDimension = 100);

    [Obsolete("Not used in new implementation")]
    float[] UpdateVectorIncremental(
        float[] oldVector,
        Guid? newMajorId = null,
        IReadOnlyList<Guid>? newSubjectIds = null,
        IReadOnlyList<Guid>? newTagIds = null,
        float learningRate = 0.2f);
}
