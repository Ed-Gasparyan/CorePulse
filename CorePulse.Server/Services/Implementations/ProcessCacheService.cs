using CorePulse.Shared.DTOs.Responses;
using System.Diagnostics;
using System.ComponentModel;

namespace CorePulse.Server.Services.Implementations
{
    /// <summary>
    /// In-memory cache service that stores and manages the list of system processes.
    /// Provides thread-safe operations for updating, paging, and removing processes.
    /// </summary>
    public class ProcessCacheService
    {
        // volatile ensures that the most up-to-date value is always read across threads
        private volatile List<ProcessMetricsDTO> _allProcesses = [];

        // Semaphore to prevent race conditions during sensitive operations like killing a process
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger<ProcessCacheService> _logger;

        public ProcessCacheService(ILogger<ProcessCacheService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Updates the full list of cached processes.
        /// </summary>
        public void Update(List<ProcessMetricsDTO> updatedProcesses)
        {
            _allProcesses = updatedProcesses;
        }

        /// <summary>
        /// Performs server-side filtering, sorting, and paging on the cached process list.
        /// </summary>
        public PagedResultDTO<ProcessMetricsDTO> GetPaged(int startIndex, int count, string? search)
        {
            var query = _allProcesses.AsQueryable();

            // Filter by name if search string is provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Sort by memory usage (highest first)
            query = query.OrderByDescending(p => p.MemoryUsage);

            var items = query.Skip(startIndex).Take(count).ToList();

            return new PagedResultDTO<ProcessMetricsDTO> { Items = items, TotalCount = query.Count() };
        }

        /// <summary>
        /// Terminates a system process by ID and removes it from the local cache.
        /// Uses asynchronous waiting to prevent blocking the thread pool.
        /// </summary>
        /// <param name="pid">Process ID (PID).</param>
        public async Task KillAndRemove(int pid)
        {
            await _semaphore.WaitAsync();
            try
            {
                // Attempt to find the process by its PID
                using var process = Process.GetProcessById(pid);

                // Trigger the kill command
                process.Kill();

                // Wait for the process to actually exit within a reasonable timeframe (e.g., 5 seconds)
                // This prevents the "Timeout" error on the client side by ensuring the task finishes properly
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                await process.WaitForExitAsync(cts.Token);

                // Remove the process from our local memory cache to keep the UI in sync
                var processMetrics = _allProcesses.FirstOrDefault(x => x.Id == pid);
                if (processMetrics != null)
                {
                    _allProcesses.Remove(processMetrics);
                    _logger.LogInformation("Successfully killed and removed process {Pid} from cache.", pid);
                }
            }
            catch (ArgumentException)
            {
                _logger.LogWarning("Process {Pid} was not found (it might have closed already).", pid);
            }
            catch (Win32Exception ex)
            {
                _logger.LogError(ex, "Access denied when trying to kill process {Pid}.", pid);
                throw new Exception("You do not have permission to terminate this process.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while killing process {Pid}", pid);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}