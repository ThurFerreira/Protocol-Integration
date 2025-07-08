using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Models.SupervisoryModel.RegistryModel.WindyForecast;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/forecast/windy/")]
public class ForecastController(ForecastService service) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>>? Create([FromBody] ForecastRegistry registry)
    {
        registry.SetIdSistema();
        ResponseClient response = await service.Create(registry);
        
        return Ok(response);
    }

    [HttpPut("edit")]
    public async Task<ActionResult> Update([FromBody] ForecastRegistry registry)
    {
        ResponseClient response = await service.Edit(registry);
        return Ok(response);
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ResponseClient>> Delete([FromRoute] int id)
    {
        ResponseClient response = await service.Delete(id);
        return Ok(response);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseClient), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    public ActionResult<ResponseClient> GetOneSupervisoryRegister([FromRoute] int id)
    {
        var responseClient = ForecastService.GetOne(id);
        return Ok(responseClient);
    }
    
    [HttpGet("location")]
    public async Task<ActionResult> GetForecastOnLocation(Location location, string varType)
    {
        ResponseClient response = await service.GetLocationForecast(location, varType);
        return Ok(response);
    }


    [HttpGet("registries/{varType}")]
    public async Task<ActionResult> GetAllForecastsForVariable([FromRoute] string varType)
    {
        ResponseClient response = await service.GetAllForecastForVariable(varType);
        return Ok(response);
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