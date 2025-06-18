using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/forecast/")]
public class ForecastController(ForecastService forecastService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] ForecastRegistry forecast)
    {
        forecast.SetIdSistema();
        ResponseClient response = await forecastService.Create(forecast);
        
        return StatusCode(200, response);
    }

    [HttpPut("edit")]
    public ActionResult Update([FromBody] ForecastRegistry forecast)
    {
        ResponseClient response = forecastService.Edit(forecast).Result;
        return StatusCode(200, response);
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ResponseClient>> Delete([FromRoute] string id)
    {
        ResponseClient response = await forecastService.Delete(id);
        return StatusCode(200, response);
    }
    
    [HttpGet("location")]
    public async Task<ActionResult> GetForecastOnLocation(double lat, double lng, string varType)
    {
        ResponseClient response = await forecastService.GetLocationForecast(lat, lng, varType);
        return StatusCode(200, response);
    }


    [HttpGet("registries/{varType}")]
    public async Task<ActionResult> GetAllForecastsForVariable([FromRoute] string varType)
    {
        ResponseClient response = await forecastService.GetAllForecastForVariable(varType);
        return StatusCode(200, response);
    }
}