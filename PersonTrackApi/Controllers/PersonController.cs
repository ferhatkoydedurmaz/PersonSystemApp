using Microsoft.AspNetCore.Mvc;
using PersonTrackApi.Models;
using PersonTrackApi.Services;
using RabbitMQEventBus.Constants;
using RabbitMQEventBus.Events;
using RabbitMQEventBus.Producer;
using System.Net;

namespace PersonTrackApi.Controllers;
[Route("movements")]
[ApiController]

public class PersonController : ControllerBase
{

    private readonly PersonMovementService _personMovementService;
    private readonly PersonMovementReportService _personMovementReportService;

    public PersonController(PersonMovementService personMovementService, PersonMovementReportService personMovementReportService)
    {
        _personMovementService = personMovementService;
        _personMovementReportService = personMovementReportService;
    }

    [HttpPost("enter")]
    public async Task<IActionResult> EmployeeEnter(int id)
    {
        var result = await _personMovementService.AddPersonEnterMovementAsync(id);
        return Ok(result);
    }
    [HttpPost("exit")]
    public async Task<IActionResult> EmployeeExit(int id)
    {
        var result = await _personMovementService.AddPersonExitMovementAsync(id);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPersonMovement(int id)
    {
        var result = await _personMovementService.GetPersonMovementById(id);
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> EmployeeMovement([FromQuery] PersonMovementSearchKey searchKeys)
    {
         var result = await _personMovementService.GetPersonMovementsWithQuery(searchKeys);
        return Ok(result);
    }
    [HttpGet("reports")]
    public async Task<IActionResult> EmployeeMovementReport([FromQuery] PersonMovementSearchKey searchKeys)
    {
        var result = await _personMovementReportService.GetPersonMovementReportWithQuery(searchKeys);
        return Ok(result);
    }
}
