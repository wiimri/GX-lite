namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.13";
        public const string ReleaseName = "GX Light Browser 1.13";

        public static string[] Highlights()
        {
            return new string[]
            {
                "El instalador usa la carpeta Program Files nativa de Windows.",
                "GX Light detecta versiones nuevas desde GitHub al iniciar.",
                "La actualizacion se descarga desde Releases y verifica su SHA-256.",
                "El instalador se abre directamente y conserva perfil, passwords y sesion.",
                "Incluye las mejoras de pestanas, favicons e islas colapsables de 1.12."
            };
        }
    }
}
