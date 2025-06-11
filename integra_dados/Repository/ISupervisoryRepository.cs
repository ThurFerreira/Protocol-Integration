using System.Reflection.Metadata;
using integra_dados.Models;

namespace integra_dados.Repository;

public interface ISupervisoryRepository
{
    public Task<SupervisoryRegistry> FindByName(String name);
    public Task<SupervisoryRegistry> Save(SupervisoryRegistry document);
    public Task<SupervisoryRegistry> FindById(int idSistema);
    public void DeleteByIdSistema(int idSistema);
    public List<SupervisoryRegistry> FindAll();
}