using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GXLightBrowser
{
    internal static class SessionManager
    {
        private static readonly string SessionFile = Path.Combine(AppPaths.AppData, "session.dat");

        public static void SaveSession(
            bool maximized,
            int x, int y, int width, int height,
            int activeIndex,
            Dictionary<int, Color> islandColors,
            List<BrowserTab> tabs)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("WindowMaximized:" + maximized);
                sb.AppendLine(string.Format("WindowBounds:{0},{1},{2},{3}", x, y, width, height));
                sb.AppendLine("ActiveIndex:" + activeIndex);

                foreach (KeyValuePair<int, Color> kv in islandColors)
                {
                    sb.AppendLine(string.Format("IslandColor:{0}|{1},{2},{3}", kv.Key, kv.Value.R, kv.Value.G, kv.Value.B));
                }

                sb.AppendLine("FormatVersion:2");
                foreach (var tab in tabs)
                {
                    string url = tab.IsSuspended ? tab.SuspendedUrl : (tab.WebView != null && tab.WebView.Source != null ? tab.WebView.Source.ToString() : "gxlight://home");
                    string title = tab.IsSuspended ? tab.SuspendedTitle : tab.Page.Text;
                    sb.AppendLine(string.Format("Tab2:{0}|{1}|{2}|{3}",
                        Encode(url), Encode(title), tab.IslandId, tab.IsSuspended));
                }

                AppPaths.Ensure();
                File.WriteAllText(SessionFile, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save session: " + ex.Message);
            }
        }

        public static SessionData LoadSession()
        {
            if (!File.Exists(SessionFile))
            {
                return null;
            }

            try
            {
                SessionData data = new SessionData();
                string[] lines = File.ReadAllLines(SessionFile, Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int idx = line.IndexOf(':');
                    if (idx < 0) continue;
                    string key = line.Substring(0, idx).Trim();
                    string val = line.Substring(idx + 1).Trim();

                    switch (key)
                    {
                        case "WindowMaximized":
                            data.Maximized = bool.Parse(val);
                            break;
                        case "WindowBounds":
                            string[] b = val.Split(',');
                            if (b.Length == 4)
                            {
                                data.X = int.Parse(b[0]);
                                data.Y = int.Parse(b[1]);
                                data.Width = int.Parse(b[2]);
                                data.Height = int.Parse(b[3]);
                            }
                            break;
                        case "ActiveIndex":
                            data.ActiveIndex = int.Parse(val);
                            break;
                        case "IslandColor":
                            string[] ic = val.Split('|');
                            if (ic.Length == 2)
                            {
                                int id = int.Parse(ic[0]);
                                string[] rgb = ic[1].Split(',');
                                if (rgb.Length == 3)
                                {
                                    Color color = Color.FromArgb(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
                                    data.IslandColors[id] = color;
                                }
                            }
                            break;
                        case "Tab2":
                            string[] t2 = val.Split('|');
                            if (t2.Length == 4)
                            {
                                TabData td2 = new TabData();
                                td2.Url = Decode(t2[0]);
                                td2.Title = Decode(t2[1]);
                                td2.IslandId = ParseInt(t2[2], 0);
                                td2.IsSuspended = ParseBool(t2[3], true);
                                data.Tabs.Add(td2);
                            }
                            break;
                        case "Tab":
                            string[] t = val.Split('|');
                            if (t.Length >= 4)
                            {
                                TabData td = new TabData();
                                td.Url = t[0];
                                td.Title = t[1];
                                td.IslandId = ParseInt(t[t.Length - 2], 0);
                                td.IsSuspended = ParseBool(t[t.Length - 1], true);
                                data.Tabs.Add(td);
                            }
                            break;
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load session: " + ex.Message);
                return null;
            }
        }

        public static void DeleteSession()
        {
            try
            {
                if (File.Exists(SessionFile))
                {
                    File.Delete(SessionFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to delete session: " + ex.Message);
            }
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string Decode(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value ?? string.Empty));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int ParseInt(string value, int fallback)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : fallback;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : fallback;
        }
    }

    internal class SessionData
    {
        public bool Maximized { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ActiveIndex { get; set; }
        public Dictionary<int, Color> IslandColors { get; set; }
        public List<TabData> Tabs { get; set; }

        public SessionData()
        {
            X = -1; Y = -1; Width = 1280; Height = 780;
            ActiveIndex = 0;
            IslandColors = new Dictionary<int, Color>();
            Tabs = new List<TabData>();
        }
    }

    internal class TabData
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int IslandId { get; set; }
        public bool IsSuspended { get; set; }
    }
}
