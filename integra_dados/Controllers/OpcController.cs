using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel;
using integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;
using integra_dados.Supervisory.OPC;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/supervisory/opcua/")]
public class OpcController(OpcService service) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] OpcRegistry registry)
    {
        registry.SetIdSistema();

        var responseFromRegistry = await service.Create(registry);

        return Ok(responseFromRegistry);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromRoute] int id)
    {
        var responseClient = OpcService.GetOne(id);
        return Ok(responseClient);
    }
    
    [HttpPut("edit")]
    public async Task<ActionResult> EditSupervisory([FromBody] OpcRegistry registry)
    {
        ResponseClient responseFromEdition = await service.Edit(registry);
        return Ok(responseFromEdition);
    }
    
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteSupervisoryRegistry([FromRoute] int id)
    {
        service.Delete(id);
        return Ok(new ResponseClient("Registro deletado com sucesso"));
    }

    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<Registry> registries = service.GetRegistries();
        ResponseClient response = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );
        return Ok(response);
    }
}