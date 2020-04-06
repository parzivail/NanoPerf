using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Humanizer.Bytes;
using Mono.Unix.Native;

namespace NanoPerf.Performance
{
    internal class UnixPeformanceMetricProvider : PerformanceMetricProvider
    {
        private class CpuMeasurement
        {
            public long Total { get; }
            public long Idle { get; }

            public CpuMeasurement(long total, long idle)
            {
                Total = total;
                Idle = idle;
            }
        }

        private class NetDeviceInfo
        {
            public long RxBytes { get; }
            public long RxPackets { get; }
            public long RxErrs { get; }
            public long RxDrop { get; }
            public long RxFifo { get; }
            public long RxFrame { get; }
            public long RxCompressed { get; }
            public long RxMulticast { get; }
            public long TxBytes { get; }
            public long TxPackets { get; }
            public long TxErrs { get; }
            public long TxDrop { get; }
            public long TxFifo { get; }
            public long TxFrame { get; }
            public long TxCompressed { get; }
            public long TxMulticast { get; }

            public NetDeviceInfo(long rxBytes, long rxPackets, long rxErrs, long rxDrop, long rxFifo, long rxFrame, long rxCompressed, long rxMulticast, long txBytes, long txPackets, long txErrs, long txDrop, long txFifo, long txFrame, long txCompressed, long txMulticast)
            {
                RxBytes = rxBytes;
                RxPackets = rxPackets;
                RxErrs = rxErrs;
                RxDrop = rxDrop;
                RxFifo = rxFifo;
                RxFrame = rxFrame;
                RxCompressed = rxCompressed;
                RxMulticast = rxMulticast;
                TxBytes = txBytes;
                TxPackets = txPackets;
                TxErrs = txErrs;
                TxDrop = txDrop;
                TxFifo = txFifo;
                TxFrame = txFrame;
                TxCompressed = txCompressed;
                TxMulticast = txMulticast;
            }
        }

        private readonly Dictionary<string, Measurement> _cpuMeasurements;
        private readonly Dictionary<string, Measurement> _networkMeasurements;

        /// <inheritdoc />
        public override int NumberOfProcessors { get; }

        /// <inheritdoc />
        public override int NumberOfCores { get; }

        /// <inheritdoc />
        public override ByteSize TotalMemory { get; }

        /// <inheritdoc />
        public override ByteSize TotalSwap { get; }

        public UnixPeformanceMetricProvider()
        {
            _cpuMeasurements = new Dictionary<string, Measurement>();
            var cpuMeasurements = GetCpuMeasurements();

            foreach (var (key, _) in cpuMeasurements)
            {
                _cpuMeasurements.Add($"idle-{key}", new Measurement());
                _cpuMeasurements.Add($"total-{key}", new Measurement());
            }

            NumberOfProcessors = 1;
            NumberOfCores = cpuMeasurements.Count - 1;

            var memoryMeasurements = GetMemoryMeasurements();

            TotalMemory = memoryMeasurements["MemTotal"];
            TotalSwap = memoryMeasurements["SwapTotal"];

            _networkMeasurements = new Dictionary<string, Measurement>();
            var networkMeasurements = GetNetworkMeasurements();

            foreach (var (key, _) in networkMeasurements)
            {
                _networkMeasurements.Add($"tx-{key}", new Measurement());
                _networkMeasurements.Add($"rx-{key}", new Measurement());
            }
        }

        private static Dictionary<string, CpuMeasurement> GetCpuMeasurements()
        {
            var d = new Dictionary<string, CpuMeasurement>();

            using (var sr = new StreamReader("/proc/stat"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line == null || !line.StartsWith("cpu"))
                        break;

                    var columns = Regex.Split(line, "\\s+");

                    var cpuIdx = columns[0];

                    var totalTime = columns.Skip(1).Aggregate(0L, (i, s) => i + long.Parse(s));
                    var idleTime = long.Parse(columns[4]);

                    d.Add(cpuIdx, new CpuMeasurement(totalTime, idleTime));
                }
            }

            return d;
        }

        private static Dictionary<string, ByteSize> GetMemoryMeasurements()
        {
            var d = new Dictionary<string, ByteSize>();

            using (var sr = new StreamReader("/proc/meminfo"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line == null)
                        continue;

                    var match = Regex.Match(line, "(.+):\\s+(\\d+) kB");

                    if (!match.Success)
                        continue;

                    d.Add(match.Groups[1].Value, long.Parse(match.Groups[2].Value).Kilobytes());
                }
            }

