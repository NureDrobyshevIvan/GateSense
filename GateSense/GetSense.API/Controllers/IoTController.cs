using Domain.Models.DTOS.IoT;
using Domain.Models.DTOS.Sensors;
using GateSense.Application.IoT.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[ApiController]
[Route("iot")]
public class IoTController : ControllerBase
{
    private readonly IIoTService _iotService;

    public IoTController(IIoTService iotService)
    {
        _iotService = iotService;
    }

    [HttpPost("sensor-data")]
    public async Task<IActionResult> SubmitSensorData([FromBody] SensorDataSubmissionRequest request)
    {
        var result = await _iotService.SubmitSensorDataAsync(request);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> SendHeartbeat([FromBody] HeartbeatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            return BadRequest(new { error = "SerialNumber is required" });
        }

        var result = await _iotService.SendHeartbeatAsync(request.SerialNumber);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("gate-state")]
    public async Task<IActionResult> GetGateState([FromQuery] string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return BadRequest(new { error = "SerialNumber is required" });
        }

        var result = await _iotService.GetGateStateBySerialNumberAsync(serialNumber);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }
}

