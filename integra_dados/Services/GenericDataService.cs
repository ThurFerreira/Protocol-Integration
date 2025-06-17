using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class GenericDataService
{
    public List<object> GetSupervisoryAndForecastData()
    {
        var supervisories = RegistryManager.GetRegistries();
        // var forecasts = RegistryManager.GetRegistries();

        var combinedList = new List<object>(supervisories.Count);

        combinedList.AddRange(supervisories);
        // combinedList.AddRange(forecasts);

        return combinedList;
    }

}