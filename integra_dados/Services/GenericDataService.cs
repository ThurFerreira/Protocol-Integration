
namespace integra_dados.Services;

public class GenericDataService
{
    public List<object> GetSupervisoryAndForecastData()
    {
        var supervisories = SupervisoryService.GetRegistries();
        // var forecasts = RegistryManager.GetRegistries();

        var combinedList = new List<object>(supervisories.Count);

        combinedList.AddRange(supervisories);
        // combinedList.AddRange(forecasts);

        return combinedList;
    }

}