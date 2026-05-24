using Domain.Models.DTOS.Admin;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Admin.Interfaces;

public interface IBackupService
{
    Task<Result<BackupDto>> ExportAsync();
    Task<Result<ImportBackupResponse>> ImportAsync(BackupDto backup);
}
