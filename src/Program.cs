using System;
using System.Windows.Forms;

namespace GXLightBrowser
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                Environment.SetEnvironmentVariable("COREWEBVIEW2_TELEMETRY_OPT_OUT", "1");
            }
            catch
            {
            }

            try
            {
                // Enable TLS 1.2 and TLS 1.3 for external web downloads (like favicons)
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls |
                    (System.Net.SecurityProtocolType)12288; // TLS 1.3
            }
            catch
            {
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BrowserForm());
        }
    }
}
