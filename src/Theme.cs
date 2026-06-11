using System.Drawing;

namespace GXLightBrowser
{
    internal static class Theme
    {
        public static readonly Color Window = Color.FromArgb(13, 15, 20);
        public static readonly Color Topbar = Color.FromArgb(31, 33, 42);
        public static readonly Color Sidebar = Color.FromArgb(17, 19, 27);
        public static readonly Color Panel = Color.FromArgb(24, 27, 35);
        public static readonly Color Address = Color.FromArgb(35, 38, 48);
        public static readonly Color Button = Color.FromArgb(37, 40, 51);
        public static readonly Color Hover = Color.FromArgb(48, 53, 67);
        public static readonly Color Selected = Color.FromArgb(53, 58, 73);
        public static readonly Color Border = Color.FromArgb(72, 77, 92);
        public static readonly Color Warning = Color.FromArgb(255, 105, 116);
        public static readonly Color Text = Color.FromArgb(238, 247, 250);
        public static readonly Color Muted = Color.FromArgb(161, 176, 190);

        // Mutable Accent color based on theme selection
        public static Color Accent = Color.FromArgb(250, 17, 79); // Default is Classic (Red)

        public static string AccentHex
        {
            get { return "#" + Accent.R.ToString("X2") + Accent.G.ToString("X2") + Accent.B.ToString("X2"); }
        }

        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                themeName = "Classic";
            }

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
        }
    }
}
