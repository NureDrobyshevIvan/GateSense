using Domain.Models.Garages;
using Domain.Models.DTOS.Access;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Access.Interfaces;

public interface IAccessService
{
    Task<Result<IEnumerable<GarageAccess>>> GetMembersAsync(int garageId, int userId);

    Task<Result> AssignFamilyAccessAsync(AssignFamilyAccessRequest request, int userId);

    Task<Result<CreateGuestAccessResponse>> CreateGuestAccessAsync(CreateGuestAccessRequest request, int userId);

    Task<Result> RevokeAccessAsync(int accessId, int userId);
}

