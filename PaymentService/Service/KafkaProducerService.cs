using Confluent.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public KafkaProducerService(IConfiguration config)
    {
        var kafkaConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(kafkaConfig).Build();
        _topic = config["Kafka:Topic"];
    }

    public async Task ProduceAsync(string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Message delivered to {result.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            throw;
        }
    }
        
}
