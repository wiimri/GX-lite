namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.21";
        public const string ReleaseName = "GX Light Browser 1.21";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Soporte completo para temas Claro (Light), Oscuro (Dark) y Automatico (Auto).",
                "Fondos y paneles inmersivos que se adaptan con sutiles matices del color de acento.",
                "Intercepcion robusta de atajos de teclado (Ctrl+T, Ctrl+W) con foco dentro del WebView2.",
                "Selector de buscador predeterminado funcional e integrado con la pagina de inicio y direccion."
            };
        }
    }
}
