using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Humanizer;
using NanoPerf.Koi.Panels;
using NanoPerf.Koi.Utils;
using NanoPerf.Performance;
using Pastel;

namespace NanoPerf.KoiExt
{
    class NetworkAdapterPanel : Panel
    {
        private readonly PerformanceMetricProvider _metricProvider;
        private readonly string _adapter;
        
        private readonly float[] _dataTx;
        private readonly float[] _dataRx;
        private int _dataCursor;

        /// <inheritdoc />
        public NetworkAdapterPanel(int width, int height, PerformanceMetricProvider metricProvider, string adapter) : base(adapter, width, height)
        {
            _metricProvider = metricProvider;
            _adapter = adapter;

            _dataTx = new float[width - 2];
            _dataRx = new float[width - 2];
        }

        private void AddData(float tx, float rx)
        {
            _dataTx[_dataCursor] = tx;
            _dataRx[_dataCursor] = rx;
            _dataCursor = (_dataCursor + 1) % _dataTx.Length;
        }

        /// <inheritdoc />
        protected override void DrawContents()
        {
            var txBps = _metricProvider.GetNetworkTxSpeed(_adapter).Bits;
            var rxBps = _metricProvider.GetNetworkRxSpeed(_adapter).Bits;

            AddData(txBps, rxBps);

            Program.Out.WriteLine();

            Program.Out.WriteLine($"Sending: {MetricNumeralExtensions.ToMetric(txBps, false, true, 2)}bps");
            Program.Out.WriteLine($"Total Sent: {_metricProvider.GetNetworkTxTotal(_adapter).Bytes.ToMetric(false, true, 2)}B");
            DrawSparkline(_dataTx, Color.FromArgb(225,247,213));
            
            Program.Out.WriteLine($"Receiving: {MetricNumeralExtensions.ToMetric(rxBps, false, true, 2)}bps");
            Program.Out.WriteLine($"Total Received: {_metricProvider.GetNetworkRxTotal(_adapter).Bytes.ToMetric(false, true, 2)}B");
            DrawSparkline(_dataRx, Color.FromArgb(201,201,255));
        }

        private void DrawSparkline(float[] data, Color color)
        {
            var max = data.Max();
            var min = data.Min();

            if (min == max)
                max++;

            var str = data
                .Select((t, i) => data[(_dataCursor + i) % data.Length])
                .Select(x => BoxDrawing.GetBoxChar(x.Remap(min, max, 0, 1)))
                .Aggregate("", (current, box) => current + box);

            Program.Out.WriteLine(str, color);
        }
    }
}
