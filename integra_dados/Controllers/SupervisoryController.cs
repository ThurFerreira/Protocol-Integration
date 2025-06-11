using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

public class SupervisoryController(SupervisoryService supervisoryService) : ControllerBase
{
    [HttpPost("create")]
    // OpenAPI/Swagger annotations via comments or Swashbuckle attributes (less verbose for common cases)
    // [SwaggerOperation(Summary = "Cria um novo registro de supervisório para ser monitorado.", Description = "<<Não implementado ainda (Definir atributos do corpo)>>")]
    // [SwaggerResponse(200, "Registro criado com sucesso", typeof(string))]
    // [SwaggerResponse(409, "Registro já foi criado", typeof(string))]
    // [SwaggerResponse(500, "Erro interno no servidor.", typeof(string))]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] SupervisoryRegistry supervisory)
    {
        supervisory.SetIdSistema(); // Assuming this method exists on SupervisoryRegistry

        var responseFromRegistry = await supervisoryService.Create(supervisory);

        return StatusCode((int) responseFromRegistry.ResponseStatus, responseFromRegistry);
    }
}