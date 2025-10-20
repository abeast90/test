using System.Windows.Forms;

namespace SmartYouTubeDownloader;

partial class MainForm
{
    private System.ComponentModel.IContainer components;
    private TextBox tbUrl;
    private Button btnFetch;
    private Button btnDownload;
    private Button btnBrowse;
    private Button btnOpen;
    private TextBox tbSave;
    private Label lblMeta;
    private ProgressBar progress;
    private Label lblStatus;
    private CheckBox ckAdvanced;
    private Panel pnlAdvancedHost;
    private TableLayoutPanel pnlAdvanced;
    private ComboBox cbQuality;
    private CheckBox ckAudio;
    private RadioButton rbCompatOff;
    private RadioButton rbCompatOn;
    private CheckBox ckMulti;
    private TableLayoutPanel pnlMultiList;
    private Button btnAddUrl;
    private CheckBox ckUseProxy;
    private TextBox tbProxyHost;
    private TextBox tbProxyPort;
    private TextBox tbProxyUser;
    private TextBox tbProxyPass;
    private Button btnTools;
    private ContextMenuStrip menuTools;
    private ToolTip toolTip;
    private FlowLayoutPanel pnlAdvancedContent;
    private TableLayoutPanel mainLayout;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tbUrl = new TextBox();
        btnFetch = new Button();
        btnDownload = new Button();
        btnBrowse = new Button();
        btnOpen = new Button();
        tbSave = new TextBox();
        lblMeta = new Label();
        progress = new ProgressBar();
        lblStatus = new Label();
        ckAdvanced = new CheckBox();
        pnlAdvancedHost = new Panel();
        pnlAdvancedContent = new FlowLayoutPanel();
        pnlAdvanced = new TableLayoutPanel();
        cbQuality = new ComboBox();
        ckAudio = new CheckBox();
        rbCompatOff = new RadioButton();
        rbCompatOn = new RadioButton();
        ckMulti = new CheckBox();
        pnlMultiList = new TableLayoutPanel();
        btnAddUrl = new Button();
        ckUseProxy = new CheckBox();
        tbProxyHost = new TextBox();
        tbProxyPort = new TextBox();
        tbProxyUser = new TextBox();
        tbProxyPass = new TextBox();
        btnTools = new Button();
        menuTools = new ContextMenuStrip(components);
        toolTip = new ToolTip(components);
        mainLayout = new TableLayoutPanel();
        pnlAdvancedHost.SuspendLayout();
        pnlAdvancedContent.SuspendLayout();
        SuspendLayout();
        //
        // mainLayout
        //
        mainLayout.ColumnCount = 4;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        mainLayout.RowCount = 6;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.Dock = DockStyle.Top;
        mainLayout.Padding = new Padding(10);
        mainLayout.Controls.Add(tbUrl, 0, 0);
        mainLayout.SetColumnSpan(tbUrl, 2);
        mainLayout.Controls.Add(btnFetch, 2, 0);
        mainLayout.Controls.Add(btnDownload, 3, 0);
        mainLayout.Controls.Add(tbSave, 0, 1);
        mainLayout.SetColumnSpan(tbSave, 2);
        mainLayout.Controls.Add(btnBrowse, 2, 1);
        mainLayout.Controls.Add(btnOpen, 3, 1);
        mainLayout.Controls.Add(lblMeta, 0, 2);
        mainLayout.SetColumnSpan(lblMeta, 4);
        mainLayout.Controls.Add(progress, 0, 3);
        mainLayout.SetColumnSpan(progress, 4);
        mainLayout.Controls.Add(lblStatus, 0, 4);
        mainLayout.SetColumnSpan(lblStatus, 4);
        mainLayout.Controls.Add(ckAdvanced, 0, 5);
        mainLayout.Controls.Add(btnTools, 3, 5);
        //
        // tbUrl
        //
        tbUrl.Dock = DockStyle.Fill;
        tbUrl.PlaceholderText = "YouTube URL";
        //
        // btnFetch
        //
        btnFetch.Text = "Fetch";
        btnFetch.Dock = DockStyle.Fill;
        //
        // btnDownload
        //
        btnDownload.Text = "Download";
        btnDownload.Dock = DockStyle.Fill;
        //
        // tbSave
        //
        tbSave.Dock = DockStyle.Fill;
        //
        // btnBrowse
        //
        btnBrowse.Text = "Browse";
        btnBrowse.Dock = DockStyle.Fill;
        //
        // btnOpen
        //
        btnOpen.Text = "Open";
        btnOpen.Dock = DockStyle.Fill;
        //
        // lblMeta
        //
        lblMeta.AutoSize = true;
        lblMeta.Text = "Metadata will appear here.";
        //
        // progress
        //
        progress.Dock = DockStyle.Fill;
        //
        // lblStatus
        //
        lblStatus.AutoSize = true;
        lblStatus.Text = "Status";
        //
        // ckAdvanced
        //
        ckAdvanced.Text = "Advanced mode";
        ckAdvanced.AutoSize = true;
        ckAdvanced.Dock = DockStyle.Left;
        //
        // btnTools
        //
        btnTools.Text = "Tools";
        btnTools.Dock = DockStyle.Right;
        btnTools.Click += (_, _) => menuTools.Show(btnTools, 0, btnTools.Height);
        //
        // pnlAdvancedHost
        //
        pnlAdvancedHost.Dock = DockStyle.Fill;
        pnlAdvancedHost.Padding = new Padding(10);
        pnlAdvancedHost.Controls.Add(pnlAdvancedContent);
        pnlAdvancedHost.Visible = false;
        //
        // pnlAdvancedContent
        //
        pnlAdvancedContent.Dock = DockStyle.Fill;
        pnlAdvancedContent.FlowDirection = FlowDirection.TopDown;
        pnlAdvancedContent.WrapContents = false;
        pnlAdvancedContent.AutoScroll = true;
        pnlAdvancedContent.Controls.Add(pnlAdvanced);
        //
        // pnlAdvanced
        //
        pnlAdvanced.ColumnCount = 2;
        pnlAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        pnlAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        pnlAdvanced.AutoSize = true;
        pnlAdvanced.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pnlAdvanced.Padding = new Padding(5);

