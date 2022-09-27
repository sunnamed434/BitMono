using BitMono.Core.Attributes;
using BitMono.Core.Logging;
using System;
using System.Runtime.CompilerServices;

namespace BitMono.CLI
{
    [ServiceImplementation]
    public class ConsoleLogger : ILogger
    {
        private void writeLineWithColor(string text, ConsoleColor? color = default)
        {
            ConsoleColor lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color ?? lastColor;
            Console.WriteLine(text);
            Console.ForegroundColor = lastColor;
        }
        private void writeWithColor(string text, ConsoleColor? color = default)
        {
            ConsoleColor lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color ?? lastColor;
            Console.Write(text);
            Console.ForegroundColor = lastColor;
        }
        private void writeLineWithBackgroundColor(string text, ConsoleColor? color = default)
        {
            ConsoleColor lastColor = Console.BackgroundColor;
            Console.BackgroundColor = color ?? lastColor;
            Console.WriteLine(text);
            Console.BackgroundColor = lastColor;
        }
        private void writeWithBackgroundColor(string text, ConsoleColor? color = default)
        {
            ConsoleColor lastColor = Console.BackgroundColor;
            Console.BackgroundColor = color ?? lastColor;
            Console.Write(text);
            Console.BackgroundColor = lastColor;
        }
        public void Debug(string message, [CallerMemberName] string caller = null)
        {
            var time = DateTime.Now.ToLocalTime();
            Console.Write("[");
            writeWithColor(time.ToString() + " ", ConsoleColor.Gray);
            writeWithBackgroundColor("DBG", ConsoleColor.DarkYellow);
            Console.Write("] ");
            writeWithColor($"[{caller ?? "NOT SPECIFIED"}] ", ConsoleColor.White);
            writeWithColor($"{message}\n", ConsoleColor.White);
        }
        public void Info(string message, [CallerMemberName] string caller = null)
        {
            var time = DateTime.Now.ToLocalTime();
            Console.Write("[");
            writeWithColor(time.ToString() + " ", ConsoleColor.Gray);
            writeWithBackgroundColor("INF", ConsoleColor.Yellow);
            Console.Write("] ");
            writeWithColor($"[{caller ?? "NOT SPECIFIED"}] ", ConsoleColor.White);
            writeWithColor($"{message}\n", ConsoleColor.White);
        }
        public void Warn(string message, [CallerMemberName] string caller = null)
        {
            var time = DateTime.Now.ToLocalTime();
            Console.Write("[");
            writeWithColor(time.ToString() + " ", ConsoleColor.Gray);
            writeWithBackgroundColor("WRN", ConsoleColor.Red);
            Console.Write("] ");
            writeWithColor($"[{caller ?? "NOT SPECIFIED"}] ", ConsoleColor.White);
            writeWithColor($"{message}\n", ConsoleColor.White);
        }
        public void Error(Exception ex, string message = null, [CallerMemberName] string caller = null)
        {
            var time = DateTime.Now.ToLocalTime();
            Console.Write("[");
            writeWithColor(time.ToString() + " ", ConsoleColor.Gray);
            writeWithBackgroundColor("ERR", ConsoleColor.DarkRed);
            Console.Write("] ");
            writeWithColor($"[{caller ?? "NOT SPECIFIED"}] ", ConsoleColor.White);
            writeWithColor($"{message ?? "NO MESSAGE"} ", ConsoleColor.White);
            writeWithColor($"{ex}\n", ConsoleColor.White);
        }
    }
}