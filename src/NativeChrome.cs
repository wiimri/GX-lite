using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GXLightBrowser
{
    internal static class NativeChrome
    {
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20h1 = 19;
        private const int DwmwaWindowCornerPreference = 33;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);

        public static void ApplyDarkFrame(Form form)
        {
            if (form == null || form.Handle == IntPtr.Zero)
            {
                return;
            }

            int enabled = 1;
            DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int));
            DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkModeBefore20h1, ref enabled, sizeof(int));

            int rounded = 2;
            DwmSetWindowAttribute(form.Handle, DwmwaWindowCornerPreference, ref rounded, sizeof(int));
        }
    }
}
