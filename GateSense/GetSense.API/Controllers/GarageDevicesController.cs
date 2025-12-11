using Domain.Models.DTOS.Devices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/devices")]
public class GarageDevicesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDevices(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost]
    public IActionResult RegisterDevice(int garageId, [FromBody] RegisterDeviceRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("{deviceId:int}")]
    public IActionResult RemoveDevice(int garageId, int deviceId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

