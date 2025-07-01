using Confluent.Kafka;
using integra_dados.Config;
using integra_dados.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace integra_dados.Services.Kafka;

public class KafkaService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaService> _logger;

    public KafkaService(IOptions<KafkaConfig> kafkaConfig, ILogger<KafkaService> logger)
    {
        _logger = logger;
        
        //Configurando o producer kafka
        var config = new ProducerConfig()
        {
            BootstrapServers = kafkaConfig.Value.BootstrapServers,
        };
        
        //criando de fato o produtor kafka
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Event1000_1 CreateBrokerPackage(Registry registry, int? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            0,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, bool? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            0,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, float[]? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            0,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, float? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            0,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }

    public async void Publish(string topic, Event1000_1 pkg)
    {
        try
        {
            ;
            string json = JsonConvert.SerializeObject(pkg);
            DeliveryResult<string, string> result = await _producer.ProduceAsync(topic, new Message<string, string> { Key = "", Value = json} );
            // _logger.LogInformation("Message " + json + $" delivered to {result.TopicPartitionOffset}");
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}"); ;
        }
    }
    
}