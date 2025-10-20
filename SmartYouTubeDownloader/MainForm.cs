using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartYouTubeDownloader.Models;
using SmartYouTubeDownloader.Services;
using SmartYouTubeDownloader.Settings;
using SmartYouTubeDownloader.Utilities;

namespace SmartYouTubeDownloader
{
public partial class MainForm : Form
{
    private readonly string _appToolsDirectory = Path.Combine(AppContext.BaseDirectory, "tools");
    private readonly List<string> _batchUrls = new List<string>();
    private readonly UserSettings _settings;
    private VideoMetadata? _currentMetadata;
    private string? _ytDlpPath;
    private string? _ffmpegPath;
    private CollisionStrategy? _sessionCollisionStrategy;
    private CancellationTokenSource? _cts;

    public MainForm()
    {
        InitializeComponent();
        _settings = UserSettings.Load();
        Directory.CreateDirectory(_appToolsDirectory);
        InitializeMenu();
        ApplySettings();
        UpdateAdvancedVisibility(_settings.AdvancedMode);
        tbSave.Text = _settings.SaveDirectory;
        tbUrl.Text = _settings.LastUrl;
        ckUseProxy.Checked = _settings.UseProxy;
        tbProxyHost.Text = _settings.ProxyHost;
        tbProxyPort.Text = _settings.ProxyPort == 0 ? string.Empty : _settings.ProxyPort.ToString();
        tbProxyUser.Text = _settings.ProxyUser;
        tbProxyPass.Text = _settings.ProxyPassword;
        ckAdvanced.Checked = _settings.AdvancedMode;
        ckAudio.Checked = _settings.AudioOnly;
        ckMulti.Checked = _settings.BatchMode;
        rbCompatOn.Checked = _settings.CompatibilityMode;
        rbCompatOff.Checked = !_settings.CompatibilityMode;
        foreach (var url in _settings.BatchUrls)
        {
            AddUrlToQueue(url);
        }
        ToggleProxyInputs();
        ToggleBatchMode();

        btnFetch.Click += async (sender, args) => await FetchMetadataAsync();
        btnDownload.Click += async (sender, args) => await DownloadAsync();
        btnBrowse.Click += (sender, args) => BrowseSaveFolder();
        btnOpen.Click += (sender, args) => OpenSaveFolder();
        ckAdvanced.CheckedChanged += (sender, args) => ToggleAdvanced();
        ckMulti.CheckedChanged += (sender, args) => ToggleBatchMode();
        btnAddUrl.Click += (sender, args) => EnqueueCurrentUrl();
        ckUseProxy.CheckedChanged += (sender, args) => ToggleProxyInputs();
        FormClosing += (sender, args) => PersistSettings();
        toolTip.SetToolTip(cbQuality, "Select the maximum quality to download.");
        toolTip.SetToolTip(ckAudio, "Download audio only using the best available quality.");
        toolTip.SetToolTip(rbCompatOn, "Re-encode to a Premiere-compatible H.264 + AAC MP4.");
        toolTip.SetToolTip(rbCompatOff, "Keep original container when possible.");
        toolTip.SetToolTip(ckMulti, "Queue multiple URLs for sequential downloads.");
        toolTip.SetToolTip(btnAddUrl, "Add the URL above to the batch queue.");
        toolTip.SetToolTip(ckUseProxy, "Route downloads through the specified proxy.");
        toolTip.SetToolTip(tbProxyHost, "Proxy host name or IP address.");
        toolTip.SetToolTip(tbProxyPort, "Proxy port number.");
        toolTip.SetToolTip(tbProxyUser, "Proxy username (if required).");
        toolTip.SetToolTip(tbProxyPass, "Proxy password (if required).");

        _ = Task.Run(LocateToolsAsync);
    }

    private void InitializeMenu()
    {
        menuTools.Items.Clear();
        menuTools.Items.Add("Install/Update yt-dlp", null, async (sender, args) => await InstallYtDlpAsync());
        menuTools.Items.Add("Install/Update FFmpeg", null, async (sender, args) => await InstallFfmpegAsync());
        menuTools.Items.Add(new ToolStripSeparator());
        menuTools.Items.Add("Open tools folder", null, (sender, args) => OpenFolder(UserSettings.GetToolsDirectory()));
        menuTools.Items.Add("Open cache folder", null, (sender, args) => OpenFolder(UserSettings.GetSettingsDirectory()));
        menuTools.Items.Add("Clear temp files", null, (sender, args) => ClearTempFiles());
        menuTools.Items.Add(new ToolStripSeparator());
        menuTools.Items.Add("Reset settings", null, (sender, args) => ResetSettings());
    }

