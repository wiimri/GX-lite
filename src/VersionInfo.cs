namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.0";
        public const string ReleaseName = "GX Light Browser 1.0";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Base Windows ligera con WebView2.",
                "Pestanas, tab islands, cierre con x, middle-click y atajos.",
                "GX Control configurable con RAM limiter, hard limit, hot tabs killer, CPU policy y network policy.",
                "Suspension real de pestanas para liberar WebView2 y memoria.",
                "Privacy Firewall local y bloqueo nativo de trackers/anuncios.",
                "Secciones internas para historial, descargas, extensiones, passwords, memoria, shields y settings.",
                "Pruebas Playwright para UI responsive, tabs y YouTube Shields."
            };
        }
    }
}
