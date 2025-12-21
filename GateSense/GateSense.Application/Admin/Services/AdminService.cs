using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.DTOS.Admin;
using Domain.Models.Garages;
using GateSense.Application.Admin.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Admin.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGenericRepository<Garage> _garageRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Error UserNotFound =
        Error.NotFound("admin.USER_NOT_FOUND", "User not found");

    private static readonly Error GarageNotFound =
        Error.NotFound("admin.GARAGE_NOT_FOUND", "Garage not found");

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IGenericRepository<Garage> garageRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _garageRepository = garageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<AdminUserResponse>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userResponses = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userResponses.Add(new AdminUserResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Roles = roles.ToList()
            });
        }

        return Result<IEnumerable<AdminUserResponse>>.Success(userResponses);
    }

    public async Task<Result<AdminUserResponse>> GetUserByIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result<AdminUserResponse>.Failure(UserNotFound);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = new AdminUserResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAtUtc = DateTimeOffset.FromUnixTimeSeconds(0),
            Roles = roles.ToList()
        };

        return Result<AdminUserResponse>.Success(response);
    }

    public async Task<Result> UpdateUserAsync(int userId, AdminUpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(UserNotFound);
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(request.Email);
            if (emailExists != null && emailExists.Id != userId)
            {
                return Result.Failure(Error.Conflict("admin.EMAIL_EXISTS", "Email already exists"));
            }

            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        if (request.EmailConfirmed.HasValue)
        {
            user.EmailConfirmed = request.EmailConfirmed.Value;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure(Error.Validation("admin.UPDATE_FAILED", 
                string.Join(", ", updateResult.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(UserNotFound);
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return Result.Failure(Error.Validation("admin.DELETE_FAILED",
                string.Join(", ", deleteResult.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result<IEnumerable<AdminGarageResponse>>> GetAllGaragesAsync()
    {
        var includes = new List<Func<IQueryable<Garage>, IQueryable<Garage>>>
        {
            q => q.Include(g => g.Owner)
        };

        var garagesResult = await _garageRepository.GetListByConditionAsync(g => true, includes);
        if (!garagesResult.IsSuccess)
        {
            return Result<IEnumerable<AdminGarageResponse>>.Failure(garagesResult.Errors);
        }

        var responses = garagesResult.Value.Select(g => new AdminGarageResponse
        {
            Id = g.Id,
            Name = g.Name,
            Address = g.Address ?? string.Empty,
            TimeZone = g.TimeZone ?? string.Empty,
            OwnerId = g.OwnerId ?? 0,
            OwnerEmail = g.Owner?.Email,
            CreatedAtUtc = g.CreatedOn
        }).ToList();

        return Result<IEnumerable<AdminGarageResponse>>.Success(responses);
    }

    public async Task<Result<AdminGarageResponse>> GetGarageByIdAsync(int garageId)
    {
        var includes = new List<Func<IQueryable<Garage>, IQueryable<Garage>>>
        {
            q => q.Include(g => g.Owner)
        };

        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId, includes);
        if (!garageResult.IsSuccess)
        {
            return Result<AdminGarageResponse>.Failure(GarageNotFound);
        }

        var garage = garageResult.Value;
        var response = new AdminGarageResponse
        {
            Id = garage.Id,
            Name = garage.Name,
            Address = garage.Address ?? string.Empty,
            TimeZone = garage.TimeZone ?? string.Empty,
            OwnerId = garage.OwnerId ?? 0,
            OwnerEmail = garage.Owner?.Email,
            CreatedAtUtc = garage.CreatedOn
        };

        return Result<AdminGarageResponse>.Success(response);
    }

    public async Task<Result> UpdateGarageAsync(int garageId, Domain.Models.DTOS.Garages.UpdateGarageRequest request)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
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

    public async Task<Result> DeleteGarageAsync(int garageId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        return await _garageRepository.DeleteAsync(g => g.Id == garageId);
    }
}

