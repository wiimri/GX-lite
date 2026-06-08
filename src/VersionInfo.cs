namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.2";
        public const string ReleaseName = "GX Light Browser 1.2";

        public static string[] Highlights()
        {
            return new string[]
            {
                "El navegador consulta update.json en GitHub al iniciar.",
                "Las novedades ya no dependen solo de VersionInfo compilado dentro del exe.",
                "Si GitHub no responde, se usa un manifiesto local de respaldo.",
                "El aviso se sigue mostrando solo una vez por version publicada.",
                "El menu Update notes abre la informacion cargada desde el manifiesto."
            };
        }
    }
}
