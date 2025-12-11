using Domain.Models.DTOS.Gates;
using Domain.Models.Garages;
using Domain.Models.Gates;
using GateSense.Application.Logs.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.PagedList;
using Infrastructure.Common.Predicate;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Logs.Services;

public class LogService : ILogService
{
    private readonly IGenericRepository<GateEvent> _gateEventRepository;
    private readonly IGenericRepository<Garage> _garageRepository;

    private static readonly Error GarageNotFound =
        Error.NotFound("log.GARAGE_NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("log.FORBIDDEN", "You do not have access to this garage");

    public LogService(
        IGenericRepository<GateEvent> gateEventRepository,
        IGenericRepository<Garage> garageRepository)
    {
        _gateEventRepository = gateEventRepository;
        _garageRepository = garageRepository;
    }

    public async Task<Result<IPagedList<GateEvent>>> GetGateLogsAsync(int garageId, int userId, GateLogQuery query)
    {
        // Verify garage access
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result<IPagedList<GateEvent>>.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result<IPagedList<GateEvent>>.Failure(ForbiddenGarageAccess);
        }

        // Build conditions
        var conditions = new List<(System.Linq.Expressions.Expression<Func<GateEvent, bool>>, PredicateOptions)>
        {
            (g => g.GarageId == garageId, PredicateOptions.AND)
        };

        if (query.From.HasValue)
        {
            conditions.Add((g => g.CreatedOn >= query.From.Value, PredicateOptions.AND));
        }

        if (query.To.HasValue)
        {
            conditions.Add((g => g.CreatedOn <= query.To.Value, PredicateOptions.AND));
        }

        if (!string.IsNullOrWhiteSpace(query.TriggerSource) && 
            Enum.TryParse<GateTriggerSource>(query.TriggerSource, ignoreCase: true, out var triggerSource))
        {
            conditions.Add((g => g.TriggerSource == triggerSource, PredicateOptions.AND));
        }

        var includes = new List<Func<IQueryable<GateEvent>, IQueryable<GateEvent>>>
        {
            q => q.Include(g => g.InitiatorUser),
            q => q.Include(g => g.AccessKey)
        };

        var result = await _gateEventRepository.FetchPaginatedByConditions(
            conditions,
            (g => g.CreatedOn, true), // Order by CreatedOn descending (newest first)
            includes,
            isNoTracking: true,
            isSplitQuery: false,
            query.PageNumber,
            query.PageSize);

        return result;
    }
}

