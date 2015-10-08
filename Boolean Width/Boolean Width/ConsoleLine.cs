using System;
using System.Diagnostics;

namespace BooleanWidth
{
    class ConsoleLine
    {
        static readonly object _lock = new object();

        readonly int _line;
        public ConsoleLine ()
        {
            lock (_lock)
            {
                _line = Console.CursorTop;
                Console.WriteLine();
            }
        }

        public void Write(string text)
        {
            lock (_lock)
            {
                int temp = Console.CursorTop;
                Console.CursorTop = _line;
                Console.CursorLeft = 0;
                Console.Write(text.Length > Console.WindowWidth ? text.Substring(0, Console.WindowWidth) : text.PadRight(Console.WindowWidth));
                Console.CursorTop = temp;
            }
        }

        public void Write(string text, params object[] arg)
        {
            Write(String.Format(text, arg));
        }

    }
}