namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.3";
        public const string ReleaseName = "GX Light Browser 1.3";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Los botones de la pagina de novedades ahora navegan mediante el host del navegador.",
                "Ver release y Abrir GitHub ya no quedan atrapados dentro del documento interno.",
                "El canal interno de navegacion esta restringido a paginas generadas por GX Light.",
                "El manifiesto remoto update.json sigue controlando la pestana de novedades."
            };
        }
    }
}
