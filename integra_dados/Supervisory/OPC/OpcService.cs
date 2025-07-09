using System.Collections.Concurrent;
using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Services.Notifier;
using MongoDB.Bson;
using Opc.Ua;
using Opc.Ua.Client;


namespace integra_dados.Supervisory.OPC;

public class OpcService
{
    private static Dictionary<int, OpcRegistry> registries = new Dictionary<int, OpcRegistry>();
    private static ApplicationConfiguration config = new ApplicationConfiguration();
    private IRepository<OpcRegistry> _opcRepository;
    private KafkaService _kafkaService;
    private Report _report;


    public static ConcurrentDictionary<string, Session> clientesConectados = new ConcurrentDictionary<string, Session>();

    public OpcService(IRepository<OpcRegistry> opcRepository, KafkaService kafkaService, Report report)
    {
        _opcRepository = opcRepository;
        _kafkaService = kafkaService;
        _report = report;
        
        config = new ApplicationConfiguration()
        {
            ApplicationName = "IEDMonitor_Gerenciador",
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier(),
                AutoAcceptUntrustedCertificates = true
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
        };

        config.Validate(ApplicationType.Client);
    }

    public static async Task<Session> ConnectClientOpc(OpcRegistry registry)
    {
        int tentativas = 0;
        // string link = "opc.tcp://10.3.195.224:4840";
        string link = registry.GetConnectionLink();
        Session primarySession = null;

        if (!clientesConectados.ContainsKey(link))
        {
            while (tentativas < 5)
            {
                try
                {
                    var selectedEndpoint = CoreClientUtils.SelectEndpoint(link, false);
                    var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);
                    primarySession = await Session.Create(config, endpoint, false, "IEDMonitor_Integra", 60000, null, null);
                   
                    if (primarySession.Connected)
                    {
                        clientesConectados[link] = primarySession;
                        Console.WriteLine("OpcUa connected successfully");
                    }
                    
                    break;
                }
                catch (Exception ex)
                {
                    tentativas++;
                    registry.UpgradeStatusToUnavailable();
                    Console.WriteLine("Client opc failed to connect " + ex.Message);
                    Task.Delay(5000).Wait();
                }
            }
        }
        
