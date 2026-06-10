using System;
using System.Threading;
using System.Windows.Forms;

namespace GXLightBrowser
{
    internal static class Program
    {
        private static Mutex _installMutex;

        [STAThread]
        private static void Main()
        {
            _installMutex = new Mutex(false, "GXLightBrowser-Install-Mutex");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BrowserForm());
        }
    }
}
