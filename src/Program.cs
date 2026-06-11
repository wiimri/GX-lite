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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BrowserForm());
        }
    }
}
