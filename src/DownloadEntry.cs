using System;

namespace GXLightBrowser
{
    internal sealed class DownloadEntry
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Uri { get; set; }
        public string State { get; set; }
        public DateTime StartedUtc { get; set; }
    }
}
