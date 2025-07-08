using System.Reflection.Metadata;
using integra_dados.Models;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using MongoDB.Bson;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class ModbusRepository(IMongoCollection<ModbusRegistry> registryCollection) : IRepository<ModbusRegistry>
{

    public async Task<List<ModbusRegistry>> FindByName(string name)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await registryCollection.FindAsync(filter);
        return cursor.ToList();
    }

    public Task<ModbusRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    Task<ModbusRegistry> IRepository<ModbusRegistry>.Save(ModbusRegistry document)
    {
        return Save(document);
    }

    async Task<ModbusRegistry> Save(ModbusRegistry document)
    {
        await registryCollection.InsertOneAsync(document);
        return document;
    }

    public async Task<ModbusRegistry> FindById(int id)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.CodeId, id);
        using var cursor = await registryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await registryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<ModbusRegistry>> FindAll()
    {
        var result = await registryCollection.FindAsync(FilterDefinition<ModbusRegistry>.Empty);
        return await result.ToListAsync();
    }

    public async Task<ModbusRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<ModbusRegistry>.Filter.Eq(s => s.TipoDado, varType); //TODO passar o nomeVariavel para tipodado no front

        using var result = await registryCollection.FindAsync(filter);
        return result.FirstOrDefault();    }

    public async Task<ModbusRegistry> ReplaceOne(ModbusRegistry document)
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