using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface ISupervisoryRepository
{
    public Task<Document> FindByName(String name);
}