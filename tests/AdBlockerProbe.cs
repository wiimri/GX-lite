using System;
using System.IO;
using GXLightBrowser;

internal static class AdBlockerProbe
{
    private static int Main()
    {
        string filters = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(filters, new string[]
            {
                "||youtube.com^",
                "@@||youtube.com/get_video_info"
            });

            AdBlocker blocker = new AdBlocker();
            blocker.Load(filters);
            Uri document = new Uri("https://www.youtube.com/watch?v=test");

            if (blocker.ShouldBlock(new Uri("https://www.youtube.com/get_video_info?id=test"), document))
            {
                Console.Error.WriteLine("Expected the path-specific exception to be allowed.");
                return 1;
            }

            if (!blocker.ShouldBlock(new Uri("https://www.youtube.com/watch?v=other"), document))
            {
                Console.Error.WriteLine("Path-specific exception incorrectly allowed the entire YouTube domain.");
                return 1;
            }

            if (!blocker.ShouldBlock(new Uri("https://www.youtube.com/youtubei/v1/ads/request"), document))
            {
                Console.Error.WriteLine("Expected the built-in YouTube ads endpoint to be blocked.");
                return 1;
            }

            Console.WriteLine("Ad blocker probe passed.");
            return 0;
        }
        finally
        {
            File.Delete(filters);
        }
    }
}
