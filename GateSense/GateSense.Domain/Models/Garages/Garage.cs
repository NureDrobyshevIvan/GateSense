using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Devices;
using Domain.Models.Gates;

namespace Domain.Models.Garages;

public class Garage : BaseEntity
{
    public string Name { get; set; } = default!;

    public string? Address { get; set; }

    public string? TimeZone { get; set; }

    public int? OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

    public ICollection<GarageAccess> Accesses { get; set; } = new List<GarageAccess>();

    public ICollection<AccessKey> AccessKeys { get; set; } = new List<AccessKey>();

    public ICollection<IoTDevice> Devices { get; set; } = new List<IoTDevice>();

    public ICollection<GateEvent> GateEvents { get; set; } = new List<GateEvent>();
}

