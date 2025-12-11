using Domain.Models.DTOS.Gates;
using Domain.Models.Gates;
using Infrastructure.Common.PagedList;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Logs.Interfaces;

public interface ILogService
{
    Task<Result<IPagedList<GateEvent>>> GetGateLogsAsync(int garageId, int userId, GateLogQuery query);
}

