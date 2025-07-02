using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;

namespace integra_dados.Services;

public class ForecastService(
    IRepository<ForecastRegistry> forecastRepository,
    KafkaService kafkaService,
    ModbusService modbusService,
    WindyApiService windyService)
{
    private static Dictionary<int, ForecastRegistry> registries = new Dictionary<int, ForecastRegistry>();

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
            return new ResponseClient();
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

    public async Task<ResponseClient> Edit(ForecastRegistry forecastRegistry)
    {
        ForecastRegistry forecastFound = await forecastRepository.ReplaceOne(forecastRegistry);
        if (forecastFound != null)
        {
            ReplaceRegistry(forecastFound);

            return new ResponseClient(
                HttpStatusCode.OK,
                true, forecastFound,
                "Registro de previsão atualizado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false, null,
            "Registro de previsão com nome '" + forecastRegistry.Nome + "' não foi encontrado."
        );
    }

    public async Task<ResponseClient> Delete(int id)
    {
        bool deleted = await forecastRepository.DeleteById(id);

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

    public async Task<ResponseClient> GetAllForecastForVariable(string name)
    {
        List<ForecastRegistry> response = await forecastRepository.FindByName(name);
        return new ResponseClient(HttpStatusCode.OK, true, response, "Busca realizada com sucesso");
    }

    public async Task<ResponseClient> GetLocationForecast(Location location, string varType)
    {
        WindyResponse response = await windyService.GetWindyForecast(location, varType);
        return new ResponseClient(HttpStatusCode.OK, true, response, "Busca realizada com sucesso");
    }


    void AddRegistry(ForecastRegistry registry)
    {
        registries.Add(registry.CodeId, registry);
    }

    public static List<ForecastRegistry> GetRegistries()
    {
        return registries.Values.ToList();
    }

    public static ResponseClient GetOne(int id)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.CodeId.Equals(id));

        return CreateResponseToFoundRegistry(id, foundRegistry.Value);
    }

    static ResponseClient CreateResponseToFoundRegistry(int idSistema, ForecastRegistry? foundRegistry)
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
            if (registry.CodeId.Equals(supervisoryEdited.CodeId))
            {
                registries[registry.CodeId] = supervisoryEdited;
            }
        }
    }

    public static void StartRegistries(List<ForecastRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.CodeId, supervisoryRegistry);
        }
    }

    void DeleteRegisry(int id)
    {
        registries.Remove(id);
    }

    public void TriggerBroker(List<ForecastRegistry> registries)
    {
        foreach (ForecastRegistry forecastRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(forecastRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(ForecastRegistry registry)
    {
        if (registry.FreqLeituraSeg > 0)
        {
            if (registry.IsTimeToSendMessage(registry.FreqLeituraSeg))
            {
                //TODO ADICIONAR THREAD NO MONITOR SUPERVISORY
                MonitorForecast(registry);
            }
        }
    }

    private async void MonitorForecast(ForecastRegistry registry)
    {
        WindyResponse response = await windyService.GetWindyForecast(registry.Location, registry.TipoDado);

        try
        {
            switch (registry.TipoDado)
            {
                case "temp":
                    registry.UpdateRegistry(response.TempSurface[0]);
                    float tempValue = (float)Math.Round(response.TempSurface[0] - 273.15F, 1);

                    if (registry.ShouldSendToBroker(tempValue))
                    {
                        if (tempValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, tempValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "convPrecip":
                    registry.UpdateRegistry(response.PrecipSurface[0]);
                    float precipValue = response.PrecipSurface[0];

                    if (registry.ShouldSendToBroker(precipValue))
                    {
                        if (precipValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, precipValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "wind":
                    registry.UpdateRegistry(response.WindX[0]); //TODO arrumar wind
                    float[] windValue = new float[] { response.WindX[0], response.WindY[1] };

                    if (registry.ShouldSendToBroker(windValue))
                    {
                        if (windValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, windValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "cape":
                    registry.UpdateRegistry(response.CapeSurface[0]);
                    float capeValue = response.CapeSurface[0];

                    if (registry.ShouldSendToBroker(capeValue))
                    {
                        if (capeValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, capeValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "rh":
                    registry.UpdateRegistry(response.Rh[0]);
                    float rhValue = response.Rh[0];

                    if (registry.ShouldSendToBroker(rhValue))
                    {
                        if (rhValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, rhValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "lclouds":
                    registry.UpdateRegistry(response.LowCloudsCoverage[0]);
                    float lcValue = response.LowCloudsCoverage[0];

                    if (registry.ShouldSendToBroker(lcValue))
                    {
                        if (lcValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, lcValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "hclouds":
                    registry.UpdateRegistry(response.HighCloudsCoverage[0]);
                    float hcValue = response.HighCloudsCoverage[0];

                    if (registry.ShouldSendToBroker(hcValue))
                    {
                        if (hcValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(registry, hcValue);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            registry.UpgradeStatusToUnavailable();
        }
    }
}