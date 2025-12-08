using UteLearningHub.Application.Services.Recommendation;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class VectorCalculationService : IVectorCalculationService
{
    // Vector structure: 100 dimensions
    // 0-29:  Faculty (30 dims)
    // 30-49: Type (20 dims)
    // 50-99: Tags (50 dims)
    private const int FacultyDimensionStart = 0;
    private const int FacultyDimensionEnd = 29;
    private const int TypeDimensionStart = 30;
    private const int TypeDimensionEnd = 49;
    private const int TagDimensionStart = 50;
    private const int TagDimensionEnd = 99;

    private const float FacultyWeight = 0.4f;
    private const float TypeWeight = 0.3f;
    private const float TagWeight = 0.3f;

    /// <summary>
    /// Calculate user vector from behavior data (Faculty, Type, Tag scores)
    /// </summary>
    public float[] CalculateUserVectorFromBehavior(
        IReadOnlyList<ScoreItem> facultyScores,
        IReadOnlyList<ScoreItem> typeScores,
        IReadOnlyList<ScoreItem> tagScores,
        int vectorDimension = 100)
    {
        var vector = new float[vectorDimension];

        // 1. Encode Faculty scores (dims 0-29)
        var totalFacultyScore = facultyScores.Sum(x => x.Score);
        if (totalFacultyScore > 0)
        {
            foreach (var item in facultyScores)
            {
                var idx = FacultyDimensionStart +
                    (int)(Math.Abs(item.Id.GetHashCode()) % (FacultyDimensionEnd - FacultyDimensionStart + 1));
                var normalizedScore = (float)item.Score / totalFacultyScore;
                vector[idx] += FacultyWeight * normalizedScore;
            }
        }

        // 2. Encode Type scores (dims 30-49)
        var totalTypeScore = typeScores.Sum(x => x.Score);
        if (totalTypeScore > 0)
        {
            foreach (var item in typeScores)
            {
                var idx = TypeDimensionStart +
                    (int)(Math.Abs(item.Id.GetHashCode()) % (TypeDimensionEnd - TypeDimensionStart + 1));
                var normalizedScore = (float)item.Score / totalTypeScore;
                vector[idx] += TypeWeight * normalizedScore;
            }
        }

        // 3. Encode Tag scores (dims 50-99)
        var totalTagScore = tagScores.Sum(x => x.Score);
        if (totalTagScore > 0)
        {
            foreach (var item in tagScores)
            {
                var idx = TagDimensionStart +
                    (int)(Math.Abs(item.Id.GetHashCode()) % (TagDimensionEnd - TagDimensionStart + 1));
                var normalizedScore = (float)item.Score / totalTagScore;
                vector[idx] += TagWeight * normalizedScore;
            }
        }

        // Normalize vector to unit vector
        var norm = CalculateNorm(vector);
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }

    /// <summary>
    /// Calculate conversation vector from Faculty (via Subject) and Tags
    /// </summary>
    public float[] CalculateConversationVector(
        IReadOnlyList<Guid>? facultyIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        int vectorDimension = 100)
    {
        var vector = new float[vectorDimension];
        facultyIds ??= Array.Empty<Guid>();
        tagIds ??= Array.Empty<Guid>();

        // 1. Encode Faculty (dims 0-29) - weight: 0.5
        if (facultyIds.Count > 0)
        {
            var weightPerFaculty = 0.5f / facultyIds.Count;
            foreach (var facultyId in facultyIds)
            {
                var idx = FacultyDimensionStart +
                    (int)(Math.Abs(facultyId.GetHashCode()) % (FacultyDimensionEnd - FacultyDimensionStart + 1));
                vector[idx] += weightPerFaculty;
            }
        }

        // 2. Encode Tags (dims 50-99) - weight: 0.5
        if (tagIds.Count > 0)
        {
            var weightPerTag = 0.5f / tagIds.Count;
            foreach (var tagId in tagIds)
            {
                var idx = TagDimensionStart +
                    (int)(Math.Abs(tagId.GetHashCode()) % (TagDimensionEnd - TagDimensionStart + 1));
                vector[idx] += weightPerTag;
            }
        }

        // Normalize
        var norm = CalculateNorm(vector);
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }

    // Legacy methods - kept for backwards compatibility
    [Obsolete("Use CalculateUserVectorFromBehavior instead")]
    public float[] CalculateUserVector(
        Guid? majorId = null,
        IReadOnlyList<Guid>? subjectIds = null,
        IReadOnlyList<Guid>? tagIds = null,
        int vectorDimension = 100)
    {
        // Return empty vector for legacy calls
        return new float[vectorDimension];
    }

    public float[] UpdateVectorIncremental(
        float[] oldVector,
        Guid? newMajorId = null,
        IReadOnlyList<Guid>? newSubjectIds = null,
        IReadOnlyList<Guid>? newTagIds = null,
        float learningRate = 0.2f)
    {
        // Not used in new implementation
        return oldVector;
    }

    private static float CalculateNorm(float[] vector)
    {
        float sum = 0;
        for (int i = 0; i < vector.Length; i++)
        {
            sum += vector[i] * vector[i];
        }
        return (float)Math.Sqrt(sum);
    }
}
