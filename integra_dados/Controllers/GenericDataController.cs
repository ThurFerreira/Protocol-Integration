using System.Net;
using integra_dados.Models;
using integra_dados.Models.Response;
using integra_dados.Services;
using integra_dados.Services.Modbus;
using integra_dados.Supervisory.OPC;
using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Controllers;

[ApiController]
[Route("/generic-data/")]
public class GenericDataController(
    OpcService opcService,
    ModbusService modbusService,
    ForecastService forecastService
    ) : ControllerBase
{
    [HttpGet("all")]
    public ActionResult<ResponseClient> GetAll()
    {
        List<List<ReadRegistry>> registryList = new List<List<ReadRegistry>>();
        registryList.Add(opcService.GetRegistries());
        registryList.Add(modbusService.GetRegistries());
        registryList.Add(forecastService.GetRegistries());
        return Ok(new ResponseClient(HttpStatusCode.OK, true, registryList, "Registros recuperados com sucesso!"));
    } 
}