using EasyModbus;
using integra_dados.Models;

namespace integra_dados.Services.Modbus;

public class ModbusService
{
    public static ModbusClient? ApiClient;

    public ModbusService()
    {
        ApiClient = new ModbusClient();
    }

    public static bool ConnectClientModbus(SupervisoryRegistry registry)
    {
        ApiClient.IPAddress = registry.Ip;
        ApiClient.Port = int.Parse(registry.Porta!);
        ApiClient.SerialPort = null;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!ApiClient.Connected && stopwatch.ElapsedMilliseconds < 30000)
        {
            try
            {
                ApiClient.Connect();
                if (ApiClient.Connected)
                {
                    Console.WriteLine("Connected successfully.");
                    return true;
                }
            }
            catch (System.Exception E)
            {
                Console.WriteLine("Failed to connect after multiple attempts. " + E.Message);
                return false;
            }

            Thread.Sleep(3000);
        }


        return false;
    }

    public static bool ReadDiscreteInput(SupervisoryRegistry registry)
    {
        if (!ApiClient.Connected)
        {
            ConnectClientModbus(registry);
        } 
        
        if(ApiClient.Connected) {
            try
            {
                var serverResponse = ApiClient.ReadDiscreteInputs(int.Parse(registry.EnderecoInicio), registry.QuantidadeTags);

                if (serverResponse == null ||
                    serverResponse.Length == 0) //se retornar 0 o sensor pode estar fora da agua
                {
                    System.Console.WriteLine("No data returned from the Modbus server.");
                }

                return serverResponse[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return false;
    }
    
    public static int ReadInputRegister(SupervisoryRegistry registry)
    {
        if (!ApiClient.Connected)
        {
            ConnectClientModbus(registry);
        } 
        
        if(ApiClient.Connected) {
            try
            {
                var serverResponse = ApiClient.ReadInputRegisters(int.Parse(registry.EnderecoInicio), registry.QuantidadeTags);

                if (serverResponse == null ||
                    serverResponse.Length == 0) //se retornar 0 o sensor pode estar fora da agua
                {
                    System.Console.WriteLine("No data returned from the Modbus server.");
                }

                return serverResponse[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return -1;
    }
    
    public static bool ReadCoil(SupervisoryRegistry registry)
    {
        if (!ApiClient.Connected)
        {
            ConnectClientModbus(registry);
        } 
        
        if(ApiClient.Connected) {
            try
            {
                var serverResponse = ApiClient.ReadCoils(int.Parse(registry.EnderecoInicio), registry.QuantidadeTags);

                if (serverResponse == null ||
                    serverResponse.Length == 0) //se retornar 0 o sensor pode estar fora da agua
                {
                    System.Console.WriteLine("No data returned from the Modbus server.");
                }

                return serverResponse[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return false;
    }

    public static int ReadHoldingRegister(SupervisoryRegistry registry)
    {
        if (!ApiClient.Connected)
        {
            ConnectClientModbus(registry);
        }

        if (ApiClient.Connected)
        {
            try
            {
                var serverResponse = ApiClient.ReadHoldingRegisters(int.Parse(registry.EnderecoInicio), registry.QuantidadeTags);

                if (serverResponse == null ||
                    serverResponse.Length == 0) //se retornar 0 o sensor pode estar fora da agua
                {
                    System.Console.WriteLine("No data returned from the Modbus server.");
                }

                return serverResponse[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return -1;
    }
}