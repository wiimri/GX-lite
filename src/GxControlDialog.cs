using System;
using System.Drawing;
using System.Windows.Forms;

namespace GXLightBrowser
{
    internal sealed class GxControlDialog : Form
    {
        private readonly GxControlSettings _settings;
        private readonly Label _ramValue = new Label();
        private readonly Label _cpuValue = new Label();
        private readonly CheckBox _ramEnabled = new CheckBox();
        private readonly CheckBox _hardLimit = new CheckBox();
        private readonly CheckBox _hotTabsEnabled = new CheckBox();
        private readonly CheckBox _cpuEnabled = new CheckBox();
        private readonly CheckBox _networkEnabled = new CheckBox();
        private readonly TrackBar _ramTrack = new TrackBar();
        private readonly TrackBar _cpuTrack = new TrackBar();
        private readonly ComboBox _hotTabsMode = new ComboBox();
        private readonly ComboBox _networkProfile = new ComboBox();
        private readonly CheckBox _lowResEnabled = new CheckBox();
        private readonly TrackBar _maxActiveTrack = new TrackBar();
        private readonly Label _maxActiveLabel = new Label();

        public GxControlDialog(GxControlSettings settings)
        {
            _settings = settings;
            Text = "Gan Pulse";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(390, 720);
            MinimumSize = new Size(360, 620);
            BackColor = Theme.Window;
            ForeColor = Theme.Text;
            Font = new Font("Segoe UI", 9f);
            BuildLayout();
            LoadFromSettings();
        }

        private void BuildLayout()
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.AutoScroll = true;
            root.Padding = new Padding(12);
            root.BackColor = Theme.Window;
            Controls.Add(root);

            Label title = new Label();
            title.Text = "GAN PULSE";
            title.ForeColor = Theme.Warning;
            title.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            title.AutoSize = true;
            title.Top = 8;
            title.Left = 12;
            root.Controls.Add(title);

            int top = 46;
            top = AddRamCard(root, top);
            top = AddNetworkCard(root, top + 12);
            top = AddHotTabsCard(root, top + 12);
            top = AddCpuCard(root, top + 12);
            top = AddLowResourceCard(root, top + 12);

            ChromeButton save = new ChromeButton();
            save.Text = "Apply";
            save.Width = 100;
            save.Height = 34;
            save.Left = 242;
            save.Top = top + 14;
            save.Click += delegate
            {
                SaveToSettings();
                DialogResult = DialogResult.OK;
                Close();
            };
            root.Controls.Add(save);
        }

        private int AddRamCard(Control root, int top)
        {
            Panel card = Card(root, top, 212);
            AddHeader(card, "RAM LIMITER", _ramEnabled);
            _ramValue.Left = 0;
            _ramValue.Top = 46;
            _ramValue.Width = card.Width;
            _ramValue.Height = 58;
            _ramValue.TextAlign = ContentAlignment.MiddleCenter;
            _ramValue.ForeColor = Theme.Text;
            _ramValue.Font = new Font("Segoe UI", 26f, FontStyle.Bold);
            card.Controls.Add(_ramValue);

            Label label = SmallLabel("Memory Limit (GB)", 16, 112);
            card.Controls.Add(label);

            _ramTrack.Left = 16;
            _ramTrack.Top = 136;
            _ramTrack.Width = 296;
            _ramTrack.Minimum = 256;
            _ramTrack.Maximum = 8192;
            _ramTrack.TickFrequency = 512;
            _ramTrack.SmallChange = 128;
            _ramTrack.LargeChange = 512;
            _ramTrack.Scroll += delegate { UpdateRamLabel(); };
            card.Controls.Add(_ramTrack);

            _hardLimit.Text = "Hard limit";
            StyleCheck(_hardLimit);
            _hardLimit.Left = 16;
            _hardLimit.Top = 180;
            _hardLimit.Width = 170;
            card.Controls.Add(_hardLimit);
            return top + card.Height;
        }

        private int AddNetworkCard(Control root, int top)
        {
            Panel card = Card(root, top, 130);
            AddHeader(card, "NETWORK LIMITER", _networkEnabled);
            _networkProfile.Left = 16;
            _networkProfile.Top = 62;
            _networkProfile.Width = 296;
            _networkProfile.DropDownStyle = ComboBoxStyle.DropDownList;
            _networkProfile.Items.AddRange(new object[] {
                "1 MB/s - 8 Mbps",
                "5 MB/s - 40 Mbps",
                "25 MB/s - 200 Mbps",
                "Unlimited"
            });
            card.Controls.Add(_networkProfile);
            Label note = SmallLabel("Policy setting only; true throttling needs a local proxy.", 16, 94);
            note.Width = 300;
            card.Controls.Add(note);
            return top + card.Height;
        }

        private int AddHotTabsCard(Control root, int top)
        {
            Panel card = Card(root, top, 136);
            AddHeader(card, "HOT TABS KILLER", _hotTabsEnabled);
            _hotTabsMode.Left = 16;
            _hotTabsMode.Top = 60;
            _hotTabsMode.Width = 296;
            _hotTabsMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _hotTabsMode.Items.AddRange(new object[] { "CPU", "RAM" });
            card.Controls.Add(_hotTabsMode);
            Label note = SmallLabel("Suspends inactive tabs when limits are exceeded.", 16, 94);
            note.Width = 300;
            card.Controls.Add(note);
            return top + card.Height;
        }

