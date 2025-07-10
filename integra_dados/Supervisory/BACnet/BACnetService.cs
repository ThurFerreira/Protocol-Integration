using System.IO.BACnet;
using System.IO.BACnet.Serialize;
using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel.RegistryModel.BACnet;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Notifier;

namespace integra_dados.Services.BACnet;

public class BACnetService(
    IRepository<BACnetRegistry> bacnetRepository,
    KafkaService kafkaService,
    Report report)

{
    public static Dictionary<string, BacnetClient>? BACnetClientList = new Dictionary<string, BacnetClient>();
    private static Dictionary<int, BACnetRegistry> registries = new Dictionary<int, BACnetRegistry>();

    public async Task<ResponseClient> Create(BACnetRegistry registry)
    {
        try
        {
            if (registry.TopicoBroker == null || registry.TopicoBroker.Equals(""))
            {
                registry.TopicoBroker = registry.Nome;
            }

            if (bacnetRepository.FindByName(registry.Nome) != null)
            {
                var savedRegistry = await bacnetRepository.Save(registry);
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
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: while editing supervisory ({e.Message}).");
            report.ModerateException(Status.RUNNING);

            return new ResponseClient(
                HttpStatusCode.InternalServerError,
                false,
                null,
                "Erro ao processar a solicitação."
            );
        }
    }

    public async Task<ResponseClient> Edit(BACnetRegistry registry)
    {
        try
        {
            BACnetRegistry bacnetFound = await bacnetRepository.ReplaceOne(registry);
            if (bacnetFound != null)
            {
                ReplaceRegistry(bacnetFound);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    bacnetFound,
                    "Registro atualizado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro com nome '{bacnetFound.Nome}' não foi encontrado."
            );
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: while editing supervisory ({e.Message}).");
            report.ModerateException(Status.RUNNING);

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
        bool deleted = await bacnetRepository.DeleteById(id);

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
        foreach (BACnetRegistry supervisoryRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(BACnetRegistry registry)
    {
        if (registry.FreqLeituraSeg > 0)
        {
            if (registry.IsTimeToSendMessage(registry.FreqLeituraSeg))
            {
                MonitorSupervisory(registry);
            }
        }
    }
    
    private void MonitorSupervisory(BACnetRegistry registry)
    {
        try
        {
            Event1000_1 event1000 = null;

            switch (registry.BacnetObjectTypes)
            {
                case BacnetObjectTypes.OBJECT_ANALOG_INPUT:
                    float? analogValue = ReadAnalog(registry);
                    registry.UpdateRegistry(analogValue);
                    ReplaceRegistry(registry);
                    if (registry.ShouldSendToBroker(analogValue))
                    {
                        if (analogValue != null)
                        {
                            event1000 = kafkaService.CreateBrokerPackage(registry, analogValue);
                            kafkaService.Publish(registry.TopicoBroker, event1000);

                        }
                    }
                    break;
                case BacnetObjectTypes.OBJECT_BINARY_INPUT:
                    bool? binaryValue = ReadBinary(registry);
                    registry.UpdateRegistry(binaryValue);
                    ReplaceRegistry(registry);
                    if (registry.ShouldSendToBroker(binaryValue))
                    {
                        if (binaryValue != null)
                        {
                            event1000 = kafkaService.CreateBrokerPackage(registry, binaryValue);
                            kafkaService.Publish(registry.TopicoBroker, event1000);
                        }
                    }
                    break;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("Error on monitor supervisory bacnet " + e.Message);
            report.ModerateException(Status.ERROR);
        }
    }

    public float? ReadAnalog(BACnetRegistry registry)
    {
        BacnetClient _bacnetClient = BACnetClientList.ContainsKey(registry.Ip) && BACnetClientList[registry.Ip] != null
            ? BACnetClientList[registry.Ip]
            : new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));
        _bacnetClient.Start();

        BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, registry.Ip);
        var objectId = new BacnetObjectId(registry.BacnetObjectTypes, registry.Instance);
        try
        {
            if (_bacnetClient.ReadPropertyRequest(
                    bacnetAddress,
                    objectId,
                    BacnetPropertyIds.PROP_PRESENT_VALUE,
                    out IList<BacnetValue> values
                ))
            {
                return Convert.ToSingle(values[0].Value);
            }
        }
        catch(Exception e)
        {
            report.LightException(Status.EMPTY_READ);
            Console.WriteLine("Falha na leitura do dispositivo bacnet.");
            return null;

        }

        return null;
    }

    public bool? ReadBinary(BACnetRegistry registry)
    {
        BacnetClient _bacnetClient = BACnetClientList.ContainsKey(registry.Ip) && BACnetClientList[registry.Ip] != null
            ? BACnetClientList[registry.Ip]
            : new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));
        _bacnetClient.Start();
        BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, registry.Ip);
        var objectId = new BacnetObjectId(registry.BacnetObjectTypes, registry.Instance);
        if (_bacnetClient.ReadPropertyRequest(
                bacnetAddress,
                objectId,
                BacnetPropertyIds.PROP_PRESENT_VALUE,
                out IList<BacnetValue> values
            ))
        {
            return Convert.ToBoolean(values[0].Value);
        }
        else
        {
            report.LightException(Status.EMPTY_READ);
            Console.WriteLine("Falha na leitura.");
            return null;

        }
    }

    public static void AddRegistry(BACnetRegistry registry)
    {
        registries.Add(registry.CodeId, registry);
    }

    public List<Registry> GetRegistries()
    {
        return registries.Values.Cast<Registry>().ToList(); //buscar no banco isso aqui é muito burro
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

    public static void ReplaceRegistry(BACnetRegistry edited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.CodeId.Equals(edited.CodeId))
            {
                registries[registry.CodeId] = edited;
            }
        }
    }

    public static void StartRegistries(List<BACnetRegistry> updateRegistries)
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