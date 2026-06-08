using System;
using System.IO;
using System.Text;

namespace GXLightBrowser
{
    internal sealed class AppSettings
    {
        public string LastSeenVersion { get; set; }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            if (!File.Exists(AppPaths.Settings))
            {
                return settings;
            }

            foreach (string rawLine in File.ReadAllLines(AppPaths.Settings, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                int equals = rawLine.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                string key = rawLine.Substring(0, equals).Trim();
                string value = rawLine.Substring(equals + 1).Trim();
                if (string.Equals(key, "LastSeenVersion", StringComparison.OrdinalIgnoreCase))
                {
                    settings.LastSeenVersion = value;
                }
            }

            return settings;
        }

        public void Save()
        {
            AppPaths.Ensure();
            StringBuilder builder = new StringBuilder();
            builder.Append("LastSeenVersion=").Append(LastSeenVersion ?? string.Empty).AppendLine();
            File.WriteAllText(AppPaths.Settings, builder.ToString(), Encoding.UTF8);
        }
    }
}