        private int AddCpuCard(Control root, int top)
        {
            Panel card = Card(root, top, 196);
            AddHeader(card, "CPU LIMITER", _cpuEnabled);
            _cpuValue.Left = 0;
            _cpuValue.Top = 48;
            _cpuValue.Width = card.Width;
            _cpuValue.Height = 50;
            _cpuValue.TextAlign = ContentAlignment.MiddleCenter;
            _cpuValue.ForeColor = Theme.Text;
            _cpuValue.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
            card.Controls.Add(_cpuValue);

            Label label = SmallLabel("Processor Limit (%)", 16, 110);
            card.Controls.Add(label);

            _cpuTrack.Left = 16;
            _cpuTrack.Top = 134;
            _cpuTrack.Width = 296;
            _cpuTrack.Minimum = 1;
            _cpuTrack.Maximum = 100;
            _cpuTrack.TickFrequency = 10;
            _cpuTrack.Scroll += delegate { UpdateCpuLabel(); };
            card.Controls.Add(_cpuTrack);
            return top + card.Height;
        }

        private Panel Card(Control root, int top, int height)
        {
            Panel card = new Panel();
            card.Left = 10;
            card.Top = top;
            card.Width = 332;
            card.Height = height;
            card.BackColor = Theme.Panel;
            card.Padding = new Padding(16);
            root.Controls.Add(card);
            return card;
        }

        private void AddHeader(Control parent, string text, CheckBox toggle)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Theme.Text;
            label.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            label.Left = 16;
            label.Top = 16;
            label.Width = 220;
            parent.Controls.Add(label);

            StyleCheck(toggle);
            toggle.Left = 266;
            toggle.Top = 14;
            toggle.Width = 50;
            parent.Controls.Add(toggle);
        }

        private static Label SmallLabel(string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Theme.Muted;
            label.Left = left;
            label.Top = top;
            label.Width = 220;
            label.Height = 20;
            return label;
        }

        private static void StyleCheck(CheckBox check)
        {
            check.ForeColor = Theme.Text;
            check.BackColor = Theme.Panel;
            check.FlatStyle = FlatStyle.Flat;
        }

        private void LoadFromSettings()
        {
            _ramEnabled.Checked = _settings.RamLimiterEnabled;
            _ramTrack.Value = Math.Max(_ramTrack.Minimum, Math.Min(_ramTrack.Maximum, _settings.MemoryLimitMb));
            _hardLimit.Checked = _settings.HardMemoryLimit;
            _networkEnabled.Checked = _settings.NetworkLimiterEnabled;
            _networkProfile.SelectedItem = _settings.NetworkProfile;
            if (_networkProfile.SelectedIndex < 0)
            {
                _networkProfile.SelectedIndex = 2;
            }
            _hotTabsEnabled.Checked = _settings.HotTabsKillerEnabled;
            _hotTabsMode.SelectedItem = _settings.HotTabsMode;
            if (_hotTabsMode.SelectedIndex < 0)
            {
                _hotTabsMode.SelectedIndex = 1;
            }
            _cpuEnabled.Checked = _settings.CpuLimiterEnabled;
            _cpuTrack.Value = Math.Max(_cpuTrack.Minimum, Math.Min(_cpuTrack.Maximum, _settings.CpuLimitPercent));
            _lowResEnabled.Checked = _settings.LowResourcesModeEnabled;
            _maxActiveTrack.Value = Math.Max(_maxActiveTrack.Minimum, Math.Min(_maxActiveTrack.Maximum, _settings.MaxActiveTabs));
            UpdateRamLabel();
            UpdateCpuLabel();
            UpdateMaxActiveLabel();
        }

        private void SaveToSettings()
        {
            _settings.RamLimiterEnabled = _ramEnabled.Checked;
            _settings.MemoryLimitMb = _ramTrack.Value;
            _settings.HardMemoryLimit = _hardLimit.Checked;
            _settings.NetworkLimiterEnabled = _networkEnabled.Checked;
            _settings.NetworkProfile = _networkProfile.SelectedItem == null ? "25 MB/s - 200 Mbps" : _networkProfile.SelectedItem.ToString();
            _settings.HotTabsKillerEnabled = _hotTabsEnabled.Checked;
            _settings.HotTabsMode = _hotTabsMode.SelectedItem == null ? "RAM" : _hotTabsMode.SelectedItem.ToString();
            _settings.CpuLimiterEnabled = _cpuEnabled.Checked;
            _settings.CpuLimitPercent = _cpuTrack.Value;
            _settings.LowResourcesModeEnabled = _lowResEnabled.Checked;
            _settings.MaxActiveTabs = _maxActiveTrack.Value;
        }

        private int AddLowResourceCard(Control root, int top)
        {
            Panel card = Card(root, top, 146);
            AddHeader(card, "LOW RESOURCE MODE", _lowResEnabled);

            _maxActiveLabel.Left = 16;
            _maxActiveLabel.Top = 56;
            _maxActiveLabel.Width = 220;
            _maxActiveLabel.Height = 20;
            _maxActiveLabel.ForeColor = Theme.Muted;
            card.Controls.Add(_maxActiveLabel);

            _maxActiveTrack.Left = 16;
            _maxActiveTrack.Top = 80;
            _maxActiveTrack.Width = 296;
            _maxActiveTrack.Minimum = 2;
            _maxActiveTrack.Maximum = 20;
            _maxActiveTrack.TickFrequency = 2;
            _maxActiveTrack.Scroll += delegate { UpdateMaxActiveLabel(); };
            card.Controls.Add(_maxActiveTrack);

            return top + card.Height;
        }

        private void UpdateRamLabel()
        {
            _ramValue.Text = (_ramTrack.Value / 1024.0).ToString("0.0") + "\nGB";
        }

        private void UpdateCpuLabel()
        {
            _cpuValue.Text = _cpuTrack.Value + "\n%";
        }

        private void UpdateMaxActiveLabel()
        {
            _maxActiveLabel.Text = "Max active tabs: " + _maxActiveTrack.Value;
        }
    }
}
