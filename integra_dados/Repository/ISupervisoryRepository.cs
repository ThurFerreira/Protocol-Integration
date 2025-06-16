using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface ISupervisoryRepository
{
    public Task<SupervisoryRegistry> FindByName(String name);
    public Task<SupervisoryRegistry> Save(SupervisoryRegistry document);
    public Task<SupervisoryRegistry> FindById(string? idSistema);
    public Task<bool> DeleteById(string id);
    public Task<List<SupervisoryRegistry>> FindAll();
}