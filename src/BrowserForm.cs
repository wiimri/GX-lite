using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GXLightBrowser
{
    public sealed class BrowserForm : Form
    {
        private const string HomeUrl = "gxlight://home";
        private const string UpdatedUrl = "gxlight://updated";
        private const string ChromeStoreUrl = "https://chromewebstore.google.com/category/extensions?pli=1";

        private readonly AdBlocker _adBlocker = new AdBlocker();
        private readonly PrivacyFirewall _privacyFirewall = new PrivacyFirewall();
        private readonly ExtensionImporter _extensionImporter = new ExtensionImporter();
        private readonly TabControl _tabs = new TabControl();
        private readonly FlowLayoutPanel _tabStrip = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _bookmarksBar = new FlowLayoutPanel();
        private readonly TextBox _address = new TextBox();
        private readonly Label _status = new Label();
        private readonly ToolTip _tips = new ToolTip();

        private readonly ChromeButton _back = new ChromeButton();
        private readonly ChromeButton _forward = new ChromeButton();
        private readonly ChromeButton _reload = new ChromeButton();
        private readonly ChromeButton _newTab = new ChromeButton();
        private readonly ChromeButton _shield = new ChromeButton();
        private readonly ChromeButton _extensions = new ChromeButton();
        private readonly ChromeButton _chromeStore = new ChromeButton();
        private readonly ChromeButton _tabStripNewTab = new ChromeButton();
        private readonly ChromeButton _menuButton = new ChromeButton();
        private readonly ChromeButton _memoryLimitButton = new ChromeButton();
        private readonly Label _memoryLabel = new Label();
        private readonly Timer _memoryTimer = new Timer();
        private readonly List<HistoryEntry> _history = new List<HistoryEntry>();
        private readonly List<DownloadEntry> _downloads = new List<DownloadEntry>();
        private readonly List<BookmarkEntry> _bookmarks = new List<BookmarkEntry>();
        private readonly List<PasswordVaultEntry> _passwordVault = new List<PasswordVaultEntry>();
        private readonly Dictionary<int, Color> _islandColors = new Dictionary<int, Color>();
        private readonly GxControlSettings _gxControl = new GxControlSettings();
        private AppSettings _appSettings = new AppSettings();
        private UpdateManifest _updateManifest = UpdateManifest.LocalFallback();

        private CoreWebView2Environment _environment;
        private bool _adBlockEnabled = true;
        private bool _privacyFirewallEnabled = true;
        private bool _passwordSavingEnabled = true;
        private int _nextIslandId = 1;
        private int _activeIslandId;
        private DateTime _startedUtc = DateTime.UtcNow;

        public BrowserForm()
        {
            Text = "GX Light Browser";
            MinimumSize = new Size(760, 540);
            Size = new Size(1280, 780);
            BackColor = Theme.Window;
            ForeColor = Theme.Text;
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;
            KeyPreview = true;

            BuildLayout();
            SizeChanged += delegate { ApplyResponsiveMode(); };
            FormClosing += delegate { SaveSession(); };
            Load += async delegate
            {
                NativeChrome.ApplyDarkFrame(this);
                await InitializeAsync();
            };
        }

        private void BuildLayout()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Theme.Window;
            root.ColumnCount = 2;
            root.RowCount = 3;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            Controls.Add(root);

            Panel side = new Panel();
            side.Dock = DockStyle.Fill;
            side.Padding = new Padding(8, 12, 8, 8);
            side.BackColor = Theme.Sidebar;
            root.Controls.Add(side, 0, 0);
            root.SetRowSpan(side, 3);

            AddSideButton(side, "YT", "YouTube", "https://www.youtube.com/");
            AddSideButton(side, "TW", "Twitch", "https://www.twitch.tv/");
            AddSideButton(side, "DC", "Discord", "https://discord.com/app");
            AddSideButton(side, "GH", "GitHub", "https://github.com/");
            AddSideButton(side, "CWS", "Chrome Web Store", ChromeStoreUrl);

            TableLayoutPanel top = new TableLayoutPanel();
            top.Dock = DockStyle.Fill;
            top.BackColor = Theme.Topbar;
            top.Padding = new Padding(8, 6, 10, 7);
            top.RowCount = 3;
            top.ColumnCount = 1;
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(top, 1, 0);

            _tabStrip.Dock = DockStyle.Fill;
            _tabStrip.WrapContents = false;
            _tabStrip.AutoScroll = false;
            _tabStrip.BackColor = Theme.Topbar;
            top.Controls.Add(_tabStrip, 0, 0);

            TableLayoutPanel nav = new TableLayoutPanel();
            nav.Dock = DockStyle.Fill;
            nav.ColumnCount = 3;
            nav.RowCount = 1;
            nav.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            nav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            nav.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 294));
            top.Controls.Add(nav, 0, 1);

            FlowLayoutPanel leftNav = new FlowLayoutPanel();
            leftNav.Dock = DockStyle.Fill;
            leftNav.WrapContents = false;
            leftNav.BackColor = Theme.Topbar;
            nav.Controls.Add(leftNav, 0, 0);

            ConfigureButton(_back, "<", 38, "Volver");
            ConfigureButton(_forward, ">", 38, "Avanzar");
            ConfigureButton(_reload, "Reload", 66, "Recargar pagina");
            ConfigureButton(_newTab, "+", 38, "Nueva pestana");
            leftNav.Controls.AddRange(new Control[] { _back, _forward, _reload, _newTab });

            Panel addressShell = new Panel();
            addressShell.Dock = DockStyle.Fill;
            addressShell.Padding = new Padding(12, 7, 12, 0);
            addressShell.Margin = new Padding(6, 2, 8, 2);
            addressShell.BackColor = Theme.Address;
            addressShell.Paint += PaintAddressShell;
            addressShell.Resize += delegate { addressShell.Invalidate(); };
            nav.Controls.Add(addressShell, 1, 0);

            _address.BorderStyle = BorderStyle.None;
            _address.Dock = DockStyle.Fill;
            _address.BackColor = Theme.Address;
            _address.ForeColor = Theme.Text;
            _address.Font = new Font("Segoe UI", 10f);
            addressShell.Controls.Add(_address);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.WrapContents = false;
            actions.BackColor = Theme.Topbar;
            nav.Controls.Add(actions, 2, 0);

            _memoryLabel.Width = 92;
            _memoryLabel.Height = 32;
            _memoryLabel.Margin = new Padding(2, 2, 4, 2);
            _memoryLabel.TextAlign = ContentAlignment.MiddleCenter;
            _memoryLabel.ForeColor = Theme.Text;
            _memoryLabel.BackColor = Theme.Panel;
            _memoryLabel.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            ConfigureButton(_memoryLimitButton, "Limit 768", 88, "Limitador de memoria");
            ConfigureButton(_menuButton, "☰", 38, "Menu principal");
            _menuButton.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            actions.Controls.AddRange(new Control[] { _menuButton, _memoryLimitButton, _memoryLabel });

            _bookmarksBar.Dock = DockStyle.Fill;
            _bookmarksBar.WrapContents = false;
            _bookmarksBar.AutoScroll = false;
            _bookmarksBar.BackColor = Theme.Topbar;
            _bookmarksBar.Padding = new Padding(0, 2, 0, 0);
            top.Controls.Add(_bookmarksBar, 0, 2);

            _tabs.Dock = DockStyle.Fill;
            _tabs.Appearance = TabAppearance.FlatButtons;
            _tabs.SizeMode = TabSizeMode.Fixed;
            _tabs.ItemSize = new Size(0, 1);
            _tabs.Padding = new Point(0, 0);
            _tabs.BackColor = Theme.Window;
            root.Controls.Add(_tabs, 1, 1);

            _status.Dock = DockStyle.Fill;
            _status.Padding = new Padding(12, 5, 12, 0);
            _status.BackColor = Theme.Topbar;
            _status.ForeColor = Theme.Muted;
            _status.Font = new Font("Segoe UI", 8.25f);
            root.Controls.Add(_status, 1, 2);

            WireEvents();
            ApplyResponsiveMode();
            RebuildTabStrip();
        }

        private void WireEvents()
        {
            _back.Click += delegate
            {
                WebView2 web = ActiveWebView();
                if (web != null && web.CanGoBack)
                {
                    web.GoBack();
                }
            };

            _forward.Click += delegate
            {
                WebView2 web = ActiveWebView();
                if (web != null && web.CanGoForward)
                {
                    web.GoForward();
                }
            };

            _reload.Click += delegate
            {
                WebView2 web = ActiveWebView();
                if (web != null)
                {
                    web.Reload();
                }
            };

            _newTab.Click += async delegate { await CreateTabAsync(HomeUrl); };
            _tabStripNewTab.Click += async delegate { await CreateTabAsync(HomeUrl); };
            _shield.Click += delegate
            {
                _adBlockEnabled = !_adBlockEnabled;
                _appSettings.AdBlockEnabled = _adBlockEnabled;
                _appSettings.Save();
                UpdateStatus();
            };
            _extensions.Click += async delegate { await ShowExtensionMenuAsync(); };
            _menuButton.Click += delegate { ShowMainMenu(); };
            _memoryLimitButton.Click += delegate
            {
                ShowGxControl();
            };

            _address.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    NavigateAddress();
                }
            };

            KeyDown += async delegate(object sender, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.T)
                {
                    e.SuppressKeyPress = true;
                    await CreateTabAsync(HomeUrl);
                }

                if (e.Control && e.KeyCode == Keys.N)
                {
                    e.SuppressKeyPress = true;
                    if (e.Shift)
                    {
                        MessageBox.Show(this, "Modo privado aun no esta implementado.", "GX Light");
                    }
                    else
                    {
                        StartNewWindow();
                    }
                }

                if (e.Control && e.KeyCode == Keys.W)
                {
                    e.SuppressKeyPress = true;
                    CloseTab(_tabs.SelectedTab);
                }

                if (e.Control && e.KeyCode == Keys.J)
                {
                    e.SuppressKeyPress = true;
                    NavigateInternal("downloads");
                }

                if (e.Control && e.KeyCode == Keys.H)
                {
                    e.SuppressKeyPress = true;
                    NavigateInternal("history");
                }

                if (e.Control && e.KeyCode == Keys.D)
                {
                    e.SuppressKeyPress = true;
                    AddCurrentBookmark();
                }

                if (e.Alt && e.KeyCode == Keys.T)
                {
                    e.SuppressKeyPress = true;
                    _activeIslandId = _nextIslandId++;
                    _islandColors[_activeIslandId] = ColorFromText(GetTabUrl(ActiveTab()));
                    await CreateTabAsync(HomeUrl);
                }

                if (e.Control && e.KeyCode == Keys.L)
                {
                    e.SuppressKeyPress = true;
                    _address.Focus();
                    _address.SelectAll();
                }

                if (e.Control && e.KeyCode == Keys.F)
                {
                    e.SuppressKeyPress = true;
                    ExecuteFind();
                }

                if (e.Alt && e.KeyCode == Keys.P)
                {
                    e.SuppressKeyPress = true;
                    NavigateInternal("settings");
                }

                if (e.KeyCode == Keys.F12)
                {
                    e.SuppressKeyPress = true;
                    WebView2 web = ActiveWebView();
                    if (web != null && web.CoreWebView2 != null)
                    {
                        web.CoreWebView2.OpenDevToolsWindow();
                    }
                }
            };

            _tabs.SelectedIndexChanged += async delegate
            {
                BrowserTab selected = ActiveTab();
                if (selected != null)
                {
                    selected.LastActiveUtc = DateTime.UtcNow;
                    if (selected.IsSuspended)
                    {
                        await RestoreSuspendedTabAsync(selected);
                    }
                }
                SyncAddress();
                RebuildTabStrip();
                ApplyTabResourcePolicy();
                UpdateStatus();
                SaveSession();
            };

            _memoryTimer.Interval = 5000;
            _memoryTimer.Tick += delegate
            {
                UpdateMemoryMonitor();
                EnforceMemoryLimit();
                EnforceLowResourceLimit();
                SuspendIdleTabs();
            };
        }

        private async Task InitializeAsync()
        {
            AppPaths.Ensure();
            _appSettings = AppSettings.Load();

            // Cargar preferencias del usuario
            _adBlockEnabled = _appSettings.AdBlockEnabled;
            _privacyFirewallEnabled = _appSettings.PrivacyFirewallEnabled;
            _passwordSavingEnabled = _appSettings.PasswordSavingEnabled;
            _gxControl.RamLimiterEnabled = _appSettings.RamLimiterEnabled;
            _gxControl.MemoryLimitMb = _appSettings.MemoryLimitMb;
            _gxControl.HardMemoryLimit = _appSettings.HardMemoryLimit;
            _gxControl.HotTabsKillerEnabled = _appSettings.HotTabsKillerEnabled;
            _gxControl.HotTabsMode = _appSettings.HotTabsMode;
            _gxControl.CpuLimiterEnabled = _appSettings.CpuLimiterEnabled;
            _gxControl.CpuLimitPercent = _appSettings.CpuLimitPercent;
            _gxControl.NetworkLimiterEnabled = _appSettings.NetworkLimiterEnabled;
            _gxControl.NetworkProfile = _appSettings.NetworkProfile;
            _gxControl.LowResourcesModeEnabled = _appSettings.LowResourcesModeEnabled;
            _gxControl.MaxActiveTabs = _appSettings.MaxActiveTabs;

            LoadBookmarks();
            LoadPasswordVault();
            RebuildBookmarksBar();
            EnsureDefaultFilters();
            _adBlocker.Load(AppPaths.Filters);

            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();
            options.AreBrowserExtensionsEnabled = true;
            options.AdditionalBrowserArguments = "--disable-features=HeavyAdPrivacyMitigations";
            _environment = await CoreWebView2Environment.CreateAsync(null, AppPaths.Profile, options);

            // Restaurar sesión previa si existe
            SessionData session = SessionManager.LoadSession();
            if (session != null && session.Tabs.Count > 0)
            {
                Logger.Info("Restoring session with " + session.Tabs.Count + " tabs.");
                if (session.X >= 0 && session.Y >= 0)
                {
                    Location = new Point(session.X, session.Y);
                    Size = new Size(session.Width, session.Height);
                }
                if (session.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                foreach (System.Collections.Generic.KeyValuePair<int, Color> kv in session.IslandColors)
                {
                    _islandColors[kv.Key] = kv.Value;
                }

                int maxIslandId = 0;
                foreach (var tab in session.Tabs)
                {
                    if (tab.IslandId > maxIslandId) maxIslandId = tab.IslandId;
                }
                _nextIslandId = maxIslandId + 1;

                for (int i = 0; i < session.Tabs.Count; i++)
                {
                    var tabData = session.Tabs[i];
                    if (tabData.IsSuspended)
                    {
                        CreateSuspendedTab(tabData.Url, tabData.Title, tabData.IslandId);
                    }
                    else
                    {
                        await CreateTabAsync(tabData.Url);
                        BrowserTab lastTab = _tabs.TabPages[_tabs.TabPages.Count - 1].Tag as BrowserTab;
                        if (lastTab != null)
                        {
                            lastTab.IslandId = tabData.IslandId;
                        }
                    }
                }

                int targetIndex = Math.Min(session.ActiveIndex, _tabs.TabPages.Count - 1);
                _tabs.SelectedIndex = Math.Max(0, targetIndex);
            }
            else
            {
                await CreateTabAsync(HomeUrl);
            }

            await ShowUpdateNoticeIfNeededAsync();
            _memoryTimer.Start();
            UpdateMemoryMonitor();
            UpdateStatus();
        }

        private async Task ShowUpdateNoticeIfNeededAsync()
        {
            _updateManifest = await UpdateManifest.LoadLatestAsync();
            if (string.Equals(_appSettings.LastSeenVersion, _updateManifest.Version, StringComparison.Ordinal))
            {
                return;
            }

            await CreateTabAsync(UpdatedUrl);
            _appSettings.LastSeenVersion = _updateManifest.Version;
            _appSettings.Save();
        }

        private async Task<BrowserTab> CreateTabAsync(string url)
        {
            TabPage page = new TabPage("Nueva pestana");
            page.BackColor = Theme.Window;

            WebView2 web = new WebView2();
            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = Theme.Window;
            page.Controls.Add(web);
            BrowserTab tab = new BrowserTab(page, web);
            tab.IslandId = _activeIslandId;
            if (tab.IslandId > 0 && !_islandColors.ContainsKey(tab.IslandId))
            {
                _islandColors[tab.IslandId] = ColorFromText(url);
            }
            page.Tag = tab;

            _tabs.TabPages.Add(page);
            _tabs.SelectedTab = page;
            RebuildTabStrip();

            await web.EnsureCoreWebView2Async(_environment);
            ConfigureWebView(page, web);
            if (!string.IsNullOrWhiteSpace(url))
            {
                Navigate(web, url);
            }

            ApplyTabResourcePolicy();
            EnforceLowResourceLimit();
            SaveSession();
            return tab;
        }

        private void ConfigureWebView(TabPage page, WebView2 web)
        {
            web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            web.CoreWebView2.Settings.AreDevToolsEnabled = true;
            web.CoreWebView2.Settings.IsStatusBarEnabled = false;
            web.CoreWebView2.Settings.IsZoomControlEnabled = true;
            web.CoreWebView2.Settings.IsPasswordAutosaveEnabled = _passwordSavingEnabled;
            web.CoreWebView2.Settings.IsGeneralAutofillEnabled = _passwordSavingEnabled;
            web.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced;
            web.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(YouTubeShieldsScript());
            web.CoreWebView2.WebResourceRequested += WebResourceRequested;
            web.CoreWebView2.WebMessageReceived += WebMessageReceived;
            web.CoreWebView2.NewWindowRequested += NewWindowRequested;
            web.CoreWebView2.DownloadStarting += DownloadStarting;
            web.CoreWebView2.NavigationStarting += NavigationStarting;
            web.CoreWebView2.NavigationCompleted += NavigationCompletedHandler;
            web.CoreWebView2.DocumentTitleChanged += DocumentTitleChangedHandler;
            web.CoreWebView2.ProcessFailed += Web_ProcessFailed;
        }

        private async void NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            CoreWebView2Deferral deferral = e.GetDeferral();
            try
            {
                if (!e.IsUserInitiated)
                {
                    e.Handled = true;
                    BrowserTab active = ActiveTab();
                    if (active != null)
                    {
                        active.BlockedRequests++;
                    }
                    UpdateStatus();
                    return;
                }

            BrowserTab newTab = await CreateTabAsync(null);
            e.NewWindow = newTab.WebView.CoreWebView2;
            }
            catch
            {
                e.Handled = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (_privacyFirewallEnabled)
            {
                string clean = _privacyFirewall.StripTrackingParameters(e.Uri);
                if (!string.Equals(clean, e.Uri, StringComparison.Ordinal))
                {
                    e.Cancel = true;
                    CoreWebView2 web = sender as CoreWebView2;
                    if (web != null)
                    {
                        web.Navigate(clean);
                    }
                    return;
                }
            }

            UpdateStatus();
        }

        private static string Base64Decode(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return "";
                byte[] bytes = Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Source) || !e.Source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string message;
            try
            {
                message = e.TryGetWebMessageAsString();
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            const string navigatePrefix = "gxlight:navigate:";
            if (message.StartsWith(navigatePrefix, StringComparison.Ordinal))
            {
                string url = message.Substring(navigatePrefix.Length);
                WebView2 web = WebViewForCore(sender as CoreWebView2) ?? ActiveWebView();
                if (web != null && web.CoreWebView2 != null)
                {
                    Navigate(web, url);
                }
                return;
            }

            const string deletePrefix = "gxlight:bookmarks:delete:";
            if (message.StartsWith(deletePrefix, StringComparison.Ordinal))
            {
                string data = message.Substring(deletePrefix.Length);
                string[] parts = data.Split('|');
                if (parts.Length >= 3)
                {
                    string title = Base64Decode(parts[0]);
                    string url = Base64Decode(parts[1]);
                    string folder = Base64Decode(parts[2]);

                    _bookmarks.RemoveAll(delegate(BookmarkEntry b)
                    {
                        return string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(b.Folder, folder, StringComparison.OrdinalIgnoreCase);
                    });

                    SaveBookmarks();
                    RebuildBookmarksBar();
                    NavigateInternal("bookmarks");
                }
                return;
            }

            const string movePrefix = "gxlight:bookmarks:move:";
            if (message.StartsWith(movePrefix, StringComparison.Ordinal))
            {
                string data = message.Substring(movePrefix.Length);
                string[] parts = data.Split('|');
                if (parts.Length >= 4)
                {
                    string title = Base64Decode(parts[0]);
                    string url = Base64Decode(parts[1]);
                    string folder = Base64Decode(parts[2]);
                    string newFolder = Base64Decode(parts[3]);

                    for (int i = 0; i < _bookmarks.Count; i++)
                    {
                        BookmarkEntry b = _bookmarks[i];
                        if (string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(b.Folder, folder, StringComparison.OrdinalIgnoreCase))
                        {
                            b.Folder = newFolder;
                            break;
                        }
                    }

                    SaveBookmarks();
                    RebuildBookmarksBar();
                    NavigateInternal("bookmarks");
                }
                return;
            }

            const string createFolderPrefix = "gxlight:bookmarks:create-folder:";
            if (message.StartsWith(createFolderPrefix, StringComparison.Ordinal))
            {
                string base64Folder = message.Substring(createFolderPrefix.Length);
                string folderName = Base64Decode(base64Folder);
                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    bool exists = false;
                    for (int i = 0; i < _bookmarks.Count; i++)
                    {
                        if (string.Equals(_bookmarks[i].Folder, folderName, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        BookmarkEntry folderPlaceholder = new BookmarkEntry();
                        folderPlaceholder.Title = folderName;
                        folderPlaceholder.Url = "";
                        folderPlaceholder.Folder = folderName;
                        folderPlaceholder.CreatedUtc = DateTime.UtcNow;
                        _bookmarks.Add(folderPlaceholder);
                        SaveBookmarks();
                        RebuildBookmarksBar();
                    }
                    NavigateInternal("bookmarks");
                }
                return;
            }
        }

        private void WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!_adBlockEnabled)
            {
                return;
            }

            Uri requestUri;
            if (!Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out requestUri))
            {
                return;
            }

            WebView2 active = ActiveWebView();
            Uri documentUri = null;
            if (active != null && active.Source != null)
            {
                documentUri = active.Source;
            }

            bool shouldBlock = _adBlocker.ShouldBlock(requestUri, documentUri);
            if (!shouldBlock && _privacyFirewallEnabled)
            {
                shouldBlock = _privacyFirewall.ShouldBlock(requestUri, documentUri);
            }

            if (!shouldBlock)
            {
                return;
            }

            BrowserTab tab = ActiveTab();
            if (tab != null)
            {
                tab.BlockedRequests++;
            }

            e.Response = _environment.CreateWebResourceResponse(
                new MemoryStream(new byte[0]),
                403,
                "Blocked",
                "Content-Type: text/plain");
            UpdateStatus();
        }

        private void DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            string downloads = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            string result = Path.Combine(downloads, e.DownloadOperation.ResultFilePath == null
                ? "download"
                : Path.GetFileName(e.DownloadOperation.ResultFilePath));
            e.ResultFilePath = result;
            e.Handled = false;

            DownloadEntry entry = new DownloadEntry();
            entry.FileName = Path.GetFileName(result);
            entry.Path = result;
            entry.Uri = e.DownloadOperation.Uri;
            entry.StartedUtc = DateTime.UtcNow;
            entry.State = "Downloading";
            _downloads.Insert(0, entry);

            e.DownloadOperation.StateChanged += delegate
            {
                entry.State = e.DownloadOperation.State.ToString();
            };
        }

        private void AddHistoryEntry(TabPage page, WebView2 web)
        {
            if (web == null || web.Source == null)
            {
                return;
            }

            string url = web.Source.ToString();
            if (url == "about:blank" || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HistoryEntry entry = new HistoryEntry();
            entry.Title = string.IsNullOrWhiteSpace(page.Text) ? url : page.Text;
            entry.Url = url;
            entry.VisitedUtc = DateTime.UtcNow;
            _history.Insert(0, entry);

            BrowserTab tab = page.Tag as BrowserTab;
            if (tab != null)
            {
                tab.LastActiveUtc = DateTime.UtcNow;
            }

            while (_history.Count > 500)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        private void NavigateAddress()
        {
            NavigateActive(_address.Text);
        }

        private void NavigateActive(string url)
        {
            WebView2 web = ActiveWebView();
            if (web == null)
            {
                return;
            }

            Navigate(web, NormalizeInput(url));
        }

        private void Navigate(WebView2 web, string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            string normalized = NormalizeInput(url);
            if (normalized.StartsWith("gxlight://", StringComparison.OrdinalIgnoreCase))
            {
                string pageName = normalized.Substring("gxlight://".Length).Trim().ToLowerInvariant();
                if (pageName == "home")
                {
                    web.NavigateToString(HomeHtml());
                    _address.Text = HomeUrl;
                }
                else if (pageName == "novedades")
                {
                    web.NavigateToString(UpdateNoticeHtml());
                    _address.Text = UpdatedUrl;
                }
                else if (pageName == "extensions")
                {
                    Task dummy = NavigateExtensionsPageAsync();
                }
                else if (pageName == "free-memory")
                {
                    FreeMemoryNow();
                    NavigateInternal("memory");
                }
                else
                {
                    NavigateInternal(pageName);
                }
                return;
            }

            web.CoreWebView2.Navigate(normalized);
        }

        private async void NavigateInternal(string pageName)
        {
            BrowserTab tab = ActiveTab();
            if (tab == null)
            {
                tab = await CreateTabAsync(HomeUrl);
            }

            if (tab.IsSuspended)
            {
                await RestoreSuspendedTabAsync(tab);
            }

            if (tab.WebView == null || tab.WebView.CoreWebView2 == null)
            {
                return;
            }

            tab.Page.Text = PageTitle(pageName);
            tab.WebView.NavigateToString(InternalPageHtml(pageName));
            _address.Text = "gxlight://" + pageName;
            RebuildTabStrip();
        }

        private async Task NavigateExtensionsPageAsync()
        {
            BrowserTab tab = ActiveTab();
            if (tab == null)
            {
                tab = await CreateTabAsync(HomeUrl);
            }

            if (tab.IsSuspended)
            {
                await RestoreSuspendedTabAsync(tab);
            }

            string html = await ExtensionsPageHtmlAsync();
            tab.Page.Text = "Extensions";
            tab.WebView.NavigateToString(html);
            _address.Text = "gxlight://extensions";
            RebuildTabStrip();
        }

        private static string PageTitle(string pageName)
        {
            switch (pageName)
            {
                case "history": return "History";
                case "downloads": return "Downloads";
                case "passwords": return "Passwords";
                case "bookmarks": return "Bookmarks";
                case "memory": return "Memory";
                case "shields": return "Shields";
                case "settings": return "Settings";
                default: return "GX Light";
            }
        }

        private async Task ShowExtensionMenuAsync()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;
            menu.Items.Add("Importar extension desempaquetada...", null, async delegate { await ImportExtensionAsync(); });
            menu.Items.Add("Ver extensiones instaladas", null, async delegate { await ShowExtensionsAsync(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Abrir Chrome Web Store", null, delegate { NavigateActive(ChromeStoreUrl); });
            menu.Show(_extensions, new Point(0, _extensions.Height + 4));
            await Task.FromResult(0);
        }

        private void ShowMainMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;
            menu.Items.Add("New tab                                      Ctrl+T", null, async delegate { await CreateTabAsync(HomeUrl); });
            menu.Items.Add("New tab in tab island                       Alt+T", null, async delegate
            {
                _activeIslandId = _nextIslandId++;
                _islandColors[_activeIslandId] = ColorFromText(GetTabUrl(ActiveTab()));
                await CreateTabAsync(HomeUrl);
            });
            menu.Items.Add("New window                                  Ctrl+N", null, delegate { StartNewWindow(); });
            menu.Items.Add("New private window                          Ctrl+Shift+N", null, delegate { MessageBox.Show(this, "Modo privado aun no esta implementado.", "GX Light"); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("History                                     Ctrl+H", null, delegate { NavigateInternal("history"); });
            menu.Items.Add("Downloads                                   Ctrl+J", null, delegate { NavigateInternal("downloads"); });
            ToolStripMenuItem bookmarks = new ToolStripMenuItem("Bookmarks");
            bookmarks.DropDownItems.Add("Add current page                            Ctrl+D", null, delegate { AddCurrentBookmark(); });
            bookmarks.DropDownItems.Add("Manage bookmarks", null, delegate { NavigateInternal("bookmarks"); });
            bookmarks.DropDownItems.Add(new ToolStripSeparator());
            bookmarks.DropDownItems.Add("Import bookmarks HTML...", null, delegate { ImportBookmarks(); });
            bookmarks.DropDownItems.Add("Export bookmarks HTML...", null, delegate { ExportBookmarks(); });
            menu.Items.Add(bookmarks);
            menu.Items.Add("Extensions", null, async delegate { await NavigateExtensionsPageAsync(); });
            ToolStripMenuItem passwords = new ToolStripMenuItem("Passwords and autofill");
            passwords.DropDownItems.Add("Password settings", null, delegate { NavigateInternal("passwords"); });
            passwords.DropDownItems.Add("Import passwords CSV...", null, delegate { ImportPasswords(); });
            passwords.DropDownItems.Add("Export passwords CSV...", null, delegate { ExportPasswords(); });
            passwords.DropDownItems.Add("Export CSV template...", null, delegate { ExportPasswordTemplate(); });
            menu.Items.Add(passwords);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Suspend inactive tabs now", null, delegate { SuspendIdleTabsNow(); });
            menu.Items.Add("GX Control / limiters", null, delegate { ShowGxControl(); });
            menu.Items.Add("Memory monitor", null, delegate { NavigateInternal("memory"); });
            menu.Items.Add("Shields / Privacy Firewall", null, delegate { NavigateInternal("shields"); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Find...                                     Ctrl+F", null, delegate { ExecuteFind(); });
            menu.Items.Add("Settings                                    Alt+P", null, delegate { NavigateInternal("settings"); });
            menu.Items.Add("Update notes                                v" + _updateManifest.Version, null, delegate { NavigateActive(UpdatedUrl); });
            menu.Items.Add("Developer tools                             F12", null, delegate
            {
                WebView2 web = ActiveWebView();
                if (web != null && web.CoreWebView2 != null)
                {
                    web.CoreWebView2.OpenDevToolsWindow();
                }
            });
            menu.Items.Add("Exit", null, delegate { Close(); });
            menu.Show(_menuButton, new Point(0, _menuButton.Height + 4));
        }

        private void ShowGxControl()
        {
            using (GxControlDialog dialog = new GxControlDialog(_gxControl))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _appSettings.RamLimiterEnabled = _gxControl.RamLimiterEnabled;
                    _appSettings.MemoryLimitMb = _gxControl.MemoryLimitMb;
                    _appSettings.HardMemoryLimit = _gxControl.HardMemoryLimit;
                    _appSettings.HotTabsKillerEnabled = _gxControl.HotTabsKillerEnabled;
                    _appSettings.HotTabsMode = _gxControl.HotTabsMode;
                    _appSettings.CpuLimiterEnabled = _gxControl.CpuLimiterEnabled;
                    _appSettings.CpuLimitPercent = _gxControl.CpuLimitPercent;
                    _appSettings.NetworkLimiterEnabled = _gxControl.NetworkLimiterEnabled;
                    _appSettings.NetworkProfile = _gxControl.NetworkProfile;
                    _appSettings.LowResourcesModeEnabled = _gxControl.LowResourcesModeEnabled;
                    _appSettings.MaxActiveTabs = _gxControl.MaxActiveTabs;
                    _appSettings.Save();

                    EnforceMemoryLimit();
                    EnforceLowResourceLimit();
                    UpdateMemoryMonitor();
                }
            }
        }

        private void StartNewWindow()
        {
            try
            {
                Process.Start(Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "No se pudo abrir otra ventana");
            }
        }

        private void ExecuteFind()
        {
            WebView2 web = ActiveWebView();
            if (web != null && web.CoreWebView2 != null)
            {
                web.CoreWebView2.ExecuteScriptAsync("window.find(prompt('Buscar en la pagina') || '')");
            }
        }

        private async Task ImportExtensionAsync()
        {
            BrowserTab tab = ActiveTab();
            if (tab == null || tab.WebView.CoreWebView2 == null)
            {
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecciona una extension desempaquetada o una carpeta de extension de Chrome/Edge.";
                string chromePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "User Data", "Default", "Extensions");
                if (Directory.Exists(chromePath))
                {
                    dialog.SelectedPath = chromePath;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    string imported = await _extensionImporter.ImportAsync(tab.WebView.CoreWebView2.Profile, dialog.SelectedPath);
                    MessageBox.Show(this, "Extension cargada: " + imported, "GX Light Browser");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudo cargar la extension");
                }
            }
        }

        private async Task ShowExtensionsAsync()
        {
            BrowserTab tab = ActiveTab();
            if (tab == null || tab.WebView.CoreWebView2 == null)
            {
                return;
            }

            System.Collections.Generic.IReadOnlyList<CoreWebView2BrowserExtension> extensions =
                await tab.WebView.CoreWebView2.Profile.GetBrowserExtensionsAsync();
            if (extensions.Count == 0)
            {
                MessageBox.Show(this, "No hay extensiones cargadas todavia. Abre Extensions > Importar extension desempaquetada.", "Extensiones");
                return;
            }

            string text = string.Empty;
            for (int i = 0; i < extensions.Count; i++)
            {
                text += extensions[i].Name + " - " + (extensions[i].IsEnabled ? "activa" : "desactivada") + Environment.NewLine;
            }

            MessageBox.Show(this, text, "Extensiones");
        }

        private void AddSideButton(Control parent, string label, string tooltip, string url)
        {
            ChromeButton button = new ChromeButton();
            ConfigureButton(button, label, 46, tooltip);
            button.Height = 38;
            button.Left = 8;
            button.Top = 12 + parent.Controls.Count * 46;
            button.Click += delegate { NavigateActive(url); };
            button.MouseUp += async delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    await CreateTabAsync(url);
                }
            };
            parent.Controls.Add(button);
        }

        private void ConfigureButton(ChromeButton button, string text, int width, string tooltip)
        {
            button.Text = text;
            button.Width = width;
            button.Height = 32;
            button.Margin = new Padding(2, 2, 4, 2);
            button.TabStop = false;
            _tips.SetToolTip(button, tooltip);
        }

        private void LoadBookmarks()
        {
            _bookmarks.Clear();
            if (!File.Exists(AppPaths.Bookmarks))
            {
                return;
            }

            string[] lines = File.ReadAllLines(AppPaths.Bookmarks, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('\t');
                if (parts.Length < 3)
                {
                    continue;
                }

                BookmarkEntry entry = new BookmarkEntry();
                entry.Title = DecodeField(parts[0]);
                entry.Url = DecodeField(parts[1]);
                entry.Folder = DecodeField(parts[2]);
                entry.CreatedUtc = parts.Length > 3 ? new DateTime(ParseLong(parts[3], DateTime.UtcNow.Ticks), DateTimeKind.Utc) : DateTime.UtcNow;
                _bookmarks.Add(entry);
            }
        }

        private void SaveBookmarks()
        {
            AppPaths.Ensure();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                BookmarkEntry entry = _bookmarks[i];
                builder.Append(EncodeField(entry.Title)).Append('\t')
                    .Append(EncodeField(entry.Url)).Append('\t')
                    .Append(EncodeField(entry.Folder)).Append('\t')
                    .Append(entry.CreatedUtc.Ticks).AppendLine();
            }
            File.WriteAllText(AppPaths.Bookmarks, builder.ToString(), Encoding.UTF8);
        }

        private void ShowFolderDropdownMenu(string folderName, Control owner)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;

            List<BookmarkEntry> folderItems = new List<BookmarkEntry>();
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                BookmarkEntry entry = _bookmarks[i];
                if (string.Equals(entry.Folder, folderName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(entry.Url))
                {
                    folderItems.Add(entry);
                }
            }

            if (folderItems.Count == 0)
            {
                menu.Items.Add("(Vacio)").Enabled = false;
            }
            else
            {
                for (int i = 0; i < folderItems.Count; i++)
                {
                    BookmarkEntry entry = folderItems[i];
                    string label = Trim(string.IsNullOrWhiteSpace(entry.Title) ? entry.Url : entry.Title, 30);
                    ToolStripMenuItem item = new ToolStripMenuItem(label);
                    item.Click += delegate { NavigateActive(entry.Url); };
                    menu.Items.Add(item);
                }
            }
            menu.Show(owner, new Point(0, owner.Height + 4));
        }

        private void ShowFolderContextMenu(string folderName, Control owner)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;

            menu.Items.Add("Eliminar carpeta y su contenido", null, delegate
            {
                if (MessageBox.Show(this, "¿Seguro que deseas eliminar la carpeta '" + folderName + "' y todos sus favoritos?", "Eliminar carpeta", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _bookmarks.RemoveAll(delegate(BookmarkEntry b)
                    {
                        return string.Equals(b.Folder, folderName, StringComparison.OrdinalIgnoreCase);
                    });
                    SaveBookmarks();
                    RebuildBookmarksBar();
                }
            });
            menu.Show(owner, new Point(0, owner.Height + 4));
        }

        private void RebuildBookmarksBar()
        {
            _bookmarksBar.SuspendLayout();
            _bookmarksBar.Controls.Clear();

            List<string> folders = new List<string>();
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                string f = _bookmarks[i].Folder;
                if (!string.IsNullOrWhiteSpace(f) &&
                    !string.Equals(f, "Favorites bar", StringComparison.OrdinalIgnoreCase) &&
                    !folders.Contains(f))
                {
                    folders.Add(f);
                }
            }

            for (int i = 0; i < folders.Count; i++)
            {
                string folderName = folders[i];
                ChromeButton folderBtn = new ChromeButton();
                ConfigureButton(folderBtn, "📁 " + Trim(folderName, 15), 110, "Carpeta: " + folderName);
                folderBtn.Height = 24;
                folderBtn.Margin = new Padding(0, 1, 6, 1);
                folderBtn.Click += delegate { ShowFolderDropdownMenu(folderName, folderBtn); };
                folderBtn.MouseUp += delegate(object sender, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        ShowFolderContextMenu(folderName, folderBtn);
                    }
                };
                _bookmarksBar.Controls.Add(folderBtn);
            }

            List<BookmarkEntry> rootBookmarks = new List<BookmarkEntry>();
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                BookmarkEntry entry = _bookmarks[i];
                if ((string.IsNullOrWhiteSpace(entry.Folder) || string.Equals(entry.Folder, "Favorites bar", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(entry.Url))
                {
                    rootBookmarks.Add(entry);
                }
            }

            if (folders.Count == 0 && rootBookmarks.Count == 0)
            {
                ChromeButton add = new ChromeButton();
                ConfigureButton(add, "+ Bookmark", 100, "Guardar pagina actual en favoritos");
                add.Height = 24;
                add.Margin = new Padding(0, 1, 6, 1);
                add.Click += delegate { AddCurrentBookmark(); };
                _bookmarksBar.Controls.Add(add);
                _bookmarksBar.ResumeLayout();
                return;
            }

            int limit = Math.Min(rootBookmarks.Count, Width < 900 ? 5 : 10);
            for (int i = 0; i < limit; i++)
            {
                BookmarkEntry entry = rootBookmarks[i];
                ChromeButton button = new ChromeButton();
                ConfigureButton(button, Trim(string.IsNullOrWhiteSpace(entry.Title) ? entry.Url : entry.Title, 18), Width < 900 ? 92 : 132, entry.Url);
                button.Height = 24;
                button.Margin = new Padding(0, 1, 6, 1);
                button.Click += delegate { NavigateActive(entry.Url); };
                button.MouseUp += async delegate(object sender, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        await CreateTabAsync(entry.Url);
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        ShowBookmarkContextMenu(entry, button);
                    }
                };
                _bookmarksBar.Controls.Add(button);
            }

            ChromeButton manage = new ChromeButton();
            ConfigureButton(manage, "...", 34, "Administrar favoritos");
            manage.Height = 24;
            manage.Margin = new Padding(0, 1, 6, 1);
            manage.Click += delegate { NavigateInternal("bookmarks"); };
            _bookmarksBar.Controls.Add(manage);

            _bookmarksBar.ResumeLayout();
        }

        private void ShowBookmarkContextMenu(BookmarkEntry entry, Control owner)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;
            menu.Items.Add("Open", null, delegate { NavigateActive(entry.Url); });
            menu.Items.Add("Open in new tab", null, async delegate { await CreateTabAsync(entry.Url); });
            menu.Items.Add("Copy address", null, delegate { Clipboard.SetText(entry.Url); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Remove bookmark", null, delegate
            {
                _bookmarks.Remove(entry);
                SaveBookmarks();
                RebuildBookmarksBar();
            });
            menu.Show(owner, new Point(0, owner.Height + 4));
        }

        private void AddCurrentBookmark()
        {
            BrowserTab tab = ActiveTab();
            string url = GetTabUrl(tab);
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            for (int i = 0; i < _bookmarks.Count; i++)
            {
                if (string.Equals(_bookmarks[i].Url, url, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "Ese favorito ya existe.", "Bookmarks");
                    return;
                }
            }

            BookmarkEntry entry = new BookmarkEntry();
            entry.Title = GetTabTitle(tab);
            entry.Url = url;
            entry.Folder = "Favorites bar";
            entry.CreatedUtc = DateTime.UtcNow;
            _bookmarks.Insert(0, entry);
            SaveBookmarks();
            RebuildBookmarksBar();
            MessageBox.Show(this, "Favorito guardado en la barra.", "Bookmarks");
        }

        private void ImportBookmarks()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Importar bookmarks";
                dialog.Filter = "Bookmarks HTML (*.html;*.htm)|*.html;*.htm|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    int added = ImportBookmarksFromHtml(File.ReadAllText(dialog.FileName, Encoding.UTF8));
                    SaveBookmarks();
                    RebuildBookmarksBar();
                    MessageBox.Show(this, "Bookmarks importados: " + added, "Bookmarks");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudieron importar bookmarks");
                }
            }
        }

        private void ExportBookmarks()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Exportar bookmarks";
                dialog.Filter = "Bookmarks HTML (*.html)|*.html";
                dialog.FileName = "gx-light-bookmarks.html";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, BuildBookmarksHtml(), Encoding.UTF8);
                    MessageBox.Show(this, "Bookmarks exportados.", "Bookmarks");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudieron exportar bookmarks");
                }
            }
        }

        private int ImportBookmarksFromHtml(string html)
        {
            int added = 0;
            Stack<string> folders = new Stack<string>();
            string pendingFolder = null;

            Regex folderRegex = new Regex("<H3[^>]*>(?<title>.*?)</H3>", RegexOptions.IgnoreCase);
            Regex linkRegex = new Regex("<A\\s+[^>]*HREF\\s*=\\s*(\"|')(?<url>.*?)\\1[^>]*>(?<title>.*?)</A>", RegexOptions.IgnoreCase);
            string[] lines = html.Replace("\r", string.Empty).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                Match folderMatch = folderRegex.Match(line);
                if (folderMatch.Success)
                {
                    pendingFolder = CleanImportedText(folderMatch.Groups["title"].Value);
                }

                if (line.IndexOf("<DL", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string folderToPush = pendingFolder ?? (folders.Count == 0 ? "Favorites bar" : folders.Peek());
                    folders.Push(folderToPush);
                    pendingFolder = null;
                }

                if (line.IndexOf("</DL", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (folders.Count > 0)
                    {
                        folders.Pop();
                    }
                }

                Match linkMatch = linkRegex.Match(line);
                if (linkMatch.Success)
                {
                    string url = WebUtility.HtmlDecode(linkMatch.Groups["url"].Value);
                    if (string.IsNullOrWhiteSpace(url) || BookmarkExists(url))
                    {
                        continue;
                    }

                    BookmarkEntry entry = new BookmarkEntry();
                    entry.Url = url;
                    entry.Title = CleanImportedText(linkMatch.Groups["title"].Value);
                    entry.Folder = folders.Count > 0 ? folders.Peek() : "Imported";
                    entry.CreatedUtc = DateTime.UtcNow;
                    _bookmarks.Add(entry);
                    added++;
                }
            }

            return added;
        }

        private string BuildBookmarksHtml()
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE NETSCAPE-Bookmark-file-1>");
            html.AppendLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">");
            html.AppendLine("<TITLE>GX Light Bookmarks</TITLE>");
            html.AppendLine("<H1>GX Light Bookmarks</H1>");
            html.AppendLine("<DL><p>");
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                BookmarkEntry entry = _bookmarks[i];
                long unix = (long)(entry.CreatedUtc - new DateTime(1970, 1, 1)).TotalSeconds;
                html.Append("    <DT><A HREF=\"").Append(EscapeHtml(entry.Url)).Append("\" ADD_DATE=\"")
                    .Append(unix).Append("\">").Append(EscapeHtml(entry.Title)).AppendLine("</A>");
            }
            html.AppendLine("</DL><p>");
            return html.ToString();
        }

        private bool BookmarkExists(string url)
        {
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                if (string.Equals(_bookmarks[i].Url, url, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadPasswordVault()
        {
            _passwordVault.Clear();
            if (!File.Exists(AppPaths.PasswordVault))
            {
                return;
            }

            string[] lines = File.ReadAllLines(AppPaths.PasswordVault, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    byte[] protectedBytes = Convert.FromBase64String(lines[i]);
                    byte[] clearBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                    string csv = Encoding.UTF8.GetString(clearBytes);
                    string[] parts = SplitCsvLine(csv);
                    if (parts.Length >= 4)
                    {
                        PasswordVaultEntry entry = new PasswordVaultEntry();
                        entry.Name = parts[0];
                        entry.Url = parts[1];
                        entry.Username = parts[2];
                        entry.Password = parts[3];
                        entry.Note = parts.Length > 4 ? parts[4] : string.Empty;
                        entry.ImportedUtc = parts.Length > 5 ? new DateTime(ParseLong(parts[5], DateTime.UtcNow.Ticks), DateTimeKind.Utc) : DateTime.UtcNow;
                        _passwordVault.Add(entry);
                    }
                }
                catch
                {
                }
            }
        }

        private void SavePasswordVault()
        {
            AppPaths.Ensure();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _passwordVault.Count; i++)
            {
                PasswordVaultEntry entry = _passwordVault[i];
                string csv = CsvEscape(entry.Name) + "," + CsvEscape(entry.Url) + "," + CsvEscape(entry.Username) + "," +
                    CsvEscape(entry.Password) + "," + CsvEscape(entry.Note) + "," + entry.ImportedUtc.Ticks.ToString();
                byte[] clearBytes = Encoding.UTF8.GetBytes(csv);
                byte[] protectedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
                builder.AppendLine(Convert.ToBase64String(protectedBytes));
            }
            File.WriteAllText(AppPaths.PasswordVault, builder.ToString(), Encoding.UTF8);
        }

        private void ImportPasswords()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Importar passwords CSV";
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    string[] lines = File.ReadAllLines(dialog.FileName, Encoding.UTF8);
                    int added = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                        {
                            continue;
                        }

                        string[] parts = SplitCsvLine(lines[i]);
                        if (parts.Length < 4 || IsPasswordHeader(parts))
                        {
                            continue;
                        }

                        PasswordVaultEntry entry = new PasswordVaultEntry();
                        entry.Name = parts[0];
                        entry.Url = parts[1];
                        entry.Username = parts[2];
                        entry.Password = parts[3];
                        entry.Note = parts.Length > 4 ? parts[4] : string.Empty;
                        entry.ImportedUtc = DateTime.UtcNow;
                        _passwordVault.Add(entry);
                        added++;
                    }

                    SavePasswordVault();
                    MessageBox.Show(this, "Passwords importadas a la boveda local: " + added + Environment.NewLine +
                        "Nota: WebView2 no permite inyectarlas directo al gestor nativo/autofill.", "Passwords");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudieron importar passwords");
                }
            }
        }

        private void ExportPasswords()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Exportar passwords CSV";
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = "gx-light-passwords.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("name,url,username,password,note");
                    for (int i = 0; i < _passwordVault.Count; i++)
                    {
                        PasswordVaultEntry entry = _passwordVault[i];
                        csv.Append(CsvEscape(entry.Name)).Append(',')
                            .Append(CsvEscape(entry.Url)).Append(',')
                            .Append(CsvEscape(entry.Username)).Append(',')
                            .Append(CsvEscape(entry.Password)).Append(',')
                            .Append(CsvEscape(entry.Note)).AppendLine();
                    }
                    File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show(this, "Passwords exportadas. Este CSV contiene secretos en texto visible.", "Passwords");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "No se pudieron exportar passwords");
                }
            }
        }

        private void ExportPasswordTemplate()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Exportar plantilla CSV";
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = "gx-light-password-template.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                File.WriteAllText(dialog.FileName, "name,url,username,password,note" + Environment.NewLine, Encoding.UTF8);
                MessageBox.Show(this, "Plantilla CSV exportada.", "Passwords");
            }
        }

        private static bool IsPasswordHeader(string[] parts)
        {
            return parts.Length >= 4 &&
                string.Equals(parts[0], "name", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(parts[1], "url", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(parts[2], "username", StringComparison.OrdinalIgnoreCase);
        }

        private void RebuildTabStrip()
        {
            _tabStrip.SuspendLayout();
            _tabStrip.Controls.Clear();

            int width = CalculateTabWidth();
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                TabPage page = _tabs.TabPages[i];
                BrowserTab browserTab = page.Tag as BrowserTab;
                ChromeButton tab = new ChromeButton();
                tab.Text = Trim(page.Text, Width < 900 ? 16 : 24);
                tab.Width = width;
                tab.Height = 28;
                tab.Margin = new Padding(0, 1, 6, 3);
                tab.IsSelected = page == _tabs.SelectedTab;
                tab.ShowCloseGlyph = true;
                if (browserTab != null && browserTab.IsSelectedForIsland)
                {
                    tab.Accent = Theme.Warning;
                }
                if (browserTab != null && browserTab.IslandId > 0)
                {
                    tab.ShowIslandStripe = true;
                    tab.IslandColor = GetIslandColor(browserTab);
                    tab.Accent = tab.IslandColor;
                }
                int index = i;
                tab.MouseUp += delegate(object sender, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && tab.IsCloseHit(e.Location)))
                    {
                        CloseTab(page);
                        return;
                    }

                    if (e.Button == MouseButtons.Left && index < _tabs.TabPages.Count)
                    {
                        if ((ModifierKeys & Keys.Control) == Keys.Control && browserTab != null)
                        {
                            browserTab.IsSelectedForIsland = !browserTab.IsSelectedForIsland;
                            RebuildTabStrip();
                            return;
                        }

                        _tabs.SelectedIndex = index;
                        return;
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        ShowTabContextMenu(page, tab);
                    }
                };
                _tips.SetToolTip(tab, page.Text);
                _tabStrip.Controls.Add(tab);
            }

            ConfigureButton(_tabStripNewTab, "+", 34, "Nueva pestana");
            _tabStripNewTab.Height = 28;
            _tabStripNewTab.Margin = new Padding(0, 1, 6, 3);
            _tabStrip.Controls.Add(_tabStripNewTab);

            _tabStrip.ResumeLayout();
        }

        private void ShowTabContextMenu(TabPage page, Control owner)
        {
            BrowserTab tab = page.Tag as BrowserTab;
            if (tab == null)
            {
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;
            menu.Items.Add("New tab                                      Ctrl+T", null, async delegate { await CreateTabAsync(HomeUrl); });
            menu.Items.Add("Create tab island from selected             Alt+T", null, delegate { CreateIslandFromSelection(tab); });
            menu.Items.Add(tab.IsSelectedForIsland ? "Deselect tab" : "Select tab", null, delegate
            {
                tab.IsSelectedForIsland = !tab.IsSelectedForIsland;
                RebuildTabStrip();
            });
            menu.Items.Add("Clear tab selection", null, delegate { ClearTabSelection(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Reload selected tabs", null, async delegate { await ReloadSelectedTabsAsync(tab); });
            menu.Items.Add("Copy page addresses", null, delegate { CopySelectedTabAddresses(tab); });
            menu.Items.Add("Duplicate selected tabs", null, async delegate { await DuplicateSelectedTabsAsync(tab); });
            menu.Items.Add(tab.IsPinned ? "Unpin tab" : "Pin tab", null, delegate
            {
                tab.IsPinned = !tab.IsPinned;
                page.Text = tab.IsPinned ? "[Pinned] " + Trim(GetTabTitle(tab), 18) : GetTabTitle(tab);
                RebuildTabStrip();
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Close selected tabs                          Ctrl+W", null, delegate { CloseSelectedTabs(tab); });
            menu.Items.Add("Close tabs to the right", null, delegate { CloseTabsToRight(page); });
            menu.Show(owner, new Point(0, owner.Height + 4));
        }

        private List<BrowserTab> SelectedTabsOr(BrowserTab fallback)
        {
            List<BrowserTab> selected = new List<BrowserTab>();
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && tab.IsSelectedForIsland)
                {
                    selected.Add(tab);
                }
            }

            if (selected.Count == 0 && fallback != null)
            {
                selected.Add(fallback);
            }

            return selected;
        }

        private void CreateIslandFromSelection(BrowserTab fallback)
        {
            List<BrowserTab> selected = SelectedTabsOr(fallback);
            if (selected.Count == 0)
            {
                return;
            }

            int islandId = _nextIslandId++;
            Color color = ColorFromText(GetTabUrl(selected[0]));
            _islandColors[islandId] = color;
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].IslandId = islandId;
                selected[i].IsSelectedForIsland = false;
            }
            _activeIslandId = islandId;
            RebuildTabStrip();
        }

        private async Task ReloadSelectedTabsAsync(BrowserTab fallback)
        {
            List<BrowserTab> selected = SelectedTabsOr(fallback);
            for (int i = 0; i < selected.Count; i++)
            {
                BrowserTab tab = selected[i];
                if (tab.IsSuspended)
                {
                    await RestoreSuspendedTabAsync(tab);
                }
                else if (tab.WebView != null)
                {
                    tab.WebView.Reload();
                }
            }
        }

        private async Task DuplicateSelectedTabsAsync(BrowserTab fallback)
        {
            List<BrowserTab> selected = SelectedTabsOr(fallback);
            for (int i = 0; i < selected.Count; i++)
            {
                await CreateTabAsync(GetTabUrl(selected[i]));
            }
        }

        private void CopySelectedTabAddresses(BrowserTab fallback)
        {
            List<BrowserTab> selected = SelectedTabsOr(fallback);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < selected.Count; i++)
            {
                builder.AppendLine(GetTabUrl(selected[i]));
            }

            if (builder.Length > 0)
            {
                Clipboard.SetText(builder.ToString());
            }
        }

        private void CloseSelectedTabs(BrowserTab fallback)
        {
            List<BrowserTab> selected = SelectedTabsOr(fallback);
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].Page != null && _tabs.TabPages.Contains(selected[i].Page))
                {
                    CloseTab(selected[i].Page);
                }
            }
        }

        private void CloseTabsToRight(TabPage page)
        {
            int index = _tabs.TabPages.IndexOf(page);
            if (index < 0)
            {
                return;
            }

            List<TabPage> toClose = new List<TabPage>();
            for (int i = index + 1; i < _tabs.TabPages.Count; i++)
            {
                toClose.Add(_tabs.TabPages[i]);
            }

            for (int i = 0; i < toClose.Count; i++)
            {
                CloseTab(toClose[i]);
            }
        }

        private void ClearTabSelection()
        {
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null)
                {
                    tab.IsSelectedForIsland = false;
                }
            }
            RebuildTabStrip();
        }

        private void CloseTab(TabPage page)
        {
            if (page == null)
            {
                return;
            }

            if (_tabs.TabPages.Count <= 1)
            {
                BrowserTab only = page.Tag as BrowserTab;
                if (only != null)
                {
                    only.BlockedRequests = 0;
                    page.Text = "Nueva pestana";
                    if (only.WebView != null)
                    {
                        Navigate(only.WebView, HomeUrl);
                    }
                    else
                    {
                        only.SuspendedTitle = "Nueva pestana";
                        only.SuspendedUrl = HomeUrl;
                    }
                    RebuildTabStrip();
                    UpdateStatus();
                }
                return;
            }

            int closedIndex = _tabs.TabPages.IndexOf(page);
            BrowserTab tab = page.Tag as BrowserTab;
            _tabs.TabPages.Remove(page);
            if (tab != null && tab.WebView != null)
            {
                UnsubscribeWebViewEvents(tab.WebView);
                tab.WebView.Dispose();
            }
            page.Dispose();

            if (_tabs.TabPages.Count > 0)
            {
                _tabs.SelectedIndex = Math.Min(Math.Max(0, closedIndex), _tabs.TabPages.Count - 1);
            }

            ApplyTabResourcePolicy();
            RebuildTabStrip();
            SyncAddress();
            UpdateStatus();
            SaveSession();
        }

        private void ApplyTabResourcePolicy()
        {
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab == null || tab.IsSuspended || tab.WebView == null || tab.WebView.CoreWebView2 == null)
                {
                    continue;
                }

                tab.WebView.CoreWebView2.MemoryUsageTargetLevel =
                    _tabs.TabPages[i] == _tabs.SelectedTab
                        ? CoreWebView2MemoryUsageTargetLevel.Normal
                        : CoreWebView2MemoryUsageTargetLevel.Low;
            }
        }

        private void UpdateMemoryMonitor()
        {
            long totalMb = EstimateBrowserMemoryMb();
            _memoryLabel.Text = totalMb + " MB";
            _memoryLimitButton.Text = _gxControl.RamLimiterEnabled ? "GX " + (_gxControl.MemoryLimitMb / 1024.0).ToString("0.0") + "G" : "GX Off";
            _memoryLabel.ForeColor = _gxControl.RamLimiterEnabled && totalMb > _gxControl.MemoryLimitMb ? Theme.Warning : Theme.Text;
        }

        private long EstimateBrowserMemoryMb()
        {
            long total = 0;
            try
            {
                Process current = Process.GetCurrentProcess();
                total += current.WorkingSet64;

                Process[] webviews = Process.GetProcessesByName("msedgewebview2");
                for (int i = 0; i < webviews.Length; i++)
                {
                    try
                    {
                        if (webviews[i].StartTime.ToUniversalTime() >= _startedUtc.AddSeconds(-10))
                        {
                            total += webviews[i].WorkingSet64;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        webviews[i].Dispose();
                    }
                }
            }
            catch
            {
                total = Process.GetCurrentProcess().WorkingSet64;
            }

            return Math.Max(1, total / (1024 * 1024));
        }

        private void EnforceMemoryLimit()
        {
            if (!_gxControl.RamLimiterEnabled)
            {
                return;
            }

            long total = EstimateBrowserMemoryMb();
            if (total <= _gxControl.MemoryLimitMb)
            {
                return;
            }

            SuspendOldestInactiveTab();
            if (_gxControl.HardMemoryLimit)
            {
                while (EstimateBrowserMemoryMb() > _gxControl.MemoryLimitMb && SuspendOldestInactiveTab())
                {
                }
            }
        }

        private void SuspendIdleTabs()
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-5);
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && _tabs.TabPages[i] != _tabs.SelectedTab && !tab.IsSuspended && tab.LastActiveUtc < cutoff)
                {
                    if (tab.WebView != null && tab.WebView.CoreWebView2 != null && tab.WebView.CoreWebView2.IsDocumentPlayingAudio)
                    {
                        continue;
                    }
                    if (_gxControl.HotTabsKillerEnabled)
                    {
                        SuspendTab(tab);
                    }
                }
            }
        }

        private void SuspendIdleTabsNow()
        {
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && _tabs.TabPages[i] != _tabs.SelectedTab && !tab.IsSuspended)
                {
                    if (tab.WebView != null && tab.WebView.CoreWebView2 != null && tab.WebView.CoreWebView2.IsDocumentPlayingAudio)
                    {
                        continue;
                    }
                    SuspendTab(tab);
                }
            }
            UpdateMemoryMonitor();
        }

        private bool SuspendOldestInactiveTab()
        {
            BrowserTab oldest = null;
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab == null || tab.IsSuspended || _tabs.TabPages[i] == _tabs.SelectedTab)
                {
                    continue;
                }

                if (tab.WebView != null && tab.WebView.CoreWebView2 != null && tab.WebView.CoreWebView2.IsDocumentPlayingAudio)
                {
                    continue;
                }

                if (oldest == null || tab.LastActiveUtc < oldest.LastActiveUtc)
                {
                    oldest = tab;
                }
            }

            if (oldest != null)
            {
                SuspendTab(oldest);
                return true;
            }

            return false;
        }

        private void SuspendTab(BrowserTab tab)
        {
            if (tab == null || tab.IsSuspended || tab.WebView == null)
            {
                return;
            }

            Logger.Info("Suspending tab: " + tab.Page.Text);
            tab.SuspendedUrl = tab.WebView.Source == null || tab.WebView.Source.ToString() == "about:blank" ? HomeUrl : tab.WebView.Source.ToString();
            tab.SuspendedTitle = tab.Page.Text.Replace("[Crashed] ", "").Replace("[Suspended] ", "");
            
            UnsubscribeWebViewEvents(tab.WebView);

            tab.Page.Controls.Clear();
            tab.WebView.Dispose();
            tab.WebView = null;
            tab.IsSuspended = true;

            SetupSuspensionPlaceholder(tab);
            tab.Page.Text = "[Suspended] " + Trim(tab.SuspendedTitle, 18);
            RebuildTabStrip();
            SaveSession();
        }

        private async Task RestoreSuspendedTabAsync(BrowserTab tab)
        {
            if (tab == null || !tab.IsSuspended)
            {
                return;
            }

            tab.Page.Controls.Clear();
            WebView2 web = new WebView2();
            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = Theme.Window;
            tab.Page.Controls.Add(web);
            tab.WebView = web;
            tab.IsSuspended = false;
            tab.LastActiveUtc = DateTime.UtcNow;

            await web.EnsureCoreWebView2Async(_environment);
            ConfigureWebView(tab.Page, web);
            Navigate(web, string.IsNullOrWhiteSpace(tab.SuspendedUrl) ? HomeUrl : tab.SuspendedUrl);
            tab.Page.Text = string.IsNullOrWhiteSpace(tab.SuspendedTitle) ? "Restored" : tab.SuspendedTitle;
            RebuildTabStrip();
            EnforceLowResourceLimit();
            SaveSession();
        }

        private int CalculateTabWidth()
        {
            int available = Math.Max(260, _tabStrip.Width - 12);
            int count = Math.Max(1, _tabs.TabPages.Count);
            int width = (available / count) - 8;
            if (width < 118)
            {
                return 118;
            }
            if (width > 190)
            {
                return 190;
            }
            return width;
        }

        private BrowserTab ActiveTab()
        {
            return _tabs.SelectedTab == null ? null : _tabs.SelectedTab.Tag as BrowserTab;
        }

        private WebView2 ActiveWebView()
        {
            BrowserTab tab = ActiveTab();
            return tab == null ? null : tab.WebView;
        }

        private WebView2 WebViewForCore(CoreWebView2 core)
        {
            if (core == null)
            {
                return null;
            }

            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && tab.WebView != null && tab.WebView.CoreWebView2 == core)
                {
                    return tab.WebView;
                }
            }

            return null;
        }

        private void SyncAddress()
        {
            BrowserTab tab = ActiveTab();
            if (tab != null && tab.IsSuspended)
            {
                _address.Text = string.IsNullOrWhiteSpace(tab.SuspendedUrl) ? HomeUrl : tab.SuspendedUrl;
                return;
            }

            WebView2 web = ActiveWebView();
            if (web != null && web.Source != null)
            {
                _address.Text = web.Source.ToString() == "about:blank" ? HomeUrl : web.Source.ToString();
            }
        }

        private void UpdateStatus()
        {
            BrowserTab tab = ActiveTab();
            int blocked = tab == null ? 0 : tab.BlockedRequests;
            _shield.Text = _adBlockEnabled ? "Shields On" : "Shields Off";
            _shield.Accent = _adBlockEnabled ? Theme.Accent : Theme.Warning;
            _shield.Invalidate();
            _status.Text = "Bloqueador " + (_adBlockEnabled ? "activo" : "pausado") +
                "   reglas: " + _adBlocker.RuleCount +
                "   firewall: " + (_privacyFirewallEnabled ? "activo" : "pausado") +
                "   reglas firewall: " + _privacyFirewall.RuleCount +
                "   bloqueadas en pestana: " + blocked;
        }

        private void ApplyResponsiveMode()
        {
            bool compact = Width < 980;
            bool veryCompact = Width < 820;

            _reload.Text = compact ? "R" : "Reload";
            _reload.Width = compact ? 38 : 66;
            _newTab.Width = 38;
            _extensions.Text = compact ? "Ext" : "Extensions";
            _extensions.Width = compact ? 58 : 98;
            _shield.Text = _adBlockEnabled ? (compact ? "Shield" : "Shields On") : (compact ? "Off" : "Shields Off");
            _shield.Width = compact ? 72 : 106;

            _chromeStore.Visible = !veryCompact;
            RebuildTabStrip();
            RebuildBookmarksBar();
        }

        private string GetTabUrl(BrowserTab tab)
        {
            if (tab == null)
            {
                return HomeUrl;
            }

            if (tab.IsSuspended)
            {
                return string.IsNullOrWhiteSpace(tab.SuspendedUrl) ? HomeUrl : tab.SuspendedUrl;
            }

            if (tab.WebView != null && tab.WebView.Source != null && tab.WebView.Source.ToString() != "about:blank")
            {
                return tab.WebView.Source.ToString();
            }

            if (!string.IsNullOrWhiteSpace(tab.SuspendedUrl))
            {
                return tab.SuspendedUrl;
            }

            return HomeUrl;
        }

        private string GetTabTitle(BrowserTab tab)
        {
            if (tab == null || tab.Page == null || string.IsNullOrWhiteSpace(tab.Page.Text))
            {
                return "GX Light";
            }

            string title = tab.Page.Text;
            if (title.StartsWith("[Suspended] ", StringComparison.Ordinal))
            {
                title = title.Substring("[Suspended] ".Length);
            }
            if (title.StartsWith("[Pinned] ", StringComparison.Ordinal))
            {
                title = title.Substring("[Pinned] ".Length);
            }

            return title;
        }

        private Color GetIslandColor(BrowserTab tab)
        {
            if (tab == null || tab.IslandId <= 0)
            {
                return Theme.Accent;
            }

            Color color;
            if (_islandColors.TryGetValue(tab.IslandId, out color))
            {
                return color;
            }

            color = ColorFromText(GetTabUrl(tab));
            _islandColors[tab.IslandId] = color;
            return color;
        }

        private static Color ColorFromText(string value)
        {
            string text = string.IsNullOrWhiteSpace(value) ? "gx-light" : value.ToLowerInvariant();
            int hash = 17;
            for (int i = 0; i < text.Length; i++)
            {
                hash = unchecked(hash * 31 + text[i]);
            }

            int palette = (hash & 0x7fffffff) % 8;
            Color[] colors = new Color[]
            {
                Color.FromArgb(255, 98, 132),
                Color.FromArgb(255, 159, 67),
                Color.FromArgb(72, 219, 251),
                Color.FromArgb(29, 209, 161),
                Color.FromArgb(254, 202, 87),
                Color.FromArgb(95, 39, 205),
                Color.FromArgb(238, 82, 83),
                Color.FromArgb(16, 172, 132)
            };
            return colors[palette];
        }

        private static string EncodeField(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string DecodeField(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static long ParseLong(string value, long fallback)
        {
            long parsed;
            return long.TryParse(value, out parsed) ? parsed : fallback;
        }

        private static string CleanImportedText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Imported";
            }

            string noTags = Regex.Replace(value, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        private static string CsvEscape(string value)
        {
            string text = value ?? string.Empty;
            if (text.IndexOf('"') >= 0 || text.IndexOf(',') >= 0 || text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0)
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> values = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Length = 0;
                    continue;
                }

                current.Append(ch);
            }

            values.Add(current.ToString());
            return values.ToArray();
        }

        private static string NormalizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return HomeUrl;
            }

            string value = input.Trim();
            if (string.Equals(value, HomeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return HomeUrl;
            }

            if (string.Equals(value, UpdatedUrl, StringComparison.OrdinalIgnoreCase))
            {
                return UpdatedUrl;
            }

            if (value.IndexOf("://", StringComparison.Ordinal) >= 0)
            {
                return value;
            }

            if (value.IndexOf('.') >= 0 && value.IndexOf(' ') < 0)
            {
                return "https://" + value;
            }

            return "https://duckduckgo.com/?q=" + Uri.EscapeDataString(value);
        }

        private static bool IsYouTubeHost(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            string value = host.ToLowerInvariant();
            return value == "youtube.com" || value.EndsWith(".youtube.com", StringComparison.Ordinal) ||
                value == "youtu.be" || value.EndsWith(".youtu.be", StringComparison.Ordinal);
        }

        private static string YouTubeShieldsScript()
        {
            return @"(() => {
  if (window.__gxLightYouTubeShieldsInstalled) return;
  window.__gxLightYouTubeShieldsInstalled = true;

  const adSelectors = [
    'ytd-ad-slot-renderer',
    'ytd-promoted-video-renderer',
    'ytd-display-ad-renderer',
    'ytd-companion-slot-renderer',
    'ytd-action-companion-ad-renderer',
    'ytd-in-feed-ad-layout-renderer',
    'ytd-player-legacy-desktop-watch-ads-renderer',
    'ytd-engagement-panel-section-list-renderer[target-id=""engagement-panel-ads""]',
    '.ytp-ad-overlay-container',
    '.ytp-ad-player-overlay',
    '.ytp-ad-image-overlay',
    '.video-ads',
    '#player-ads',
    '#masthead-ad'
  ];

  const skipSelectors = [
    '.ytp-ad-skip-button',
    '.ytp-ad-skip-button-modern',
    '.ytp-skip-ad-button',
    'button.ytp-ad-skip-button-modern',
    'button[aria-label^=""Skip""]',
    'button[aria-label^=""Saltar""]',
    'button[aria-label^=""Omitir""]'
  ];

  function clickSkips() {
    for (const selector of skipSelectors) {
      document.querySelectorAll(selector).forEach((node) => {
        if (node && typeof node.click === 'function') node.click();
      });
    }
  }

  function removeAdNodes() {
    for (const selector of adSelectors) {
      document.querySelectorAll(selector).forEach((node) => {
        if (!node || !node.parentNode) return;
        node.remove();
      });
    }
  }

  function accelerateVideoAds() {
    const player = document.querySelector('.html5-video-player');
    if (!player || !player.classList.contains('ad-showing')) return;
    document.querySelectorAll('video').forEach((video) => {
      try {
        video.muted = true;
        video.playbackRate = 16;
        if (Number.isFinite(video.duration) && video.duration > 0 && video.currentTime < video.duration - 0.35) {
          video.currentTime = Math.max(video.currentTime, video.duration - 0.25);
        }
      } catch (_) {}
    });
  }

  function run() {
    if (!/(\.|^)youtube\.com$/.test(location.hostname) && location.hostname !== 'youtu.be') return;
    clickSkips();
    removeAdNodes();
    accelerateVideoAds();
  }

  window.__gxLightRunYouTubeShields = run;
  run();
  setInterval(run, 350);
  new MutationObserver(run).observe(document.documentElement, { childList: true, subtree: true });
})();";
        }

        private string InternalPageHtml(string pageName)
        {
            switch (pageName)
            {
                case "history":
                    return HtmlShell("History", HistoryHtml());
                case "downloads":
                    return HtmlShell("Downloads", DownloadsHtml());
                case "passwords":
                    return HtmlShell("Passwords and autofill", PasswordsHtml());
                case "bookmarks":
                    return BookmarksHtml();
                case "memory":
                    return HtmlShell("Memory monitor",
                        "<p>Estimated browser memory: <b>" + EstimateBrowserMemoryMb() + " MB</b></p>" +
                        "<p>RAM limiter: <b>" + (_gxControl.RamLimiterEnabled ? (_gxControl.MemoryLimitMb / 1024.0).ToString("0.0") + " GB" : "Off") + "</b></p>" +
                        "<p>Hard limit: <b>" + (_gxControl.HardMemoryLimit ? "On" : "Off") + "</b></p>" +
                        "<p>Hot tabs killer: <b>" + (_gxControl.HotTabsKillerEnabled ? _gxControl.HotTabsMode : "Off") + "</b></p>" +
                        "<p>CPU limiter policy: <b>" + (_gxControl.CpuLimiterEnabled ? _gxControl.CpuLimitPercent + "%" : "Off") + "</b></p>" +
                        "<p>Network limiter policy: <b>" + (_gxControl.NetworkLimiterEnabled ? _gxControl.NetworkProfile : "Off") + "</b></p>" +
                        "<p>Low Resource Mode: <b>" + (_gxControl.LowResourcesModeEnabled ? "On (Max active: " + _gxControl.MaxActiveTabs + ")" : "Off") + "</b></p>" +
                        "<p>Inactive tabs are moved to low-memory mode and can be suspended/discarded to free their WebView.</p>" +
                        GetMemoryProcessesHtml());
                case "shields":
                    return HtmlShell("Shields and Privacy Firewall",
                        "<p>Ad blocker: <b>" + (_adBlockEnabled ? "enabled" : "disabled") + "</b></p>" +
                        "<p>Privacy Firewall: <b>" + (_privacyFirewallEnabled ? "enabled" : "disabled") + "</b></p>" +
                        "<p>Rules: " + _adBlocker.RuleCount + " ad rules, " + _privacyFirewall.RuleCount + " firewall rules.</p>");
                case "settings":
                    return HtmlShell("Settings",
                        "<p>Low-resource defaults are active: inactive tabs use low-memory mode, menu-first commands, and local-only privacy controls.</p>");
                default:
                    return HtmlShell("GX Light", "<p>Section not found.</p>");
            }
        }

        private async Task<string> ExtensionsPageHtmlAsync()
        {
            BrowserTab tab = ActiveTab();
            StringBuilder body = new StringBuilder();
            body.Append("<p>Import unpacked Chrome/Edge extensions from the main menu.</p>");
            body.Append("<p><a href='" + ChromeStoreUrl + "'>Chrome Web Store</a></p>");

            if (tab != null && tab.WebView != null && tab.WebView.CoreWebView2 != null)
            {
                System.Collections.Generic.IReadOnlyList<CoreWebView2BrowserExtension> extensions =
                    await tab.WebView.CoreWebView2.Profile.GetBrowserExtensionsAsync();
                if (extensions.Count == 0)
                {
                    body.Append("<p>No extensions loaded yet.</p>");
                }
                else
                {
                    body.Append("<table><tr><th>Name</th><th>Status</th><th>ID</th></tr>");
                    for (int i = 0; i < extensions.Count; i++)
                    {
                        body.Append("<tr><td>" + EscapeHtml(extensions[i].Name) + "</td><td>" +
                            (extensions[i].IsEnabled ? "Enabled" : "Disabled") + "</td><td>" +
                            EscapeHtml(extensions[i].Id) + "</td></tr>");
                    }
                    body.Append("</table>");
                }
            }

            return HtmlShell("Extensions", body.ToString());
        }

        private string HistoryHtml()
        {
            if (_history.Count == 0)
            {
                return "<p>No history in this session yet.</p>";
            }

            StringBuilder body = new StringBuilder();
            body.Append("<table><tr><th>Time</th><th>Title</th><th>URL</th></tr>");
            int count = Math.Min(100, _history.Count);
            for (int i = 0; i < count; i++)
            {
                HistoryEntry entry = _history[i];
                body.Append("<tr><td>" + entry.VisitedUtc.ToLocalTime().ToString("HH:mm") + "</td><td>" +
                    EscapeHtml(entry.Title) + "</td><td><a href='" + EscapeHtml(entry.Url) + "'>" +
                    EscapeHtml(entry.Url) + "</a></td></tr>");
            }
            body.Append("</table>");
            return body.ToString();
        }

        private string DownloadsHtml()
        {
            if (_downloads.Count == 0)
            {
                return "<p>No downloads in this session yet.</p>";
            }

            StringBuilder body = new StringBuilder();
            body.Append("<table><tr><th>Time</th><th>File</th><th>Status</th><th>Path</th></tr>");
            for (int i = 0; i < _downloads.Count; i++)
            {
                DownloadEntry entry = _downloads[i];
                body.Append("<tr><td>" + entry.StartedUtc.ToLocalTime().ToString("HH:mm") + "</td><td>" +
                    EscapeHtml(entry.FileName) + "</td><td>" + EscapeHtml(entry.State) + "</td><td>" +
                    EscapeHtml(entry.Path) + "</td></tr>");
            }
            body.Append("</table>");
            return body.ToString();
        }

        private static string SafeJsString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private string BookmarksJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < _bookmarks.Count; i++)
            {
                BookmarkEntry entry = _bookmarks[i];
                if (i > 0) sb.Append(",");
                sb.Append("{")
                  .Append("\"title\":\"").Append(SafeJsString(entry.Title)).Append("\",")
                  .Append("\"url\":\"").Append(SafeJsString(entry.Url)).Append("\",")
                  .Append("\"folder\":\"").Append(SafeJsString(entry.Folder)).Append("\",")
                  .Append("\"created\":").Append((long)(entry.CreatedUtc - new DateTime(1970, 1, 1)).TotalMilliseconds)
                  .Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string BookmarksHtml()
        {
            string json = BookmarksJson();
            return "<!doctype html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>" +
                "<title>Bookmarks Manager</title>" +
                "<style>" +
                "body{margin:0;background:#0d0f14;color:#eef7fa;font-family:Segoe UI,Arial,sans-serif;display:flex;min-height:100vh}" +
                "aside{width:240px;background:#13171f;border-right:1px solid #222936;padding:20px;display:flex;flex-direction:column;gap:15px}" +
                "aside h2{color:#72f5ff;font-size:16px;text-transform:uppercase;letter-spacing:1px;margin:0 0 10px;display:flex;align-items:center;gap:8px}" +
                ".folder-list{list-style:none;padding:0;margin:0;display:flex;flex-direction:column;gap:5px}" +
                ".folder-item{padding:8px 12px;cursor:pointer;border-radius:4px;transition:background .2s,color .2s;display:flex;align-items:center;gap:8px;color:#aeb8c4;font-size:14px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}" +
                ".folder-item:hover,.folder-item.active{background:#1c2331;color:#72f5ff}" +
                ".btn-new-folder{background:transparent;border:1px dashed #484d5c;color:#aeb8c4;padding:10px;border-radius:4px;cursor:pointer;text-align:center;font-size:13px;transition:border-color .2s,color .2s;margin-top:10px}" +
                ".btn-new-folder:hover{border-color:#72f5ff;color:#72f5ff}" +
                "main{flex:1;padding:30px;display:flex;flex-direction:column;gap:20px}" +
                ".header-bar{display:flex;justify-content:space-between;align-items:center;gap:20px;flex-wrap:wrap}" +
                "h1{color:#72f5ff;font-size:28px;margin:0}" +
                ".search-box{background:#171a22;border:1px solid #2e3440;border-radius:4px;padding:8px 14px;width:300px;display:flex;align-items:center}" +
                ".search-box input{background:transparent;border:none;color:#fff;font-size:14px;outline:none;width:100%}" +
                ".table-container{background:#13171f;border:1px solid #222936;border-radius:6px;overflow:hidden}" +
                "table{border-collapse:collapse;width:100%;text-align:left}" +
                "th,td{padding:12px 16px;border-bottom:1px solid #222936;font-size:14px}" +
                "th{background:#1c2331;color:#72f5ff;font-weight:600;text-transform:uppercase;font-size:12px;letter-spacing:.5px}" +
                "tr:last-child td{border-bottom:none}" +
                "tr:hover td{background:#171c26}" +
                "a{color:#72f5ff;text-decoration:none;word-break:break-all}" +
                "a:hover{text-decoration:underline}" +
                ".actions{display:flex;align-items:center;gap:10px}" +
                ".btn-delete{background:transparent;border:none;color:#ff6b6b;cursor:pointer;padding:4px 8px;border-radius:4px;transition:background .2s}" +
                ".btn-delete:hover{background:rgba(255,107,107,.1)}" +
                "select{background:#171a22;border:1px solid #2e3440;color:#eef7fa;padding:4px 8px;border-radius:4px;outline:none;cursor:pointer;font-size:13px}" +
                "select:hover{border-color:#72f5ff}" +
                "</style></head><body>" +
                "<aside><h2>📁 Carpetas</h2><ul class='folder-list' id='folderList'></ul><button class='btn-new-folder' onclick='createNewFolder()'>+ Nueva Carpeta</button></aside>" +
                "<main><div class='header-bar'><h1 id='pageTitle'>Todos los favoritos</h1><div class='search-box'><input type='text' id='searchInput' placeholder='Buscar favoritos...' oninput='renderBookmarks()'></div></div>" +
                "<div class='table-container'><table><thead><tr><th>Título</th><th>URL</th><th>Carpeta</th><th>Acciones</th></tr></thead>" +
                "<tbody id='bookmarksTableBody'></tbody></table></div></main>" +
                "<script>" +
                "const bookmarks=" + json + ";" +
                "let activeFilter='all';" +
                "bookmarks.forEach((b,idx)=>b.originalIndex=idx);" +
                "function getFolders(){const folders=new Set();bookmarks.forEach(b=>{if(b.folder&&b.folder.trim()!==''){folders.add(b.folder);}});return Array.from(folders);}" +
                "function renderFolders(){const folderList=document.getElementById('folderList');const uniqueFolders=getFolders();" +
                "let html='<li class=\"folder-item '+(activeFilter==='all'?'active':'')+'\" onclick=\"filterFolder(\\'all\\')\">📂 Todos</li>'+" +
                "'<li class=\"folder-item '+(activeFilter==='Favorites bar'?'active':'')+'\" onclick=\"filterFolder(\\'Favorites bar\\')\">⭐ Barra</li>';" +
                "uniqueFolders.forEach(f=>{if(f!=='Favorites bar'){" +
                "html+='<li class=\"folder-item '+(activeFilter===f?\"active\":\"\")+'\" onclick=\"filterFolder(\\''+escapeJs(f)+'\\')\">📁 '+escapeHtml(f)+'</li>';" +
                "}});" +
                "folderList.innerHTML=html;}" +
                "function filterFolder(folder){activeFilter=folder;document.getElementById('pageTitle').innerText=folder==='all'?'Todos':folder;renderFolders();renderBookmarks();}" +
                "function renderBookmarks(){const tbody=document.getElementById('bookmarksTableBody');const search=document.getElementById('searchInput').value.toLowerCase();const uniqueFolders=getFolders();" +
                "if(!uniqueFolders.includes('Favorites bar')){uniqueFolders.push('Favorites bar');}" +
                "let filtered=bookmarks;" +
                "if(activeFilter==='all'){filtered=bookmarks.filter(b=>b.url&&b.url.trim()!=='');}else{filtered=bookmarks.filter(b=>b.folder===activeFilter);}" +
                "if(search){filtered=filtered.filter(b=>(b.title&&b.title.toLowerCase().includes(search))||(b.url&&b.url.toLowerCase().includes(search)));}" +
                "let html='';" +
                "if(filtered.length===0){html='<tr><td colspan=\"4\" style=\"text-align:center;color:#aeb8c4;\">No hay favoritos en esta seccion.</td></tr>';}" +
                "else{filtered.forEach(b=>{" +
                "const isPlaceholder=!b.url||b.url.trim()==='';" +
                "const displayUrl=isPlaceholder?'<i>(Carpeta Vacia)</i>':('<a href=\"#\" onclick=\"navigate('+b.originalIndex+');return false;\">'+escapeHtml(b.url)+'</a>');" +
                "let folderOptions='';" +
                "uniqueFolders.forEach(f=>{" +
                "folderOptions+='<option value=\"'+escapeHtml(f)+'\" '+(b.folder===f?'selected':'')+'>'+escapeHtml(f)+'</option>';" +
                "});" +
                "html+='<tr><td style=\"font-weight:500;\">'+escapeHtml(b.title||'Sin titulo')+'</td><td>'+displayUrl+'</td><td><select onchange=\"moveBookmark('+b.originalIndex+',this.value)\">'+folderOptions+'</select></td><td><div class=\"actions\"><button class=\"btn-delete\" onclick=\"deleteBookmark('+b.originalIndex+')\">Eliminar</button></div></td></tr>';" +
                "});}" +
                "tbody.innerHTML=html;}" +
                "function navigate(idx){window.chrome.webview.postMessage('gxlight:navigate:'+bookmarks[idx].url);}" +
                "function createNewFolder(){const name=prompt('Nombre de la nueva carpeta:');if(name&&name.trim()!==''){" +
                "const base64Folder=btoa(unescape(encodeURIComponent(name.trim())));" +
                "window.chrome.webview.postMessage('gxlight:bookmarks:create-folder:'+base64Folder);" +
                "}}" +
                "function deleteBookmark(idx){" +
                "const b=bookmarks[idx];" +
                "if(confirm('¿Seguro que quieres eliminar este favorito?')){" +
                "const b64Title=btoa(unescape(encodeURIComponent(b.title||\"\")));" +
                "const b64Url=btoa(unescape(encodeURIComponent(b.url||\"\")));" +
                "const b64Folder=btoa(unescape(encodeURIComponent(b.folder||\"\")));" +
                "window.chrome.webview.postMessage('gxlight:bookmarks:delete:'+b64Title+'|'+b64Url+'|'+b64Folder);" +
                "}}" +
                "function moveBookmark(idx,newFolder){" +
                "const b=bookmarks[idx];" +
                "const b64Title=btoa(unescape(encodeURIComponent(b.title||\"\")));" +
                "const b64Url=btoa(unescape(encodeURIComponent(b.url||\"\")));" +
                "const b64Folder=btoa(unescape(encodeURIComponent(b.folder||\"\")));" +
                "const b64NewFolder=btoa(unescape(encodeURIComponent(newFolder)));" +
                "window.chrome.webview.postMessage('gxlight:bookmarks:move:'+b64Title+'|'+b64Url+'|'+b64Folder+'|'+b64NewFolder);" +
                "}" +
                "function escapeHtml(str){if(!str)return '';return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/\"/g,'&quot;');}" +
                "function escapeJs(str){if(!str)return '';return str.replace(/\\\\/g,'\\\\\\\\').replace(/\\'/g,'\\\\\\'').replace(/\"/g,'\\\\\"');}" +
                "renderFolders();renderBookmarks();" +
                "</script></body></html>";
        }

        private string PasswordsHtml()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<p>Password autosave and general autofill are <b>")
                .Append(_passwordSavingEnabled ? "enabled" : "disabled")
                .Append("</b> in WebView2 settings.</p>");
            body.Append("<p>WebView2 manages its native saved passwords inside the Edge profile. GX Light also has a local DPAPI-protected CSV vault for import/export.</p>");
            body.Append("<p>Vault entries imported: <b>").Append(_passwordVault.Count).Append("</b>. Use Menu > Passwords and autofill to import/export CSV.</p>");

            if (_passwordVault.Count == 0)
            {
                return body.ToString();
            }

            body.Append("<table><tr><th>Name</th><th>URL</th><th>Username</th><th>Imported</th></tr>");
            for (int i = 0; i < _passwordVault.Count; i++)
            {
                PasswordVaultEntry entry = _passwordVault[i];
                body.Append("<tr><td>").Append(EscapeHtml(entry.Name)).Append("</td><td>")
                    .Append(EscapeHtml(entry.Url)).Append("</td><td>")
                    .Append(EscapeHtml(entry.Username)).Append("</td><td>")
                    .Append(entry.ImportedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm")).Append("</td></tr>");
            }
            body.Append("</table>");
            return body.ToString();
        }

        private static string HtmlShell(string title, string body)
        {
            return "<!doctype html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>" +
                "<style>body{margin:0;background:#0d0f14;color:#eef7fa;font-family:Segoe UI,Arial,sans-serif}" +
                "main{padding:28px;max-width:1100px}h1{color:#72f5ff;margin:0 0 18px;font-size:30px}p{color:#c3ced8}" +
                "table{border-collapse:collapse;width:100%;background:#171a22}th,td{border-bottom:1px solid #2e3440;padding:10px;text-align:left;vertical-align:top}" +
                "th{color:#72f5ff;font-size:13px}a{color:#72f5ff}.pill{display:inline-block;background:#252833;border:1px solid #484d5c;padding:6px 9px;margin:3px}</style></head>" +
                "<body><main><h1>" + EscapeHtml(title) + "</h1>" + body + "</main></body></html>";
        }

        private static string EscapeHtml(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string Trim(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + "...";
        }

        private static void PaintAddressShell(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            using (Pen pen = new Pen(Color.FromArgb(70, 74, 88)))
            {
                Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private static string HomeHtml()
        {
            return "<!doctype html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>" +
                "<style>body{margin:0;background:#0d0f14;color:#eef7fa;font-family:Segoe UI,Arial,sans-serif}" +
                ".wrap{min-height:100vh;display:grid;place-items:center;padding:28px;background:linear-gradient(135deg,#111620,#0d0f14 55%,#13171d)}" +
                ".box{width:min(860px,92vw)}h1{font-size:44px;margin:0 0 10px;color:#72f5ff;letter-spacing:0}" +
                "p{color:#aeb8c4;margin:0 0 24px}.search{display:flex;gap:10px}.search input{flex:1;background:#20242d;border:1px solid #4a5360;color:#fff;padding:15px 16px;font-size:16px;outline:0}" +
                ".search button,.link{background:#72f5ff;color:#061116;border:0;padding:0 18px;font-weight:700;text-decoration:none;display:inline-flex;align-items:center;justify-content:center;min-height:44px}" +
                ".grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(210px,1fr));gap:12px;margin-top:18px}.card{border:1px solid #2e3440;background:#171a22;padding:16px}.card b{display:block;margin-bottom:6px}" +
                "@media(max-width:620px){h1{font-size:34px}.search{flex-direction:column}.search button{padding:13px}}</style></head>" +
                "<body><main class='wrap'><section class='box'><h1>GX Light</h1><p>Navegacion ligera con bloqueo nativo, extensiones locales y accesos rapidos.</p>" +
                "<form class='search' action='https://duckduckgo.com/'><input name='q' autofocus placeholder='Buscar o escribir una URL'><button>Buscar</button></form>" +
                "<div class='grid'><article class='card'><b>Chrome Web Store</b><a class='link' href='" + ChromeStoreUrl + "'>Abrir tienda</a></article>" +
                "<article class='card'><b>Shields</b><span>Bloqueador activo desde el navegador, no como extension.</span></article></div>" +
                "</section></main></body></html>";
        }

        private string UpdateNoticeHtml()
        {
            StringBuilder items = new StringBuilder();
            UpdateManifest manifest = _updateManifest ?? UpdateManifest.LocalFallback();
            string[] highlights = manifest.Highlights ?? VersionInfo.Highlights();
            for (int i = 0; i < highlights.Length; i++)
            {
                items.Append("<li>").Append(EscapeHtml(highlights[i])).Append("</li>");
            }

            string links = string.Empty;
            if (!string.IsNullOrWhiteSpace(manifest.DownloadUrl))
            {
                links += "<a class='link' data-open-url='" + EscapeHtml(manifest.DownloadUrl) + "' href='" + EscapeHtml(manifest.DownloadUrl) + "'>Ver release</a>";
            }
            if (!string.IsNullOrWhiteSpace(manifest.SourceUrl))
            {
                links += "<a class='link secondary' data-open-url='" + EscapeHtml(manifest.SourceUrl) + "' href='" + EscapeHtml(manifest.SourceUrl) + "'>Abrir GitHub</a>";
            }

            return "<!doctype html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>" +
                "<style>body{margin:0;background:#0d0f14;color:#eef7fa;font-family:Segoe UI,Arial,sans-serif}" +
                "main{min-height:100vh;display:grid;place-items:center;padding:28px;background:linear-gradient(135deg,#121620,#0d0f14 60%,#17111e)}" +
                "section{width:min(820px,92vw);border:1px solid #2e3440;background:#171a22;padding:28px}" +
                "h1{margin:0 0 8px;color:#72f5ff;font-size:34px;letter-spacing:0}p{color:#c3ced8;line-height:1.5}" +
                "ul{margin:18px 0 0;padding-left:22px}li{margin:10px 0;color:#eef7fa}.tag{display:inline-block;color:#061116;background:#72f5ff;padding:5px 9px;font-weight:700;margin-bottom:14px}" +
                ".links{display:flex;gap:10px;flex-wrap:wrap;margin-top:22px}.link{background:#72f5ff;color:#061116;text-decoration:none;font-weight:700;padding:10px 13px}.secondary{background:#252833;color:#eef7fa;border:1px solid #484d5c}</style></head>" +
                "<body><main><section><span class='tag'>Version " + EscapeHtml(manifest.Version) + "</span>" +
                "<h1>" + EscapeHtml(manifest.ReleaseName) + "</h1>" +
                "<p>Esta pestana se carga desde el manifiesto de actualizaciones en GitHub y aparece solo una vez por version publicada.</p>" +
                "<p>Cliente instalado: <b>" + EscapeHtml(VersionInfo.CurrentVersion) + "</b>" +
                (string.IsNullOrWhiteSpace(manifest.PublishedAt) ? string.Empty : " - Publicado: <b>" + EscapeHtml(manifest.PublishedAt) + "</b>") + "</p>" +
                "<ul>" + items.ToString() + "</ul><div class='links'>" + links + "</div></section></main>" +
                "<script>document.querySelectorAll('[data-open-url]').forEach(function(link){link.addEventListener('click',function(event){event.preventDefault();window.chrome.webview.postMessage('gxlight:navigate:'+link.getAttribute('data-open-url'));});});</script>" +
                "</body></html>";
        }

        private static void EnsureDefaultFilters()
        {
            if (File.Exists(AppPaths.Filters))
            {
                return;
            }

            File.WriteAllText(AppPaths.Filters,
                "! GX Light local filters" + Environment.NewLine +
                "! Add EasyList/EasyPrivacy content here or run scripts\\Update-Filters.ps1" + Environment.NewLine,
                System.Text.Encoding.UTF8);
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minSize, IntPtr maxSize);

        private void NavigationCompletedHandler(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            CoreWebView2 core = sender as CoreWebView2;
            if (core == null) return;
            WebView2 web = WebViewForCore(core);
            if (web == null) return;
            TabPage page = PageForWebView(web);
            if (page == null) return;

            if (web.Source != null && IsYouTubeHost(web.Source.Host))
            {
                core.ExecuteScriptAsync("window.__gxLightRunYouTubeShields && window.__gxLightRunYouTubeShields();");
            }
            AddHistoryEntry(page, web);
            SyncAddress();
            UpdateStatus();
        }

        private void DocumentTitleChangedHandler(object sender, object e)
        {
            CoreWebView2 core = sender as CoreWebView2;
            if (core == null) return;
            WebView2 web = WebViewForCore(core);
            if (web == null) return;
            TabPage page = PageForWebView(web);
            if (page == null) return;

            string title = core.DocumentTitle;
            page.Text = string.IsNullOrWhiteSpace(title) ? "Pestana" : Trim(title, 30);
            RebuildTabStrip();
        }

        private TabPage PageForWebView(WebView2 web)
        {
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && tab.WebView == web)
                {
                    return _tabs.TabPages[i];
                }
            }
            return null;
        }

        private void UnsubscribeWebViewEvents(WebView2 web)
        {
            if (web == null) return;
            try
            {
                if (web.CoreWebView2 != null)
                {
                    web.CoreWebView2.ProcessFailed -= Web_ProcessFailed;
                    web.CoreWebView2.WebResourceRequested -= WebResourceRequested;
                    web.CoreWebView2.NewWindowRequested -= NewWindowRequested;
                    web.CoreWebView2.DownloadStarting -= DownloadStarting;
                    web.CoreWebView2.NavigationStarting -= NavigationStarting;
                    web.CoreWebView2.NavigationCompleted -= NavigationCompletedHandler;
                    web.CoreWebView2.DocumentTitleChanged -= DocumentTitleChangedHandler;
                    web.CoreWebView2.WebMessageReceived -= WebMessageReceived;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error unsubscribing events: " + ex.Message);
            }
        }

        private void Web_ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            WebView2 web = sender as WebView2;
            if (web == null) return;

            Logger.Error("WebView2 process failed. Kind: " + e.ProcessFailedKind + ", Reason: " + e.Reason + ", ExitCode: " + e.ExitCode);

            BrowserTab tab = null;
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab t = _tabs.TabPages[i].Tag as BrowserTab;
                if (t != null && t.WebView == web)
                {
                    tab = t;
                    break;
                }
            }

            if (tab != null)
            {
                ShowCrashRecovery(tab, "El proceso de la pestaña falló (" + e.ProcessFailedKind + ").");
            }
        }

        private void ShowCrashRecovery(BrowserTab tab, string message)
        {
            if (tab == null || tab.WebView == null)
            {
                return;
            }

            tab.SuspendedUrl = tab.WebView.Source == null ? HomeUrl : tab.WebView.Source.ToString();
            tab.SuspendedTitle = tab.Page.Text.Replace("[Crashed] ", "").Replace("[Suspended] ", "");

            UnsubscribeWebViewEvents(tab.WebView);

            tab.Page.Controls.Clear();
            tab.WebView.Dispose();
            tab.WebView = null;
            tab.IsSuspended = true;

            Panel placeholder = new Panel();
            placeholder.Dock = DockStyle.Fill;
            placeholder.BackColor = Theme.Window;

            Label title = new Label();
            title.Text = "La pagina ha fallado (Crash)";
            title.ForeColor = Theme.Warning;
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 40;
            title.Top = 40;
            placeholder.Controls.Add(title);

            Label detail = new Label();
            detail.Text = message + "\nURL: " + tab.SuspendedUrl;
            detail.ForeColor = Theme.Muted;
            detail.AutoSize = true;
            detail.Left = 42;
            detail.Top = 82;
            placeholder.Controls.Add(detail);

            ChromeButton reload = new ChromeButton();
            reload.Text = "Recargar pagina";
            reload.Width = 150;
            reload.Height = 34;
            reload.Left = 42;
            reload.Top = 140;
            reload.Click += async delegate { await RestoreSuspendedTabAsync(tab); };
            placeholder.Controls.Add(reload);

            tab.Page.Controls.Add(placeholder);
            tab.Page.Text = "[Crashed] " + Trim(tab.SuspendedTitle, 18);
            RebuildTabStrip();
        }

        private void SetupSuspensionPlaceholder(BrowserTab tab)
        {
            Panel placeholder = new Panel();
            placeholder.Dock = DockStyle.Fill;
            placeholder.BackColor = Theme.Window;

            Label title = new Label();
            title.Text = "Pestana suspendida para ahorrar memoria";
            title.ForeColor = Theme.Text;
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 40;
            title.Top = 40;
            placeholder.Controls.Add(title);

            Label urlLabel = new Label();
            urlLabel.Text = tab.SuspendedUrl;
            urlLabel.ForeColor = Theme.Muted;
            urlLabel.AutoSize = true;
            urlLabel.Left = 42;
            urlLabel.Top = 82;
            placeholder.Controls.Add(urlLabel);

            ChromeButton restore = new ChromeButton();
            restore.Text = "Restaurar pestana";
            restore.Width = 150;
            restore.Height = 34;
            restore.Left = 42;
            restore.Top = 118;
            restore.Click += async delegate { await RestoreSuspendedTabAsync(tab); };
            placeholder.Controls.Add(restore);

            tab.Page.Controls.Add(placeholder);
        }

        private BrowserTab CreateSuspendedTab(string url, string title, int islandId)
        {
            TabPage page = new TabPage("[Suspended] " + Trim(title, 18));
            page.BackColor = Theme.Window;

            BrowserTab tab = new BrowserTab(page, null);
            tab.IslandId = islandId;
            tab.IsSuspended = true;
            tab.SuspendedUrl = url;
            tab.SuspendedTitle = title;
            page.Tag = tab;

            SetupSuspensionPlaceholder(tab);
            _tabs.TabPages.Add(page);
            RebuildTabStrip();
            SaveSession();
            return tab;
        }

        private void SaveSession()
        {
            try
            {
                bool maximized = WindowState == FormWindowState.Maximized;
                int x = Location.X;
                int y = Location.Y;
                int width = Width;
                int height = Height;

                if (WindowState == FormWindowState.Minimized)
                {
                    x = -1; y = -1;
                }

                int activeIndex = _tabs.SelectedIndex;
                List<BrowserTab> tabsList = new List<BrowserTab>();
                for (int i = 0; i < _tabs.TabPages.Count; i++)
                {
                    BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                    if (tab != null)
                    {
                        tabsList.Add(tab);
                    }
                }

                SessionManager.SaveSession(maximized, x, y, width, height, activeIndex, _islandColors, tabsList);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save session from form: " + ex.Message);
            }
        }

        private void EnforceLowResourceLimit()
        {
            if (!_gxControl.LowResourcesModeEnabled)
            {
                return;
            }

            int activeCount = 0;
            List<BrowserTab> activeTabs = new List<BrowserTab>();
            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                BrowserTab tab = _tabs.TabPages[i].Tag as BrowserTab;
                if (tab != null && !tab.IsSuspended)
                {
                    activeCount++;
                    if (_tabs.TabPages[i] != _tabs.SelectedTab)
                    {
                        activeTabs.Add(tab);
                    }
                }
            }

            int limit = _gxControl.MaxActiveTabs;
            if (activeCount > limit && activeTabs.Count > 0)
            {
                activeTabs.Sort(delegate(BrowserTab a, BrowserTab b) {
                    return a.LastActiveUtc.CompareTo(b.LastActiveUtc);
                });

                int toSuspend = activeCount - limit;
                int suspendedCount = 0;
                for (int i = 0; i < activeTabs.Count && suspendedCount < toSuspend; i++)
                {
                    BrowserTab tab = activeTabs[i];
                    if (tab.WebView != null && tab.WebView.CoreWebView2 != null)
                    {
                        if (tab.WebView.CoreWebView2.IsDocumentPlayingAudio)
                        {
                            continue;
                        }
                    }
                    SuspendTab(tab);
                    suspendedCount++;
                }
            }
        }

        private void FreeMemoryNow()
        {
            Logger.Info("Manually requesting memory release...");
            SuspendIdleTabsNow();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to trim working set: " + ex.Message);
            }

            Logger.Info("Memory release complete.");
        }

        private string GetMemoryProcessesHtml()
        {
            if (_environment == null)
            {
                return "<p>No active WebView2 environment.</p>";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<h3>Active WebView2 Processes</h3>");
            sb.Append("<table><tr><th>Process ID</th><th>Type</th><th>Memory (Working Set)</th></tr>");

            try
            {
                var infos = _environment.GetProcessInfos();
                if (infos != null)
                {
                    long total = 0;
                    foreach (var info in infos)
                    {
                        string typeStr = info.Kind.ToString();
                        string memStr = "Unknown";
                        try
                        {
                            using (Process p = Process.GetProcessById((int)info.ProcessId))
                            {
                                long memMb = p.WorkingSet64 / (1024 * 1024);
                                memStr = memMb + " MB";
                                total += memMb;
                            }
                        }
                        catch
                        {
                            memStr = "Access Denied / Exited";
                        }
                        sb.Append("<tr><td>" + info.ProcessId + "</td><td>" + typeStr + "</td><td>" + memStr + "</td></tr>");
                    }
                    sb.Append("<tr style='font-weight:bold;color:#72f5ff;'><td>Total Environment</td><td>-</td><td>" + total + " MB</td></tr>");
                }
            }
            catch (Exception ex)
            {
                sb.Append("<tr><td colspan='3'>Error retrieving processes: " + EscapeHtml(ex.Message) + "</td></tr>");
            }
            sb.Append("</table>");
            sb.Append("<div style='margin-top:20px;'><button onclick='location.href=\"gxlight://free-memory\"' style='background:#72f5ff;color:#061116;border:0;padding:10px 20px;font-weight:700;cursor:pointer;'>Liberar memoria ahora</button></div>");

            return sb.ToString();
        }

        private sealed class HistoryEntry
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public DateTime VisitedUtc { get; set; }
        }

        private sealed class DownloadEntry
        {
            public string FileName { get; set; }
            public string Path { get; set; }
            public string Uri { get; set; }
            public string State { get; set; }
            public DateTime StartedUtc { get; set; }
        }

        private sealed class BookmarkEntry
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string Folder { get; set; }
            public DateTime CreatedUtc { get; set; }
        }

        private sealed class PasswordVaultEntry
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Note { get; set; }
            public DateTime ImportedUtc { get; set; }
        }
    }
}
