using System;
using GXLightBrowser;

internal static class PrivacyFirewallProbe
{
    private static int Main()
    {
        PrivacyFirewall firewall = new PrivacyFirewall();
        Uri document = new Uri("https://example.com/article");
        Uri tracker = new Uri("https://www.google-analytics.com/collect?v=1");
        Uri firstParty = new Uri("https://example.com/assets/app.js");

        if (!firewall.ShouldBlock(tracker, document))
        {
            Console.Error.WriteLine("Expected third-party analytics request to be blocked.");
            return 1;
        }

        if (firewall.ShouldBlock(firstParty, document))
        {
            Console.Error.WriteLine("Expected first-party asset request to be allowed.");
            return 1;
        }

        string clean = firewall.StripTrackingParameters("https://example.com/page?utm_source=x&fbclid=abc&id=42");
        if (clean.IndexOf("utm_source", StringComparison.OrdinalIgnoreCase) >= 0 ||
            clean.IndexOf("fbclid", StringComparison.OrdinalIgnoreCase) >= 0 ||
            clean.IndexOf("id=42", StringComparison.OrdinalIgnoreCase) < 0)
        {
            Console.Error.WriteLine("Expected tracking parameters to be stripped while preserving id=42.");
            return 1;
        }

        Console.WriteLine("Privacy firewall probe passed.");
        return 0;
    }
}
