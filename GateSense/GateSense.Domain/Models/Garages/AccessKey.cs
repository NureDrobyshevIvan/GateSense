using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Gates;

namespace Domain.Models.Garages;

public class AccessKey : BaseEntity
{
    public int GarageId { get; set; }

    public Garage Garage { get; set; } = default!;

    public int IssuedByUserId { get; set; }

    public ApplicationUser IssuedByUser { get; set; } = default!;

    public AccessKeyType KeyType { get; set; }

    public AccessKeyStatus Status { get; set; } = AccessKeyStatus.Active;

    public string Token { get; set; } = default!;

    public DateTimeOffset? ExpiresOn { get; set; }

    public ICollection<GateEvent> GateEvents { get; set; } = new List<GateEvent>();
}

