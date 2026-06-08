namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.6";
        public const string ReleaseName = "GX Light Browser 1.6";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Se corrigio Eliminar todos en el gestor de bookmarks.",
                "Los comandos internos de bookmarks ya no dependen solo de data:text/html como origen.",
                "La importacion de bookmarks conserva carpetas anidadas como rutas Padre / Hija.",
                "La exportacion HTML agrupa favoritos por carpeta.",
                "README, CHANGELOG y manifiesto remoto quedan sincronizados con la version actual."
            };
        }
    }
}
