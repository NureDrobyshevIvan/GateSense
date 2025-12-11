using Domain.Models.DTOS.Garages;
using Domain.Models.Garages;
using GateSense.Application.Garages.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;

namespace GateSense.Application.Garages.Services;

public class GarageService : IGarageService
{
    private readonly IGenericRepository<Garage> _garageRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Error GarageNotFound =
        Error.NotFound("garage.NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("garage.FORBIDDEN", "You do not have access to this garage");

    public GarageService(
        IGenericRepository<Garage> garageRepository,
        IUnitOfWork unitOfWork)
    {
        _garageRepository = garageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<Garage>>> GetUserGaragesAsync(int userId)
    {
        return await _garageRepository.GetListByConditionAsync(g => g.OwnerId == userId);
    }

    public async Task<Result<Garage>> GetGarageAsync(int garageId, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result<Garage>.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result<Garage>.Failure(ForbiddenGarageAccess);
        }

        return garageResult;
    }

    public async Task<Result<int>> CreateGarageAsync(CreateGarageRequest request, int userId)
    {
        var garage = new Garage
        {
            Name = request.Name,
            Address = request.Address,
            TimeZone = request.TimeZone,
            OwnerId = userId
        };

        return await _garageRepository.AddAsync(garage);
    }

    public async Task<Result> UpdateGarageAsync(int garageId, UpdateGarageRequest request, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        var garage = garageResult.Value;
        garage.Name = request.Name;
        garage.Address = request.Address;
        garage.TimeZone = request.TimeZone;

        await _unitOfWork.BeginTransactionAsync();

        var updateResult = await _garageRepository.UpdateAsync(garage);
        if (!updateResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return updateResult;
        }

        await _unitOfWork.CommitTransactionAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteGarageAsync(int garageId, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        return await _garageRepository.DeleteAsync(g => g.Id == garageId && g.OwnerId == userId);
    }
}

