namespace UteLearningHub.Domain.Repositories.Base;

public interface IConcurrencyHandler<in TEntity>
{
    void SetRowVersion(TEntity entity, byte[] version);
    bool IsDbUpdateConcurrencyException(Exception ex);
}
