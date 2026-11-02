using Microsoft.AspNetCore.Mvc;
using SpendWise.DTOs;
using SpendWise.Services;

namespace SpendWise.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(ReportService reports) : ControllerBase
{
    [HttpGet("monthly/{year:int}/{month:int}")]
    public async Task<ActionResult<MonthlyReportDto>> GetMonthly(int year, int month, CancellationToken cancellationToken) => Ok(await reports.GetMonthlyAsync(year, month, cancellationToken));
}
