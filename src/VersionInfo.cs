namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.16";
        public const string ReleaseName = "GX Light Browser 1.16";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Las paginas internas y modelos fueron extraidos de BrowserForm para facilitar mantenimiento.",
                "Update notes ahora muestra una bitacora acumulativa descargada desde CHANGELOG.md.",
                "Se corrigieron gxlight://home y gxlight://updated para evitar Section not found.",
                "Se agrego suspension manual de pestanas seleccionadas y medicion exacta de memoria WebView2.",
                "Se reforzaron favicons, telemetria, el bloqueador y YouTube Shields.",
                "El instalador y los accesos directos fueron reparados para actualizaciones confiables."
            };
        }
    }
}
