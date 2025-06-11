// using integra_dados.Models.Response;
// using integra_dados.Models.Response.Api;
// using Microsoft.AspNetCore.Mvc;
//
// namespace integra_dados.Controllers;
//
// [ApiController]
// public class ForecastController
// {
//     [HttpGet("forecast-on-location")]
//     [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ResponseClient), StatusCodes.Status500InternalServerError)]
//     public IActionResult GetForecastForToday(
//         [FromQuery] decimal lat = -25.144548639707363m,
//         [FromQuery] decimal lng = -50.17815056912481m)
//     {
//         // var result = _weatherService.GetForecast(new Location(lat, lng), _forecastVariableName);
//         // return Ok(result);
//     }
// }