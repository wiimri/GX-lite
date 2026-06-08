using System;
using System.IO;

namespace GXLightBrowser
{
    internal static class AppPaths
    {
        public static readonly string AppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GXLightBrowser");

        public static readonly string Profile = Path.Combine(AppData, "Profile");
        public static readonly string Extensions = Path.Combine(AppData, "Extensions");
        public static readonly string Filters = Path.Combine(AppData, "filters.txt");
        public static readonly string Settings = Path.Combine(AppData, "settings.ini");
        public static readonly string Bookmarks = Path.Combine(AppData, "bookmarks.tsv");
        public static readonly string PasswordVault = Path.Combine(AppData, "password-vault.dat");

        public static void Ensure()
        {
            Directory.CreateDirectory(AppData);
            Directory.CreateDirectory(Profile);
            Directory.CreateDirectory(Extensions);
        }
    }
}
