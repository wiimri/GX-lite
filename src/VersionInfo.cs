namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.18";
        public const string ReleaseName = "GX Light Browser 1.18";

        public static string[] Highlights()
        {
            return new string[]
            {
                "YouTube Shields elimina instrucciones publicitarias de la respuesta del reproductor antes de reproducirlas.",
                "La barra superior incorpora un boton visible Block Ads On/Off.",
                "Los favicons usan varias fuentes y se guardan en un cache local por dominio.",
                "El bloqueo conserva los datos normales del video sin acelerar anuncios ni robar foco."
            };
        }
    }
}
