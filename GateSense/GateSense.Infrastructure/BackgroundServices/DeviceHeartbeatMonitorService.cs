using Domain.Models.Devices;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundServices;

public class DeviceHeartbeatMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceHeartbeatMonitorService> _logger;
    private readonly HeartbeatMonitorOptions _options;

    public DeviceHeartbeatMonitorService(
        IServiceProvider serviceProvider,
        ILogger<DeviceHeartbeatMonitorService> logger,
        IOptions<HeartbeatMonitorOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device Heartbeat Monitor Service started. Check interval: {Interval} minutes, Timeout: {Timeout} minutes",
            _options.CheckIntervalMinutes, _options.HeartbeatTimeoutMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDevicesHeartbeatAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking device heartbeat");
            }

            // Чекаємо перед наступною перевіркою
            await Task.Delay(TimeSpan.FromMinutes(_options.CheckIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Device Heartbeat Monitor Service stopped");
    }

    private async Task CheckDevicesHeartbeatAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var deviceRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<IoTDevice>>();

        // Отримуємо всі пристрої, які зараз Online
        var onlineDevicesResult = await deviceRepository.GetListByConditionAsync(
            d => d.Status == DeviceStatus.Online && d.LastHeartbeatOn.HasValue);

        if (!onlineDevicesResult.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve online devices: {Error}", 
                string.Join(", ", onlineDevicesResult.Errors.Select(e => e.Description)));
            return;
        }

        var onlineDevices = onlineDevicesResult.Value;
        if (!onlineDevices.Any())
        {
            _logger.LogDebug("No online devices found to check");
            return;
        }

        var timeoutThreshold = DateTimeOffset.UtcNow.AddMinutes(-_options.HeartbeatTimeoutMinutes);
        var devicesMarkedOffline = 0;

        foreach (var device in onlineDevices)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Якщо LastHeartbeatOn старіше за поріг, встановлюємо статус Offline
            if (device.LastHeartbeatOn.HasValue && device.LastHeartbeatOn.Value < timeoutThreshold)
            {
                _logger.LogWarning(
                    "Device {SerialNumber} (ID: {DeviceId}) in Garage {GarageId} is offline. Last heartbeat: {LastHeartbeat}",
                    device.SerialNumber, device.Id, device.GarageId, device.LastHeartbeatOn);

                device.Status = DeviceStatus.Offline;

                var updateResult = await deviceRepository.UpdateAsync(device);
                if (updateResult.IsSuccess)
                {
                    devicesMarkedOffline++;
                    _logger.LogInformation(
                        "Device {SerialNumber} (ID: {DeviceId}) marked as Offline",
                        device.SerialNumber, device.Id);
                }
                else
                {
                    _logger.LogError(
                        "Failed to update device {SerialNumber} (ID: {DeviceId}) status: {Errors}",
                        device.SerialNumber, device.Id,
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
            }
        }

        if (devicesMarkedOffline > 0)
        {
            _logger.LogInformation(
                "Heartbeat check completed. {Count} device(s) marked as offline",
                devicesMarkedOffline);
        }
        else
        {
            _logger.LogDebug(
                "Heartbeat check completed. All {Count} online device(s) are responding",
                onlineDevices.Count());
        }
    }
}

public class HeartbeatMonitorOptions
{
    public const string SectionName = "HeartbeatMonitor";

    /// <summary>
    /// Інтервал перевірки heartbeat (в хвилинах)
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// Таймаут heartbeat - якщо пристрій не надіслав heartbeat за цей час, він вважається офлайн (в хвилинах)
    /// </summary>
    public int HeartbeatTimeoutMinutes { get; set; } = 5;
}

