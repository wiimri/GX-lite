using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GXLightBrowser
{
    internal sealed class ExtensionImporter
    {
        public async Task<string> ImportAsync(CoreWebView2Profile profile, string selectedPath)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("WebView2 profile is not ready.");
            }

            string source = ResolveManifestFolder(selectedPath);
            if (source == null)
            {
                throw new InvalidOperationException("No manifest.json was found in the selected folder.");
            }

            string name = ReadManifestName(Path.Combine(source, "manifest.json"));
            string safeName = MakeSafeName(name.Length == 0 ? new DirectoryInfo(source).Name : name);
            string target = Path.Combine(AppPaths.Extensions, safeName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss"));

            CopyExtension(source, target);
            CoreWebView2BrowserExtension extension = await profile.AddBrowserExtensionAsync(target);
            return extension.Name + " (" + extension.Id + ")";
        }

        private static string ResolveManifestFolder(string selectedPath)
        {
            if (string.IsNullOrWhiteSpace(selectedPath) || !Directory.Exists(selectedPath))
            {
                return null;
            }

            if (File.Exists(Path.Combine(selectedPath, "manifest.json")))
            {
                return selectedPath;
            }

            DirectoryInfo root = new DirectoryInfo(selectedPath);
            DirectoryInfo[] children = root.GetDirectories();
            Array.Sort(children, delegate(DirectoryInfo left, DirectoryInfo right)
            {
                return DateTime.Compare(right.LastWriteTimeUtc, left.LastWriteTimeUtc);
            });

            for (int i = 0; i < children.Length; i++)
            {
                string candidate = Path.Combine(children[i].FullName, "manifest.json");
                if (File.Exists(candidate))
                {
                    return children[i].FullName;
                }
            }

            return null;
        }

        private static void CopyExtension(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(directory);
                if (name.StartsWith("_", StringComparison.Ordinal))
                {
                    continue;
                }

                string relative = directory.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(target, relative));
            }

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string[] parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                bool skip = false;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("_", StringComparison.Ordinal))
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip)
                {
                    continue;
                }

                string destination = Path.Combine(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
            }
        }

        private static string ReadManifestName(string manifestPath)
        {
            string json = File.ReadAllText(manifestPath);
            Match match = Regex.Match(json, "\"name\"\\s*:\\s*\"(?<name>[^\"]+)\"");
            return match.Success ? match.Groups["name"].Value : string.Empty;
        }

        private static string MakeSafeName(string value)
        {
            string safe = Regex.Replace(value, "[^A-Za-z0-9._-]+", "-").Trim('-');
            return safe.Length == 0 ? "extension" : safe;
        }
    }
}
