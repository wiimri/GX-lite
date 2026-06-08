# GX Light Browser

GX Light Browser is a lightweight Windows browser prototype inspired by the workflow of Opera GX and Brave, without copying either product's branding or protected UI.

It uses Microsoft Edge WebView2 instead of Electron, so the app is small and relies on the WebView2 runtime already present on many Windows systems.

## Current features

- Multi-tab browsing.
- Isolated persistent profile under `%LOCALAPPDATA%\GXLightBrowser`.
- WebView2 browser extensions enabled.
- Import unpacked Chrome/Edge extensions from a folder.
- Native request blocking before requests hit the page.
- Local ABP-style filter file at `%LOCALAPPDATA%\GXLightBrowser\filters.txt`.
- Strict WebView2 tracking prevention.
- Polished compact shell with soft buttons, dark window frame, responsive toolbar, tab strip, sidebar shortcuts, and clear tooltips.
- Extension marketplace shortcuts for Chrome Web Store and Opera Addons.
- Tab close controls: click the `x`, middle-click a tab, or press `Ctrl+W`.
- Middle-click shortcuts open in a new internal tab.
- YouTube Shields helper that removes common YouTube ad containers, clicks skip buttons, and speeds through in-player ad states.
- Low-memory target for inactive tabs.
- Faster domain-rule lookup in the native blocker.
- Built-in Privacy Firewall for known third-party trackers, telemetry-like endpoints, and tracking URL parameters.
- Main menu inspired by Opera/Brave with shortcuts for tabs, history, downloads, extensions, passwords, memory, shields, settings, and developer tools.
- Session history page.
- Session downloads page.
- WebView2 password autosave and autofill enabled.
- Tab islands for grouped new tabs.
- Real tab suspension/discarding for inactive tabs.
- Visible memory monitor and configurable GX Control limiters.

## Build

Run from PowerShell:

```powershell
cd C:\Users\arias\Documents\DEV\GXLightBrowser
.\scripts\Build.ps1
.\scripts\Run.ps1
```

The build script downloads the official `Microsoft.Web.WebView2` NuGet package, compiles with the .NET Framework compiler included in Windows, and writes `bin\GXLightBrowser.exe`.

## Extensions

Open `Menu > Extensions` and use one of these options:

- `Importar extension desempaquetada...`
- `Ver extensiones instaladas`
- `Abrir Chrome Web Store`
- `Abrir Opera Addons`

When importing, select either:

- an unpacked extension folder containing `manifest.json`, or
- a Chrome/Edge installed extension version folder.

The importer copies the extension into the GX Light profile and removes `_metadata`-style folders because WebView2 rejects extension paths containing names that start with `_`.

Important: WebView2 supports browser extensions from local folders, but it is not the Chrome Web Store UI. Some extensions that depend on browser chrome UI, store-specific installation behavior, or unsupported APIs may need adaptation.

The store buttons are navigation shortcuts. Direct one-click installation from Chrome Web Store is controlled by the store/browser integration and may not work inside WebView2, so the reliable path for now is downloading or locating the extension folder and importing it.

## Ad blocking

The first version ships with a native blocker that intercepts WebView2 requests through `WebResourceRequested`. It supports the highest-impact subset of ABP/EasyList network rules:

- `||domain.com^`
- substring URL rules
- basic `@@` allow rules
- comments and cosmetic rules are ignored safely

To refresh larger filter lists:

```powershell
.\scripts\Update-Filters.ps1
```

## Brave-level blocker path

Brave's production blocker is powered by the open-source `brave/adblock-rust` engine. This prototype is structured so the `AdBlocker` class can be replaced by a native `adblock-rust` binding later. That requires either:

- Rust toolchain plus a C ABI/UniFFI wrapper for C#, or
- the Node `adblock-rs` binding as a local sidecar.

The current app already blocks requests natively and does not rely on Manifest V3 extension APIs for its built-in blocker.

## Privacy Firewall

GX Light Browser now includes a local browser-level Privacy Firewall. It does not install system drivers or modify Windows Firewall rules; it runs inside the browser request pipeline.

It currently:

- blocks known third-party tracker domains such as analytics, pixels, RUM, ad-tech, and attribution hosts
- blocks third-party requests that look like telemetry, beacons, pixels, or tracking endpoints
- strips tracking query parameters such as `utm_*`, `fbclid`, `gclid`, `msclkid`, `mc_cid`, and similar IDs during navigation
- keeps site compatibility exceptions for YouTube video playback resources

This is not a VPN and cannot hide your IP from websites. It reduces browser-level tracking surfaces but does not replace a full native adblock engine such as `adblock-rust`.

## UI verification

The responsive browser chrome is mirrored in `docs/ui-preview.html` so it can be checked with Playwright:

```powershell
npm.cmd run test:ui
npm.cmd run test:firewall
```

This saves screenshots under `screenshots/` and checks desktop plus compact widths for horizontal overflow.
It also verifies tab closing, middle-click shortcut behavior, and the YouTube Shields script against a mocked YouTube ad page.
The firewall probe checks tracker blocking and tracking-parameter stripping.

## Low-resource controls

- The top bar keeps navigation, address, memory usage, memory limit, and menu.
- Other browser features live under `Menu`.
- `GX 0.8G` opens GX Control with RAM, hard limit, hot tabs killer, CPU policy, and network policy controls.
- Inactive tabs are marked low-memory and can be suspended/discarded.
- `Menu > Suspend inactive tabs now` discards inactive WebViews while leaving the tab visible.

## Audit

See `AUDIT.md` for the current architecture review, low-resource roadmap, measured memory notes, and next priorities.

## Sources checked

- Microsoft WebView2 extension APIs: `CoreWebView2EnvironmentOptions.AreBrowserExtensionsEnabled`, `CoreWebView2Profile.AddBrowserExtensionAsync`, and `GetBrowserExtensionsAsync`.
- Brave adblock engine: `https://github.com/brave/adblock-rust`.
