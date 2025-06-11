using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class SupervisoryRepository(IMongoCollection<SupervisoryRegistry> supervisoryRegistryCollection) : ISupervisoryRepository
{

    public Task<SupervisoryRegistry> FindByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<SupervisoryRegistry> Save(SupervisoryRegistry document)
    {
        await supervisoryRegistryCollection.InsertOneAsync(document);
        return document;
    }

    public Task<SupervisoryRegistry> FindById(int idSistema)
    {
        throw new NotImplementedException();
    }

    public void DeleteByIdSistema(int idSistema)
    {
        throw new NotImplementedException();
    }

    public List<SupervisoryRegistry> FindAll()
    {
        throw new NotImplementedException();
    }
}