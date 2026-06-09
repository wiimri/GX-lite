using System;
using System.IO;
using System.Text;

namespace GXLightBrowser
{
    internal sealed class AppSettings
    {
        public string LastSeenVersion { get; set; }
        public bool AdBlockEnabled { get; set; }
        public bool PrivacyFirewallEnabled { get; set; }
        public bool PasswordSavingEnabled { get; set; }
        
        // GxControlSettings properties
        public bool RamLimiterEnabled { get; set; }
        public int MemoryLimitMb { get; set; }
        public bool HardMemoryLimit { get; set; }
        public bool HotTabsKillerEnabled { get; set; }
        public string HotTabsMode { get; set; }
        public bool CpuLimiterEnabled { get; set; }
        public int CpuLimitPercent { get; set; }
        public bool NetworkLimiterEnabled { get; set; }
        public string NetworkProfile { get; set; }
        
        // Low Resource Mode properties
        public bool LowResourcesModeEnabled { get; set; }
        public int MaxActiveTabs { get; set; }
        public bool ShowPageIcons { get; set; }
        public bool CompactIconTabs { get; set; }

        public AppSettings()
        {
            LastSeenVersion = string.Empty;
            AdBlockEnabled = true;
            PrivacyFirewallEnabled = true;
            PasswordSavingEnabled = true;
            
            RamLimiterEnabled = true;
            MemoryLimitMb = 768;
            HardMemoryLimit = false;
            HotTabsKillerEnabled = true;
            HotTabsMode = "RAM";
            CpuLimiterEnabled = false;
            CpuLimitPercent = 50;
            NetworkLimiterEnabled = false;
            NetworkProfile = "25 MB/s - 200 Mbps";
            
            LowResourcesModeEnabled = false;
            MaxActiveTabs = 5;
            ShowPageIcons = true;
            CompactIconTabs = false;
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            if (!File.Exists(AppPaths.Settings))
            {
                return settings;
            }

            try
            {
                foreach (string rawLine in File.ReadAllLines(AppPaths.Settings, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(rawLine)) continue;
                    int equals = rawLine.IndexOf('=');
                    if (equals <= 0) continue;

                    string key = rawLine.Substring(0, equals).Trim();
                    string value = rawLine.Substring(equals + 1).Trim();

                    if (string.Equals(key, "LastSeenVersion", StringComparison.OrdinalIgnoreCase))
                        settings.LastSeenVersion = value;
                    else if (string.Equals(key, "AdBlockEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.AdBlockEnabled = bool.Parse(value);
                    else if (string.Equals(key, "PrivacyFirewallEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.PrivacyFirewallEnabled = bool.Parse(value);
                    else if (string.Equals(key, "PasswordSavingEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.PasswordSavingEnabled = bool.Parse(value);
                    else if (string.Equals(key, "RamLimiterEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.RamLimiterEnabled = bool.Parse(value);
                    else if (string.Equals(key, "MemoryLimitMb", StringComparison.OrdinalIgnoreCase))
                        settings.MemoryLimitMb = int.Parse(value);
                    else if (string.Equals(key, "HardMemoryLimit", StringComparison.OrdinalIgnoreCase))
                        settings.HardMemoryLimit = bool.Parse(value);
                    else if (string.Equals(key, "HotTabsKillerEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.HotTabsKillerEnabled = bool.Parse(value);
                    else if (string.Equals(key, "HotTabsMode", StringComparison.OrdinalIgnoreCase))
                        settings.HotTabsMode = value;
                    else if (string.Equals(key, "CpuLimiterEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.CpuLimiterEnabled = bool.Parse(value);
                    else if (string.Equals(key, "CpuLimitPercent", StringComparison.OrdinalIgnoreCase))
                        settings.CpuLimitPercent = int.Parse(value);
                    else if (string.Equals(key, "NetworkLimiterEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.NetworkLimiterEnabled = bool.Parse(value);
                    else if (string.Equals(key, "NetworkProfile", StringComparison.OrdinalIgnoreCase))
                        settings.NetworkProfile = value;
                    else if (string.Equals(key, "LowResourcesModeEnabled", StringComparison.OrdinalIgnoreCase))
                        settings.LowResourcesModeEnabled = bool.Parse(value);
                    else if (string.Equals(key, "MaxActiveTabs", StringComparison.OrdinalIgnoreCase))
                        settings.MaxActiveTabs = int.Parse(value);
                    else if (string.Equals(key, "ShowPageIcons", StringComparison.OrdinalIgnoreCase))
                        settings.ShowPageIcons = bool.Parse(value);
                    else if (string.Equals(key, "CompactIconTabs", StringComparison.OrdinalIgnoreCase))
                        settings.CompactIconTabs = bool.Parse(value);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading AppSettings: " + ex.Message);
            }

            return settings;
        }

        public void Save()
        {
            try
            {
                AppPaths.Ensure();
                StringBuilder builder = new StringBuilder();
                builder.Append("LastSeenVersion=").Append(LastSeenVersion ?? string.Empty).AppendLine();
                builder.Append("AdBlockEnabled=").Append(AdBlockEnabled).AppendLine();
                builder.Append("PrivacyFirewallEnabled=").Append(PrivacyFirewallEnabled).AppendLine();
                builder.Append("PasswordSavingEnabled=").Append(PasswordSavingEnabled).AppendLine();
                builder.Append("RamLimiterEnabled=").Append(RamLimiterEnabled).AppendLine();
                builder.Append("MemoryLimitMb=").Append(MemoryLimitMb).AppendLine();
                builder.Append("HardMemoryLimit=").Append(HardMemoryLimit).AppendLine();
                builder.Append("HotTabsKillerEnabled=").Append(HotTabsKillerEnabled).AppendLine();
                builder.Append("HotTabsMode=").Append(HotTabsMode ?? "RAM").AppendLine();
                builder.Append("CpuLimiterEnabled=").Append(CpuLimiterEnabled).AppendLine();
                builder.Append("CpuLimitPercent=").Append(CpuLimitPercent).AppendLine();
                builder.Append("NetworkLimiterEnabled=").Append(NetworkLimiterEnabled).AppendLine();
                builder.Append("NetworkProfile=").Append(NetworkProfile ?? "25 MB/s - 200 Mbps").AppendLine();
                builder.Append("LowResourcesModeEnabled=").Append(LowResourcesModeEnabled).AppendLine();
                builder.Append("MaxActiveTabs=").Append(MaxActiveTabs).AppendLine();
                builder.Append("ShowPageIcons=").Append(ShowPageIcons).AppendLine();
                builder.Append("CompactIconTabs=").Append(CompactIconTabs).AppendLine();

                File.WriteAllText(AppPaths.Settings, builder.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving AppSettings: " + ex.Message);
            }
        }
    }
}
