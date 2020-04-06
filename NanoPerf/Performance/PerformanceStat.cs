using System;
using System.Diagnostics;

namespace NanoPerf.Performance
{
    internal class PerformanceStat
    {
        public string Instance { get; }
        public PerformanceCounter Counter { get; }
        public PerformanceCounterCategory Category { get; }

        public PerformanceStat(string category, string counter, string instance = null)
        {
            Instance = instance;
            Counter = new PerformanceCounter(category, counter, instance, true);
            Category = new PerformanceCounterCategory(Counter.CategoryName);
        }

        public float GetSample()
        {
            if (Instance != null)
            {
                if (Category.CategoryType == PerformanceCounterCategoryType.SingleInstance)
                    throw new ArgumentException("Cannot read the non-default sample of a SingleInstance category");
            }
            else if (Category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                throw new ArgumentException("Cannot read the default sample of a MultiInstance category");

            return Counter.NextValue();
        }
    }
}