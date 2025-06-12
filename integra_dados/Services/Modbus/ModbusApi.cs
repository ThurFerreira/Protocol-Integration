using EasyModbus;
using integra_dados.Models;

namespace integra_dados.Services.Modbus;

public class ModbusApi
{
    public static ModbusClient? ApiClient;

    public static bool ConnectClientModbus(SupervisoryRegistry registry)
    {
        ApiClient.IPAddress = registry.Ip;
        ApiClient.Port = registry.Porta;
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

    public static int ReadDiscreteInput(int startingAddress, int numberOfInputs)
    {
        if (ApiClient.Connected)
        {
            try
            {
                var serverResponse = ApiClient.ReadInputRegisters(startingAddress, numberOfInputs);

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