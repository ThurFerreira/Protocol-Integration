using System.Net;
using EasyModbus;
using EasyModbus.Exceptions;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Repository;
using integra_dados.Services.Kafka;
using integra_dados.Services.Notifier;

namespace integra_dados.Services.Modbus;

public class ModbusService(
    IRepository<ModbusReadRegistry> supervisoryRepository,
    KafkaService kafkaService,
    Report report)
{
    public static Dictionary<string, ModbusClient>? ModbusClientList = new Dictionary<string, ModbusClient>();
    private static Dictionary<int, ModbusReadRegistry> registries = new Dictionary<int, ModbusReadRegistry>();

    public bool ConnectClientModbus(ModbusClient modbusClient)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!modbusClient.Connected && stopwatch.ElapsedMilliseconds < 30000)
        {
            try
            {
                modbusClient.Connect();
                if (modbusClient.Connected)
                {
                    ModbusClientList[modbusClient.IPAddress] = modbusClient;
                    Console.WriteLine("Modbus connected successfully.");
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

    public bool? ReadDiscreteInput(ModbusReadRegistry readRegistry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(readRegistry.Ip) && ModbusClientList[readRegistry.Ip] != null ? ModbusClientList[readRegistry.Ip] : new ModbusClient(readRegistry.Ip, readRegistry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient);
        }

        if (modbusClient.Connected)
        {
            try
            {
                bool[]? serverResponse = null;
                serverResponse = modbusClient.ReadDiscreteInputs(readRegistry.EnderecoInicio, readRegistry.QuantidadeTags);

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

    public int? ReadInputRegister(ModbusReadRegistry readRegistry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(readRegistry.Ip) && ModbusClientList[readRegistry.Ip] != null ? ModbusClientList[readRegistry.Ip] : new ModbusClient(readRegistry.Ip, readRegistry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient);
        }

        if (modbusClient.Connected)
        {
            try
            {
                int[]? serverResponse = null;
                serverResponse = modbusClient.ReadInputRegisters(readRegistry.EnderecoInicio, readRegistry.QuantidadeTags);
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
                readRegistry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }

        return -1;
    }

    public bool? ReadCoil(ModbusReadRegistry readRegistry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(readRegistry.Ip) && ModbusClientList[readRegistry.Ip] != null ? ModbusClientList[readRegistry.Ip] : new ModbusClient(readRegistry.Ip, readRegistry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient);
        }

        if (modbusClient.Connected)
        {
            try
            {
                bool[]? serverResponse = null;
                serverResponse = modbusClient.ReadCoils(readRegistry.EnderecoInicio, readRegistry.QuantidadeTags);

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
                readRegistry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }
        
        return null;
    }

    public int? ReadHoldingRegister(ModbusReadRegistry readRegistry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(readRegistry.Ip) && ModbusClientList[readRegistry.Ip] != null ? ModbusClientList[readRegistry.Ip] : new ModbusClient(readRegistry.Ip, readRegistry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient);
        }

        if (modbusClient.Connected)
        {
            try
            {
                int[]? serverResponse = null;
                serverResponse =
                    modbusClient.ReadHoldingRegisters(readRegistry.EnderecoInicio, readRegistry.QuantidadeTags);

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
                readRegistry.UpgradeStatusToUnavailable();
                modbusClient.Disconnect();
            }
        }
        
        return -1;
    }

    public async Task<ResponseClient> Create(ModbusReadRegistry readRegistry)
    {
        try
        {
            if (readRegistry.TopicoBroker == null || readRegistry.TopicoBroker.Equals(""))
            {
                readRegistry.TopicoBroker = readRegistry.Nome;
            }

            if (supervisoryRepository.FindByName(readRegistry.Nome) != null)
            {
                var savedRegistry = await supervisoryRepository.Save(readRegistry);
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
                "Registro de previsão com nome '" + readRegistry.Nome + "' já foi criado.");
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

    public async Task<ResponseClient> Edit(ModbusReadRegistry modbusReadRegistry)
    {
        try
        {
            ModbusReadRegistry modbusReadFound = await supervisoryRepository.ReplaceOne(modbusReadRegistry);
            if (modbusReadFound != null)
            {
                ReplaceRegistry(modbusReadFound);

                return new ResponseClient(
                    HttpStatusCode.OK,
                    true,
                    modbusReadRegistry,
                    "Registro atualizado com sucesso."
                );
            }

            return new ResponseClient(
                HttpStatusCode.Conflict,
                false,
                null,
                $"Registro com nome '{modbusReadRegistry.Nome}' não foi encontrado."
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

    public void TriggerBroker(List<ReadRegistry> registries)
    {
        foreach (ModbusReadRegistry supervisoryRegistry in registries.ToList())
        {
            CheckWhetherShouldTriggerBroker(supervisoryRegistry);
        }
    }

    public void CheckWhetherShouldTriggerBroker(ModbusReadRegistry readRegistry)
    {
        if (readRegistry.FreqLeituraSeg > 0)
        {
            if (readRegistry.IsTimeToSendMessage(readRegistry.FreqLeituraSeg))
            {
                MonitorSupervisory(readRegistry);
            }
        }
    }

    private void MonitorSupervisory(ModbusReadRegistry readRegistry)
    {
        try
        {
            switch (readRegistry.TipoDado)
            {
                case "discreteInput":
                    var registerValueStatus = ReadDiscreteInput(readRegistry);
                    readRegistry.UpdateRegistry(registerValueStatus);
                    ReplaceRegistry(readRegistry);
                    if (readRegistry.ShouldSendToBroker(registerValueStatus))
                    {
                        if (registerValueStatus != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, registerValueStatus);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "inputRegister":
                    var registerValueIntRegister = ReadInputRegister(readRegistry);
                    readRegistry.UpdateRegistry(registerValueIntRegister);
                    ReplaceRegistry(readRegistry);
                    if (readRegistry.ShouldSendToBroker(registerValueIntRegister))
                    {
                        if (registerValueIntRegister != null && registerValueIntRegister != -1)
                        {
                            Event1000_1 brokerPackage =
                                kafkaService.CreateBrokerPackage(readRegistry, registerValueIntRegister);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "coil":
                    var registerValueCoil = ReadCoil(readRegistry);
                    readRegistry.UpdateRegistry(registerValueCoil);
                    ReplaceRegistry(readRegistry);
                    if (readRegistry.ShouldSendToBroker(registerValueCoil))
                    {
                        if (registerValueCoil != null)
                        {
                            Event1000_1 brokerPackage = kafkaService.CreateBrokerPackage(readRegistry, registerValueCoil);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
                        }
                    }

                    break;
                case "holdingRegister":
                    var registerValueHolding = ReadHoldingRegister(readRegistry);
                    readRegistry.UpdateRegistry(registerValueHolding);
                    ReplaceRegistry(readRegistry);
                    if (readRegistry.ShouldSendToBroker(registerValueHolding))
                    {
                        if (registerValueHolding != null && registerValueHolding != -1)
                        {
                            Event1000_1 brokerPackage =
                                kafkaService.CreateBrokerPackage(readRegistry, registerValueHolding);
                            kafkaService.Publish(readRegistry.TopicoBroker, brokerPackage);
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

    public static void AddRegistry(ModbusReadRegistry readRegistry)
    {
        registries.Add(readRegistry.CodeId, readRegistry);
    }

    public List<ReadRegistry> GetRegistries()
    {
        return registries.Values.Cast<ReadRegistry>().ToList(); //buscar no banco isso aqui é muito burro
    }

    public static ResponseClient GetOne(int id)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.CodeId.Equals(id));

        return CreateResponseToFoundRegistry(id, foundRegistry.Value);
    }

    private static ResponseClient CreateResponseToFoundRegistry(int idSistema, ReadRegistry? foundRegistry)
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

    public static void ReplaceRegistry(ModbusReadRegistry modbusReadEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.CodeId.Equals(modbusReadEdited.CodeId))
            {
                registries[registry.CodeId] = modbusReadEdited;
            }
        }
    }

    public static void StartRegistries(List<ModbusReadRegistry> updateRegistries)
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

    public bool? WriteDiscreteInput(ModbusReadRegistry registry)
    {
        ModbusClient modbusClient = ModbusClientList.ContainsKey(registry.Ip) && ModbusClientList[registry.Ip] != null
            ? ModbusClientList[registry.Ip]
            : new ModbusClient(registry.Ip, registry.Porta);

        if (!modbusClient.Connected)
        {
            ConnectClientModbus(modbusClient);
        }

        if (modbusClient.Connected)
        {
            try
            {
                // modbusClient.WriteSingleCoil(registry.EnderecoInicio);
            }
            catch (ConnectionException ce)
            {
                Console.WriteLine(ce.Message);
                modbusClient.Disconnect();
            }
        }

        return null;
    }
}