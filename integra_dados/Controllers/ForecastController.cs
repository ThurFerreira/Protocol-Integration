using integra_dados.Models;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/forecast/")]
public class ForecastController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create([FromBody] ForecastRegistry forecast)
    {
        // var reponseClient = this.weatherService.createForecast(
        //     forecastRegistry,
        //     this.getForecastVariableName()
        // );
        // return ResponseEntity.status(reponseClient.getResponseStatus()).body(reponseClient);
        return StatusCode(201);
    }

    [HttpPut("update")]
    public IActionResult Update([FromBody] ForecastRegistry forecast)
    {
        // var reponseClient = this.weatherService.editForecast(
        //     forecastRegistry,
        //     this.getForecastVariableName()
        // );
        // return ResponseEntity.status(reponseClient.getResponseStatus()).body(reponseClient);
        return StatusCode(201);
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

    [HttpPost("registries/{name}")]
    public IActionResult GetAllForecastsForVariable([FromRoute] string name)
    {
        // return this.weatherService.getAllForecastForVariable(this.getForecastVariableName());
        return StatusCode(201);
    }
}