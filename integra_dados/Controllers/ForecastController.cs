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
    [HttpPost("create")] //POST 
    public async Task<ActionResult<ResponseClient>>? Create([FromBody] ForecastRegistry forecast)
    {
        forecast.SetIdSistema();
        ResponseClient response = await forecastService.Create(forecast);
        
        return Ok(response);
    }

    [HttpPut("edit")]
    public ActionResult Update([FromBody] ForecastRegistry forecast)
    {
        ResponseClient response = forecastService.Edit(forecast).Result;
        return Ok(response);
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ResponseClient>> Delete([FromRoute] int id)
    {
        ResponseClient response = await forecastService.Delete(id);
        return Ok(response);
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
}