using CorePulse.Server.Services.Implementations;
using CorePulse.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CorePulse.Server.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time system metrics and process management.
    /// This hub enables bi-directional communication between the server and connected Blazor clients.
    /// </summary>
    [Authorize] // Requires users to be authenticated to connect to the hub
    public class MetricsHub : Hub
    {
        private readonly ProcessCacheService _cache;
        private readonly ILogger<MetricsHub> _logger;

        public MetricsHub(ProcessCacheService cache, ILogger<MetricsHub> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Fetches a paged list of processes from the cache based on client requests.
        /// Used by the Blazor 'Virtualize' component for efficient scrolling.
        /// </summary>
        /// <param name="startIndex">The offset to start from.</param>
        /// <param name="count">Number of processes to fetch.</param>
        /// <param name="search">Optional filter for process names.</param>
        /// <returns>A paged result of process metrics.</returns>
        public Task<PagedResultDTO<ProcessMetricsDTO>> GetProcesses(int startIndex, int count, string? search)
        {
            var result = _cache.GetPaged(startIndex, count, search);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Terminates a specific system process. 
        /// Restricted to users with the 'Admin' role for security reasons.
        /// </summary>
        /// <param name="pid">The ID of the process to kill.</param>
        [Authorize(Roles = "Admin")]
        public void KillProcessById(int pid)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _cache.KillAndRemove(pid);

                    await Clients.All.SendAsync("ReceiveLog", new LogDTO
                    {
                        Timestamp = DateTime.Now,
                        Level = "Warning",
                        Message = $"Process {pid} was terminated successfully."
                    });
                }
                catch (Exception ex)
                {
                    await Clients.All.SendAsync("ReceiveLog", new LogDTO
                    {
                        Timestamp = DateTime.Now,
                        Level = "Error",
                        Message = $"Critical error killing process {pid}: {ex.Message}"
                    });
                }
            });
        }
    }
}