        return primarySession;
    }


    public DataValueCollection ReadNodes(OpcRegistry opcRegistry)
    {
        try
        {
            clientesConectados.TryGetValue(opcRegistry.GetConnectionLink(), out Session? opcClient);
            if (opcClient == null || !opcClient.Connected)
            {
                opcRegistry.UpgradeStatusToUnavailable();
                opcClient = ConnectClientOpc(opcRegistry).Result;
            }

            if (opcClient != null && opcClient.Connected)
            {
                ReadValueIdCollection readCollection = new ReadValueIdCollection();
                for (int i = 0; i < opcRegistry.NodeAddress.Count; i++)
                {
                    readCollection.Add(
                        new ReadValueId
                        {
                            NodeId = new NodeId(opcRegistry.NodeAddress[i]), //endereço do no para leitura
                            AttributeId = Attributes.Value //escolhendo o que deseja retornar (nesse caso o valor atual do no)
                        }
                        );
                }

                opcClient.Read(null, 0, TimestampsToReturn.Neither, readCollection, out DataValueCollection results, out _);

                if (results != null && results.Count > 0)
                {
                    return results;
                }
            }
            else
            {
                opcRegistry.UpgradeStatusToUnavailable();
                _report.LightException(Status.NOT_CONNECTED);
                return null;
            }
        }
        catch (Exception ex)
        {
            opcRegistry.UpgradeStatusToUnavailable();
            Console.WriteLine(ex.Message);
        }

        return new DataValueCollection();
    }

    public static void WriteNodes(Event1000_2 event10002)
    {
        OpcRegistry registry = registries[event10002.IdSistema];
        clientesConectados.TryGetValue(registry.GetConnectionLink(), out Session? opcClient);
        if (opcClient == null || !opcClient.Connected)
        {
            opcClient = ConnectClientOpc(registry).Result;
        }

        if (opcClient != null && opcClient.Connected)
        {
            WriteValueCollection valuesToWrite = new WriteValueCollection();
            StatusCodeCollection results = new StatusCodeCollection();
            DiagnosticInfoCollection diagnosticInfos;
            bool? firstValue = null;
            bool? secondValue = null;

            switch (event10002.WriteProtocol)
            {
                case WriteProtocol.LATCH_ON:
                    firstValue = true;
                    break;
                case WriteProtocol.LATCH_OFF:
                    firstValue = true;
                    break;
                case WriteProtocol.PULSE_ON:
                    firstValue = true;
                    secondValue = false;
                    break;
                case WriteProtocol.PULSE_OFF:
                    firstValue = false;
                    secondValue = true;
                    break;
            }

            if (firstValue.HasValue)
            {
                valuesToWrite = new WriteValueCollection { GetWriteValue(registry.NodeAddress[0], firstValue.Value) };
                opcClient.Write(null, valuesToWrite, out results, out diagnosticInfos);
            }

            if (secondValue.HasValue)
            {
                valuesToWrite = new WriteValueCollection { GetWriteValue(registry.NodeAddress[0], secondValue.Value) };
                opcClient.Write(null, valuesToWrite, out results, out diagnosticInfos);
            }


            if (StatusCode.IsGood(results[0]))
                Console.WriteLine("Valor escrito com sucesso!");
            else
                Console.WriteLine($"Erro ao escrever: {results[0]}");
        }
    }

    public static WriteValue GetWriteValue(string nodeId , bool value)
    {
        return new WriteValue
        {
            NodeId = new NodeId(nodeId),
            AttributeId = Attributes.Value,
            Value = new DataValue(new Variant())
        };
    }

    public async Task<ResponseClient> Create(OpcRegistry registry)
    {
        if (registry.TopicoBroker == null || registry.TopicoBroker.Equals(""))
        {
            registry.TopicoBroker = registry.Nome;
        }

        if (_opcRepository.FindByName(registry.Nome) != null)
        {
            var savedRegistry = await _opcRepository.Save(registry);
            AddRegistry(savedRegistry);
            return new ResponseClient(
                HttpStatusCode.OK,
                true, savedRegistry,
                "Registro de previsão adicionado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false, null,
            "Registro de previsão com nome '" + registry.Nome + "' já foi criado.");
    }

    public async Task<ResponseClient> Edit(OpcRegistry opcRegistry)
    {
        try
        {
            OpcRegistry opcFound = await _opcRepository.ReplaceOne(opcRegistry);
            if (opcFound != null)
            {
                ReplaceRegistry(opcFound);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    opcRegistry,
                    "Registro atualizado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro com nome '{opcRegistry.Nome}' não foi encontrado."
            );
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: while editing supervisory ({e.Message}).");
            // report.ModerateException(Status.Running);

            return new ResponseClient(
                HttpStatusCode.InternalServerError,
                false,
                null,
                "Erro ao processar a solicitação."
            );
        }
    }

    public async Task<ResponseClient> Delete(int id)
    {
        bool deleted = await _opcRepository.DeleteById(id);

        if (deleted)
        {
            DeleteRegisry(id);
            return new ResponseClient(
                HttpStatusCode.OK,
                true,
                "Registro de previsão deletado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false,
            "Registro de previsão com id '" + id + "' não foi encontrado."
        );
    }

    public void TriggerBroker(List<Registry> registries)
    {
        
        foreach (OpcRegistry supervisoryRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(OpcRegistry registry)
    {
        if (registry.FreqLeituraSeg > 0)
        {
            if (registry.IsTimeToSendMessage(registry.FreqLeituraSeg))
            {
                MonitorOpc(registry);
            }
        }
    }

    private void MonitorOpc(OpcRegistry registry)
    {
        Event1000_1 brokerPackage;
        DataValueCollection opcResponse = ReadNodes(registry);
        if (opcResponse != null)
        {
            foreach (var res in opcResponse)
            {
                brokerPackage = _kafkaService.CreateBrokerPackage(registry, Convert.ToInt64(res.Value));
                _kafkaService.Publish(registry.TopicoBroker, brokerPackage);
            }
        }
    }

    public static void AddRegistry(OpcRegistry registry)
    {
        registries.Add(registry.CodeId, registry);
    }

    public List<Registry> GetRegistries()
    {
        return  registries.Values.Cast<Registry>().ToList();; 
    }

    public static ResponseClient GetOne(int id)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.CodeId.Equals(id));

        return CreateResponseToFoundRegistry(id, foundRegistry.Value);
    }

    private static ResponseClient CreateResponseToFoundRegistry(int idSistema, Registry? foundRegistry)
    {
        if (foundRegistry != null)
        {
            return new ResponseClient(
                HttpStatusCode.OK,
                true,
                foundRegistry,
                "Previsão recuperado com sucesso."
            );
        }
        else
        {
            return new ResponseClient(
                HttpStatusCode.OK,
                false,
                null,
                $"Não foi possível recuperar a previsão cujo id = {idSistema}."
            );
        }
    }

    public static void ReplaceRegistry(OpcRegistry modbusEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.CodeId.Equals(modbusEdited.CodeId))
            {
                registries[registry.CodeId] = modbusEdited;
            }
        }
    }

    public static void StartRegistries(List<OpcRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.CodeId, supervisoryRegistry);
        }
    }

    public static void DeleteRegisry(int id)
    {
        registries.Remove(id);
    }
}