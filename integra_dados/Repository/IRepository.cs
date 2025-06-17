using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface IRepository
{
    public Task<Registry> FindByName(String name);
    public Task<Registry> Save(Registry document);
    public Task<Registry> FindById(string? idSistema);
    public Task<bool> DeleteById(string id);
    public Task<List<Registry>> FindAll();
}