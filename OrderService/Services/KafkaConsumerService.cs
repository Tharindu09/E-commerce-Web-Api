using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Dtos;

namespace OrderService.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public KafkaConsumerService(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaConsumerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config)
            .SetErrorHandler((_, e) =>
            {
                _logger.LogError("Kafka error: {Reason}", e.Reason);
            })
            .SetPartitionsAssignedHandler((_, partitions) =>
            {
                _logger.LogInformation("Partitions assigned: {Partitions}", string.Join(",", partitions));
            })
            .SetPartitionsRevokedHandler((_, partitions) =>
            {
                _logger.LogWarning("Partitions revoked: {Partitions}", string.Join(",", partitions));
            })
            .Build();

        consumer.Subscribe(_settings.Topic);

        _logger.LogInformation("Kafka consumer started for topic: {Topic} group: {GroupId}",
            _settings.Topic, _settings.GroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? consumeResult = null;

                try
                {
                    consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null)
                        continue;

                    if (consumeResult.IsPartitionEOF)
                        continue;

                    var raw = consumeResult.Message?.Value;

                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        _logger.LogWarning("Empty Kafka message received at {TopicPartitionOffset}",
                            consumeResult.TopicPartitionOffset);
                        continue;
                    }

                    _logger.LogInformation("Message received at {TopicPartitionOffset}: {Message}",
                        consumeResult.TopicPartitionOffset, raw);

                    PaymentKafkaDto? paymentDto;

                    try
                    {
                        paymentDto = JsonSerializer.Deserialize<PaymentKafkaDto>(raw, JsonOpts);
                    }
                    catch (JsonException jex)
                    {
                        _logger.LogError(jex, "Failed to deserialize Kafka message at {TopicPartitionOffset}",
                            consumeResult.TopicPartitionOffset);

                        continue;
                    }

                    if (paymentDto == null)
                    {
                        _logger.LogWarning("Deserialized dto is null at {TopicPartitionOffset}",
                            consumeResult.TopicPartitionOffset);
                        continue;
                    }

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                        await orderService.UpdateOrderAsync(paymentDto);

                        consumer.Commit(consumeResult);
                        _logger.LogInformation("Committed offset for {TopicPartitionOffset}",
                            consumeResult.TopicPartitionOffset);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed processing message. PaymentId {PaymentId} OrderId {OrderId}. Offset {TopicPartitionOffset}",
                            paymentDto.PaymentId, paymentDto.OrderId, consumeResult.TopicPartitionOffset);

                        // No commit here, message will be retried
                        // If this can permanently fail (like Order not found), you should implement a DLQ or retry limit
                    }
                }
                catch (ConsumeException cex)
                {
                    _logger.LogError(cex, "Kafka consume exception");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping...");
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing Kafka consumer");
            }
        }
    }
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}
