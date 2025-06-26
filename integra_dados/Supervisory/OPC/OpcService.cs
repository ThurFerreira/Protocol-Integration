using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;

namespace integra_dados.Supervisory.OPC;

public class OpcService(
    IRepository<OpcRegistry> opcRepository,
    KafkaService kafkaService)
{
    
    private static Dictionary<int, OpcRegistry> registries = new Dictionary<int, OpcRegistry>();

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

    public async Task<ResponseClient> Edit(OpcRegistry modbusRegistry)
    {
        try
        {
            OpcRegistry modbusFound = await opcRepository.ReplaceOne(modbusRegistry);
            if (modbusFound != null)
            {
                ReplaceRegistry(modbusFound);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    modbusRegistry,
                    "Registro atualizado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro com nome '{modbusRegistry.Nome}' não foi encontrado."
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
                //TODO ADICIONAR THREAD NO MONITOR SUPERVISORY
                MonitorOpc(registry);
            }
        }
    }

    private void MonitorOpc(OpcRegistry registry)
    {
    }

    public static void AddRegistry(OpcRegistry registry)
    {
        registries.Add(registry.Id, registry);
    }

    public List<OpcRegistry> GetRegistries()
    {
        return registries.Values.ToList(); //buscar no banco isso aqui é muito burro
    }

    public static ResponseClient GetOne(int id)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.Id.Equals(id));

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
            if (registry.Id.Equals(modbusEdited.Id))
            {
                registries[registry.Id] = modbusEdited;
            }
        }
    }

    public static void StartRegistries(List<OpcRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.Id, supervisoryRegistry);
        }
    }

    public static void DeleteRegisry(int id)
    {
        registries.Remove(id);
    }
}