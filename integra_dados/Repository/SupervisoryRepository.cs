using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class SupervisoryRepository(IMongoCollection<SupervisoryRegistry> supervisoryRegistryCollection) : IRepository<SupervisoryRegistry>
{

    public async Task<List<SupervisoryRegistry>> FindByName(string name)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<SupervisoryRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
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

    public async Task<SupervisoryRegistry> FindById(int id)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(s => s.Id, id);
        using var cursor = await supervisoryRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<SupervisoryRegistry>.Filter.Eq(x => x.Id, id);
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

        if (result.ModifiedCount == 0)
            return null;
        
        return document;
    }
}