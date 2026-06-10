namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.12";
        public const string ReleaseName = "GX Light Browser 1.12";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Passwords: GX Light pregunta antes de guardar y Windows cifra las credenciales.",
                "Las pestanas suspendidas ahora usan el indicador corto [S].",
                "Favicons reforzados y tamanos de pestana configurables.",
                "Seleccion multiple con Ctrl o Shift e islas colapsables persistentes.",
                "El menu puede buscar y descargar el instalador de la ultima version."
            };
        }
    }
}
