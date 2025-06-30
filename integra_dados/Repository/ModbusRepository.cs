using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class ModbusRepository(IMongoCollection<ModbusRegistry> modbusRegistryCollection) : IRepository<ModbusRegistry>
{

    public async Task<List<ModbusRegistry>> FindByName(string name)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.Nome, name);
        using var cursor = await modbusRegistryCollection.FindAsync(filter);
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
        await modbusRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public async Task<ModbusRegistry> FindById(int id)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.CodeId, id);
        using var cursor = await modbusRegistryCollection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(int id)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(x => x.CodeId, id);
        var result = await modbusRegistryCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<List<ModbusRegistry>> FindAll()
    {
        var result = await modbusRegistryCollection.FindAsync(FilterDefinition<ModbusRegistry>.Empty);
        return await result.ToListAsync();
    }

    public async Task<ModbusRegistry> FindByNameAndVarType(string name, string varType)
    {
        var filter = Builders<ModbusRegistry>.Filter.Eq(s => s.Nome, name) & 
                     Builders<ModbusRegistry>.Filter.Eq(s => s.TipoDado, varType); //TODO passar o nomeVariavel para tipodado no front

        using var result = await modbusRegistryCollection.FindAsync(filter);
        return result.FirstOrDefault();    }

    public async Task<ModbusRegistry> ReplaceOne(ModbusRegistry document)
    {
        var result = await modbusRegistryCollection.ReplaceOneAsync(
            f => f.CodeId == document.CodeId, 
            document
        );

        if (result.ModifiedCount == 0)
            return null;
        
        return document;
    }
}