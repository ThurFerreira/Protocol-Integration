using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface IRepository<T>
{
    public Task<T> FindByName(String name);
    public Task<T> Save(T document);
    public Task<T> FindById(string? idSistema);
    public Task<bool> DeleteById(string id);
    public Task<List<T>> FindAll();
}