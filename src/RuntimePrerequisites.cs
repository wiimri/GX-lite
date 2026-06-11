using Microsoft.Web.WebView2.Core;
using System;

namespace GXLightBrowser
{
    internal static class RuntimePrerequisites
    {
        public static bool TryGetWebView2Version(out string version)
        {
            version = string.Empty;
            try
            {
                version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                return !string.IsNullOrWhiteSpace(version);
            }
            catch
            {
                return false;
            }
        }

        public static string MissingWebView2Message()
        {
            return "Gan Browser necesita Microsoft Edge WebView2 Runtime para mostrar paginas.\n\n" +
                "El sistema no lo tiene instalado o fue eliminado/deshabilitado. Esto es comun en instalaciones " +
                "modificadas como Atlas OS.\n\n" +
                "Ejecuta nuevamente el instalador de Gan Browser para reparar WebView2.";
        }
    }
}
