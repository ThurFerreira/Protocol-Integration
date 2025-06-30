using integra_dados.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class ForecastRepository(IMongoCollection<ForecastRegistry> forecastRegistryCollection) : IRepository<ForecastRegistry>
{
    public async Task<ForecastRegistry> FindOneByName(string name)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await forecastRegistryCollection.FindAsync(filter);
        return cursor.FirstOrDefault();
    }
    
    public async Task<List<ForecastRegistry>> FindByName(string name)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await forecastRegistryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public async Task<ForecastRegistry> Save(ForecastRegistry document)
    {
        await forecastRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public Task<ForecastRegistry> FindById(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await forecastRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<ForecastRegistry>> FindAll()
    {
        var result = await forecastRegistryCollection.FindAsync(FilterDefinition<ForecastRegistry>.Empty);
        return await result.ToListAsync();
    }

    public async Task<ForecastRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<ForecastRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<ForecastRegistry>.Filter.Eq(s => s.TipoDado, varType); 

        using var result = await forecastRegistryCollection.FindAsync(filter);
        return result.FirstOrDefault();

    }

    public async Task<ForecastRegistry> ReplaceOne(ForecastRegistry document)
    {
        var existing = await forecastRegistryCollection
            .Find(x => x.CodeId == document.CodeId)
            .FirstOrDefaultAsync();

        if (existing is null)
            return null;

        // Copia o _id antigo para evitar erro
        document._Id = existing._Id;

        // Substitui o documento
        var result = await forecastRegistryCollection.ReplaceOneAsync(
            x => x.CodeId == document.CodeId,
            document
        );

        return result.MatchedCount > 0 ? document : null;
    }
}