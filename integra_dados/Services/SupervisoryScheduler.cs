using integra_dados.Repository;
using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetService<ISupervisoryRepository>();
            RegistryManager.UpdateRegistries(repository.FindAll().Result);

            while (!stoppingToken.IsCancellationRequested)
            {
                var supervisoryService = scope.ServiceProvider.GetRequiredService<SupervisoryService>();

                try
                {
                    supervisoryService.TriggerBroker(RegistryManager.GetRegistries());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]: SupervisoryService " + ex.Message);
                }
            }

            await Task.Delay(5000, stoppingToken); // 5 segundos
        }
    }
}