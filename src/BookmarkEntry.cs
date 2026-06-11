using System;

namespace GXLightBrowser
{
    internal sealed class BookmarkEntry
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
