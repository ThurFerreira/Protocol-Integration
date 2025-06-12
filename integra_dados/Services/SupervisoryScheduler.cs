using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryScheduler(SupervisoryService supervisoryService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            supervisoryService.TriggerBroker(RegistryManager.GetRegistries());
            await Task.Delay(1000, stoppingToken); // 1 segundo
        }
    }
}