using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class ForecastRepository(IMongoCollection<ForecastRegistry> forecastRegistryCollection) : IRepository<ForecastRegistry>
{
    public Task<ForecastRegistry> FindByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<ForecastRegistry> Save(ForecastRegistry document)
    {
        throw new NotImplementedException();
    }

    public Task<ForecastRegistry> FindById(string? idSistema)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteById(string id)
    {
        throw new NotImplementedException();
    }

    public Task<List<ForecastRegistry>> FindAll()
    {
        throw new NotImplementedException();
    }
}