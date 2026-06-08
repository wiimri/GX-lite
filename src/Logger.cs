using System;
using System.IO;
using System.Text;

namespace GXLightBrowser
{
    internal static class Logger
    {
        private static readonly string LogFile = Path.Combine(AppPaths.Logs, "browser.log");
        private static readonly object LockObj = new object();
        private const long MaxLogSize = 1024 * 1024; // 1 MB

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARN", message);
        }

        public static void Error(string message)
        {
            Log("ERROR", message);
        }

        private static void Log(string level, string message)
        {
            lock (LockObj)
            {
                try
                {
                    AppPaths.Ensure();
                    RotateIfNeeded();
                    string line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}", DateTime.Now, level, message);
                    File.AppendAllText(LogFile, line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Fail silently to avoid crashing the app if logging fails
                }
            }
        }

        private static void RotateIfNeeded()
        {
            if (!File.Exists(LogFile)) return;
            try
            {
                FileInfo fi = new FileInfo(LogFile);
                if (fi.Length < MaxLogSize) return;

                for (int i = 3; i >= 1; i--)
                {
                    string oldPath = LogFile + "." + i;
                    string newPath = LogFile + "." + (i + 1);
                    if (File.Exists(oldPath))
                    {
                        if (i == 3) File.Delete(oldPath);
                        else File.Move(oldPath, newPath);
                    }
                }
                File.Move(LogFile, LogFile + ".1");
            }
            catch
            {
                // Ignore rotation errors
            }
        }
    }
}
