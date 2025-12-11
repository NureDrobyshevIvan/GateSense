namespace Domain.Models.DTOS.Gates;

public class GateStateResponse
{
    public int GarageId { get; set; }
    
    public string State { get; set; } = default!; // "Open", "Closed", "Unknown"
    
    public string? LastAction { get; set; } // "Open", "Close", null
    
    public DateTimeOffset? LastActionTime { get; set; }
}

