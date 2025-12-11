using Domain.Models.DTOS.Sensors;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[ApiController]
[Route("iot")]
public class IoTController : ControllerBase
{
    [HttpPost("sensor-data")]
    public IActionResult SubmitSensorData([FromBody] SensorDataSubmissionRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("heartbeat")]
    public IActionResult SendHeartbeat([FromBody] string serialNumber)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

