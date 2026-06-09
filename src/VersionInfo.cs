namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.8";
        public const string ReleaseName = "GX Light Browser 1.8";

        public static string[] Highlights()
        {
            return new string[]
            {
                "YouTube Shields ya no se inyecta en Crunchyroll ni en otros sitios.",
                "El MutationObserver espera a que exista el documento antes de iniciarse.",
                "Se agrego una prueba Playwright para verificar aislamiento fuera de YouTube.",
                "Los rechazos HTTP 403 de Crunchyroll se identifican como respuesta del sitio.",
                "La barra de estado diferencia esos rechazos de los bloqueos creados por Shields."
            };
        }
    }
}
