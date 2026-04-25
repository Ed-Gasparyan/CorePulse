namespace CorePulse.Shared.DTOs.Responses
{
    public class ProcessMetricsDTO
    {
        public int Id {  get; set; }

        public string Name { get; set; } = string.Empty;

        public double MemoryUsage { get; set; }

        public int ThreadCount { get; set; }
    }
}
