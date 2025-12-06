using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Common.Events;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Api.BackgroundServices;

public class KafkaMessageConsumerService : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly IMessageHubService _messageHubService;
    private readonly ILogger<KafkaMessageConsumerService> _logger;

    public KafkaMessageConsumerService(
        IOptions<KafkaOptions> options,
        IMessageHubService messageHubService,
        ILogger<KafkaMessageConsumerService> logger)
    {
        _options = options.Value;
        _messageHubService = messageHubService;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BootstrapServers) ||
            string.IsNullOrWhiteSpace(_options.MessageTopic))
        {
            _logger.LogWarning("Kafka configuration is missing. Kafka consumer will not start.");
            return Task.CompletedTask;
        }

        return Task.Run(async () =>
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.ConsumerGroupId ?? "ute-learninghub-chat-consumers",
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(_options.MessageTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        if (result?.Message?.Value == null)
                            continue;

                        var envelope = JsonSerializer.Deserialize<MessageQueueEvent>(result.Message.Value);
                        if (envelope == null)
                            continue;

                        switch (envelope.EventType)
                        {
                            case MessageQueueEventTypes.MessageCreated:
                                _logger.LogInformation("Received MessageCreated event: MessageId={MessageId}", envelope.Payload.Id);
                                await _messageHubService.BroadcastMessageCreatedAsync(envelope.Payload, stoppingToken);
                                break;

                            case MessageQueueEventTypes.MessageUpdated:
                                _logger.LogInformation("Received MessageUpdated event: MessageId={MessageId}", envelope.Payload.Id);
                                await _messageHubService.BroadcastMessageUpdatedAsync(envelope.Payload, stoppingToken);
                                break;

                            case MessageQueueEventTypes.MessageDeleted:
                                _logger.LogInformation("Received MessageDeleted event: MessageId={MessageId}, ConversationId={ConversationId}", 
                                    envelope.Payload.Id, envelope.Payload.ConversationId);
                                await _messageHubService.BroadcastMessageDeletedAsync(
                                    envelope.Payload.Id, 
                                    envelope.Payload.ConversationId, 
                                    stoppingToken);
                                break;

                            case MessageQueueEventTypes.MessagePinned:
                                _logger.LogInformation("Received MessagePinned event: MessageId={MessageId}", envelope.Payload.Id);
                                await _messageHubService.BroadcastMessagePinnedAsync(envelope.Payload, stoppingToken);
                                break;

                            case MessageQueueEventTypes.MessageUnpinned:
                                _logger.LogInformation("Received MessageUnpinned event: MessageId={MessageId}", envelope.Payload.Id);
                                await _messageHubService.BroadcastMessageUnpinnedAsync(envelope.Payload, stoppingToken);
                                break;

                            default:
                                _logger.LogWarning("Kafka event type {EventType} is not supported", envelope.EventType);
                                break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to deserialize Kafka message");
                    }
                    catch (ConsumeException consumeEx)
                    {
                        _logger.LogError(consumeEx, "Kafka consume error");
                    }
                }
            }
            finally
            {
                consumer.Close();
            }

        }, stoppingToken);
    }
}

