using System.Reflection.Metadata;

namespace integra_dados.Repository;

public class SupervisoryRepository() : ISupervisoryRepository
{
    public Task<Document> FindByName(string name)
    {
        throw new NotImplementedException();
    }
}