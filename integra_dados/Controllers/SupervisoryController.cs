using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/supervisory/registry/")]
public class SupervisoryController(SupervisoryService supervisoryService) : ControllerBase
{
    [HttpPut("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] SupervisoryRegistry supervisory)
    {
        supervisory.SetIdSistema();

        var responseFromRegistry = await supervisoryService.Create(supervisory);

        return StatusCode((int) responseFromRegistry.ResponseStatus, responseFromRegistry);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromRoute] int idSistema)
    {
        var responseClient = SupervisoryService.GetOne(idSistema);
        return StatusCode((int)responseClient.ResponseStatus, responseClient);
    }
    
    [HttpPut("edit")]
    public ActionResult EditSupervisory([FromBody] SupervisoryRegistry supervisory)
    {
        ResponseClient responseFromEdition = supervisoryService.Edit(supervisory).Result;
        return StatusCode((int)responseFromEdition.ResponseStatus, responseFromEdition);
    }
    
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteSupervisoryRegistry([FromRoute] string id)
    {
        supervisoryService.Delete(id);
        return StatusCode(StatusCodes.Status200OK, new ResponseClient("Registro deletado com sucesso"));
    }

    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<SupervisoryRegistry> registries = SupervisoryService.GetRegistries();
        ResponseClient response = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );
        return StatusCode((int)HttpStatusCode.OK, response);
    }
    
        
    [HttpGet("variable/{id}/all")]
    public ActionResult<ResponseClient> GetAllSupervisoryForVariable()
    {
        List<SupervisoryRegistry> registries = SupervisoryService.GetRegistries();

        var responseClient = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );

        return StatusCode((int)responseClient.ResponseStatus, responseClient);
    }
}