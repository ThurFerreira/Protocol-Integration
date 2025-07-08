using integra_dados.Models;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class OpcRepository(IMongoCollection<OpcRegistry> registryCollection) : IRepository<OpcRegistry>
{
    public async Task<List<OpcRegistry>> FindByName(string name)
    {
        var filter = Builders<OpcRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await registryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<OpcRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcRegistry> Save(OpcRegistry document)
    {
        await registryCollection.InsertOneAsync(document);
        return document;
    }

    public Task<OpcRegistry> FindById(int idSistema)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<OpcRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await registryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<OpcRegistry>> FindAll()
    {
        var result = await registryCollection.FindAsync(FilterDefinition<OpcRegistry>.Empty);
        return await result.ToListAsync();
    }

    public Task<OpcRegistry> FindByNameAndVarType(string name, string varType)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcRegistry> ReplaceOne(OpcRegistry document)
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