using Microsoft.AspNetCore.Mvc;

namespace integra_dados.Services.Notifier;

[ApiController]
[Route("health/")]
public class HealthController(Report report) : ControllerBase
{
    
    [HttpGet("")]
    public Status GetStatus()
    {
        return report.Status;
    }

    [HttpGet("report")]
    public Report GetLastReport()
    {
        Report lastReport = (Report) report.Clone();
        report.Reset();
        return lastReport;
    }
}