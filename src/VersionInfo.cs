namespace GXLightBrowser
{
    internal static class VersionInfo
    {
        public const string CurrentVersion = "1.5";
        public const string ReleaseName = "GX Light Browser 1.5";

        public static string[] Highlights()
        {
            return new string[]
            {
                "Se agrego seleccion multiple de bookmarks con checkboxes.",
                "Se puede eliminar varios favoritos a la vez con el boton Eliminar seleccionados.",
                "Se agrego boton Eliminar todos los favoritos con confirmacion.",
                "La tecla Suprimir (Delete) elimina los favoritos seleccionados.",
                "Ctrl+A selecciona o deselecciona todos los favoritos visibles.",
                "Eliminacion individual ya no requiere confirmacion extra.",
                "Se removio el boton de Opera Addons ya que no funcionaba.",
                "El boton de menu ahora usa el icono hamburguesa estandar.",
                "Se mejoro la importacion de bookmarks conservando la jerarquia de carpetas."
            };
        }
    }
}