    private async Task LocateToolsAsync()
    {
        try
        {
            var locator = new ToolLocator(_appToolsDirectory);
            _ytDlpPath = _settings.YtDlpPath ?? locator.FindYtDlp();
            _ffmpegPath = _settings.FfmpegPath ?? locator.FindFfmpeg();
            if (string.IsNullOrWhiteSpace(_ytDlpPath) || !File.Exists(_ytDlpPath))
            {
                UpdateStatus("yt-dlp not found. Please install via Tools.");
            }
            if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            {
                UpdateStatus("FFmpeg not found. Compatibility mode will be unavailable until installed.");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Tool discovery failed: {ex.Message}");
        }
    }

    private async Task FetchMetadataAsync()
    {
        if (string.IsNullOrWhiteSpace(tbUrl.Text))
        {
            MessageBox.Show(this, "Enter a YouTube URL.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_ytDlpPath) || !File.Exists(_ytDlpPath))
        {
            MessageBox.Show(this, "yt-dlp is not available. Install it from the Tools menu.", "Missing dependency", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            SetUiBusy(true);
            UpdateStatus("Fetching metadata…");
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ytDlp = new YtDlpService(_ytDlpPath);
            _currentMetadata = await ytDlp.FetchMetadataAsync(tbUrl.Text.Trim(), _cts.Token).ConfigureAwait(true);
            lblMeta.Text = $"{_currentMetadata.Title}\n{_currentMetadata.Duration:hh\\:mm\\:ss}";
            PopulateQualityOptions(_currentMetadata);
            UpdateStatus("Metadata loaded.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Metadata error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Metadata fetch failed.");
        }
        finally
        {
            SetUiBusy(false);
        }
    }

    private void PopulateQualityOptions(VideoMetadata metadata)
    {
        cbQuality.Items.Clear();
        cbQuality.Items.Add(new QualityItem("Best", null, null));
        foreach (var format in metadata.Formats.Where(f => f.Height.HasValue && f.HasVideo))
        {
            cbQuality.Items.Add(new QualityItem(format.DisplayLabel, format.Id, format.Height));
        }
        cbQuality.SelectedIndex = 0;
        cbQuality.Enabled = true;
    }

    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(tbSave.Text))
        {
            MessageBox.Show(this, "Choose a save location.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!Directory.Exists(tbSave.Text))
        {
            Directory.CreateDirectory(tbSave.Text);
        }

        var urls = ckMulti.Checked ? _batchUrls.ToList() : new List<string>();
        if (!ckMulti.Checked || string.IsNullOrWhiteSpace(tbUrl.Text) == false)
        {
            var current = tbUrl.Text.Trim();
            if (!string.IsNullOrWhiteSpace(current) && !urls.Contains(current, StringComparer.OrdinalIgnoreCase))
            {
                urls.Insert(0, current);
            }
        }

        if (urls.Count == 0)
        {
            MessageBox.Show(this, "Add at least one URL to download.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_ytDlpPath) || !File.Exists(_ytDlpPath))
        {
            MessageBox.Show(this, "yt-dlp is not available.", "Missing dependency", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (rbCompatOn.Checked && (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath)))
        {
            MessageBox.Show(this, "FFmpeg is required for compatibility mode.", "Missing dependency", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            SetUiBusy(true);
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            progress.Value = 0;
            _sessionCollisionStrategy = null;
            var ytDlp = new YtDlpService(_ytDlpPath);
            var ffmpeg = string.IsNullOrWhiteSpace(_ffmpegPath) ? null : new FfmpegService(_ffmpegPath);
            var proxy = BuildProxy();
            if (ckUseProxy.Checked && proxy is null)
            {
                UpdateStatus("Proxy configuration invalid.");
                return;
            }

            for (var i = 0; i < urls.Count; i++)
            {
                var url = urls[i];
                progress.Value = 0;
                UpdateStatus($"Item {i + 1} of {urls.Count} — preparing");
                var metadata = _currentMetadata;
                if (urls.Count > 1 || metadata == null || !string.Equals(metadata.Id, _currentMetadata?.Id, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        metadata = await ytDlp.FetchMetadataAsync(url, _cts.Token).ConfigureAwait(true);
                        _currentMetadata = metadata;
                        lblMeta.Text = $"{metadata.Title}\n{metadata.Duration:hh\\:mm\\:ss}";
                        PopulateQualityOptions(metadata);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Failed to fetch metadata for {url}: {ex.Message}", "Metadata error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                }

                var request = BuildDownloadRequest(url, metadata, proxy);
                if (request is null)
                {
                    UpdateStatus($"Skipped existing file for {url}");
                    continue;
                }

                var finalPath = await PerformDownloadAsync(ytDlp, ffmpeg, request).ConfigureAwait(true);
                if (string.IsNullOrEmpty(finalPath))
                {
                    UpdateStatus($"Skipped {url}");
                    continue;
                }

                UpdateStatus($"Done — saved to {finalPath}");
            }
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Operation cancelled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus("Download failed.");
        }
        finally
        {
            SetUiBusy(false);
            progress.Value = 0;
        }
    }

    private async Task<string> PerformDownloadAsync(YtDlpService ytDlp, FfmpegService? ffmpeg, DownloadRequest request)
    {
        if (_cts is null)
        {
            throw new InvalidOperationException("Download token not initialized.");
        }

        var progressReporter = new Progress<DownloadProgress>(p =>
        {
            progress.Value = Math.Max(0, Math.Min(100, p.Percent));
            lblStatus.Text = p.Status;
        });

        var pattern = request.OutputTemplate.Replace("%(ext)s", "*");
        var existingBefore = new HashSet<string>(Directory.GetFiles(request.OutputDirectory, pattern), StringComparer.OrdinalIgnoreCase);

        await ytDlp.DownloadAsync(request, progressReporter, _cts.Token).ConfigureAwait(true);

        var after = Directory.GetFiles(request.OutputDirectory, pattern);
        var latest = after.Except(existingBefore, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault() ?? after.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();

        if (!request.CompatibilityMode || ffmpeg == null || string.IsNullOrEmpty(latest))
        {
            progress.Value = 100;
            return latest ?? string.Empty;
        }

        if (string.IsNullOrEmpty(latest))
        {
            return string.Empty;
        }

        var finalPath = await ffmpeg.TranscodeForPremiereAsync(latest, request.OutputDirectory, request.Metadata ?? new VideoMetadata(), progressReporter, _cts.Token).ConfigureAwait(true);
        return finalPath;
    }

    private DownloadRequest? BuildDownloadRequest(string url, VideoMetadata? metadata, string? proxy)
    {
        var selectedQuality = cbQuality.SelectedItem as QualityItem;
        var baseName = metadata != null
            ? SanitizeFileName($"{metadata.Title} [{metadata.Id}]")
            : "%(title)s [%(id)s]";

        var collisionStrategy = _sessionCollisionStrategy;
        if (metadata != null)
        {
            var existing = Directory.GetFiles(tbSave.Text, $"{baseName}.*");
            if (existing.Length > 0)
            {
                if (!collisionStrategy.HasValue)
                {
                    collisionStrategy = PromptForCollisionStrategy(existing[0]);
                }

                if (!_sessionCollisionStrategy.HasValue)
                {
                    _sessionCollisionStrategy = collisionStrategy;
                }
                if (collisionStrategy == CollisionStrategy.Skip)
                {
                    return null;
                }

                if (collisionStrategy == CollisionStrategy.Overwrite)
                {
                    foreach (var file in existing)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
                else if (collisionStrategy == CollisionStrategy.AutoRename)
                {
                    baseName = GenerateUniqueBaseName(baseName);
                }
            }
        }
        else
        {
            if (!collisionStrategy.HasValue)
            {
                collisionStrategy = CollisionStrategy.AutoRename;
            }
        }

        return new DownloadRequest
        {
            Url = url,
            OutputDirectory = tbSave.Text,
            OutputTemplate = $"{baseName}.%(ext)s",
            Mode = ckAudio.Checked ? DownloadMode.AudioOnly : DownloadMode.Video,
            FormatId = selectedQuality?.FormatId,
            MaxHeight = selectedQuality?.MaxHeight,
            CompatibilityMode = rbCompatOn.Checked,
            UseProxy = !string.IsNullOrWhiteSpace(proxy),
            ProxyAddress = proxy,
            Metadata = metadata,
            CollisionStrategy = collisionStrategy ?? CollisionStrategy.AutoRename
        };
    }

    private CollisionStrategy PromptForCollisionStrategy(string path)
    {
        if (!File.Exists(path))
        {
            return CollisionStrategy.AutoRename;
        }

        var result = MessageBox.Show(this, "File exists. Choose Yes to overwrite, No to skip, Cancel to auto-rename future collisions.", "File exists", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
        switch (result)
        {
            case DialogResult.Yes:
                return CollisionStrategy.Overwrite;
            case DialogResult.No:
                return CollisionStrategy.Skip;
            default:
                return CollisionStrategy.AutoRename;
        }
    }

    private string? BuildProxy()
    {
        if (!ckUseProxy.Checked)
        {
            return null;
        }

        if (!int.TryParse(tbProxyPort.Text, out var port))
        {
            MessageBox.Show(this, "Proxy port must be numeric.", "Proxy error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        try
        {
            return ProxyUtility.BuildProxyUrl(tbProxyHost.Text.Trim(), port, string.IsNullOrWhiteSpace(tbProxyUser.Text) ? null : tbProxyUser.Text, string.IsNullOrWhiteSpace(tbProxyPass.Text) ? null : tbProxyPass.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Proxy error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
    }

    private async Task<string> PerformToolInstallAsync(Func<ToolInstaller, IProgress<string>, Task<string>> installerAction)
    {
        using (var client = new HttpClient())
        {
            var installer = new ToolInstaller(client);
            var progressReporter = new Progress<string>(UpdateStatus);
            return await installerAction(installer, progressReporter).ConfigureAwait(true);
        }
    }

    private async Task InstallYtDlpAsync()
    {
        try
        {
            SetUiBusy(true);
            var path = await PerformToolInstallAsync((installer, progressReporter) => installer.DownloadYtDlpAsync(UserSettings.GetToolsDirectory(), progressReporter)).ConfigureAwait(true);
            _ytDlpPath = path;
            UpdateStatus("yt-dlp installed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Install error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetUiBusy(false);
        }
    }

    private async Task InstallFfmpegAsync()
    {
        try
        {
            SetUiBusy(true);
            var path = await PerformToolInstallAsync((installer, progressReporter) => installer.DownloadFfmpegAsync(UserSettings.GetToolsDirectory(), progressReporter)).ConfigureAwait(true);
            _ffmpegPath = path;
            UpdateStatus("FFmpeg installed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Install error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetUiBusy(false);
        }
    }

    private void ResetSettings()
    {
        var confirm = MessageBox.Show(this, "Reset settings to defaults?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            File.Delete(Path.Combine(UserSettings.GetSettingsDirectory(), "settings.json"));
        }
        catch
        {
            // ignore
        }
        Application.Restart();
    }

    private void ClearTempFiles()
    {
        var tempDir = Path.Combine(UserSettings.GetSettingsDirectory(), "temp");
        if (!Directory.Exists(tempDir))
        {
            UpdateStatus("No temp files to clear.");
            return;
        }

        foreach (var file in Directory.EnumerateFiles(tempDir, "*", SearchOption.AllDirectories))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // ignore
            }
        }

        foreach (var directory in Directory.EnumerateDirectories(tempDir, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
                // ignore
            }
        }

        UpdateStatus("Temp files cleared.");
    }

    private void OpenFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Open folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleBatchMode()
    {
        pnlMultiList.Enabled = ckMulti.Checked;
        pnlMultiList.Visible = ckMulti.Checked;
        btnAddUrl.Enabled = ckMulti.Checked && btnFetch.Enabled;
    }

    private void ToggleProxyInputs()
    {
        var enabled = ckUseProxy.Checked;
        tbProxyHost.Enabled = enabled;
        tbProxyPort.Enabled = enabled;
        tbProxyUser.Enabled = enabled;
        tbProxyPass.Enabled = enabled;
    }

    private void ToggleAdvanced()
    {
        UpdateAdvancedVisibility(ckAdvanced.Checked);
    }

    private void UpdateAdvancedVisibility(bool visible)
    {
        pnlAdvancedHost.Visible = visible;
        AdjustFormHeight(visible);
    }

    private void AdjustFormHeight(bool advanced)
    {
        if (!advanced)
        {
            Height = 420;
            return;
        }

        var screen = Screen.FromControl(this);
        var desired = mainLayout.Height + pnlAdvancedHost.PreferredSize.Height + 80;
        Height = Math.Min(screen.WorkingArea.Height - 80, desired);
    }

    private void BrowseSaveFolder()
    {
        using (var dialog = new FolderBrowserDialog
        {
            SelectedPath = tbSave.Text
        })
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                tbSave.Text = dialog.SelectedPath;
            }
        }
    }

    private void OpenSaveFolder()
    {
        if (Directory.Exists(tbSave.Text))
        {
            OpenFolder(tbSave.Text);
        }
        else
        {
            MessageBox.Show(this, "Save folder does not exist.", "Open", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void EnqueueCurrentUrl()
    {
        var url = tbUrl.Text.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        AddUrlToQueue(url);
        tbUrl.Clear();
    }

    private void AddUrlToQueue(string url)
    {
        if (_batchUrls.Contains(url, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        _batchUrls.Add(url);
        var row = pnlMultiList.RowCount++;
        pnlMultiList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var panel = new FlowLayoutPanel { AutoSize = true };
        var label = new Label { Text = url, AutoSize = true, MaximumSize = new System.Drawing.Size(400, 0) };
        var removeButton = new Button { Text = "Remove", AutoSize = true };
        removeButton.Click += (sender, args) => RemoveUrl(url, panel);
        panel.Controls.Add(label);
        panel.Controls.Add(removeButton);
        pnlMultiList.Controls.Add(panel, 0, row);
    }

    private void RemoveUrl(string url, Control panel)
    {
        _batchUrls.Remove(url);
        pnlMultiList.Controls.Remove(panel);
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim().Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }

    private string GenerateUniqueBaseName(string baseName)
    {
        var candidate = baseName;
        var counter = 1;
        while (Directory.GetFiles(tbSave.Text, $"{candidate}.*").Length > 0)
        {
            candidate = $"{baseName} ({counter++})";
        }

        return candidate;
    }

    private void ApplySettings()
    {
        tbSave.Text = _settings.SaveDirectory;
    }

    private void PersistSettings()
    {
        _settings.SaveDirectory = tbSave.Text;
        _settings.AdvancedMode = ckAdvanced.Checked;
        _settings.UseProxy = ckUseProxy.Checked;
        _settings.ProxyHost = tbProxyHost.Text;
        _settings.ProxyUser = tbProxyUser.Text;
        _settings.ProxyPassword = tbProxyPass.Text;
        _settings.ProxyPort = int.TryParse(tbProxyPort.Text, out var port) ? port : 0;
        _settings.LastUrl = tbUrl.Text;
        _settings.AudioOnly = ckAudio.Checked;
        _settings.CompatibilityMode = rbCompatOn.Checked;
        _settings.BatchMode = ckMulti.Checked;
        _settings.BatchUrls = _batchUrls.ToList();
        _settings.YtDlpPath = _ytDlpPath;
        _settings.FfmpegPath = _ffmpegPath;
        _settings.Save();
    }

    private void SetUiBusy(bool busy)
    {
        btnFetch.Enabled = btnDownload.Enabled = btnBrowse.Enabled = btnOpen.Enabled = btnTools.Enabled = !busy;
        btnAddUrl.Enabled = !busy && ckMulti.Checked;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void UpdateStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStatus(text)));
            return;
        }

        lblStatus.Text = text;
    }

    private sealed class QualityItem
    {
        public QualityItem(string label, string? formatId, int? maxHeight)
        {
            Label = label;
            FormatId = formatId;
            MaxHeight = maxHeight;
        }

        public string Label { get; }

        public string? FormatId { get; }

        public int? MaxHeight { get; }

        public override string ToString()
        {
            return Label;
        }
    }
}
}
