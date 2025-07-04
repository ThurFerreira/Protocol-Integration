using integra_dados.Models;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class OpcRepository(IMongoCollection<OpcReadRegistry> opcRegistryCollection) : IRepository<OpcReadRegistry>
{
    public async Task<List<OpcReadRegistry>> FindByName(string name)
    {
        var filter = Builders<OpcReadRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await opcRegistryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<OpcReadRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcReadRegistry> Save(OpcReadRegistry document)
    {
        await opcRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public Task<OpcReadRegistry> FindById(int idSistema)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<OpcReadRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await opcRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<OpcReadRegistry>> FindAll()
    {
        var result = await opcRegistryCollection.FindAsync(FilterDefinition<OpcReadRegistry>.Empty);
        return await result.ToListAsync();
    }

    public Task<OpcReadRegistry> FindByNameAndVarType(string name, string varType)
    {
        throw new NotImplementedException();
    }

    public async Task<OpcReadRegistry> ReplaceOne(OpcReadRegistry document)
    {
        var existing = await opcRegistryCollection
            .Find(x => x.CodeId == document.CodeId)
            .FirstOrDefaultAsync();

        if (existing is null)
            return null;

        // Copia o _id antigo para evitar erro
        document._Id = existing._Id;

        // Substitui o documento
        var result = await opcRegistryCollection.ReplaceOneAsync(
            x => x.CodeId == document.CodeId,
            document
        );

        return result.MatchedCount > 0 ? document : null;
    }
}