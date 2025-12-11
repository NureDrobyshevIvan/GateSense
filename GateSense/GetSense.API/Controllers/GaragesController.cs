using Domain.Models.DTOS.Garages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages")]
public class GaragesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUserGarages()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("{garageId:int}")]
    public IActionResult GetGarageDetails(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost]
    public IActionResult CreateGarage([FromBody] CreateGarageRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("{garageId:int}")]
    public IActionResult UpdateGarage(int garageId, [FromBody] UpdateGarageRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("{garageId:int}")]
    public IActionResult DeleteGarage(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

