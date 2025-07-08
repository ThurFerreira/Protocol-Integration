using Confluent.Kafka;
using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Util;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace integra_dados.Services.Kafka;

public class KafkaService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaService> _logger;
    public ConsumerConfig ConsumerConfig { get; set; }
    public ProducerConfig ProducerConfig { get; set; }

    
    public KafkaService(IOptions<KafkaConfig> kafkaConfig, ILogger<KafkaService> logger)
    {
        _logger = logger;
        
        //Configurando o producer kafka
        ConsumerConfig = new ConsumerConfig()
        {
            BootstrapServers = kafkaConfig.Value.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Latest,
            GroupId = "write-protocol-group"
        };
        
        //Configurando o producer kafka
        ProducerConfig = new ProducerConfig()
        {
            BootstrapServers = kafkaConfig.Value.BootstrapServers,
        };

        Console.WriteLine("KAFKA BOOTSTRAP SERVERS "+ kafkaConfig.Value.BootstrapServers);
        //criando de fato o produtor kafka
        _producer = new ProducerBuilder<string, string>(ProducerConfig).Build();
    }

    public Event1000_1 CreateBrokerPackage(Registry registry, int? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, bool? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, float[]? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
            reisterValue,
            registry.UltimaAtualizacao
        );
    }
    
    public Event1000_1 CreateBrokerPackage(Registry registry, float? reisterValue)
    {
        return new Event1000_1(
            registry.CodeId,
            registry.Nome,
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

    public void ConsumeAndWriteDispositive(string topic, Action<Event1000_2> writeDiscreteInput)
    {
        using var consumer = new ConsumerBuilder<string, Event1000_2>(ConsumerConfig).SetValueDeserializer(new SimpleJsonDeserializer<Event1000_2>()).Build();
        consumer.Subscribe(topic);
        
        CancellationTokenSource cts = new();

        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                ConsumeResult<string, Event1000_2> result = new ConsumeResult<string, Event1000_2>();
                try
                {
                    result = consumer.Consume(cts.Token);

                    if (result.Message.Value != null)
                    {
                        writeDiscreteInput(result.Message.Value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("No messages in topic " + topic);
                    Task.Delay(10000);

                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Consumer closed.");
        }
        finally
        {
            consumer.Close(); // fecha a conexão com o broker
        }
    }
}