using Domain.Models;
using Domain.Models.Auth;

namespace Domain.Models.Garages;

public class GarageAccess : BaseEntity
{
    public int GarageId { get; set; }

    public Garage Garage { get; set; } = default!;

    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = default!;

    public AccessLevel AccessLevel { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }
}

