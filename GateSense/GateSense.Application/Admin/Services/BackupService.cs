using Domain.Models.Auth;
using Domain.Models.DTOS.Admin;
using Domain.Models.Garages;
using Domain.Models.Gates;
using Domain.Models.Devices;
using Domain.Models.Sensors;
using GateSense.Application.Admin.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Admin.Services;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BackupService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result<BackupDto>> ExportAsync()
    {
        try
        {
            var dto = new BackupDto();

            var users = await _userManager.Users.ToListAsync();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                dto.Users.Add(new BackupUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = roles.ToList()
                });
            }

            // Load entities into memory first, then map (avoids EF translation issues with enum casts)
            var garages = await _db.Garages.AsNoTracking().ToListAsync();
            dto.Garages = garages.Select(g => new BackupGarage
            {
                Id = g.Id,
                Name = g.Name,
                Address = g.Address,
                TimeZone = g.TimeZone,
                OwnerId = g.OwnerId
            }).ToList();

            var devices = await _db.Devices.AsNoTracking().ToListAsync();
            dto.Devices = devices.Select(d => new BackupDevice
            {
                Id = d.Id,
                GarageId = d.GarageId,
                SerialNumber = d.SerialNumber,
                DeviceType = (int)d.DeviceType,
                Status = (int)d.Status,
                FirmwareVersion = d.FirmwareVersion,
                LastHeartbeatOn = d.LastHeartbeatOn
            }).ToList();

            var keys = await _db.AccessKeys.AsNoTracking().ToListAsync();
            dto.AccessKeys = keys.Select(k => new BackupAccessKey
            {
                Id = k.Id,
                GarageId = k.GarageId,
                IssuedByUserId = k.IssuedByUserId,
                KeyType = (int)k.KeyType,
                Status = (int)k.Status,
                Token = k.Token,
                ExpiresOn = k.ExpiresOn
            }).ToList();

            var accesses = await _db.GarageAccesses.AsNoTracking().ToListAsync();
            dto.GarageAccess = accesses.Select(a => new BackupGarageAccess
            {
                Id = a.Id,
                GarageId = a.GarageId,
                UserId = a.UserId,
                AccessLevel = (int)a.AccessLevel,
                ExpiresOn = a.ExpiresOn
            }).ToList();

            var events = await _db.GateEvents.AsNoTracking().ToListAsync();
            dto.GateEvents = events.Select(e => new BackupGateEvent
            {
                Id = e.Id,
                GarageId = e.GarageId,
                InitiatorUserId = e.InitiatorUserId,
                AccessKeyId = e.AccessKeyId,
                TriggerSource = (int)e.TriggerSource,
                Action = (int)e.Action,
                Result = (int)e.Result,
                FailureReason = e.FailureReason,
                CreatedOn = e.CreatedOn
            }).ToList();

            var readings = await _db.SensorReadings.AsNoTracking().ToListAsync();
            dto.SensorReadings = readings.Select(s => new BackupSensorReading
            {
                Id = s.Id,
                DeviceId = s.DeviceId,
                SensorType = (int)s.SensorType,
                Value = s.Value,
                Unit = s.Unit,
                RecordedOn = s.RecordedOn
            }).ToList();

            return Result<BackupDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<BackupDto>.Failure(
                Error.InternalServerError("backup.EXPORT_FAILED",
                    $"Export failed: {ex.GetType().Name}: {ex.Message}"));
        }
    }

    public async Task<Result<ImportBackupResponse>> ImportAsync(BackupDto backup)
    {
        if (backup == null)
        {
            return Result<ImportBackupResponse>.Failure(
                Error.Validation("backup.INVALID_PAYLOAD", "Backup payload is empty"));
        }

        var stats = new ImportBackupResponse();

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Garages
            foreach (var g in backup.Garages)
            {
                var existing = await _db.Garages.FindAsync(g.Id);
                if (existing == null)
                {
                    _db.Garages.Add(new Garage
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Address = g.Address,
                        TimeZone = g.TimeZone,
                        OwnerId = g.OwnerId
                    });
                }
                else
                {
                    existing.Name = g.Name;
                    existing.Address = g.Address;
                    existing.TimeZone = g.TimeZone;
                    existing.OwnerId = g.OwnerId;
                }
                stats.GaragesImported++;
            }
            await _db.SaveChangesAsync();

            // Devices
            foreach (var d in backup.Devices)
            {
                var existing = await _db.Devices.FindAsync(d.Id);
                if (existing == null)
                {
                    _db.Devices.Add(new IoTDevice
                    {
                        Id = d.Id,
                        GarageId = d.GarageId,
                        SerialNumber = d.SerialNumber,
                        DeviceType = (DeviceType)d.DeviceType,
                        Status = (DeviceStatus)d.Status,
                        FirmwareVersion = d.FirmwareVersion,
                        LastHeartbeatOn = d.LastHeartbeatOn
                    });
                }
                else
                {
                    existing.GarageId = d.GarageId;
                    existing.SerialNumber = d.SerialNumber;
                    existing.DeviceType = (DeviceType)d.DeviceType;
                    existing.Status = (DeviceStatus)d.Status;
                    existing.FirmwareVersion = d.FirmwareVersion;
                    existing.LastHeartbeatOn = d.LastHeartbeatOn;
                }
                stats.DevicesImported++;
            }
            await _db.SaveChangesAsync();

            // AccessKeys
            foreach (var k in backup.AccessKeys)
            {
                var existing = await _db.AccessKeys.FindAsync(k.Id);
                if (existing == null)
                {
                    _db.AccessKeys.Add(new AccessKey
                    {
                        Id = k.Id,
                        GarageId = k.GarageId,
                        IssuedByUserId = k.IssuedByUserId,
                        KeyType = (AccessKeyType)k.KeyType,
                        Status = (AccessKeyStatus)k.Status,
                        Token = k.Token,
                        ExpiresOn = k.ExpiresOn
                    });
                }
                else
                {
                    existing.Status = (AccessKeyStatus)k.Status;
                    existing.ExpiresOn = k.ExpiresOn;
                }
                stats.AccessKeysImported++;
            }
            await _db.SaveChangesAsync();

            // GarageAccess
            foreach (var a in backup.GarageAccess)
            {
                var existing = await _db.GarageAccesses.FindAsync(a.Id);
                if (existing == null)
                {
                    _db.GarageAccesses.Add(new GarageAccess
                    {
                        Id = a.Id,
                        GarageId = a.GarageId,
                        UserId = a.UserId,
                        AccessLevel = (AccessLevel)a.AccessLevel,
                        ExpiresOn = a.ExpiresOn
                    });
                }
                else
                {
                    existing.AccessLevel = (AccessLevel)a.AccessLevel;
                    existing.ExpiresOn = a.ExpiresOn;
                }
                stats.GarageAccessImported++;
            }
            await _db.SaveChangesAsync();

            // GateEvents (insert-only, never update history)
            foreach (var e in backup.GateEvents)
            {
                var exists = await _db.GateEvents.AnyAsync(x => x.Id == e.Id);
                if (!exists)
                {
                    _db.GateEvents.Add(new GateEvent
                    {
                        Id = e.Id,
                        GarageId = e.GarageId,
                        InitiatorUserId = e.InitiatorUserId,
                        AccessKeyId = e.AccessKeyId,
                        TriggerSource = (GateTriggerSource)e.TriggerSource,
                        Action = (GateAction)e.Action,
                        Result = (GateActionResult)e.Result,
                        FailureReason = e.FailureReason
                    });
                    stats.GateEventsImported++;
                }
            }
            await _db.SaveChangesAsync();

            // SensorReadings (insert-only)
            foreach (var s in backup.SensorReadings)
            {
                var exists = await _db.SensorReadings.AnyAsync(x => x.Id == s.Id);
                if (!exists)
                {
                    _db.SensorReadings.Add(new SensorReading
                    {
                        Id = s.Id,
                        DeviceId = s.DeviceId,
                        SensorType = (SensorType)s.SensorType,
                        Value = s.Value,
                        Unit = s.Unit,
                        RecordedOn = s.RecordedOn
                    });
                    stats.SensorReadingsImported++;
                }
            }
            await _db.SaveChangesAsync();

            stats.UsersImported = backup.Users.Count;

            await transaction.CommitAsync();
            return Result<ImportBackupResponse>.Success(stats);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<ImportBackupResponse>.Failure(
                Error.InternalServerError("backup.IMPORT_FAILED", $"Import failed: {ex.Message}"));
        }
    }
}
