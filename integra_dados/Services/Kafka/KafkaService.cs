using Confluent.Kafka;
using integra_dados.Config;
using integra_dados.Models;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Util;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace integra_dados.Services.Kafka;

public class KafkaService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaService> _logger;
    public ProducerConfig ProducerConfig { get; set; }
    public JsonSerializerSettings JsonSettings { get; set; }
    public KafkaConfig KafkaConfig { get; set; }

    
    public KafkaService(IOptions<KafkaConfig> kafkaConfig, ILogger<KafkaService> logger)
    {
        _logger = logger;
        KafkaConfig = kafkaConfig.Value;
        
        //Configurando o producer kafka
        ProducerConfig = new ProducerConfig()
        {
            BootstrapServers = kafkaConfig.Value.BootstrapServers,
        };
        
        JsonSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

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
        var groupId = "consumer-" + new Random().Next(0, 100);
        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaConfig.BootstrapServers,
            GroupId =  groupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
        };
        

        using var consumer = new ConsumerBuilder<Ignore, Event1000_2>(config).SetValueDeserializer(new SimpleJsonDeserializer<Event1000_2>()) .Build();

        consumer.Subscribe(topic);

        CancellationTokenSource cts = new();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cts.Token);
                    var value= result.Message.Value;
                    Console.WriteLine($"Mensagem recebida: {result.Message.Value}");
                    if (value != null)
                    {
                        writeDiscreteInput(value);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Erro ao consumir: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelado.");
        }
        finally
        {
            consumer.Close();
        }
    }
    
    //     ConsumerConfig = new ConsumerConfig()
    //     {
    //         BootstrapServers = KafkaConfig.BootstrapServers,
    //         AutoOffsetReset = AutoOffsetReset.Earliest,
    //         GroupId = "write-protocol-group",
    //         EnableAutoCommit = true
    //     };
    //     
    //     using var consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build();
    //     consumer.Subscribe("SUPERVISORY_WRITE_TOPIC");
    //     
    //     // CancellationTokenSource cts = new();
    //     //
    //     // Console.CancelKeyPress += (_, e) => {
    //     //     e.Cancel = true;
    //     //     cts.Cancel();
    //     //     cts.CancelAfter(TimeSpan.FromSeconds(10));
    //     // };
    //
    //     try
    //     {
    //         while (true)
    //         {
    //             // ConsumeResult<Ignore, string> result = new ConsumeResult<Ignore, string>();
    //             try
    //             {
    //                 var result = consumer.Consume(TimeSpan.FromSeconds(5));
    //                 var value = JsonConvert.DeserializeObject<Event1000_2>(result.Message.Value, JsonSettings);
    //                 Console.WriteLine("Mensagem recebida:\n" + result.Message.Value);
    //

    //             }
    //             catch (Exception e)
    //             {
    //                 Console.WriteLine("No messages in topic " + topic + " " + e.Message);
    //                 Task.Delay(10000);
    //
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine("Consumer closed.");
    //     }
    //     finally
    //     {
    //         consumer.Close(); // fecha a conexão com o broker
    //     }
}