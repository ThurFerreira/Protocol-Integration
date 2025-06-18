using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class SupervisoryRepository(IMongoCollection<SupervisoryRegistry> supervisoryRegistryCollection) : IRepository<SupervisoryRegistry>
{

    public async Task<SupervisoryRegistry> FindByName(string name)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    Task<SupervisoryRegistry> IRepository<SupervisoryRegistry>.Save(SupervisoryRegistry document)
    {
        return Save(document);
    }

    async Task<SupervisoryRegistry> Save(SupervisoryRegistry document)
    {
        await supervisoryRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public async Task<SupervisoryRegistry> FindById(string? idSistema)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.IdSistema, idSistema);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(string id)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(x => x.IdSistema, id);
        var result = await supervisoryRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<SupervisoryRegistry>> FindAll()
    {
        var result = await supervisoryRegistryCollection.FindAsync(FilterDefinition<SupervisoryRegistry>.Empty);
        return await result.ToListAsync();
    }

    public async Task<SupervisoryRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<SupervisoryRegistry>.Filter.Eq(s => s.TipoDado, varType); //TODO passar o nomeVariavel para tipodado no front

        using var result = await supervisoryRegistryCollection.FindAsync(filter);
        return result.FirstOrDefault();    }

    public async Task<SupervisoryRegistry> ReplaceOne(SupervisoryRegistry document)
    {
        var result = await supervisoryRegistryCollection.ReplaceOneAsync(
            f => f.Id == document.Id, 
            document
        );

        if (result.MatchedCount == 0)
            return null;
        
        return document;
    }
}