namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.1";
        public const string ReleaseName = "GX Light Browser 1.1";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Barra de favoritos con acceso rapido y apertura con middle-click.",
                "Importacion y exportacion de bookmarks en formato HTML compatible con navegadores.",
                "Menu de bookmarks para guardar la pagina actual y administrar favoritos.",
                "Menu contextual de pestanas con seleccion multiple y creacion de tab islands coloreadas.",
                "Importacion/exportacion de passwords en una boveda local protegida por Windows DPAPI.",
                "La pestana de novedades sigue apareciendo solo una vez por version."
            };
        }
    }
}
