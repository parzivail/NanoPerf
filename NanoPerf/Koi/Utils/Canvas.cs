using System;
using System.Drawing;

namespace NanoPerf.Koi.Utils
{
    public class Canvas
    {
        /// <summary>
        /// (x,y) to the bit set in the unicode braille dot pattern character
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/Braille_Patterns"/>
        private static readonly int[,] BrailleBitMask = {
            {0b00000001, 0b00001000},
            {0b00000010, 0b00010000},
            {0b00000100, 0b00100000},
            {0b01000000, 0b10000000}
        };

        private readonly int[] _chars;
        private readonly Color?[] _colors;

        public int PhysicalWidth { get; }
        public int PhysicalHeight { get; }

        public int VirtualWidth { get; }
        public int VirtualHeight { get; }

        public Canvas(int physicalWidth, int physicalHeight)
        {
            PhysicalWidth = physicalWidth;
            PhysicalHeight = physicalHeight;

            VirtualWidth = physicalWidth * 2;
            VirtualHeight = physicalHeight * 4;

            _chars = new int[physicalWidth * physicalHeight];
            _colors = new Color?[physicalWidth * physicalHeight];
        }

        public void Clear()
        {
            for (var i = 0; i < _chars.Length; i++) _chars[i] = 0;
        }

        public void SetPixel(int x, int y, Color? color = null)
        {
            var (coord, mask) = GetSubpixel(x, y);
            _chars[coord] |= mask;
            _colors[coord] = color;
        }

        public void Line(int x1, int y1, int x2, int y2, Color? color = null)
        {
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);

            var incx = x1 < x2 ? 1 : -1;
            var incy = y1 < y2 ? 1 : -1;

            var err = (dx > dy ? dx : -dy) / 2;

            while (true)
            {
                SetPixel(x1, y1, color);
                if (x1 == x2 && y1 == y2)
                {
                    break;
                }

                var e2 = err;

                if (e2 > -dx)
                {
                    err -= dy;
                    x1 += incx;
                }

                if (e2 < dy)
                {
                    err += dx;
                    y1 += incy;
                }
            }
        }

        private Tuple<int, int> GetSubpixel(int x, int y)
        {
            var nx = x / 2;
            var ny = y / 4;
            var coord = GetCharCoord(nx, ny);
            var mask = BrailleBitMask[y % 4, x % 2];
            return new Tuple<int, int>(coord, mask);
        }

        private int GetCharCoord(int physicalX, int physicalY)
        {
            return physicalX + PhysicalWidth * physicalY;
        }

        public char GetChar(int physicalX, int physicalY)
        {
            return Convert.ToChar(0x2800 + _chars[GetCharCoord(physicalX, physicalY)]);
        }

        public Color? GetColor(int physicalX, int physicalY)
        {
            return _colors[GetCharCoord(physicalX, physicalY)];
        }
    }
}
