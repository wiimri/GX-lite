using System;
using System.Drawing;

namespace GXLightBrowser
{
    internal static class Theme
    {
        public static Color Window = Color.FromArgb(13, 15, 20);
        public static Color Topbar = Color.FromArgb(31, 33, 42);
        public static Color Sidebar = Color.FromArgb(17, 19, 27);
        public static Color Panel = Color.FromArgb(24, 27, 35);
        public static Color Address = Color.FromArgb(35, 38, 48);
        public static Color Button = Color.FromArgb(37, 40, 51);
        public static Color Hover = Color.FromArgb(48, 53, 67);
        public static Color Selected = Color.FromArgb(53, 58, 73);
        public static Color Border = Color.FromArgb(72, 77, 92);
        public static Color Warning = Color.FromArgb(255, 105, 116);
        public static Color Text = Color.FromArgb(238, 247, 250);
        public static Color Muted = Color.FromArgb(161, 176, 190);

        // Mutable Accent color based on theme selection
        public static Color Accent = Color.FromArgb(250, 17, 79); // Default is Classic (Red)

        public static string AccentHex
        {
            get { return "#" + Accent.R.ToString("X2") + Accent.G.ToString("X2") + Accent.B.ToString("X2"); }
        }

        public static string WindowHex
        {
            get { return "#" + Window.R.ToString("X2") + Window.G.ToString("X2") + Window.B.ToString("X2"); }
        }

        public static string PanelHex
        {
            get { return "#" + Panel.R.ToString("X2") + Panel.G.ToString("X2") + Panel.B.ToString("X2"); }
        }

        public static string TextHex
        {
            get { return "#" + Text.R.ToString("X2") + Text.G.ToString("X2") + Text.B.ToString("X2"); }
        }

        public static string MutedHex
        {
            get { return "#" + Muted.R.ToString("X2") + Muted.G.ToString("X2") + Muted.B.ToString("X2"); }
        }

        public static string BorderHex
        {
            get { return "#" + Border.R.ToString("X2") + Border.G.ToString("X2") + Border.B.ToString("X2"); }
        }

        public static void ApplyTheme(string themeName)
        {
            ApplyTheme(themeName, "Dark");
        }

        public static void ApplyTheme(string themeName, string themeMode)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                themeName = "Classic";
            }
            if (string.IsNullOrWhiteSpace(themeMode))
            {
                themeMode = "Dark";
            }

            // 1. Resolve Accent Color
            switch (themeName.ToLowerInvariant())
            {
                case "classic":
                case "gxclassic":
                    Accent = Color.FromArgb(250, 17, 79); // Red
                    break;
                case "ultraviolet":
                    Accent = Color.FromArgb(122, 60, 255); // Purple
                    break;
                case "subzero":
                case "sub zero":
                    Accent = Color.FromArgb(0, 191, 243); // Ice Blue
                    break;
                case "fruttidimare":
                case "frutti di mare":
                    Accent = Color.FromArgb(255, 99, 71); // Tomato Orange
                    break;
                case "purplemaze":
                case "purple maze":
                    Accent = Color.FromArgb(204, 51, 153); // Deep Magenta
                    break;
                case "vaporwave":
                    Accent = Color.FromArgb(255, 105, 180); // Hot Pink
                    break;
                case "rosequartz":
                case "rose quartz":
                    Accent = Color.FromArgb(247, 202, 201); // Soft Pink
                    break;
                case "hackerman":
                    Accent = Color.FromArgb(0, 255, 65); // Neon Green
                    break;
                case "lambda":
                    Accent = Color.FromArgb(253, 184, 19); // Orange-Yellow
                    break;
                case "aftereight":
                case "after eight":
                    Accent = Color.FromArgb(0, 153, 76); // Mint Green
                    break;
                case "paytowin":
                case "pay to win":
                    Accent = Color.FromArgb(255, 215, 0); // Gold
                    break;
                case "whitewolf":
                case "white wolf":
                    Accent = Color.FromArgb(211, 211, 211); // Silver
                    break;
                default:
                    Accent = Color.FromArgb(250, 17, 79); // Default Classic Red
                    break;
            }

            // 2. Resolve Light/Dark Mode
            bool light = false;
            if (string.Equals(themeMode, "light", StringComparison.OrdinalIgnoreCase))
            {
                light = true;
            }
            else if (string.Equals(themeMode, "auto", StringComparison.OrdinalIgnoreCase))
            {
                light = IsWindowsLightTheme();
            }

            // 3. Apply blending for immersive background tinting
            if (light)
            {
                Window = BlendColor(Accent, Color.FromArgb(245, 246, 248), 0.04f);
                Topbar = BlendColor(Accent, Color.FromArgb(232, 234, 238), 0.04f);
                Sidebar = BlendColor(Accent, Color.FromArgb(240, 241, 245), 0.06f);
                Panel = BlendColor(Accent, Color.FromArgb(255, 255, 255), 0.04f);
                Address = BlendColor(Accent, Color.FromArgb(255, 255, 255), 0.12f);
                Button = BlendColor(Accent, Color.FromArgb(220, 224, 230), 0.10f);
                Hover = BlendColor(Accent, Color.FromArgb(202, 209, 219), 0.15f);
                Selected = BlendColor(Accent, Color.FromArgb(182, 192, 206), 0.20f);
                Border = BlendColor(Accent, Color.FromArgb(162, 172, 188), 0.25f);
                Text = Color.FromArgb(22, 26, 35);
                Muted = Color.FromArgb(105, 115, 130);
            }
            else
            {
                Window = BlendColor(Accent, Color.FromArgb(13, 15, 20), 0.08f);
                Topbar = BlendColor(Accent, Color.FromArgb(24, 27, 35), 0.08f);
                Sidebar = BlendColor(Accent, Color.FromArgb(17, 19, 27), 0.08f);
                Panel = BlendColor(Accent, Color.FromArgb(24, 27, 35), 0.08f);
                Address = BlendColor(Accent, Color.FromArgb(35, 38, 48), 0.12f);
                Button = BlendColor(Accent, Color.FromArgb(37, 40, 51), 0.12f);
                Hover = BlendColor(Accent, Color.FromArgb(48, 53, 67), 0.15f);
                Selected = BlendColor(Accent, Color.FromArgb(53, 58, 73), 0.20f);
                Border = BlendColor(Accent, Color.FromArgb(72, 77, 92), 0.25f);
                Text = Color.FromArgb(238, 247, 250);
                Muted = Color.FromArgb(161, 176, 190);
            }
        }

        private static Color BlendColor(Color accent, Color baseColor, float amount)
        {
            int r = (int)(baseColor.R + (accent.R - baseColor.R) * amount);
            int g = (int)(baseColor.G + (accent.G - baseColor.G) * amount);
            int b = (int)(baseColor.B + (accent.B - baseColor.B) * amount);
            return Color.FromArgb(
                Math.Min(255, Math.Max(0, r)),
                Math.Min(255, Math.Max(0, g)),
                Math.Min(255, Math.Max(0, b))
            );
        }

        private static bool IsWindowsLightTheme()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            return (int)value == 1;
                        }
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
