using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Common.Events;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Message;

public class KafkaMessageProducer : IMessageQueueProducer, IDisposable
{
    private readonly KafkaOptions _options;
    private readonly IProducer<string, string> _producer;

    public KafkaMessageProducer(IOptions<KafkaOptions> options)
    {
        _options = options.Value;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        var envelope = new MessageQueueEvent(MessageQueueEventTypes.MessageCreated, message);
        var payload = JsonSerializer.Serialize(envelope);

        var kafkaMessage = new Message<string, string>
        {
            Key = message.ConversationId.ToString(),
            Value = payload
        };

        await _producer.ProduceAsync(_options.MessageTopic, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