            return d;
        }

        private static Dictionary<string, NetDeviceInfo> GetNetworkMeasurements()
        {
            var d = new Dictionary<string, NetDeviceInfo>();

            using (var sr = new StreamReader("/proc/net/dev"))
            {
                // 2 header lines
                sr.ReadLine();
                sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line == null)
                        continue;

                    var columns = Regex.Split(line.Trim(), "\\s+");

                    var adapterName = columns[0];
                    adapterName = adapterName.Remove(adapterName.Length - 1);

                    var rxBytes = long.Parse(columns[1]);
                    var rxPackets = long.Parse(columns[2]);
                    var rxErrs = long.Parse(columns[3]);
                    var rxDrop = long.Parse(columns[4]);
                    var rxFifo = long.Parse(columns[5]);
                    var rxFrame = long.Parse(columns[6]);
                    var rxCompressed = long.Parse(columns[7]);
                    var rxMulticast = long.Parse(columns[8]);

                    var txBytes = long.Parse(columns[9]);
                    var txPackets = long.Parse(columns[10]);
                    var txErrs = long.Parse(columns[11]);
                    var txDrop = long.Parse(columns[12]);
                    var txFifo = long.Parse(columns[13]);
                    var txFrame = long.Parse(columns[14]);
                    var txCompressed = long.Parse(columns[15]);
                    var txMulticast = long.Parse(columns[16]);

                    d.Add(adapterName, new NetDeviceInfo(rxBytes, rxPackets, rxErrs, rxDrop, rxFifo, rxFrame, rxCompressed, rxMulticast, txBytes, txPackets, txErrs, txDrop, txFifo, txFrame, txCompressed, txMulticast));
                }
            }

            return d;
        }

        /// <inheritdoc />
        public override float GetProcessorTime()
        {
            var cpuMeasurements = GetCpuMeasurements();
            var cpuTotal = cpuMeasurements["cpu"];

            var idle = _cpuMeasurements["idle-cpu"].PushMeasurement(cpuTotal.Idle);
            var total = _cpuMeasurements["total-cpu"].PushMeasurement(cpuTotal.Total);

            return (float)((1 - idle / total) * 100);
        }

        /// <inheritdoc />
        public override float GetProcessorTime(int processor)
        {
            if (processor != 0)
                throw new ArgumentException(nameof(processor));

            return GetProcessorTime();
        }

        /// <inheritdoc />
        public override float GetProcessorTime(int processor, int core)
        {
            if (processor != 0)
                throw new ArgumentException(nameof(processor));

            var cpuMeasurements = GetCpuMeasurements();
            var cpuTotal = cpuMeasurements["cpu"];

            var idle = _cpuMeasurements[$"idle-cpu{core}"].PushMeasurement(cpuTotal.Idle);
            var total = _cpuMeasurements[$"total-cpu{core}"].PushMeasurement(cpuTotal.Total);

            return (float)((1 - idle / total) * 100);
        }

        /// <inheritdoc />
        public override ByteSize GetFreeMemory()
        {
            var memoryMeasurements = GetMemoryMeasurements();
            return memoryMeasurements["MemAvailable"];
        }

        /// <inheritdoc />
        public override ByteSize GetFreeSwap()
        {
            var memoryMeasurements = GetMemoryMeasurements();
            return memoryMeasurements["SwapFree"];
        }

        /// <inheritdoc />
        public override string[] GetNetworkAdapters()
        {
            return GetNetworkMeasurements().Keys.ToArray();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkTxSpeed(string adapter)
        {
            var nowBytes = GetNetworkMeasurements()[adapter].TxBytes;
            var measurement = _networkMeasurements[$"tx-{adapter}"].PushMeasurementPerSecond(nowBytes);

            return ((long) measurement).Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkRxSpeed(string adapter)
        {
            var nowBytes = GetNetworkMeasurements()[adapter].RxBytes;
            var measurement = _networkMeasurements[$"rx-{adapter}"].PushMeasurementPerSecond(nowBytes);

            return ((long) measurement).Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkTxTotal(string adapter)
        {
            return GetNetworkMeasurements()[adapter].TxBytes.Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkRxTotal(string adapter)
        {
            return GetNetworkMeasurements()[adapter].RxBytes.Bytes();
        }
    }
}