using Domain.Models.DTOS.Sensors;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.IoT.Interfaces;

public interface IIoTService
{
    Task<Result> SubmitSensorDataAsync(SensorDataSubmissionRequest request);
    
    Task<Result> SendHeartbeatAsync(string serialNumber);
}

