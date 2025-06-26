using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class OpcRepository(IMongoCollection<OpcRegistry> opcRegistryCollection) : IRepository<OpcRegistry>
{
    public Task<List<OpcRegistry>> FindByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<OpcRegistry> FindOneByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<OpcRegistry> Save(OpcRegistry document)
    {
        throw new NotImplementedException();
    }

    public Task<OpcRegistry> FindById(int idSistema)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<OpcRegistry>> FindAll()
    {
        throw new NotImplementedException();
    }

    public Task<OpcRegistry> FindByNameAndVarType(string name, string varType)
    {
        throw new NotImplementedException();
    }

    public Task<OpcRegistry> ReplaceOne(OpcRegistry document)
    {
        throw new NotImplementedException();
    }
}