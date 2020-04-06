using System;
using System.Collections.Generic;
using System.Text;

namespace NanoPerf.Koi.Utils
{
    public static class KoiExtensions
    {
        public static float Remap(this float x, float minIn, float maxIn, float minOut, float maxOut)
        {
            return (x - minIn) / (maxIn - minIn) * (maxOut - minOut) + minOut;
        }
    }
}
