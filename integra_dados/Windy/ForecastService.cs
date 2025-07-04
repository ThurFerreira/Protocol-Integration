using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;

namespace integra_dados.Services;

public class ForecastService(
    IRepository<ForecastReadRegistry> forecastRepository,
    KafkaService kafkaService,
    ModbusService modbusService,
    WindyApiService windyService)
{
    private static Dictionary<int, ForecastReadRegistry> registries = new Dictionary<int, ForecastReadRegistry>();

    public async Task<ResponseClient> Create(ForecastReadRegistry forecastReadRegistry)
    {
        try
        {
            if (forecastReadRegistry.TopicoBroker == null || forecastReadRegistry.TopicoBroker.Equals(""))
            {
                forecastReadRegistry.TopicoBroker = forecastReadRegistry.Nome;
            }

            if (forecastRepository.FindByNameAndVarType(forecastReadRegistry.Nome, forecastReadRegistry.TipoDado) != null)
            {
                ForecastReadRegistry forecastReadCreated = await forecastRepository.Save(forecastReadRegistry);
                AddRegistry(forecastReadCreated);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    forecastReadCreated,
                    "Registro de previsão adicionado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro de previsão com nome {forecastReadRegistry.Nome} já foi criado."
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

    public async Task<ResponseClient> Edit(ForecastReadRegistry forecastReadRegistry)
    {
        ForecastReadRegistry forecastReadFound = await forecastRepository.ReplaceOne(forecastReadRegistry);
        if (forecastReadFound != null)
        {
            ReplaceRegistry(forecastReadFound);

            return new ResponseClient(
                HttpStatusCode.OK,
                true, forecastReadFound,
                "Registro de previsão atualizado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false, null,
            "Registro de previsão com nome '" + forecastReadRegistry.Nome + "' não foi encontrado."
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
        List<ForecastReadRegistry> response = await forecastRepository.FindByName(name);
        return new ResponseClient(HttpStatusCode.OK, true, response, "Busca realizada com sucesso");
    }

    public async Task<ResponseClient> GetLocationForecast(Location location, string varType)
    {
        WindyResponse response = await windyService.GetWindyForecast(location, varType);
        return new ResponseClient(HttpStatusCode.OK, true, response, "Busca realizada com sucesso");
    }


    void AddRegistry(ForecastReadRegistry readRegistry)
    {
        registries.Add(readRegistry.CodeId, readRegistry);
    }

    public List<ReadRegistry> GetRegistries()
    {
        return registries.Values.Cast<ReadRegistry>().ToList();;
    }

    public static ResponseClient GetOne(int id)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.CodeId.Equals(id));

        return CreateResponseToFoundRegistry(id, foundRegistry.Value);
    }

    static ResponseClient CreateResponseToFoundRegistry(int idSistema, ForecastReadRegistry? foundRegistry)
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

    void ReplaceRegistry(ForecastReadRegistry supervisoryEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.CodeId.Equals(supervisoryEdited.CodeId))
            {
                registries[registry.CodeId] = supervisoryEdited;
            }
        }
    }

    public static void StartRegistries(List<ForecastReadRegistry> updateRegistries)
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

    public void TriggerBroker(List<ReadRegistry> registries)
    {
        foreach (ForecastReadRegistry forecastRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(forecastRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(ForecastReadRegistry readRegistry)
    {
        if (readRegistry.FreqLeituraSeg > 0)
        {
            if (readRegistry.IsTimeToSendMessage(readRegistry.FreqLeituraSeg))
            {
                //TODO ADICIONAR THREAD NO MONITOR SUPERVISORY
                MonitorForecast(readRegistry);
            }
        }
    }

    private async void MonitorForecast(ForecastReadRegistry readRegistry)
    {
        WindyResponse response = await windyService.GetWindyForecast(readRegistry.Location, readRegistry.TipoDado);

        try
        {
            switch (readRegistry.TipoDado)
            {
                case "temp":
                    readRegistry.UpdateRegistry(response.TempSurface[0]);
                    float tempValue = (float)Math.Round(response.TempSurface[0] - 273.15F, 1);

                    if (readRegistry.ShouldSendToBroker(tempValue))
                    {
                        if (tempValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, tempValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "convPrecip":
                    readRegistry.UpdateRegistry(response.PrecipSurface[0]);
                    float precipValue = response.PrecipSurface[0];

                    if (readRegistry.ShouldSendToBroker(precipValue))
                    {
                        if (precipValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, precipValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "wind":
                    readRegistry.UpdateRegistry(response.WindX[0]); //TODO arrumar wind
                    float[] windValue = new float[] { response.WindX[0], response.WindY[1] };

                    if (readRegistry.ShouldSendToBroker(windValue))
                    {
                        if (windValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, windValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "cape":
                    readRegistry.UpdateRegistry(response.CapeSurface[0]);
                    float capeValue = response.CapeSurface[0];

                    if (readRegistry.ShouldSendToBroker(capeValue))
                    {
                        if (capeValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, capeValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "rh":
                    readRegistry.UpdateRegistry(response.Rh[0]);
                    float rhValue = response.Rh[0];

                    if (readRegistry.ShouldSendToBroker(rhValue))
                    {
                        if (rhValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, rhValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "lclouds":
                    readRegistry.UpdateRegistry(response.LowCloudsCoverage[0]);
                    float lcValue = response.LowCloudsCoverage[0];

                    if (readRegistry.ShouldSendToBroker(lcValue))
                    {
                        if (lcValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, lcValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "hclouds":
                    readRegistry.UpdateRegistry(response.HighCloudsCoverage[0]);
                    float hcValue = response.HighCloudsCoverage[0];

                    if (readRegistry.ShouldSendToBroker(hcValue))
                    {
                        if (hcValue != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, hcValue);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            readRegistry.UpgradeStatusToUnavailable();
        }
    }
}