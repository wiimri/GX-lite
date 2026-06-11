namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.19";
        public const string ReleaseName = "GX Light Browser 1.19";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Las actualizaciones se descargan y verifican en segundo plano sin cerrar GX Light.",
                "Cuando una actualizacion esta lista, GX Light avisa y permite reiniciar para aplicarla.",
                "El instalador silencioso vuelve a abrir el navegador y restaura la sesion.",
                "Los atajos Ctrl+T, Ctrl+W y demas comandos funcionan aun con WebView2 enfocado."
            };
        }
    }
}
