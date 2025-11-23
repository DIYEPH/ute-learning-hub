namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class AmazonS3Options
{
    public const string SectionName = "AWS";
    public string AccessKeyId { get; init; } = default!;
    public string SecretAccessKey { get; init; } = default!;
    public string Region { get; init; } = "ap-southeast-1";
    public string S3BucketName { get; init; } = default!;
    public string S3BaseUrl { get; init; } = default!;
}
