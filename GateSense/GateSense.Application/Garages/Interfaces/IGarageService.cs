using Domain.Models.DTOS.Garages;
using Domain.Models.Garages;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Garages.Interfaces;

public interface IGarageService
{
    Task<Result<IEnumerable<Garage>>> GetUserGaragesAsync(int userId);

    Task<Result<Garage>> GetGarageAsync(int garageId, int userId);

    Task<Result<int>> CreateGarageAsync(CreateGarageRequest request, int userId);

    Task<Result> UpdateGarageAsync(int garageId, UpdateGarageRequest request, int userId);

    Task<Result> DeleteGarageAsync(int garageId, int userId);
}

