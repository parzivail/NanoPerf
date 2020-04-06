using System.Drawing;
using System.Linq;
using Humanizer;
using NanoPerf.Koi.Utils;

namespace NanoPerf.Koi.Panels
{
    class GraphPanel : Panel
    {
        private const int dataSpacing = 2;
        const int maxYLabelLen = 8;

        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public bool AutoSize { get; set; }

        private readonly Canvas _canvas;

        private readonly float[] _data;
        private int _dataCursor;

        /// <inheritdoc />
        public GraphPanel(string title, int width, int height, float minValue, float maxValue) : base(title, width, height)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            AutoSize = false;
            _canvas = new Canvas(Width - maxYLabelLen - 4, Height - 4);

            _data = new float[_canvas.VirtualWidth / dataSpacing];
        }

        /// <inheritdoc />
        public GraphPanel(string title, int width, int height) : base(title, width, height)
        {
            AutoSize = true;
            _canvas = new Canvas(Width - maxYLabelLen - 4, Height - 4);

            _data = new float[_canvas.VirtualWidth / dataSpacing];
        }

        public void AddData(float point)
        {
            _data[_dataCursor] = point;
            _dataCursor = (_dataCursor + 1) % _data.Length;
        }

        /// <inheritdoc />
        protected override void DrawContents()
        {
            _canvas.Clear();

            if (AutoSize)
            {
                MaxValue = _data.Max();
                MinValue = _data.Min();

                if (MaxValue == MinValue)
                    MaxValue++;
            }

            for (var i = 1; i < _data.Length; i++)
            {
                var data = _data[(_dataCursor + i) % _data.Length];
                var pData = _data[(_dataCursor + i - 1) % _data.Length];

                var datY = (int) data.Remap(MaxValue, MinValue, 0, _canvas.VirtualHeight - 1);
                var pdatY = (int) pData.Remap(MaxValue, MinValue, 0, _canvas.VirtualHeight - 1);

                _canvas.Line(i * dataSpacing, datY, (i - 1) * dataSpacing, pdatY, Color.Coral);
            }

            DrawAxes();

            var graphW = Width - maxYLabelLen - 4;
            var graphH = Height - 6;

            Program.Out.StartWindow(X + 2 + maxYLabelLen, Y + 2, graphW, graphH);
            for (var y = 0; y < _canvas.PhysicalHeight; y++)
            {
                Program.Out.SetCursorPosition(0, y);
                for (var x = 0; x < _canvas.PhysicalWidth; x++)
                    Program.Out.Write($"{_canvas.GetChar(x, y)}", _canvas.GetColor(x, y));
            }
            Program.Out.EndWindow();
        }

        private void DrawAxes()
        {
            var axisW = Width - maxYLabelLen - 2;
            var axisH = Height - 2;

            Program.Out.StartWindow(X + 1 + maxYLabelLen, Y + 1, axisW, axisH);

            BoxDrawing.DrawBox(string.Empty, BoxDrawing.Alignment.Left, axisW, axisH, BoxDrawing.LineWidth.None, BoxDrawing.LineWidth.Single, BoxDrawing.LineWidth.Single, BoxDrawing.LineWidth.None);

            Program.Out.EndWindow();

            for (var i = 0; i < axisH - 1; i += 2)
            {
                Program.Out.SetCursorPosition(0, i);

                var valHere = KoiExtensions.Remap(i, 0, axisH - 2, MaxValue, MinValue);
                
                Program.Out.Write($"{MetricNumeralExtensions.ToMetric(valHere, false, true, 2)}".PadLeft(maxYLabelLen));
            }
        }
    }
}
