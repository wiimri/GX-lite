namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.11";
        public const string ReleaseName = "GX Light Browser 1.11";

        public static string[] Highlights()
        {
            return new string[]
            {
                "El guardado de passwords se aplica al perfil persistente real de WebView2.",
                "Nuevo interruptor visible para guardar passwords automaticamente.",
                "Las sesiones usan un formato robusto que conserva URLs y titulos complejos.",
                "Al iniciar, solo la pestana activa consume un WebView; las demas quedan suspendidas.",
                "Nuevo interruptor Guardar pestanas al cerrar y cierre limpio del perfil."
            };
        }
    }
}
