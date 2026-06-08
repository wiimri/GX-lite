using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;

namespace GXLightBrowser
{
    internal sealed class BrowserTab
    {
        public BrowserTab(TabPage page, WebView2 webView)
        {
            Page = page;
            WebView = webView;
            LastActiveUtc = System.DateTime.UtcNow;
        }

        public TabPage Page { get; private set; }
        public WebView2 WebView { get; set; }
        public int BlockedRequests { get; set; }
        public int IslandId { get; set; }
        public string SuspendedUrl { get; set; }
        public string SuspendedTitle { get; set; }
        public bool IsSuspended { get; set; }
        public System.DateTime LastActiveUtc { get; set; }
    }
}
