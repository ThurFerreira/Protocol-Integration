using integra_dados.Models;
using integra_dados.Repository;

namespace integra_dados.Services;

public class SupervisoryScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Inicializa o RegistryManager uma vez no início
        using (var supervisoryScope = serviceProvider.CreateScope())
        {
            var repository = supervisoryScope.ServiceProvider.GetRequiredService<IRepository<SupervisoryRegistry>>();
            var allRegistries = await repository.FindAll();
            SupervisoryService.StartRegistries(allRegistries);
        }
        
        // using (var forecastScope = serviceProvider.CreateScope())
        // {
        //     var repository = forecastScope.ServiceProvider.GetRequiredService<IRepository<ForecastRegistry>>();
        //     var allRegistries = await repository.FindAll();
        //     ForecastService.StartRegistries(allRegistries);
        // }

        // Loop principal do serviço
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var supervisoryService = scope.ServiceProvider.GetRequiredService<SupervisoryService>();
                    supervisoryService.TriggerBroker(SupervisoryService.GetRegistries());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: SupervisoryService " + ex);
            }

            // Aguarda 5 segundos antes de repetir
            await Task.Delay(5000, stoppingToken);
        }
    }
}