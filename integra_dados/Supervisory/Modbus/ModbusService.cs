using System.Net;
using EasyModbus;
using EasyModbus.Exceptions;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Notifier;

namespace integra_dados.Services.Modbus;

public class ModbusService(
    IRepository<ModbusRegistry> supervisoryRepository,
    KafkaService kafkaService,
    Report report)
{
    public static Dictionary<string, ModbusClient>? ModbusClientList = new Dictionary<string, ModbusClient>();
    private static Dictionary<int, ModbusRegistry> registries = new Dictionary<int, ModbusRegistry>();

    public bool ConnectClientModbus(ModbusClient modbusClient, ModbusRegistry registry)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!modbusClient.Connected && stopwatch.ElapsedMilliseconds < 30000)
        {
            try
            {
                modbusClient.Connect();
                if (modbusClient.Connected)
                {
                    ModbusClientList[registry.Ip] = modbusClient;
                    Console.WriteLine("Connected successfully.");
                    return true;
                }
            }
            catch (System.Exception E)
            {
                report.LightException(Status.NOT_CONNECTED);
                Console.WriteLine("Failed to connect after attempt. " + E.Message);
                
                return false;
            }

            Thread.Sleep(3000);
        }


        return false;
    }

    public bool? ReadDiscreteInput(ModbusRegistry registry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(registry.Ip) && ModbusClientList[registry.Ip] != null ? ModbusClientList[registry.Ip] : new ModbusClient(registry.Ip, registry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient, registry);
        }

        if (modbusClient.Connected)
        {
            try
            {
                bool[]? serverResponse = null;
                serverResponse = modbusClient.ReadDiscreteInputs(registry.EnderecoInicio, registry.QuantidadeTags);

                if (serverResponse != null )
                {
                    return serverResponse[0];
                }
                
                report.LightException(Status.EMPTY_READ);
                System.Console.WriteLine("No data returned from the Modbus server.");
                return null;
            }
            catch (ConnectionException ce)
            {
                Console.WriteLine(ce.Message);
                modbusClient.Disconnect();
            }
        }

        return null;
    }

    public int? ReadInputRegister(ModbusRegistry registry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(registry.Ip) && ModbusClientList[registry.Ip] != null ? ModbusClientList[registry.Ip] : new ModbusClient(registry.Ip, registry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient, registry);
        }

        if (modbusClient.Connected)
        {
            try
            {
                int[]? serverResponse = null;
                serverResponse = modbusClient.ReadInputRegisters(registry.EnderecoInicio, registry.QuantidadeTags);

                if (serverResponse != null) //se retornar 0 o sensor pode estar fora da agua
                {
                    return serverResponse[0];
                }

                report.LightException(Status.EMPTY_READ);
                System.Console.WriteLine("No data returned from the Modbus server.");
                return null;
            }
            catch (Exception ex)
            {
                report.LightException(Status.ERROR);
                Console.WriteLine(ex.Message);
                registry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }

        return -1;
    }

    public bool? ReadCoil(ModbusRegistry registry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(registry.Ip) && ModbusClientList[registry.Ip] != null ? ModbusClientList[registry.Ip] : new ModbusClient(registry.Ip, registry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient, registry);
        }

        if (modbusClient.Connected)
        {
            try
            {
                bool[]? serverResponse = null;
                serverResponse = modbusClient.ReadCoils(registry.EnderecoInicio, registry.QuantidadeTags);

                if (serverResponse != null) //se retornar 0 o sensor pode estar fora da agua
                {
                    return serverResponse[0];
                }

                report.LightException(Status.EMPTY_READ);
                System.Console.WriteLine("No data returned from the Modbus server.");
                return null;
            }
            catch (Exception ex)
            {
                report.LightException(Status.ERROR);
                Console.WriteLine(ex.Message);
                registry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }
        
        return null;
    }

    public int? ReadHoldingRegister(ModbusRegistry registry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(registry.Ip) && ModbusClientList[registry.Ip] != null ? ModbusClientList[registry.Ip] : new ModbusClient(registry.Ip, registry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient, registry);
        }

        if (modbusClient.Connected)
        {
            try
            {
                int[]? serverResponse = null;
                serverResponse =
                    modbusClient.ReadHoldingRegisters(registry.EnderecoInicio, registry.QuantidadeTags);

                if (serverResponse != null) //se retornar 0 o sensor pode estar fora da agua
                {
                    return serverResponse[0];
                }

                report.LightException(Status.EMPTY_READ);
                System.Console.WriteLine("No data returned from the Modbus server.");
                return null;
            }
            catch (Exception ex)
            {
                report.LightException(Status.ERROR);
                Console.WriteLine(ex.Message);
                registry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }
        
        return -1;
    }

    public async Task<ResponseClient> Create(ModbusRegistry registry)
    {
        try
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

    public async Task<ResponseClient> Edit(ModbusRegistry modbusRegistry)
    {
        try
        {
            ModbusRegistry modbusFound = await supervisoryRepository.ReplaceOne(modbusRegistry);
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

    public void TriggerBroker(List<ModbusRegistry> registries)
    {
        foreach (ModbusRegistry supervisoryRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(ModbusRegistry registry)
    {
        if (registry.FreqLeituraSeg > 0)
        {
            if (registry.IsTimeToSendMessage(registry.FreqLeituraSeg))
            {
                MonitorSupervisory(registry);
            }
        }
    }

    private void MonitorSupervisory(ModbusRegistry registry)
    {
        try
        {
            switch (registry.TipoDado)
            {
                case "discreteInput":
                    var registerValueStatus = ReadDiscreteInput(registry);
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
                    var registerValueIntRegister = ReadInputRegister(registry);
                    registry.UpdateRegistry(registerValueIntRegister);
                    ReplaceRegistry(registry);
                    if (registry.ShouldSendToBroker(registerValueIntRegister))
                    {
                        if (registerValueIntRegister != null && registerValueIntRegister != -1)
                        {
                            Event1000_1 brokerPackage =
                                kafkaService.CreateBrokerPackage(registry, registerValueIntRegister);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "coil":
                    var registerValueCoil = ReadCoil(registry);
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
                    var registerValueHolding = ReadHoldingRegister(registry);
                    registry.UpdateRegistry(registerValueHolding);
                    ReplaceRegistry(registry);
                    if (registry.ShouldSendToBroker(registerValueHolding))
                    {
                        if (registerValueHolding != null && registerValueHolding != -1)
                        {
                            Event1000_1 brokerPackage =
                                kafkaService.CreateBrokerPackage(registry, registerValueHolding);
                            kafkaService.Publish(registry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error on monitor supervisory modbus " + e.Message);
            report.ModerateException(Status.ERROR); 
        }
    }

    public static void AddRegistry(ModbusRegistry registry)
    {
        registries.Add(registry.CodeId, registry);
    }

    public List<ModbusRegistry> GetRegistries()
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

    public static void ReplaceRegistry(ModbusRegistry modbusEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.CodeId.Equals(modbusEdited.CodeId))
            {
                registries[registry.CodeId] = modbusEdited;
            }
        }
    }

    public static void StartRegistries(List<ModbusRegistry> updateRegistries)
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