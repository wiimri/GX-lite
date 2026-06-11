using System;

namespace GXLightBrowser
{
    internal sealed class PasswordVaultEntry
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Note { get; set; }
        public DateTime ImportedUtc { get; set; }
    }
}
