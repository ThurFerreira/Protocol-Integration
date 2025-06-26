using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services.Modbus;
using integra_dados.Supervisory.OPC;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

public class OpcController(OpcService opcService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] OpcRegistry modbus)
    {
        modbus.SetIdSistema();

        var responseFromRegistry = await opcService.Create(modbus);

        return Ok(responseFromRegistry);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromRoute] int id)
    {
        var responseClient = ModbusService.GetOne(id);
        return Ok(responseClient);
    }
    
    [HttpPut("edit")]
    public async Task<ActionResult> EditSupervisory([FromBody] OpcRegistry modbus)
    {
        ResponseClient responseFromEdition = await opcService.Edit(modbus);
        return Ok(responseFromEdition);
    }
    
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteSupervisoryRegistry([FromRoute] int id)
    {
        opcService.Delete(id);
        return Ok(new ResponseClient("Registro deletado com sucesso"));
    }

    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<OpcRegistry> registries = opcService.GetRegistries();
        ResponseClient response = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );
        return Ok(response);
    }
}