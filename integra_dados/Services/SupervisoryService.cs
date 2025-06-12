using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryService(ISupervisoryRepository supervisoryRepository, KafkaService kafkaService)
{
    // Report _report;
    public async Task<ResponseClient> Create(SupervisoryRegistry registry)
    {
        if(registry.TopicoBroker == null || registry.TopicoBroker.Equals("")){
            registry.TopicoBroker = registry.Nome;
        }
        
        if (supervisoryRepository.FindByName(registry.Nome) != null) {
            var savedRegistry = await supervisoryRepository.Save(registry);
            RegistryManager.AddRegistry(savedRegistry);
            return new ResponseClient(
                HttpStatusCode.OK,
                true, savedRegistry,
                "Registro de previsão adicionado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false, null,
            "Registro de previsão com nome '"+ registry.Nome+"' já foi criado.");
    }
    
    public ResponseClient Edit(SupervisoryRegistry supervisoryRegistry)
    {
        try
        {
            var supervisoryFound = supervisoryRepository.FindById(supervisoryRegistry.IdSistema);
            if (supervisoryFound != null)
            {
                supervisoryRegistry.IdSistema = supervisoryFound.Id;

                Task<SupervisoryRegistry> supervisoryEdited = supervisoryRepository.Save(supervisoryRegistry);

                RegistryManager.ReplaceRegistry(supervisoryEdited.Result);

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
    
    public void Delete(int idSistema) {
        supervisoryRepository.DeleteByIdSistema(idSistema);
        RegistryManager.UpdateRegistries(supervisoryRepository.FindAll().Result);
    }
    
    public void TriggerBroker(List<SupervisoryRegistry> registries)
    {
        foreach (SupervisoryRegistry supervisoryRegistry in registries)
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(SupervisoryRegistry registry) {
        if (registry.FreqLeituraSeg > 0) {
            if (registry.IsTimeToSendMessage(registry.FreqLeituraSeg)) {
//                executorService.submit(() -> {
                MonitorSupervisory(registry);
//                });
            } registry.IncrementCounter();
        }
    }

    private void MonitorSupervisory(SupervisoryRegistry registry)
    {
        if (registry.TipoDado.Equals("discreteInput"))
        {
            var reisterValue = GetSupervisoryDiscreteInputValue(registry);
            registry.UpdateRegistry(reisterValue);
            RegistryManager.ReplaceRegistry(registry);
            // if (supervisoryRegistry.shouldSendToBroker(registerValue)) {
            //     if (registerValue != VALUE_NOT_VALID) {
            //         var brokerPackage = createBrokerPackage(supervisoryRegistry, registerValue);
            //         sendSupervisoryInfoToBroker(brokerPackage, supervisoryRegistry.getTopicoBroker());
            //     }
            // }
            if (registry.ShouldSendToBroker(reisterValue))
            {
                if (reisterValue != -1)
                {
                    Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, reisterValue);
                    s
                }
            }
        }
    }

    private int GetSupervisoryDiscreteInputValue(SupervisoryRegistry registry)
    {
        return ModbusApi.ReadDiscreteInput(registry.EnderecoInicio, registry.QuantidadeTags);
    }
}