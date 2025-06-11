using System.Reflection.Metadata;
using integra_dados.Models;
using MongoDB.Driver;

namespace integra_dados.Repository;

public class SupervisoryRepository(IMongoCollection<SupervisoryRegistry> supervisoryRegistryCollection) : ISupervisoryRepository
{

    public Task<Document> FindByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<SupervisoryRegistry> Save(SupervisoryRegistry document)
    {
        await supervisoryRegistryCollection.InsertOneAsync(document);
        return document;
    }
}