using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;
using integra_dados.Util.Registries;

namespace integra_dados.Services;

public class SupervisoryService(SupervisoryRepository supervisoryRepository)
{
    public async Task<ResponseClient> Create(SupervisoryRegistry registry)
    {
        if(registry.TopicoBroker == null || registry.TopicoBroker.Equals("")){
            registry.TopicoBroker = registry.Nome;
        }
        
        if (supervisoryRepository.FindByName(registry.Nome) != null) {
            var savedRegistry = await supervisoryRepository.Save(registry);
            RegistryManager.AddRegistry(savedRegistry);
            return new ResponseClient(
                HttpStatusCode.OK,
                true, savedRegistry,
                "Registro de previsão adicionado com sucesso."
            );
        }

        return new ResponseClient(
            HttpStatusCode.Conflict,
            false, null,
            "Registro de previsão com nome '"+ registry.Nome+"' já foi criado.");
    }
}