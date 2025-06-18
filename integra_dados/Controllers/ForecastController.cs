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
    public async Task<ActionResult<ResponseClient>> Update([FromBody] ForecastRegistry forecast)
    {
        ResponseClient response = await forecastService.Edit(forecast);
        return StatusCode(200, response);
    }
    
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ResponseClient>> Delete([FromRoute] string id)
    {
        ResponseClient response = await forecastService.Delete(id);
        return StatusCode(200, response);
    }
    
    [HttpGet("location")]
    public IActionResult GetForecastOnPoint(long lat, long lng, string varType)
    {
        forecastService.GetForecast(lat, lng, varType);
        return StatusCode(201);
    }


    [HttpGet("registries/{name}")]
    public IActionResult GetAllForecastsForVariable([FromRoute] string name)
    {
        // return this.weatherService.getAllForecastForVariable(this.getForecastVariableName());
        return StatusCode(201);
    }
}