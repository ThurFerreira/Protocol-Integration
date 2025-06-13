using integra_dados.Repository;
using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryScheduler(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Inicializa o RegistryManager uma vez no início
        using (var initialScope = serviceProvider.CreateScope())
        {
            var repository = initialScope.ServiceProvider.GetRequiredService<ISupervisoryRepository>();
            var allRegistries = await repository.FindAll();
            RegistryManager.UpdateRegistries(allRegistries);
        }

        // Loop principal do serviço
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var supervisoryService = scope.ServiceProvider.GetRequiredService<SupervisoryService>();
                    supervisoryService.TriggerBroker(RegistryManager.GetRegistries());
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