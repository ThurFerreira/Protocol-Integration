using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using integra_dados.Util.Registries;
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
    
    [HttpGet("all-for-variable")]
    public ActionResult<ResponseClient> GetAllSupervisoryForVariable()
    {
        List<SupervisoryRegistry> registries = RegistryManager.GetRegistries();

        var responseClient = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );

        return StatusCode((int)responseClient.ResponseStatus, responseClient);
    }
    
    [HttpGet("find-one")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromQuery] int idSistema)
    {
        var responseClient = RegistryManager.GetOne(idSistema);
        return StatusCode((int)responseClient.ResponseStatus, responseClient);
    }
    
    [HttpGet("edit")]
    public IActionResult EditSupervisory([FromQuery] SupervisoryRegistry supervisory)
    {
        var responseFromEdition = supervisoryService.Edit(supervisory);
        return StatusCode((int)responseFromEdition.ResponseStatus, responseFromEdition);
    }
    
    [HttpDelete("delete")]
    public IActionResult DeleteSupervisoryRegistry([FromQuery] string id)
    {
        supervisoryService.Delete(id);
        return StatusCode(StatusCodes.Status200OK, new ResponseClient("Registro deletado com sucesso"));
    }

    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<SupervisoryRegistry> registries = RegistryManager.GetRegistries();
        ResponseClient response = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );
        return StatusCode((int)HttpStatusCode.OK, response);
    }
}