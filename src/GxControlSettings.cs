namespace GXLightBrowser
{
    internal sealed class GxControlSettings
    {
        public bool RamLimiterEnabled { get; set; }
        public int MemoryLimitMb { get; set; }
        public bool HardMemoryLimit { get; set; }
        public bool HotTabsKillerEnabled { get; set; }
        public string HotTabsMode { get; set; }
        public bool CpuLimiterEnabled { get; set; }
        public int CpuLimitPercent { get; set; }
        public bool NetworkLimiterEnabled { get; set; }
        public string NetworkProfile { get; set; }

        public GxControlSettings()
        {
            RamLimiterEnabled = true;
            MemoryLimitMb = 768;
            HardMemoryLimit = false;
            HotTabsKillerEnabled = true;
            HotTabsMode = "RAM";
            CpuLimiterEnabled = false;
            CpuLimitPercent = 50;
            NetworkLimiterEnabled = false;
            NetworkProfile = "25 MB/s - 200 Mbps";
        }
    }
}
