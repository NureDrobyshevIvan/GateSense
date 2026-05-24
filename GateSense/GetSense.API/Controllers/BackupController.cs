using System.Text;
using System.Text.Json;
using Domain.Models.DTOS.Admin;
using GateSense.Application.Admin.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("admin/backup")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var result = await _backupService.ExportAsync();
        if (!result.IsSuccess)
        {
            return ApiResults.ToProblemDetails(Infrastructure.Common.ResultPattern.Result.Failure(result.Errors));
        }

        var json = JsonSerializer.Serialize(result.Value, JsonOptions);
        var fileName = $"gatesense-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    [HttpPost("import")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }

        BackupDto? backup;
        try
        {
            await using var stream = file.OpenReadStream();
            backup = await JsonSerializer.DeserializeAsync<BackupDto>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = $"Invalid JSON: {ex.Message}" });
        }

        if (backup == null)
        {
            return BadRequest(new { error = "Backup payload is empty" });
        }

        var result = await _backupService.ImportAsync(backup);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }
}
