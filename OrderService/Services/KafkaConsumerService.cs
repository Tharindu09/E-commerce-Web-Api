using System;

namespace OrderService.Services;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Dtos;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _settings;
    private IConsumer<Ignore, string> _consumer;

    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumerService(IOptions<KafkaSettings> settings, ILogger<KafkaConsumerService> logger,IServiceScopeFactory scopeFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(_settings.Topic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka consumer started for topic: {Topic}", _settings.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult != null)
                    {
                        _logger.LogInformation("Message received: {Message}", consumeResult.Message.Value);
                        var paymentDto = JsonSerializer.Deserialize<PaymentKafkaDto>(
                            consumeResult.Message.Value,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (paymentDto == null)
                        {
                            _logger.LogWarning("Failed to deserialize Kafka message");
                            continue;
                        }
                        var _service = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IOrderService>(); // Resolve service within scope

                        await _service.UpdateOrderAsync(paymentDto);
                         // Commit after successful processing
                        _consumer.Commit(consumeResult);
                    }
                    
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError("Consume error: {Reason}", ex.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping...");
        }
        finally
        {
            _consumer.Close();
        }
    }
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

