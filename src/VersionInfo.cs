namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.22";
        public const string ReleaseName = "GX Light Browser 1.22";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Opcion configurable para buscar actualizaciones automaticamente en segundo plano al iniciar.",
                "Soporte completo para temas Claro (Light), Oscuro (Dark) y Automatico (Auto).",
                "Fondos y paneles inmersivos que se adaptan con sutiles matices del color de acento.",
                "Intercepcion robusta de atajos de teclado (Ctrl+T, Ctrl+W) con foco dentro del WebView2."
            };
        }
    }
}
