using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;

namespace integra_dados.Util.Registries;

public class RegistryManager
{
    private static Dictionary<string, SupervisoryRegistry> registries = new Dictionary<string, SupervisoryRegistry>();

    public static void AddRegistry(SupervisoryRegistry supervisoryRegistry)
    {
        registries.Add(supervisoryRegistry.Id, supervisoryRegistry);
    }

    public static List<SupervisoryRegistry> GetRegistries()
    {
        return registries.Values.ToList();
    }

    public static ResponseClient GetOne(int idSistema)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.Value.IdSistema.Equals(idSistema));

        return CreateResponseToFoundRegistry(idSistema, foundRegistry.Value);
    }

    private static ResponseClient CreateResponseToFoundRegistry(int idSistema, SupervisoryRegistry? foundRegistry)
    {
        if (foundRegistry != null)
        {
            return new ResponseClient(
                HttpStatusCode.OK,
                true,
                foundRegistry,
                "Previsão recuperado com sucesso."
            );
        }
        else
        {
            return new ResponseClient(
                HttpStatusCode.OK,
                false,
                null,
                $"Não foi possível recuperar a previsão cujo id = {idSistema}."
            );
        }
    }

    public static void ReplaceRegistry(SupervisoryRegistry supervisoryEdited)
    {
        foreach (var registry in registries.Values.ToList())
        {
            if (registry.IdSistema.Equals(supervisoryEdited.IdSistema))
            {
                registries[registry.IdSistema] = supervisoryEdited;
            }
        }
    }

    public static void StartRegistries(List<SupervisoryRegistry> updateRegistries)
    {
        foreach (var supervisoryRegistry in updateRegistries)
        {
            registries.Add(supervisoryRegistry.Id, supervisoryRegistry);
        }
    }

    public static void DeleteRegisry(string id)
    {
        registries.Remove(id);
    }
    
}