using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Settings;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Settings;

public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _dbContext;

    public SystemSettingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetIntAsync(string name, int defaultValue = 0, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

        return setting?.Value ?? defaultValue;
    }
}
