using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;
using integra_dados.Services;
using integra_dados.Services.Modbus;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/supervisory/modbus")]
public class ModbusController(ModbusService service) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] ModbusRegistry registry)
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
        var responseClient = ModbusService.GetOne(id);
        return Ok(responseClient);
    }
    
    [HttpPut("edit")]
    public async Task<ActionResult> EditSupervisory([FromBody] ModbusRegistry modbus)
    {
        ResponseClient responseFromEdition = await service.Edit(modbus);
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
    
        
    [HttpGet("variable/{id}/all")]
    public ActionResult<ResponseClient> GetAllSupervisoryForVariable()
    {
        List<Registry> registries = service.GetRegistries();

        var responseClient = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );

        return Ok(responseClient);
    }
}