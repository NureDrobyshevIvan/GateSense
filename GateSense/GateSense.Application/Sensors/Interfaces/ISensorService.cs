using Domain.Models.DTOS.Sensors;
using Domain.Models.Sensors;
using Infrastructure.Common.PagedList;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Sensors.Interfaces;

public interface ISensorService
{
    Task<Result<IEnumerable<SensorReading>>> GetLatestReadingsAsync(int garageId, int userId);
    
    Task<Result<IPagedList<SensorReading>>> GetSensorHistoryAsync(int garageId, int userId, SensorHistoryQuery query);
    
    Task<Result<IEnumerable<SensorReading>>> GetActiveAlertsAsync(int garageId, int userId);
}

