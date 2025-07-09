using integra_dados.Models;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.BACnet;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using integra_dados.Models.SupervisoryModel.RegistryModel.WindyForecast;
using integra_dados.Repository;
using integra_dados.Services.BACnet;
using integra_dados.Services.Kafka;
using integra_dados.Services.Modbus;
using integra_dados.Services.Notifier;
using integra_dados.Supervisory.OPC;

namespace integra_dados.Services;

public class Scheduler(IServiceProvider serviceProvider, Report report, KafkaService kafkaService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => kafkaService.ConsumeAndWriteDispositive("SUPERVISORY_WRITE_TOPIC", event10002 => ModbusService.WriteDiscreteInput(event10002)));
        
        // Inicializa o RegistryManager uma vez no início
        using (var supervisoryScope = serviceProvider.CreateScope())
        {
            var repository = supervisoryScope.ServiceProvider.GetRequiredService<IRepository<ModbusRegistry>>();
            var allRegistries = await repository.FindAll();
            ModbusService.StartRegistries(allRegistries);
        }
        
        using (var supervisoryScope = serviceProvider.CreateScope())
        {
            var repository = supervisoryScope.ServiceProvider.GetRequiredService<IRepository<BACnetRegistry>>();
            var allRegistries = await repository.FindAll();
            BACnetService.StartRegistries(allRegistries);
        }
        
        using (var forecastScope = serviceProvider.CreateScope())
        {
            var repository = forecastScope.ServiceProvider.GetRequiredService<IRepository<ForecastRegistry>>();
            var allRegistries = await repository.FindAll();
            ForecastService.StartRegistries(allRegistries);
        }
        
        using (var forecastScope = serviceProvider.CreateScope())
        {
            var repository = forecastScope.ServiceProvider.GetRequiredService<IRepository<OpcRegistry>>();
            var allRegistries = await repository.FindAll();
            OpcService.StartRegistries(allRegistries);
        }

        // Loop principal do serviço
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var modbusService = scope.ServiceProvider.GetRequiredService<ModbusService>();
                    modbusService.TriggerBroker(modbusService.GetRegistries());
                }
                
                using (var scope = serviceProvider.CreateScope())
                {
                    var forecastService = scope.ServiceProvider.GetRequiredService<ForecastService>();
                    forecastService.TriggerBroker(forecastService.GetRegistries());
                }
                
                using (var scope = serviceProvider.CreateScope())
                {
                    var opcService = scope.ServiceProvider.GetRequiredService<OpcService>();
                    opcService.TriggerBroker(opcService.GetRegistries());
                }
                
                using (var scope = serviceProvider.CreateScope())
                {
                    var opcService = scope.ServiceProvider.GetRequiredService<BACnetService>();
                    opcService.TriggerBroker(opcService.GetRegistries());
                }
            }
            catch (Exception ex)
            {
                report.ModerateException(Status.ERROR);
                Console.WriteLine("[ERROR]: SupervisoryService " + ex);
            }

            // Aguarda 5 segundos antes de repetir
            await Task.Delay(5000, stoppingToken);
        }
    }
}