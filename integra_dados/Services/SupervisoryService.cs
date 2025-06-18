using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;

namespace integra_dados.Services;

public class SupervisoryService(
    IRepository<SupervisoryRegistry> supervisoryRepository,
    KafkaService kafkaService,
    ModbusService modbusService)
{
    private static Dictionary<string, SupervisoryRegistry> registries = new Dictionary<string, SupervisoryRegistry>();

    // Report _report;
    public async Task<ResponseClient> Create(SupervisoryRegistry registry)
    {
        if (registry.TopicoBroker == null || registry.TopicoBroker.Equals(""))
        {
            registry.TopicoBroker = registry.Nome;
        }

        if (supervisoryRepository.FindByName(registry.Nome) != null)
        {
            var savedRegistry = await supervisoryRepository.Save(registry);
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

    public ResponseClient Edit(SupervisoryRegistry supervisoryRegistry)
    {
        try
        {
            var supervisoryFound = supervisoryRepository.FindById(supervisoryRegistry.IdSistema);
            if (supervisoryFound != null)
            {
                supervisoryRegistry.IdSistema = supervisoryFound.Id.ToString();

                Task<SupervisoryRegistry> supervisoryEdited = supervisoryRepository.Save(supervisoryRegistry);

                ReplaceRegistry(supervisoryEdited.Result);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    supervisoryEdited,
                    "Registro atualizado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro com nome '{supervisoryRegistry.Nome}' não foi encontrado."
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

    public async Task<ResponseClient> Delete(string id)
    {
        bool deleted = await supervisoryRepository.DeleteById(id);

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

    public void TriggerBroker(List<SupervisoryRegistry> registries)
    {
        foreach (SupervisoryRegistry supervisoryRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(SupervisoryRegistry registry)
    {
        if (int.Parse(registry.FreqLeituraSeg) > 0)
        {
            if (registry.IsTimeToSendMessage(int.Parse(registry.FreqLeituraSeg)))
            {
                //TODO ADICIONAR THREAD NO MONITOR SUPERVISORY
                MonitorSupervisory(registry);
            }
        }
    }

    private void MonitorSupervisory(SupervisoryRegistry registry)
    {
        switch (registry.TipoDado)
        {
            case "discreteInput":
                var registerValueStatus = ModbusService.ReadDiscreteInput(registry);
                registry.UpdateRegistry(registerValueStatus);
                ReplaceRegistry(registry);
                if (registry.ShouldSendToBroker(registerValueStatus))
                {
                    if (registerValueStatus != null)
                    {
                        Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, registerValueStatus);
                        kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                    }
                }

                break;
            case "inputRegister":
                var registerValueIntRegister = ModbusService.ReadInputRegister(registry);
                registry.UpdateRegistry(registerValueIntRegister);
                ReplaceRegistry(registry);
                if (registry.ShouldSendToBroker(registerValueIntRegister))
                {
                    if (registerValueIntRegister != -1)
                    {
                        Event1000_1 brokerPackage =
                            kafkaService.CreateBrokerPackage(registry, registerValueIntRegister);
                        kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                    }
                }

                break;
            case "coil":
                var registerValueCoil = ModbusService.ReadCoil(registry);
                registry.UpdateRegistry(registerValueCoil);
                ReplaceRegistry(registry);
                if (registry.ShouldSendToBroker(registerValueCoil))
                {
                    if (registerValueCoil != null)
                    {
                        Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, registerValueCoil);
                        kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                    }
                }

                break;
            case "holdingRegister":
                var registerValueHolding = ModbusService.ReadHoldingRegister(registry);
                registry.UpdateRegistry(registerValueHolding);
                ReplaceRegistry(registry);
                if (registry.ShouldSendToBroker(registerValueHolding))
                {
                    if (registerValueHolding != -1)
                    {
                        Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, registerValueHolding);
                        kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                    }
                }

                break;
        }
    }

    public static void AddRegistry(SupervisoryRegistry registry)
    {
        registries.Add(registry.Id, registry);
    }

    public static List<SupervisoryRegistry> GetRegistries()
    {
        return registries.Values.ToList();
    }

    public static ResponseClient GetOne(int idSistema)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.IdSistema.Equals(idSistema));

        return CreateResponseToFoundRegistry(idSistema, foundRegistry.Value);
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

    public static void ReplaceRegistry(SupervisoryRegistry supervisoryEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.IdSistema.Equals(supervisoryEdited.IdSistema))
            {
                registries[registry.IdSistema] = supervisoryEdited;
            }
        }
    }

    public static void StartRegistries(List<SupervisoryRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.Id, supervisoryRegistry);
        }
    }

    public static void DeleteRegisry(string id)
    {
        registries.Remove(id);
    }
}