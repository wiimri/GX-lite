using System;
using System.Collections.Generic;
using System.Text;

namespace GXLightBrowser
{
    internal sealed class PrivacyFirewall
    {
        private readonly HashSet<string> _trackerDomains = new HashSet<string>();
        private readonly HashSet<string> _trackingParameters = new HashSet<string>();

        public PrivacyFirewall()
        {
            foreach (string domain in BuiltInTrackerDomains())
            {
                _trackerDomains.Add(domain);
            }

            foreach (string parameter in BuiltInTrackingParameters())
            {
                _trackingParameters.Add(parameter);
            }
        }

        public int RuleCount
        {
            get { return _trackerDomains.Count + _trackingParameters.Count; }
        }

        public bool ShouldBlock(Uri requestUri, Uri documentUri)
        {
            if (requestUri == null)
            {
                return false;
            }

            string scheme = requestUri.Scheme.ToLowerInvariant();
            if (scheme != "http" && scheme != "https")
            {
                return false;
            }

            string host = requestUri.Host.ToLowerInvariant();
            string documentHost = documentUri == null ? string.Empty : documentUri.Host.ToLowerInvariant();

            if (IsSiteCompatibilityAllow(host, documentHost))
            {
                return false;
            }

            if (HasDomainMatch(_trackerDomains, host))
            {
                return true;
            }

            if (!IsThirdParty(host, documentHost))
            {
                return false;
            }

            string path = requestUri.AbsolutePath.ToLowerInvariant();
            string query = requestUri.Query.ToLowerInvariant();
            return LooksLikeTrackingEndpoint(path, query);
        }

        public string StripTrackingParameters(string input)
        {
            Uri uri;
            if (!Uri.TryCreate(input, UriKind.Absolute, out uri))
            {
                return input;
            }

            if (uri.Query.Length <= 1)
            {
                return input;
            }

            string query = uri.Query.Substring(1);
            string[] pairs = query.Split('&');
            StringBuilder clean = new StringBuilder();
            bool changed = false;

            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i].Length == 0)
                {
                    continue;
                }

                string key = pairs[i];
                int equals = key.IndexOf('=');
                if (equals >= 0)
                {
                    key = key.Substring(0, equals);
                }

                string decodedKey = Uri.UnescapeDataString(key).ToLowerInvariant();
                if (IsTrackingParameter(decodedKey))
                {
                    changed = true;
                    continue;
                }

                if (clean.Length > 0)
                {
                    clean.Append('&');
                }
                clean.Append(pairs[i]);
            }

            if (!changed)
            {
                return input;
            }

            UriBuilder builder = new UriBuilder(uri);
            builder.Query = clean.ToString();
            return builder.Uri.AbsoluteUri;
        }

        private bool IsTrackingParameter(string key)
        {
            return key.StartsWith("utm_", StringComparison.Ordinal) ||
                key.StartsWith("yclid", StringComparison.Ordinal) ||
                _trackingParameters.Contains(key);
        }

        private static bool LooksLikeTrackingEndpoint(string path, string query)
        {
            return path.IndexOf("/collect", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/analytics", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/telemetry", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/beacon", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/pixel", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/track", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/tracking", StringComparison.Ordinal) >= 0 ||
                query.IndexOf("fingerprint", StringComparison.Ordinal) >= 0 ||
                query.IndexOf("telemetry", StringComparison.Ordinal) >= 0;
        }

        private static bool IsThirdParty(string host, string documentHost)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(documentHost))
            {
                return false;
            }

            string requestRoot = RegistrableDomain(host);
            string documentRoot = RegistrableDomain(documentHost);
            return requestRoot.Length > 0 && documentRoot.Length > 0 && requestRoot != documentRoot;
        }

        private static string RegistrableDomain(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return string.Empty;
            }

            string[] parts = host.Split('.');
            if (parts.Length < 2)
            {
                return host;
            }

            return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
        }

        private static bool IsSiteCompatibilityAllow(string host, string documentHost)
        {
            if (IsHostOrSubdomain(documentHost, "youtube.com") ||
                IsHostOrSubdomain(documentHost, "youtu.be"))
            {
                return IsHostOrSubdomain(host, "googlevideo.com") ||
                    IsHostOrSubdomain(host, "ytimg.com") ||
                    IsHostOrSubdomain(host, "youtubei.googleapis.com");
            }

            return false;
        }

        private static bool HasDomainMatch(HashSet<string> domains, string host)
        {
            string current = host;
            while (!string.IsNullOrEmpty(current))
            {
                if (domains.Contains(current))
                {
                    return true;
                }

                int dot = current.IndexOf('.');
                if (dot < 0 || dot == current.Length - 1)
                {
                    break;
                }

                current = current.Substring(dot + 1);
            }

            return false;
        }

        private static bool IsHostOrSubdomain(string host, string domain)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            return host == domain || host.EndsWith("." + domain, StringComparison.Ordinal);
        }

        private static IEnumerable<string> BuiltInTrackerDomains()
        {
            yield return "doubleclick.net";
            yield return "googlesyndication.com";
            yield return "googleadservices.com";
            yield return "google-analytics.com";
            yield return "googletagmanager.com";
            yield return "facebook.net";
            yield return "connect.facebook.net";
            yield return "analytics.twitter.com";
            yield return "ads-twitter.com";
            yield return "scorecardresearch.com";
            yield return "hotjar.com";
            yield return "hotjar.io";
            yield return "segment.com";
            yield return "segment.io";
            yield return "amplitude.com";
            yield return "mixpanel.com";
            yield return "fullstory.com";
            yield return "clarity.ms";
            yield return "bat.bing.com";
            yield return "nr-data.net";
            yield return "newrelic.com";
            yield return "datadoghq-browser-agent.com";
            yield return "rum.browser-intake-datadoghq.com";
            yield return "bugsnag.com";
            yield return "rollbar.com";
            yield return "taboola.com";
            yield return "outbrain.com";
            yield return "criteo.com";
            yield return "criteo.net";
            yield return "quantserve.com";
            yield return "moatads.com";
            yield return "adsrvr.org";
        }

        private static IEnumerable<string> BuiltInTrackingParameters()
        {
            yield return "fbclid";
            yield return "gclid";
            yield return "dclid";
            yield return "gbraid";
            yield return "wbraid";
            yield return "msclkid";
            yield return "mc_cid";
            yield return "mc_eid";
            yield return "igshid";
            yield return "si";
            yield return "spm";
            yield return "ref_src";
            yield return "ref_url";
            yield return "vero_id";
            yield return "wickedid";
            yield return "_hsenc";
            yield return "_hsmi";
        }
    }
}
