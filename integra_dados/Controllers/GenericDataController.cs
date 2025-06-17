using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/generic-data/")]
public class GenericDataController(GenericDataService dataService) : ControllerBase
{
    [HttpGet("create")]
    public IActionResult Create()
    {
        List<object> list = dataService.GetSupervisoryAndForecastData();
        return StatusCode(StatusCodes.Status200OK, list);
    }
}