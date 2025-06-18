using integra_dados.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<bool> DeleteById(string id)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(x => x.IdSistema, id);
        var result = await forecastRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public Task<List<ForecastRegistry>> FindAll()
    {
        throw new NotImplementedException();
    }

    public async Task<ForecastRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<ForecastRegistry>.Filter.Eq(s => s.TipoDado, varType); //TODO passar o nomeVariavel para tipodado no front

        using var result = await forecastRegistryCollection.FindAsync(filter);
        return result.FirstOrDefault();

    }

    public async Task<ForecastRegistry> ReplaceOne(ForecastRegistry document)
    {
        var result = await forecastRegistryCollection.ReplaceOneAsync(
            f => f.Id == document.Id, 
            document
        );

        if (result.MatchedCount == 0)
            return null;
        
        return document;
    }
}