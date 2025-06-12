using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
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