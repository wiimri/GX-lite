namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "2.2";
        public const string ReleaseName = "Gan Browser 2.2";

        public static string[] Highlights()
        {
            return new string[]
            {
                "La barra de marcadores ahora se puede mostrar u ocultar desde el menu Marcadores.",
                "Al ocultar la barra se elimina completamente el espacio residual bajo la barra de direcciones.",
                "La preferencia de visibilidad se conserva entre sesiones."
            };
        }
    }
}
