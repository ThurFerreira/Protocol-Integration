using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

public class SupervisoryController(SupervisoryService supervisoryService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ResponseClient>> Create([FromBody] SupervisoryRegistry supervisory)
    {
        supervisory.SetIdSistema();

        var responseFromRegistry = await supervisoryService.Create(supervisory);

        return StatusCode((int) responseFromRegistry.ResponseStatus, responseFromRegistry);
    }
}