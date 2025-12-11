using Domain.Models.DTOS.Gates;
using Domain.Models.Gates;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Gates.Interfaces;

public interface IGateService
{
    Task<Result> OpenGateAsync(int garageId, GateCommandRequest request, int userId);

    Task<Result> CloseGateAsync(int garageId, GateCommandRequest request, int userId);

    Task<Result<GateStateResponse>> GetGateStateAsync(int garageId, int userId);
}

