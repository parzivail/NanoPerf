using System;
using NanoPerf.Koi.Utils;

namespace NanoPerf.Koi.Panels
{
    abstract class Panel
    {
        public readonly string Title;

        public ConsoleColor TitleColor { get; set; } = Console.ForegroundColor;
        public ConsoleColor BorderColor { get; set; } = Console.ForegroundColor;
        
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        protected Panel(string title, int width, int height)
        {
            Title = title;
            Width = width;
            Height = height;
        }

        protected abstract void DrawContents();

        public virtual void Draw()
        {
            Program.Out.StartWindow(X, Y, Width, Height);
            DrawBorder();

            Program.Out.StartWindow(X + 1, Y + 1, Width - 1, Height - 1);
            DrawContents();
            Program.Out.EndWindow();

            Program.Out.EndWindow();
        }

        protected void DrawBorder()
        {
            BoxDrawing.DrawBox(Title, BoxDrawing.Alignment.Left, Width, Height, BoxDrawing.LineWidth.Double, BoxDrawing.LineWidth.Single, BoxDrawing.LineWidth.Single, BoxDrawing.LineWidth.Single);
        }
    }
}
