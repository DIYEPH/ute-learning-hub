namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string MessageTopic { get; set; } = "ute-learninghub-messages";
    public string ConsumerGroupId { get; set; } = "ute-learninghub-chat-consumers";
}

