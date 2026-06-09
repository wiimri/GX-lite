namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.9";
        public const string ReleaseName = "GX Light Browser 1.9";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Nuevo instalador para Windows 10/11 x64.",
                "El instalador comprueba e instala WebView2 Runtime y .NET Framework 4.8.",
                "Se incluyen siempre las tres bibliotecas WebView2 requeridas junto al navegador.",
                "GX Light detecta WebView2 ausente y muestra instrucciones de reparacion.",
                "Se agrego documentacion especial para Atlas OS y sistemas modificados."
            };
        }
    }
}
