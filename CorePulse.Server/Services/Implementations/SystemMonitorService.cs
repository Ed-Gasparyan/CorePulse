using CorePulse.Server.Hubs;
using CorePulse.Shared.DTOs.Responses;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace CorePulse.Server.Services.Implementations
{
    /// <summary>
    /// Background worker service that monitors system metrics in real-time.
    /// It updates the shared cache and broadcasts global metrics to all connected clients via SignalR.
    /// </summary>
    public class SystemMonitorService : BackgroundService
    {
        private readonly ProcessCacheService _cache;
        private readonly ILogger<SystemMonitorService> _logger;
        private readonly IHubContext<MetricsHub> _hubContext;
        private readonly DateTime _startTime;

        public SystemMonitorService(ProcessCacheService cache, ILogger<SystemMonitorService> logger, IHubContext<MetricsHub> hubContext)
        {
            _cache = cache;
            _logger = logger;
            _hubContext = hubContext;
            _startTime = DateTime.UtcNow; // Record start time to calculate system uptime
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Retrieve all running processes from the OS
                    var rawProcesses = Process.GetProcesses();

                    // Map raw process data to DTOs for the client
                    var metrics = rawProcesses.Select(p =>
                    {
                        try
                        {
                            return new ProcessMetricsDTO
                            {
                                Id = p.Id,
                                Name = p.ProcessName,
                                MemoryUsage = p.WorkingSet64 / 1024.0 / 1024.0, // Convert bytes to MB
                                ThreadCount = p.Threads.Count
                            };
                        }
                        catch { return null; } // Handle cases where process exits during enumeration
                    }).Where(m => m != null).ToList();

                    // Push updated metrics to the shared cache
                    _cache.Update(metrics!);

                    // Calculate global system-wide metrics
                    double totalMemoryGb = Math.Round(rawProcesses.Sum(p => (long)p.WorkingSet64) / 1024.0 / 1024.0 / 1024.0, 2);
                    int processCount = rawProcesses.Length;
                    string uptime = (DateTime.UtcNow - _startTime).ToString(@"hh\:mm\:ss");

                    // Broadcast global metrics to all connected SignalR clients
                    await _hubContext.Clients.All.SendAsync("ReceiveGlobalMetrics",
                        0.0, // Placeholder for CPU usage (can be implemented with PerformanceCounters)
                        $"{totalMemoryGb} / 16",
                        processCount,
                        uptime,
                        stoppingToken);

                    // Dispose of process handles to prevent memory leaks
                    foreach (var p in rawProcesses)
                    {
                        p.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating process metrics");
                }

                // Wait for 1 second before the next update cycle
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}