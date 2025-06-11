using integra_dados.Models;

namespace integra_dados.Util.Registries;

public class RegistryManager
{
    private static List<SupervisoryRegistry> registries = new List<SupervisoryRegistry>();

    public static void AddRegistry(SupervisoryRegistry supervisoryRegistry) {
        registries.Add(supervisoryRegistry);
    }
}