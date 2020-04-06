using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NanoPerf.Koi.Utils
{
    class BoxDrawing
    {
        public enum LineWidth
        {
            None = 0,
            Single,
            Double
        }

        private static readonly Dictionary<byte, char> LookupTable = new Dictionary<byte, char>();

        static BoxDrawing()
        {
            LookupTable.Add(EncodeSides(), ' ');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single), '│');
            LookupTable.Add(EncodeSides(LineWidth.None, LineWidth.Single), '│');
            LookupTable.Add(EncodeSides(LineWidth.Single), '│');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, LineWidth.Single), '┤');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, LineWidth.Double), '╡');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, LineWidth.Single), '╢');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, left: LineWidth.Single), '╖');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, left: LineWidth.Double), '╕');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, LineWidth.Double), '╣');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double), '║');
            LookupTable.Add(EncodeSides(LineWidth.None, LineWidth.Double), '║');
            LookupTable.Add(EncodeSides(LineWidth.Double), '║');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, left: LineWidth.Double), '╗');
            LookupTable.Add(EncodeSides(LineWidth.Double, left: LineWidth.Double), '╝');
            LookupTable.Add(EncodeSides(LineWidth.Double, left: LineWidth.Single), '╜');
            LookupTable.Add(EncodeSides(LineWidth.Single, left: LineWidth.Double), '╛');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, left: LineWidth.Single), '┐');
            LookupTable.Add(EncodeSides(LineWidth.Single, right: LineWidth.Single), '└');
            LookupTable.Add(EncodeSides(LineWidth.Single, left: LineWidth.Single, right: LineWidth.Single), '┴');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, left: LineWidth.Single, right: LineWidth.Single), '┬');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, right: LineWidth.Single), '├');
            LookupTable.Add(EncodeSides(left: LineWidth.Single, right: LineWidth.Single), '─');
            LookupTable.Add(EncodeSides(left: LineWidth.None, right: LineWidth.Single), '─');
            LookupTable.Add(EncodeSides(left: LineWidth.Single, right: LineWidth.None), '─');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, LineWidth.Single, LineWidth.Single), '┼');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, right: LineWidth.Double), '╞');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, right: LineWidth.Single), '╟');
            LookupTable.Add(EncodeSides(LineWidth.Double, right: LineWidth.Double), '╚');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, right: LineWidth.Double), '╔');
            LookupTable.Add(EncodeSides(LineWidth.Double, left: LineWidth.Double, right: LineWidth.Double), '╩');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, left: LineWidth.Double, right: LineWidth.Double), '╦');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, right: LineWidth.Double), '╠');
            LookupTable.Add(EncodeSides(left: LineWidth.Double, right: LineWidth.Double), '═');
            LookupTable.Add(EncodeSides(left: LineWidth.None, right: LineWidth.Double), '═');
            LookupTable.Add(EncodeSides(left: LineWidth.Double), '═');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, LineWidth.Double, LineWidth.Double), '╬');
            LookupTable.Add(EncodeSides(LineWidth.Single, left: LineWidth.Double, right: LineWidth.Double), '╧');
            LookupTable.Add(EncodeSides(LineWidth.Double, left: LineWidth.Single, right: LineWidth.Single), '╨');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, left: LineWidth.Double, right: LineWidth.Double), '╤');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, left: LineWidth.Single, right: LineWidth.Single), '╥');
            LookupTable.Add(EncodeSides(LineWidth.Double, right: LineWidth.Single), '╙');
            LookupTable.Add(EncodeSides(LineWidth.Single, right: LineWidth.Double), '╘');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, right: LineWidth.Double), '╒');
            LookupTable.Add(EncodeSides(down: LineWidth.Double, right: LineWidth.Single), '╓');
            LookupTable.Add(EncodeSides(LineWidth.Double, LineWidth.Double, LineWidth.Single, LineWidth.Single), '╫');
            LookupTable.Add(EncodeSides(LineWidth.Single, LineWidth.Single, LineWidth.Double, LineWidth.Double), '╪');
            LookupTable.Add(EncodeSides(LineWidth.Single, left: LineWidth.Single), '┘');
            LookupTable.Add(EncodeSides(down: LineWidth.Single, right: LineWidth.Single), '┌');
        }

        private static byte EncodeSides(LineWidth up = LineWidth.None, LineWidth down = LineWidth.None, LineWidth left = LineWidth.None, LineWidth right = LineWidth.None)
        {
            return (byte)(((byte)up << 6) | ((byte)down << 4) | ((byte)left << 2) | (byte)right);
        }

        private static char GetChar(LineWidth up = LineWidth.None, LineWidth down = LineWidth.None, LineWidth left = LineWidth.None, LineWidth right = LineWidth.None)
        {
            return LookupTable[EncodeSides(up, down, left, right)];
        }

        /// <summary>
        /// Gets a n-height box from the Block Elements unicode block
        /// </summary>
        /// <param name="height">The height from 0 to 8</param>
        /// <returns></returns>
        public static char GetBoxChar(byte height)
        {
            if (height > 8) throw new ArgumentOutOfRangeException(nameof(height));

            if (height == 0)
                height = 1;

            return Convert.ToChar(0x2581 + height);
        }

        /// <summary>
        /// Gets a n-height box from the Block Elements unicode block
        /// </summary>
        /// <param name="height">The height from 0 to 1</param>
        /// <returns></returns>
        public static char GetBoxChar(float height)
        {
            if (height > 1 || height < 0) throw new ArgumentOutOfRangeException(nameof(height));
            return GetBoxChar((byte)(height * 8));
        }

        public static void DrawBox(string title, Alignment titleAlignment, int w, int h, LineWidth top, LineWidth bottom, LineWidth left, LineWidth right)
        {
            if (w < 2)
                title = "";
            else if (title.Length > w - 2)
                title = title.Substring(0, w - 4);

            int leftPad;
            int rightPad;

            switch (titleAlignment)
            {
                case Alignment.Left:
                    leftPad = 1;
                    rightPad = w - 3 - title.Length;
                    break;
                case Alignment.Center:
                    var split = (w - 3 - title.Length) / 2;
                    leftPad = split;
                    rightPad = w - split - title.Length - 2;
                    break;
                case Alignment.Right:
                    leftPad = w - 3 - title.Length;
                    rightPad = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(titleAlignment), titleAlignment, null);
            }

            Program.Out.WriteLine($"{GetChar(down: left, right: top)}{"".PadLeft(leftPad, GetChar(left: top, right: top))}{title}{"".PadLeft(rightPad, GetChar(left: top, right: top))}{GetChar(down: right, left: top)}");

            for (var y = 0; y < h - 2; y++)
                Program.Out.WriteLine($"{GetChar(left, left)}{"".PadLeft(w - 2)}{GetChar(right, right)}");

            Program.Out.WriteLine($"{GetChar(left, right: bottom)}{"".PadLeft(w - 2, GetChar(left: bottom, right: bottom))}{GetChar(right, left: bottom)}");
        }

        internal enum Alignment
        {
            Left,
            Center,
            Right
        }
    }
}
