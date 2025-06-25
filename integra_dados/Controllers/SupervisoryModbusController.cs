using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/supervisory/modbus")]
public class SupervisoryModbusController(SupervisoryService supervisoryService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] SupervisoryRegistry supervisory)
    {
        supervisory.SetIdSistema();

        var responseFromRegistry = await supervisoryService.Create(supervisory);

        return Ok(responseFromRegistry);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromRoute] int id)
    {
        var responseClient = SupervisoryService.GetOne(id);
        return Ok(responseClient);
    }
    
    [HttpPut("edit")]
    public ActionResult EditSupervisory([FromBody] SupervisoryRegistry supervisory)
    {
        ResponseClient responseFromEdition = supervisoryService.Edit(supervisory).Result;
        return Ok(responseFromEdition);
    }
    
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteSupervisoryRegistry([FromRoute] int id)
    {
        supervisoryService.Delete(id);
        return Ok(new ResponseClient("Registro deletado com sucesso"));
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
        return Ok(response);
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

        return Ok(responseClient);
    }
}