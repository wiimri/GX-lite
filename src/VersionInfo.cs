namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.15";
        public const string ReleaseName = "GX Light Browser 1.15";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Las pestanas se reducen automaticamente hasta formato de icono al llenar la barra.",
                "El favicon permanece visible incluso cuando ya no cabe el titulo.",
                "Las paginas sin favicon muestran un marcador de dominio reconocible.",
                "Las pestanas suspendidas recuperan su icono sin crear un WebView.",
                "Las islas compactas muestran varias barras segun sus pestanas."
            };
        }
    }
}
