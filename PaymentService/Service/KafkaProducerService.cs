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
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
    }
}
