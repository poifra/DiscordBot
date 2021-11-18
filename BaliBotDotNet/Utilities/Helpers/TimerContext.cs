using System.Timers;

namespace BaliBotDotNet.Utilities.Helpers
{
    class TimerContext : Timer
    {
        public ulong ServerID { get; set; }
        public bool CanUseCommand { get; set; }
    }
}
