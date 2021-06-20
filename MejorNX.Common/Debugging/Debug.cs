using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Common.Debugging
{
    public enum LogLevel
    {
        Low,
        Neutral,
        High
    }

    public static class Debug
    {
        public static LogLevel CurrentLevel     { get; set; } = LogLevel.Low;
        public static bool ProgramInDebugMode   { get; set; } = true;
        static object OneAtATime                { get; set; } = new object();

        //TODO: Add importance checks to not log everything.
        static void LogWithColor(object message,ConsoleColor color = ConsoleColor.White,LogLevel Level = LogLevel.Neutral)
        {
            if (Level >= CurrentLevel)
            {
                lock (OneAtATime)
                {
                    Console.ForegroundColor = color;

                    //TODO: Add time markers.

                    if (ProgramInDebugMode)
                    {
                        Console.WriteLine(message);
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public static void LogWarning(object message,LogLevel Level = LogLevel.Neutral) => LogWithColor($"Warning: {message}",ConsoleColor.Yellow,Level);

        public static void Log(string message, LogLevel Level = LogLevel.Neutral) => LogWithColor(message,ConsoleColor.White,Level);

        public static void LogError(object message, bool Crash = false)
        {
            LogWithColor($"Error: {message}", ConsoleColor.Red,LogLevel.High);

            if (Crash)
                Debug.Crash();
        }

        public static void Crash()
        {
            LogError("Indefinite emulation hault.");

            ThrowNotImplementedException();
        }

        public static void ThrowException(Exception exception) => throw exception;

        public static void ThrowNotImplementedException(object message = null)
        {
            if (message != null)
                ThrowException(new NotImplementedException(message.ToString()));

            ThrowException(new NotImplementedException());
        }
    }
}
