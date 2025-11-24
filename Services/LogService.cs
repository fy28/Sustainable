using System;
using System.IO;

namespace Sustainable.Services
{
    public static class LogService
    {
        private static readonly string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        private static readonly string logFile = Path.Combine(logDirectory, "app.log");

        static LogService()
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
        }

        public static void Log(string message)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
            File.AppendAllText(logFile, line);
        }
    }
}
