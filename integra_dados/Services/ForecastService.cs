using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class ForecastService(
    IRepository<ForecastRegistry> forecastRepository,
    KafkaService kafkaService,
    ModbusService modbusService)
{
    private static Dictionary<string, ForecastRegistry> registries = new Dictionary<string, ForecastRegistry>();
    
    public async Task<ResponseClient> Create(ForecastRegistry forecastRegistry)
    {
        try
        {
            if (forecastRegistry.TopicoBroker == null || forecastRegistry.TopicoBroker.Equals(""))
            {
                forecastRegistry.TopicoBroker = forecastRegistry.Nome;
            }
            
            if (forecastRepository.FindByNameAndVarType(forecastRegistry.Nome, forecastRegistry.TipoDado) != null)
            {
                ForecastRegistry forecastCreated = await forecastRepository.Save(forecastRegistry);
                AddRegistry(forecastCreated);
                
                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    forecastCreated,
                    "Registro de previsão adicionado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro de previsão com nome {forecastRegistry.Nome} já foi criado."
            );
        }
        catch (Exception)
        {
            //TODO notificador
            // report.ModerateException(Status.Running);
            // return new ResponseClient(
            //     HttpStatusCode.InternalServerError,
            //     false,
            //     null,
            //     "Erro ao processar a solicitação."
            // );
        }
    }
    
    void AddRegistry(ForecastRegistry registry)
    {
        registries.Add(registry.Id, registry);
    }

    List<ForecastRegistry> GetRegistries()
    {
        return registries.Values.ToList();
    }

    ResponseClient GetOne(int idSistema)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.IdSistema.Equals(idSistema));

        return CreateResponseToFoundRegistry(idSistema, foundRegistry.Value);
    }

    ResponseClient CreateResponseToFoundRegistry(int idSistema, ForecastRegistry? foundRegistry)
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

    void ReplaceRegistry(ForecastRegistry supervisoryEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.IdSistema.Equals(supervisoryEdited.IdSistema))
            {
                registries[registry.IdSistema] = supervisoryEdited;
            }
        }
    }

    void StartRegistries(List<ForecastRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.Id, supervisoryRegistry);
        }
    }

    void DeleteRegisry(string id)
    {
        registries.Remove(id);
    }
}