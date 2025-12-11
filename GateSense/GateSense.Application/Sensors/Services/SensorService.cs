using Domain.Models.DTOS.Sensors;
using Domain.Models.Garages;
using Domain.Models.Sensors;
using GateSense.Application.Sensors.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.PagedList;
using Infrastructure.Common.Predicate;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Sensors.Services;

public class SensorService : ISensorService
{
    private readonly IGenericRepository<SensorReading> _sensorReadingRepository;
    private readonly IGenericRepository<Garage> _garageRepository;

    private static readonly Error GarageNotFound =
        Error.NotFound("sensor.GARAGE_NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("sensor.FORBIDDEN", "You do not have access to this garage");

    // Alert thresholds
    private const decimal CO_ALERT_THRESHOLD = 50; // ppm
    private const decimal SMOKE_ALERT_THRESHOLD = 0; // any detection

    public SensorService(
        IGenericRepository<SensorReading> sensorReadingRepository,
        IGenericRepository<Garage> garageRepository)
    {
        _sensorReadingRepository = sensorReadingRepository;
        _garageRepository = garageRepository;
    }

    public async Task<Result<IEnumerable<SensorReading>>> GetLatestReadingsAsync(int garageId, int userId)
    {
        // Verify garage access
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result<IEnumerable<SensorReading>>.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result<IEnumerable<SensorReading>>.Failure(ForbiddenGarageAccess);
        }

        // Get all devices for this garage
        var includes = new List<Func<IQueryable<SensorReading>, IQueryable<SensorReading>>>
        {
            q => q.Include(s => s.Device).ThenInclude(d => d.Garage)
        };

        var allReadingsResult = await _sensorReadingRepository.GetListByConditionAsync(
            s => s.Device.GarageId == garageId,
            includes);

        if (!allReadingsResult.IsSuccess)
        {
            return Result<IEnumerable<SensorReading>>.Failure(allReadingsResult.Errors);
        }

        // Group by sensor type and get latest reading for each type
        var latestReadings = allReadingsResult.Value
            .GroupBy(s => s.SensorType)
            .Select(g => g.OrderByDescending(s => s.RecordedOn).First())
            .ToList();

        return Result<IEnumerable<SensorReading>>.Success(latestReadings);
    }

    public async Task<Result<IPagedList<SensorReading>>> GetSensorHistoryAsync(int garageId, int userId, SensorHistoryQuery query)
    {
        // Verify garage access
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result<IPagedList<SensorReading>>.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result<IPagedList<SensorReading>>.Failure(ForbiddenGarageAccess);
        }

        // Build conditions
        var conditions = new List<(System.Linq.Expressions.Expression<Func<SensorReading, bool>>, PredicateOptions)>
        {
            (s => s.Device.GarageId == garageId, PredicateOptions.AND)
        };

        if (query.From.HasValue)
        {
            conditions.Add((s => s.RecordedOn >= query.From.Value, PredicateOptions.AND));
        }

        if (query.To.HasValue)
        {
            conditions.Add((s => s.RecordedOn <= query.To.Value, PredicateOptions.AND));
        }

        if (!string.IsNullOrWhiteSpace(query.SensorType) && 
            Enum.TryParse<SensorType>(query.SensorType, ignoreCase: true, out var sensorType))
        {
            conditions.Add((s => s.SensorType == sensorType, PredicateOptions.AND));
        }

        var includes = new List<Func<IQueryable<SensorReading>, IQueryable<SensorReading>>>
        {
            q => q.Include(s => s.Device)
        };

        var result = await _sensorReadingRepository.FetchPaginatedByConditions(
            conditions,
            (s => s.RecordedOn, true), // Order by RecordedOn descending
            includes,
            isNoTracking: true,
            isSplitQuery: false,
            query.PageNumber,
            query.PageSize);

        return result;
    }

    public async Task<Result<IEnumerable<SensorReading>>> GetActiveAlertsAsync(int garageId, int userId)
    {
        // Verify garage access
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result<IEnumerable<SensorReading>>.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result<IEnumerable<SensorReading>>.Failure(ForbiddenGarageAccess);
        }

        var includes = new List<Func<IQueryable<SensorReading>, IQueryable<SensorReading>>>
        {
            q => q.Include(s => s.Device).ThenInclude(d => d.Garage)
        };

        // Get all readings for this garage
        var allReadingsResult = await _sensorReadingRepository.GetListByConditionAsync(
            s => s.Device.GarageId == garageId,
            includes);

        if (!allReadingsResult.IsSuccess)
        {
            return Result<IEnumerable<SensorReading>>.Failure(allReadingsResult.Errors);
        }

        // Get latest readings for each sensor type
        var latestReadings = allReadingsResult.Value
            .GroupBy(s => new { s.DeviceId, s.SensorType })
            .Select(g => g.OrderByDescending(s => s.RecordedOn).First())
            .ToList();

        // Filter alerts based on thresholds
        var alerts = latestReadings.Where(s =>
        {
            return s.SensorType switch
            {
                SensorType.CarbonMonoxide => s.Value > CO_ALERT_THRESHOLD,
                SensorType.Smoke => s.Value > SMOKE_ALERT_THRESHOLD,
                _ => false // Only CO and Smoke generate alerts
            };
        }).ToList();

        return Result<IEnumerable<SensorReading>>.Success(alerts);
    }
}