        AddAdvancedRow("Quality", cbQuality);
        AddAdvancedRow("Audio only", ckAudio);

        var compatPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        compatPanel.Controls.Add(rbCompatOff);
        compatPanel.Controls.Add(rbCompatOn);
        AddAdvancedRow("Premiere compatibility", compatPanel);

        AddAdvancedRow("Batch mode", ckMulti);
        AddAdvancedRow("Batch queue", pnlMultiList);
        AddAdvancedRow("Add URL", btnAddUrl);

        var proxyGrid = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 50F),
                new ColumnStyle(SizeType.Percent, 50F)
            }
        };
        proxyGrid.Controls.Add(tbProxyHost, 0, 0);
        proxyGrid.Controls.Add(tbProxyPort, 1, 0);
        proxyGrid.Controls.Add(tbProxyUser, 0, 1);
        proxyGrid.Controls.Add(tbProxyPass, 1, 1);

        AddAdvancedRow("Use proxy", ckUseProxy);
        AddAdvancedRow("Proxy settings", proxyGrid);

        //
        // cbQuality
        //
        cbQuality.DropDownStyle = ComboBoxStyle.DropDownList;
        cbQuality.Enabled = false;
        cbQuality.Dock = DockStyle.Fill;
        //
        // ckAudio
        //
        ckAudio.AutoSize = true;
        //
        // rbCompatOff
        //
        rbCompatOff.Text = "Off";
        rbCompatOff.AutoSize = true;
        rbCompatOff.Checked = true;
        //
        // rbCompatOn
        //
        rbCompatOn.Text = "On";
        rbCompatOn.AutoSize = true;
        //
        // ckMulti
        //
        ckMulti.AutoSize = true;
        //
        // pnlMultiList
        //
        pnlMultiList.AutoSize = true;
        pnlMultiList.ColumnCount = 1;
        pnlMultiList.RowCount = 0;
        pnlMultiList.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pnlMultiList.Dock = DockStyle.Fill;
        //
        // btnAddUrl
        //
        btnAddUrl.Text = "Add";
        btnAddUrl.AutoSize = true;
        btnAddUrl.Enabled = false;
        //
        // ckUseProxy
        //
        ckUseProxy.AutoSize = true;
        //
        // tbProxyHost
        //
        tbProxyHost.PlaceholderText = "Host";
        tbProxyHost.Width = 150;
        //
        // tbProxyPort
        //
        tbProxyPort.PlaceholderText = "Port";
        tbProxyPort.Width = 80;
        //
        // tbProxyUser
        //
        tbProxyUser.PlaceholderText = "User";
        tbProxyUser.Width = 150;
        //
        // tbProxyPass
        //
        tbProxyPass.PlaceholderText = "Password";
        tbProxyPass.Width = 150;
        tbProxyPass.UseSystemPasswordChar = true;
        //
        // MainForm
        //
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        Text = "Smart YouTube Downloader";
        Controls.Add(pnlAdvancedHost);
        Controls.Add(mainLayout);
        MinimumSize = new System.Drawing.Size(720, 400);
        pnlAdvancedHost.BringToFront();
        pnlAdvancedHost.ResumeLayout(false);
        pnlAdvancedContent.ResumeLayout(false);
        pnlAdvancedContent.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void AddAdvancedRow(string labelText, Control control)
    {
        var rowIndex = pnlAdvanced.RowCount++;
        pnlAdvanced.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Padding = new Padding(0, 6, 6, 6)
        };
        pnlAdvanced.Controls.Add(label, 0, rowIndex);
        pnlAdvanced.Controls.Add(control, 1, rowIndex);
    }
}
