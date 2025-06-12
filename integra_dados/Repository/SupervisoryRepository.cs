using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class SupervisoryRepository(IMongoCollection<SupervisoryRegistry> supervisoryRegistryCollection) : ISupervisoryRepository
{

    public async Task<SupervisoryRegistry> FindByName(string name)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<SupervisoryRegistry> Save(SupervisoryRegistry document)
    {
        await supervisoryRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public async Task<SupervisoryRegistry> FindById(int idSistema)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.IdSistema, idSistema);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteByIdSistema(int idSistema)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(x => x.IdSistema, idSistema);
        var result = await supervisoryRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<SupervisoryRegistry>> FindAll()
    {
        var result = await supervisoryRegistryCollection.FindAsync(FilterDefinition<SupervisoryRegistry>.Empty);
        return await result.ToListAsync();
    }
}