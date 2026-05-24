namespace Domain.Models.DTOS.Admin;

public class BackupDto
{
    public string Version { get; set; } = "1.0";
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public List<BackupUser> Users { get; set; } = new();
    public List<BackupGarage> Garages { get; set; } = new();
    public List<BackupDevice> Devices { get; set; } = new();
    public List<BackupAccessKey> AccessKeys { get; set; } = new();
    public List<BackupGarageAccess> GarageAccess { get; set; } = new();
    public List<BackupGateEvent> GateEvents { get; set; } = new();
    public List<BackupSensorReading> SensorReadings { get; set; } = new();
}

public class BackupUser
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class BackupGarage
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? TimeZone { get; set; }
    public int? OwnerId { get; set; }
}

public class BackupDevice
{
    public int Id { get; set; }
    public int GarageId { get; set; }
    public string SerialNumber { get; set; } = default!;
    public int DeviceType { get; set; }
    public int Status { get; set; }
    public string? FirmwareVersion { get; set; }
    public DateTimeOffset? LastHeartbeatOn { get; set; }
}

public class BackupAccessKey
{
    public int Id { get; set; }
    public int GarageId { get; set; }
    public int IssuedByUserId { get; set; }
    public int KeyType { get; set; }
    public int Status { get; set; }
    public string Token { get; set; } = default!;
    public DateTimeOffset? ExpiresOn { get; set; }
}

public class BackupGarageAccess
{
    public int Id { get; set; }
    public int GarageId { get; set; }
    public int UserId { get; set; }
    public int AccessLevel { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}

public class BackupGateEvent
{
    public int Id { get; set; }
    public int GarageId { get; set; }
    public int? InitiatorUserId { get; set; }
    public int? AccessKeyId { get; set; }
    public int TriggerSource { get; set; }
    public int Action { get; set; }
    public int Result { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}

public class BackupSensorReading
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public int SensorType { get; set; }
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTimeOffset RecordedOn { get; set; }
}

public class ImportBackupResponse
{
    public int UsersImported { get; set; }
    public int GaragesImported { get; set; }
    public int DevicesImported { get; set; }
    public int AccessKeysImported { get; set; }
    public int GarageAccessImported { get; set; }
    public int GateEventsImported { get; set; }
    public int SensorReadingsImported { get; set; }
}
