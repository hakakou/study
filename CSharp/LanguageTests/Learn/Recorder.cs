using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using static System.Diagnostics.Process;

namespace LanguageTests
{
    public static class Recorder
    {
        private static Stopwatch timer = new Stopwatch();
        private static long bytesPhysicalBefore = 0;
        private static long bytesVirtualBefore = 0;

        public static event EventHandler<string> Log;

        public static void Start(string log = null)
        {
            if (log != null)
                Log?.Invoke(null, log);
            // Force two garbage collections to release memory that is
            // no longer referenced but has not been released yet
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // store the current physical and virtual memory use
            bytesPhysicalBefore = GetCurrentProcess().WorkingSet64;
            bytesVirtualBefore = GetCurrentProcess().VirtualMemorySize64;
            timer.Restart();
        }

        public static void Report(string log = null)
        {
            var r = log ?? "Report";
            string str = $"{r}: {timer.ElapsedMilliseconds}ms";
            Log?.Invoke(null, str);
        }

        public static void Stop()
        {
            timer.Stop();
            long bytesPhysicalAfter = GetCurrentProcess().WorkingSet64;
            long bytesVirtualAfter = GetCurrentProcess().VirtualMemorySize64;
            Log?.Invoke(null, String.Format("{0:N0} physical bytes used.", bytesPhysicalAfter - bytesPhysicalBefore));
            Log?.Invoke(null, String.Format("{0:N0} virtual bytes used.", bytesVirtualAfter - bytesVirtualBefore));
            Log?.Invoke(null, String.Format("{0} time span ellapsed.", timer.Elapsed));
            Log?.Invoke(null, String.Format("{0:N0} total milliseconds ellapsed.", timer.ElapsedMilliseconds));
            Log?.Invoke(null, "");
        }
    }
}
