namespace UteLearningHub.Application.Services.Settings;

public interface ISystemSettingService
{
    Task<int> GetIntAsync(string name, int defaultValue = 0, CancellationToken cancellationToken = default);
}
