using System;

namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class TemporaryFileCleanupOptions
{
    public const string SectionName = "TemporaryFileCleanup";
    public bool Enabled { get; set; } = true;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan FileTtl { get; set; } = TimeSpan.FromHours(1);
}


