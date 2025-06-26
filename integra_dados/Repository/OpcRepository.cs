using integra_dados.Models;
using integra_dados.Models.SupervisoryModel;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class OpcRepository(IMongoCollection<OpcRegistry> opcRegistryCollection) : IRepository<OpcRegistry>
{
    public async Task<List<OpcRegistry>> FindByName(string name)
    {
        var filter = Builders<OpcRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await opcRegistryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<OpcRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcRegistry> Save(OpcRegistry document)
    {
        await opcRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public Task<OpcRegistry> FindById(int idSistema)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<OpcRegistry>.Filter.Eq(x => x.Id, id);
        var result = await opcRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<OpcRegistry>> FindAll()
    {
        var result = await opcRegistryCollection.FindAsync(FilterDefinition<OpcRegistry>.Empty);
        return await result.ToListAsync();
    }

    public Task<OpcRegistry> FindByNameAndVarType(string name, string varType)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcRegistry> ReplaceOne(OpcRegistry document)
    {
        var result = await opcRegistryCollection.ReplaceOneAsync(
            f => f.Id == document.Id,
            document
        );

        if (result.ModifiedCount == 0)
            return null;

        return document;
    }
}