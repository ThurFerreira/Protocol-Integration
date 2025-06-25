using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/forecast/windy/")]
public class ForecastController(ForecastService forecastService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>>? Create([FromBody] ForecastRegistry forecast)
    {
        forecast.SetIdSistema();
        ResponseClient response = await forecastService.Create(forecast);
        
        return Ok(response);
    }

    [HttpPut("edit")]
    public async Task<ActionResult> Update([FromBody] ForecastRegistry forecast)
    {
        ResponseClient response = await forecastService.Edit(forecast);
        return Ok(response);
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ResponseClient>> Delete([FromRoute] int id)
    {
        ResponseClient response = await forecastService.Delete(id);
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
        ResponseClient response = await forecastService.GetLocationForecast(location, varType);
        return Ok(response);
    }


    [HttpGet("registries/{varType}")]
    public async Task<ActionResult> GetAllForecastsForVariable([FromRoute] string varType)
    {
        ResponseClient response = await forecastService.GetAllForecastForVariable(varType);
        return Ok(response);
    }
    
    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<ForecastRegistry> registries = ForecastService.GetRegistries();
        ResponseClient response = new ResponseClient(
            HttpStatusCode.OK,
            true,
            registries,
            $"Busca recuperada (Quantidade de documentos: {registries.Count})."
        );
        return Ok(response);
    }
}