using System;

namespace NanoPerf.Koi.Panels
{
    class StatGraphPanel : GraphPanel
    {
        private readonly Func<float> _statistic;

        /// <inheritdoc />
        public StatGraphPanel(string title, int width, int height, float minValue, float maxValue, Func<float> statistic) : base(title, width, height, minValue, maxValue)
        {
            _statistic = statistic;
        }

        /// <inheritdoc />
        public StatGraphPanel(string title, int width, int height, Func<float> statistic) : base(title, width, height)
        {
            _statistic = statistic;
        }

        /// <inheritdoc />
        protected override void DrawContents()
        {
            AddData(_statistic.Invoke());
            base.DrawContents();
        }
    }
}
