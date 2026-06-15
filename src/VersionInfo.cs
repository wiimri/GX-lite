namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "2.3";
        public const string ReleaseName = "Gan Browser 2.3";

        public static string[] Highlights()
        {
            return new string[]
            {
                "El actualizador reintenta automaticamente las descargas interrumpidas desde GitHub.",
                "Los instaladores parciales o vacios se eliminan antes de volver a descargar.",
                "Se mejora el registro de errores transitorios durante la actualizacion."
            };
        }
    }
}
