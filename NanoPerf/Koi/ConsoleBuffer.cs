using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Pastel;

namespace NanoPerf.Koi
{
    class ConsoleBuffer
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly Stack<WindowDefinition> _scissorStack = new Stack<WindowDefinition>();

        private WindowDefinition _consoleWindow;

        private char[,] _buffer = new char[0, 0];
        private Color?[,] _colorBuffer = new Color?[0, 0];

        private int _absoluteCursorX;
        private int _absoluteCursorY;

        private int BufferWidth => _buffer.GetLength(1);
        private int BufferHeight => _buffer.GetLength(0);

        /// <summary>
        /// The X position of the cursor within the current window
        /// </summary>
        public int CursorX
        {
            get => _absoluteCursorX - Window.X;
            set => AbsoluteCursorX = Window.X + value;
        }

        /// <summary>
        /// The Y position of the cursor within the current window
        /// </summary>
        public int CursorY
        {
            get => _absoluteCursorY - Window.Y;
            set => AbsoluteCursorY = Window.Y + value;
        }

        /// <summary>
        /// The X position of the cursor within the console
        /// </summary>
        public int AbsoluteCursorX
        {
            get => _absoluteCursorX;
            set
            {
                if (value < 0 || value > AbsoluteWidth)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _absoluteCursorX = value;
            }
        }

        /// <summary>
        /// The Y position of the cursor within the console
        /// </summary>
        public int AbsoluteCursorY
        {
            get => _absoluteCursorY;
            set
            {
                if (value < 0 || value > AbsoluteHeight)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _absoluteCursorY = value;
            }
        }

        public WindowDefinition Window => _scissorStack.Count == 0 ? _consoleWindow : _scissorStack.Peek();

        public int AbsoluteWidth => _consoleWindow.Width;
        public int AbsoluteHeight => _consoleWindow.Height;

        public bool ImmediateMode { get; set; }

        public void Setup()
        {
            var consoleWidth = Console.WindowWidth;
            var consoleHeight = Console.WindowHeight;

            if (BufferWidth != consoleWidth || BufferHeight != consoleHeight)
            {
                ResizeBuffer(ref _colorBuffer, consoleWidth, consoleHeight);
                ResizeBuffer(ref _buffer, consoleWidth, consoleHeight);
                _consoleWindow = new WindowDefinition(0, 0, BufferWidth, BufferHeight);
            }

            SetCursorPosition(0, 0);
        }

        private void ResizeBuffer<T>(ref T[,] buffer, int cols, int rows)
        {
            var newArray = new T[rows, cols];
            var minRows = Math.Min(rows, BufferHeight);
            var minCols = Math.Min(cols, BufferWidth);

            for (var i = 0; i < minRows; i++)
                for (var j = 0; j < minCols; j++)
                    newArray[i, j] = buffer[i, j];

            buffer = newArray;
        }

        public void StartWindow(int x, int y, int width, int height)
        {
            _scissorStack.Push(new WindowDefinition(x, y, width, height));
            SetCursorPosition(0, 0);
        }

        public void EndWindow()
        {
            if (_scissorStack.Count == 0)
                throw new ArgumentOutOfRangeException("Not currently in a window!");

            _scissorStack.Pop();
        }

        public void Draw()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(this);
            Console.SetCursorPosition(0, 0);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            _stringBuilder.Clear();

            for (var row = 0; row < BufferHeight; row++)
            {
                for (var col = 0; col < BufferWidth; col++)
                {
                    var c = _buffer[row, col];
                    if (c == 0)
                        c = ' ';

                    var colorHere = _colorBuffer[row, col];
                    if (colorHere.HasValue)
                        _stringBuilder.Append($"{c}".Pastel(colorHere.Value));
                    else
                        _stringBuilder.Append(c);
                }
            }

            return _stringBuilder.ToString();
        }

        public void SetCursorPosition(int x, int y)
        {
            SetAbsoluteCursorPosition(Window.X + x, Window.Y + y);
        }

        private void SetAbsoluteCursorPosition(int x, int y)
        {
            AbsoluteCursorX = x;
            AbsoluteCursorY = y;
        }

        public void Write(object data, Color? color = null)
        {
            var str = $"{data}";

            foreach (var character in str)
            {
                switch (character)
                {
                    case '\r':
                        CursorX = 0;
                        break;
                    case '\n':
                        CursorX = 0;
                        CursorY++;
                        break;
                    default:
                        {
                            _buffer[AbsoluteCursorY, AbsoluteCursorX] = character;
                            _colorBuffer[AbsoluteCursorY, AbsoluteCursorX] = color;

                            CursorX++;
                            break;
                        }
                }
            }

            if (ImmediateMode)
                Draw();
        }

        public void WriteLine(object str, Color? color = null)
        {
            Write(str, color);
            WriteLine();
        }

        public void WriteLine()
        {
            Write('\n');
        }
    }
}
