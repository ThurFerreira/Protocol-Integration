using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;

namespace integra_dados.Util.Registries;

public class RegistryManager
{
    private static List<SupervisoryRegistry> registries = new List<SupervisoryRegistry>();

    public static void AddRegistry(SupervisoryRegistry supervisoryRegistry)
    {
        registries.Add(supervisoryRegistry);
    }

    public static List<SupervisoryRegistry> GetRegistries()
    {
        return registries;
    }

    public static ResponseClient GetOne(int idSistema)
    {
        var foundRegistry = registries
            .FirstOrDefault(registry => registry.IdSistema.Equals(idSistema));

        return CreateResponseToFoundRegistry(idSistema, foundRegistry);
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
        for (int i = 0; i < registries.Count; i++)
        {
            if (registries[i].IdSistema == supervisoryEdited.IdSistema)
            {
                registries[i] = supervisoryEdited;
                break;
            }
        }
    }

    public static void UpdateRegistries(List<SupervisoryRegistry> updateRegistries)
    {
        registries = updateRegistries;
    }
    
}