using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Xsl;
using Mono.Unix.Native;
using NanoPerf.Koi;
using NanoPerf.Koi.Panels;
using NanoPerf.KoiExt;
using NanoPerf.Performance;

namespace NanoPerf
{
    class Program
    {
        public static ConsoleBuffer Out = new ConsoleBuffer();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Out.ImmediateMode = true;
            Out.Setup();

            var perfMon = PerformanceMetricProvider.Instance;

            var adapters = perfMon.GetNetworkAdapters();

            var graphs = new List<Panel>();

            var windowWidth = Out.Window.Width;
            var windowHeight = Out.Window.Height;

            if (adapters.Length > 0)
            {
                var nicHeight = windowHeight / adapters.Length;
                for (var i = 0; i < adapters.Length; i++)
                {
                    graphs.Add(new NetworkAdapterPanel(30, nicHeight, perfMon, adapters[i])
                    {
                        X = 0,
                        Y = nicHeight * i
                    });
                }
            }

            graphs.Add(new StatGraphPanel("Processor Usage (%)", windowWidth - 30, windowHeight / 2, 0, 100, () => perfMon.GetProcessorTime())
            {
                X = 30,
                Y = 0
            });

            graphs.Add(new StatGraphPanel("Memory Usage (GB)", windowWidth - 30, windowHeight / 2, 0, (float)perfMon.TotalMemory.Gigabytes,
                () => (float)(perfMon.TotalMemory.Gigabytes - perfMon.GetFreeMemory().Gigabytes))
            {
                X = 30,
                Y = windowHeight / 2
            });

            Out.ImmediateMode = false;
            while (true)
            {
                Out.Setup();

                foreach (var graph in graphs) graph.Draw();

                Out.Draw();
                Thread.Sleep(1000);
            }
        }
    }
}
