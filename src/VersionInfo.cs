namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.10";
        public const string ReleaseName = "GX Light Browser 1.10";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Nuevo modo de compatibilidad automatico para Crunchyroll.",
                "Shields pausa recursos para Crunchyroll sin permitir ventanas emergentes automaticas.",
                "El host de navegacion se usa desde el primer recurso aunque WebView2 siga en about:blank.",
                "Los recursos y popups bloqueados ahora se contabilizan por separado.",
                "Se agrego un enlace permanente al instalador mas reciente."
            };
        }
    }
}
