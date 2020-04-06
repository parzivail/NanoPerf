using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Humanizer;
using Humanizer.Bytes;

namespace NanoPerf.Performance
{
    internal class WindowsPerformanceMetricProvider : PerformanceMetricProvider
    {
        private readonly PerformanceStat _pcMemory;
        private readonly Dictionary<string, PerformanceStat> _psProcessor;
        private readonly Dictionary<string, PerformanceStat> _pcNetworkTx;
        private readonly Dictionary<string, PerformanceStat> _pcNetworkRx;
        private readonly string[] _networkAdapters;
        private readonly NetworkInterface[] _networkInterfaces;

        /// <inheritdoc />
        public override int NumberOfProcessors { get; }

        /// <inheritdoc />
        public override int NumberOfCores { get; }

        /// <inheritdoc />
        public override ByteSize TotalMemory { get; }

        /// <inheritdoc />
        public override ByteSize TotalSwap { get; }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

        public WindowsPerformanceMetricProvider()
        {
            _networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            _psProcessor = new Dictionary<string, PerformanceStat>();
            var pccProcessor = new PerformanceCounterCategory("Processor Information");

            foreach (var instance in pccProcessor.GetInstanceNames())
                _psProcessor.Add(instance, new PerformanceStat("Processor Information", "% Processor Time", instance));

            _pcMemory = new PerformanceStat("Memory", "Available MBytes");
            
            var pccNetwork = new PerformanceCounterCategory("Network Interface");
            _networkAdapters = pccNetwork.GetInstanceNames().OrderBy(s => s).ToArray();

            _pcNetworkTx = new Dictionary<string, PerformanceStat>();
            _pcNetworkRx = new Dictionary<string, PerformanceStat>();

            foreach (var adapter in _networkAdapters)
            {
                _pcNetworkTx.Add(adapter, new PerformanceStat("Network Interface", "Bytes Sent/sec", adapter));
                _pcNetworkRx.Add(adapter, new PerformanceStat("Network Interface", "Bytes Received/sec", adapter));
            }

            GetPhysicallyInstalledSystemMemory(out var totalMemoryInKilobytes);
            TotalMemory = totalMemoryInKilobytes.Kilobytes();

            TotalSwap = 0.Bytes();

            var numProcessors = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                numProcessors += int.Parse(item["NumberOfProcessors"].ToString());

            NumberOfProcessors = numProcessors;
            // Environment.ProcessorCount contains number of logical cores
            NumberOfCores = Environment.ProcessorCount / numProcessors;
        }

        private NetworkInterface GetInterfaceFor(string instanceName)
        {
            return _networkInterfaces.FirstOrDefault(nic => FixNicName(nic.Description) == instanceName);
        }

        private static string FixNicName(string nicName)
        {
            nicName = nicName.Replace("\\", "_");
            nicName = nicName.Replace("/", "_");
            nicName = nicName.Replace("(", "[");
            nicName = nicName.Replace(")", "]");
            nicName = nicName.Replace("#", "_");

            return nicName;
        }

        /// <inheritdoc />
        public override float GetProcessorTime()
        {
            return _psProcessor["_Total"].GetSample();
        }

        /// <inheritdoc />
        public override float GetProcessorTime(int processor)
        {
            return _psProcessor[$"{processor},_Total"].GetSample();
        }

        /// <inheritdoc />
        public override float GetProcessorTime(int processor, int core)
        {
            return _psProcessor[$"{processor},{core}"].GetSample();
        }

        /// <inheritdoc />
        public override ByteSize GetFreeMemory()
        {
            return ((int)_pcMemory.GetSample()).Megabytes();
        }

        /// <inheritdoc />
        public override ByteSize GetFreeSwap()
        {
            return 0.Bytes();
        }

        /// <inheritdoc />
        public override string[] GetNetworkAdapters()
        {
            return _networkAdapters;
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkTxSpeed(string adapter)
        {
            var bytesPerSec = (int)_pcNetworkTx[adapter].GetSample();
            return bytesPerSec.Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkRxSpeed(string adapter)
        {
            var bytesPerSec = (int)_pcNetworkRx[adapter].GetSample();
            return bytesPerSec.Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkTxTotal(string adapter)
        {
            var nic = GetInterfaceFor(adapter);
            return (nic?.GetIPStatistics().BytesSent ?? 0).Bytes();
        }

        /// <inheritdoc />
        public override ByteSize GetNetworkRxTotal(string adapter)
        {
            var nic = GetInterfaceFor(adapter);
            return (nic?.GetIPStatistics().BytesReceived ?? 0).Bytes();
        }
    }
}