using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

public class SupervisoryController(SupervisoryService supervisoryService)
{
    SupervisoryService _supervisoryService = new SupervisoryService();
    
    [HttpPost("create")]
    // OpenAPI/Swagger annotations via comments or Swashbuckle attributes (less verbose for common cases)
    // [SwaggerOperation(Summary = "Cria um novo registro de supervisório para ser monitorado.", Description = "<<Não implementado ainda (Definir atributos do corpo)>>")]
    // [SwaggerResponse(200, "Registro criado com sucesso", typeof(string))]
    // [SwaggerResponse(409, "Registro já foi criado", typeof(string))]
    // [SwaggerResponse(500, "Erro interno no servidor.", typeof(string))]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] SupervisoryRegistry supervisory)
    {
        // Assuming setIdSistema() is a method on SupervisoryRegistry that sets an ID
        // In C#, this might be part of the constructor or a factory method, or handled in the service.
        // For now, mimicking the Java call:
        supervisory.SetIdSistema(); // Assuming this method exists on SupervisoryRegistry

        var responseFromRegistry = await _supervisoryService.Create(supervisory);

        // In ASP.NET Core, you return ActionResult<T> or IActionResult for more control over HTTP responses.
        // You can use StatusCode() or helper methods like Ok(), Conflict(), InternalServerError() (not directly)
        // It's generally better to use specific status codes for clarity.
        return StatusCode((int)responseFromRegistry.ResponseStatus, responseFromRegistry);
    }
}