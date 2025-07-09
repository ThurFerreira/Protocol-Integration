using integra_dados.Models.SupervisoryModel.RegistryModel.BACnet;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class BACnetRepository(IMongoCollection<BACnetRegistry> registryCollection) : IRepository<BACnetRegistry>
{
    public async Task<List<BACnetRegistry>> FindByName(string name)
    {
        var filter = Builders<BACnetRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await registryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<BACnetRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    Task<BACnetRegistry> IRepository<BACnetRegistry>.Save(BACnetRegistry document)
    {
        return Save(document);
    }

    async Task<BACnetRegistry> Save(BACnetRegistry document)
    {
        await registryCollection.InsertOneAsync(document);
        return document;
    }

    public async Task<BACnetRegistry> FindById(int id)
    {
        var filter = Builders<BACnetRegistry>.Filter.Eq(s => s.CodeId, id);
        using var cursor = await registryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<BACnetRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await registryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<BACnetRegistry>> FindAll()
    {
        var result = await registryCollection.FindAsync(FilterDefinition<BACnetRegistry>.Empty);
        return await result.ToListAsync();
    }

    public async Task<BACnetRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<BACnetRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<BACnetRegistry>.Filter.Eq(s => s.TipoDado, varType); //TODO passar o nomeVariavel para tipodado no front

        using var result = await registryCollection.FindAsync(filter);
        return result.FirstOrDefault();    }

    public async Task<BACnetRegistry> ReplaceOne(BACnetRegistry document)
    {
        var existing = await registryCollection
            .Find(x => x.CodeId == document.CodeId)
            .FirstOrDefaultAsync();

        if (existing is null)
            return null;

        // Copia o _id antigo para evitar erro
        document._Id = existing._Id;

        // Substitui o documento
        var result = await registryCollection.ReplaceOneAsync(
            x => x.CodeId == document.CodeId,
            document
        );

        return result.MatchedCount > 0 ? document : null;
    }
}