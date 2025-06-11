using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Repository;

namespace integra_dados.Services;

public class SupervisoryService(SupervisoryRepository supervisoryRepository)
{
    public ResponseClient Create(SupervisoryRegistry registry)
    {
        if(registry.TopicoBroker == null || registry.TopicoBroker.Equals("")){
            registry.TopicoBroker = registry.Nome;
        }
        
        if (!supervisoryRepository.existsByName(supervisoryRegistry.getNome())) {
            var savedRegistry = save(supervisoryRegistry);
            UpdatedSupervisoryRegistries.addRegistry(savedRegistry);
            return new ResponseClient(
                HttpStatus.OK,
                true, savedRegistry,
                "Registro de previsão adicionado com sucesso."
            );
        }
    }
}