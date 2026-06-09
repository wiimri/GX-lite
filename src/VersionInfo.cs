namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.7";
        public const string ReleaseName = "GX Light Browser 1.7";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Ctrl+W cierra la pestana activa incluso cuando WebView2 tiene el foco.",
                "El ejecutable y la ventana ahora tienen un icono propio.",
                "Se agregaron favicons y pestanas compactas configurables.",
                "YouTube Shields recupera la reproduccion sin eliminar el contenedor principal del reproductor.",
                "Se agrego una Playlist local y compatibilidad de recursos multimedia para Crunchyroll."
            };
        }
    }
}
