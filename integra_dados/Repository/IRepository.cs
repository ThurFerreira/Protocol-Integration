using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface IRepository<T>
{
    public Task<List<T>> FindByName(String name);
    public Task<T> FindOneByName(String name);
    public Task<T> Save(T document);
    public Task<T> FindById(string? idSistema);
    public Task<bool> DeleteById(string id);
    public Task<List<T>> FindAll();
    public Task<T> FindByNameAndVarType(string name, string varType);
    public Task<T> ReplaceOne(T document);
}