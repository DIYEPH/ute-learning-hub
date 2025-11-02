namespace UteLearningHub.Domain.Repositories;

public interface IConcurrencyHandler<in TEntity>
{
    void SetRowVersion(TEntity entity, byte[] version);
    bool IsDbUpdateConcurrencyException(Exception ex);
}
