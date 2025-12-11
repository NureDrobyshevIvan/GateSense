using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Garages;

namespace Domain.Models.Gates;

public class GateEvent : BaseEntity
{
    public int GarageId { get; set; }

    public Garage Garage { get; set; } = default!;

    public int? InitiatorUserId { get; set; }

    public ApplicationUser? InitiatorUser { get; set; }

    public int? AccessKeyId { get; set; }

    public AccessKey? AccessKey { get; set; }

    public GateTriggerSource TriggerSource { get; set; }

    public GateAction Action { get; set; }

    public GateActionResult Result { get; set; }

    public string? FailureReason { get; set; }
}

