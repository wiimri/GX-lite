using System;

namespace GXLightBrowser
{
    internal sealed class HistoryEntry
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime VisitedUtc { get; set; }
    }
}
