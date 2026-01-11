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

    public KafkaConsumerService(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaConsumerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_settings.Topic);

            _logger.LogInformation(
                "Kafka consumer started for topic: {Topic}",
                _settings.Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    _logger.LogInformation(
                        "Message received: {Message}",
                        consumeResult.Message.Value);

                    var paymentDto = JsonSerializer.Deserialize<PaymentKafkaDto>(
                        consumeResult.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (paymentDto == null)
                    {
                        _logger.LogWarning("Invalid Kafka message");
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider
                        .GetRequiredService<IOrderService>();

                    orderService.UpdateOrderAsync(paymentDto)
                        .GetAwaiter().GetResult();

                    consumer.Commit(consumeResult);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer crashed");
            }
            finally
            {
                consumer.Close();
            }
        }, stoppingToken);
    }
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}