using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePulse.Shared.DTOs.Responses
{
    public class LogDTO
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Level { get; set; } = "Info";

        public string Message { get; set; } = string.Empty;

        public string Source { get; set; } = "System";

        public string? UserName { get; set; }
    }
}

