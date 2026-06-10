namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.14";
        public const string ReleaseName = "GX Light Browser 1.14";

        public static string[] Highlights()
        {
            return new string[]
            {
                "La seleccion multiple de pestanas ahora tiene borde y marcador visibles.",
                "Ctrl+clic selecciona individualmente y Shift+clic selecciona rangos.",
                "Cada isla tiene una barra propia para colapsar o desplegar sus pestanas.",
                "Las pestanas se pueden arrastrar hacia una isla o sobre otra pestana.",
                "El menu contextual incluye tamanos y los favicons tienen un respaldo compatible."
            };
        }
    }
}
