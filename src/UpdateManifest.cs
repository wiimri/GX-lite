using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace GXLightBrowser
{
    [DataContract]
    internal sealed class UpdateManifest
    {
        public const string ManifestUrl = "https://raw.githubusercontent.com/wiimri/Gan-Browser/main/update.json";
        public const string DefaultChangelogUrl = "https://raw.githubusercontent.com/wiimri/Gan-Browser/main/CHANGELOG.md";

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "releaseName")]
        public string ReleaseName { get; set; }

        [DataMember(Name = "publishedAt")]
        public string PublishedAt { get; set; }

        [DataMember(Name = "downloadUrl")]
        public string DownloadUrl { get; set; }

        [DataMember(Name = "sha256Url")]
        public string Sha256Url { get; set; }

        [DataMember(Name = "sourceUrl")]
        public string SourceUrl { get; set; }

        [DataMember(Name = "changelogUrl")]
        public string ChangelogUrl { get; set; }

        [DataMember(Name = "highlights")]
        public string[] Highlights { get; set; }

        public string ChangelogMarkdown { get; set; }

        public static UpdateManifest LocalFallback()
        {
            return new UpdateManifest
            {
                Version = VersionInfo.CurrentVersion,
                ReleaseName = VersionInfo.ReleaseName,
                PublishedAt = "2026-06-11",
                DownloadUrl = "https://github.com/wiimri/Gan-Browser/releases",
                Sha256Url = string.Empty,
                SourceUrl = BrandInfo.RepositoryUrl,
                ChangelogUrl = DefaultChangelogUrl,
                ChangelogMarkdown = string.Empty,
                Highlights = VersionInfo.Highlights()
            };
        }

        public static async Task<UpdateManifest> LoadLatestAsync()
        {
            try
            {
                UpdateManifest manifest = await Task.Run(delegate { return DownloadLatest(); });
                if (!IsUsable(manifest))
                {
                    return LocalFallback();
                }

                manifest.ChangelogMarkdown = await Task.Run(delegate { return DownloadChangelog(manifest.ChangelogUrl); });
                return manifest;
            }
            catch
            {
                return LocalFallback();
            }
        }

        private static UpdateManifest DownloadLatest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ManifestUrl);
            request.UserAgent = "GanBrowser/" + VersionInfo.CurrentVersion;
            request.Timeout = 3500;
            request.ReadWriteTimeout = 3500;

            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UpdateManifest));
                return serializer.ReadObject(stream) as UpdateManifest;
            }
        }

        private static string DownloadChangelog(string changelogUrl)
        {
            try
            {
                string url = string.IsNullOrWhiteSpace(changelogUrl) ? DefaultChangelogUrl : changelogUrl;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "GanBrowser/" + VersionInfo.CurrentVersion;
                request.Timeout = 3500;
                request.ReadWriteTimeout = 3500;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsUsable(UpdateManifest manifest)
        {
            return manifest != null &&
                !string.IsNullOrWhiteSpace(manifest.Version) &&
                !string.IsNullOrWhiteSpace(manifest.ReleaseName) &&
                manifest.Highlights != null &&
                manifest.Highlights.Length > 0;
        }
    }
}
