using System;
using static SimplifyAptify.DevToysConsole.Services.AptifyService;

namespace SimplifyAptify.DevToysConsole.Models
{
    internal class AppSettings
    {
        public AptifyServer[] AptifyServers { get; set; } = Array.Empty<AptifyServer>();
    }
}
