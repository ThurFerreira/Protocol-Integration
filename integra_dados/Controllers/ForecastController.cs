using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using integra_dados.Util.Registries;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/forecast/")]
public class ForecastController(ForecastService forecastService) : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create([FromBody] ForecastRegistry forecast)
    {
        var responseClient = forecastService.Create(forecast);
        return StatusCode(201);
    }

    [HttpPut("update")]
    public async Task<ActionResult<ResponseClient>> Update([FromBody] ForecastRegistry forecast)
    {
        forecast.SetIdSistema();
        var responseFromRegistry = await ForecastService.Create(forecast);
        
        return StatusCode(200, responseClient);
    }

    [HttpDelete("delete/{id}")]
    public IActionResult Delete([FromRoute] int id)
    {
        // this.weatherService.deleteForecast(idSistema);
        // return ResponseEntity.status(HttpStatus.OK).body(new ResponseClient("Registro deletado com sucesso"));
        return StatusCode(201);
    }
    
    [HttpGet("location")]
    public IActionResult GetForecastOnPoint()
    {
        // return this.weatherService.getForecast(new Location(lat, lng), this.getForecastVariableName());
        return StatusCode(201);
    }


    [HttpGet("registries/{name}")]
    public IActionResult GetAllForecastsForVariable([FromRoute] string name)
    {
        // return this.weatherService.getAllForecastForVariable(this.getForecastVariableName());
        return StatusCode(201);
    }
}