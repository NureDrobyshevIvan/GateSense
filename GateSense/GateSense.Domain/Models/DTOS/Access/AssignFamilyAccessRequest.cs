namespace Domain.Models.DTOS.Access;

public class AssignFamilyAccessRequest
{
    public required int GarageId { get; set; }

    public required string Email { get; set; }
}

