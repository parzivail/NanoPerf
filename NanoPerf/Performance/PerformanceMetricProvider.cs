using System.Runtime.InteropServices;
using Humanizer.Bytes;

namespace NanoPerf.Performance
{
    abstract class PerformanceMetricProvider
    {
        public static readonly PerformanceMetricProvider Instance = Create();

        private static PerformanceMetricProvider Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new UnixPeformanceMetricProvider();
            return new WindowsPerformanceMetricProvider();
        }

        public abstract int NumberOfProcessors { get; }
        public abstract int NumberOfCores { get; }

        public abstract ByteSize TotalMemory { get; }
        public abstract ByteSize TotalSwap { get; }

        public abstract float GetProcessorTime();
        public abstract float GetProcessorTime(int processor);
        public abstract float GetProcessorTime(int processor, int core);

        public abstract ByteSize GetFreeMemory();
        public abstract ByteSize GetFreeSwap();

        public abstract string[] GetNetworkAdapters();

        public abstract ByteSize GetNetworkTxSpeed(string adapter);
        public abstract ByteSize GetNetworkRxSpeed(string adapter);

        public abstract ByteSize GetNetworkTxTotal(string adapter);
        public abstract ByteSize GetNetworkRxTotal(string adapter);
    }
}
