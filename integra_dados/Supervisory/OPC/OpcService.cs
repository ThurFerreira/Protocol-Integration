using System.Collections.Concurrent;
using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Services.Notifier;
using MongoDB.Bson;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace integra_dados.Supervisory.OPC;

public class OpcService(
    IRepository<OpcRegistry> opcRepository,
    KafkaService kafkaService,
    Report report)
{
    private static Dictionary<int, OpcRegistry> registries = new Dictionary<int, OpcRegistry>();
    public static OpcClient? OpcClient = new OpcClient();

    public static ConcurrentDictionary<string, OpcClient> clientesConectados =
        new ConcurrentDictionary<string, OpcClient>();

    public void ConnectClientOpc(OpcRegistry registry)
    {
        int tentativas = 0;
        // string link = "opc.tcp://10.3.195.224:4840";
        string link = registry.LinkConexao;
        if (!clientesConectados.ContainsKey(link))
        {
            OpcClient = new OpcClient(link);
            while (tentativas < 5)
            {
                try
                {
                    OpcClient.Connect();
                    break;
                }
                catch (Exception ex)
                {
                    tentativas++;
                    registry.UpgradeStatusToUnavailable();
                    Task.Delay(5000).Wait();
                    Console.WriteLine("Client opc failed to connect");
                }
            }
        }

        if (OpcClient.State == OpcClientState.Connected)
        {
            clientesConectados[link] = OpcClient;
        }

    }


    public List<OpcValue> ReadNodes(OpcRegistry opcRegistry)
    {
        try
        {
            if (OpcClient == null || OpcClient.State != OpcClientState.Connected)
            {
                ConnectClientOpc(opcRegistry);
            }

            if (OpcClient.State == OpcClientState.Connected)
            {
                OpcReadNode[] readNode = new OpcReadNode[opcRegistry.NodeAddress.Count];
                for (int i = 0; i < opcRegistry.NodeAddress.Count; i++)
                {
                    readNode[i] = new OpcReadNode(opcRegistry.NodeAddress[i]);
                }

                List<OpcValue> resultadoBusca = OpcClient.ReadNodes(readNode).ToList();

                if (resultadoBusca.Count > 0)
                {
                    return resultadoBusca;
                }
            }
            else
            {
                opcRegistry.UpgradeStatusToUnavailable();
                report.LightException(Status.NOT_CONNECTED);
                return null;
            }
        }
        catch (Exception ex)
        {
            opcRegistry.UpgradeStatusToUnavailable();
            Console.WriteLine(ex.Message);
        }

        return new List<OpcValue>();
    }

    public async Task<ResponseClient> Create(OpcRegistry registry)
    {
        if (registry.TopicoBroker == null || registry.TopicoBroker.Equals(""))
        {
            registry.TopicoBroker = registry.Nome;
        }

        if (opcRepository.FindByName(registry.Nome) != null)
        {
            var savedRegistry = await opcRepository.Save(registry);
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
            OpcRegistry opcFound = await opcRepository.ReplaceOne(opcRegistry);
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
        bool deleted = await opcRepository.DeleteById(id);

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

    public void TriggerBroker(List<OpcRegistry> registries)
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
        List<OpcValue> opcResponse = ReadNodes(registry);
        if (opcResponse != null)
        {
            foreach (var res in opcResponse)
            {
                brokerPackage = kafkaService.CreateBrokerPackage(registry, Convert.ToInt64(res.Value));
                kafkaService.Publish(registry.TopicoBroker, brokerPackage);
            }
        }
    }

    public static void AddRegistry(OpcRegistry registry)
    {
        registries.Add(registry.CodeId, registry);
    }

    public List<OpcRegistry> GetRegistries()
    {
        return registries.Values.ToList(); //buscar no banco isso aqui é muito burro
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