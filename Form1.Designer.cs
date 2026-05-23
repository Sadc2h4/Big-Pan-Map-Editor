namespace PikminUnitEditor;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainerMain;
    private Panel panelSidebarHost;
    private Panel panelSidebarContentHost;
    private Panel panelLeftTabHost;
    private Button buttonToggleLeftPane;
    private Panel panelSidebarScroll;
    private TableLayoutPanel tableLayoutPanelSidebar;
    private GroupBox groupBoxCommon;
    private TableLayoutPanel tableLayoutPanelCommon;
    private Label labelToolkit;
    private TextBox textBoxToolkitPath;
    private Button buttonBrowseToolkit;
    private Label labelToolkitStatus;
    private Label labelDiscRoot;
    private TextBox textBoxDiscRoot;
    private Button buttonBrowseDisc;
    private Label labelLoadFormat;
    private TextBox textBoxLoadFormat;
    private Label labelMode;
    private ComboBox comboBoxMode;
    private CheckBox checkBoxObjDirectView;
    private CheckBox checkBoxSpawnOverlay;
    private CheckBox checkBoxRouteOverlay;
    private GroupBox groupBoxDisc;
    private TableLayoutPanel tableLayoutPanelDisc;
    private Label labelArcPath;
    private TextBox textBoxArcPath;
    private Label labelUnitsPath;
    private TextBox textBoxUnitsPath;
    private Label labelUnitSet;
    private TextBox textBoxUnitSet;
    private Label labelCacheProgress;
    private ProgressBar progressBarCache;
    private Button buttonPrepareCache;
    private GroupBox groupBoxTemplates;
    private TableLayoutPanel tableLayoutPanelTemplates;
    private Label labelTemplateRoot;
    private Panel panelTemplateCardsScroll;
    private FlowLayoutPanel flowLayoutPanelTemplateCards;
    private Button buttonReloadTemplates;
    private GroupBox groupBoxFloorSummary;
    private TableLayoutPanel tableLayoutPanelFloorSummary;
    private RichTextBox richTextBoxConsole;
    private Panel panelMapUnitShell;
    private Panel panelMapUnitTabHost;
    private Button buttonToggleMapUnitPane;
    private Panel panelMapUnitHost;
    private FlowLayoutPanel flowLayoutPanelRouteEdit;
    private Button buttonSpawnMoveMode;
    private Button buttonSaveLayout;
    private Button buttonRouteMoveMode;
    private Button buttonSaveRoute;
    private Panel panelPreview;
    private Panel panelInspectorShell;
    private Panel panelInspectorPanelHost;
    private Panel panelInspectorTabHost;
    private Button buttonToggleRightPane;
    private Panel panelInspectorContent;
    private PictureBox pictureBoxPreview;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        splitContainerMain = new SplitContainer();
        panelSidebarHost = new Panel();
        panelSidebarContentHost = new Panel();
        panelSidebarScroll = new Panel();
        tableLayoutPanelSidebar = new TableLayoutPanel();
        groupBoxCommon = new GroupBox();
        tableLayoutPanelCommon = new TableLayoutPanel();
        labelToolkit = new Label();
        textBoxToolkitPath = new TextBox();
        buttonBrowseToolkit = new Button();
        labelToolkitStatus = new Label();
        labelDiscRoot = new Label();
        textBoxDiscRoot = new TextBox();
        buttonBrowseDisc = new Button();
        labelLoadFormat = new Label();
        textBoxLoadFormat = new TextBox();
        labelMode = new Label();
        comboBoxMode = new ComboBox();
        groupBoxDisc = new GroupBox();
        tableLayoutPanelDisc = new TableLayoutPanel();
        labelArcPath = new Label();
        textBoxArcPath = new TextBox();
        labelUnitsPath = new Label();
        textBoxUnitsPath = new TextBox();
        labelUnitSet = new Label();
        textBoxUnitSet = new TextBox();
        labelCacheProgress = new Label();
        progressBarCache = new ProgressBar();
        buttonPrepareCache = new Button();
        groupBoxFloorSummary = new GroupBox();
        tableLayoutPanelFloorSummary = new TableLayoutPanel();
        richTextBoxConsole = new RichTextBox();
        panelLeftTabHost = new Panel();
        buttonToggleLeftPane = new Button();
        panelPreview = new Panel();
        pictureBoxPreview = new PictureBox();
        panelMapUnitShell = new Panel();
        panelMapUnitHost = new Panel();
        groupBoxTemplates = new GroupBox();
        tableLayoutPanelTemplates = new TableLayoutPanel();
        labelTemplateRoot = new Label();
        panelTemplateCardsScroll = new Panel();
        flowLayoutPanelTemplateCards = new FlowLayoutPanel();
        buttonReloadTemplates = new Button();
        panelMapUnitTabHost = new Panel();
        buttonToggleMapUnitPane = new Button();
        panelInspectorShell = new Panel();
        panelInspectorPanelHost = new Panel();
        panelInspectorContent = new Panel();
        panelInspectorTabHost = new Panel();
        buttonToggleRightPane = new Button();
        checkBoxObjDirectView = new CheckBox();
        checkBoxSpawnOverlay = new CheckBox();
        checkBoxRouteOverlay = new CheckBox();
        flowLayoutPanelRouteEdit = new FlowLayoutPanel();
        buttonSpawnMoveMode = new Button();
        buttonSaveLayout = new Button();
        buttonRouteMoveMode = new Button();
        buttonSaveRoute = new Button();
        menuStrip1 = new MenuStrip();
        ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
        splitContainerMain.Panel1.SuspendLayout();
        splitContainerMain.Panel2.SuspendLayout();
        splitContainerMain.SuspendLayout();
        panelSidebarHost.SuspendLayout();
        panelSidebarContentHost.SuspendLayout();
        panelSidebarScroll.SuspendLayout();
        tableLayoutPanelSidebar.SuspendLayout();
        groupBoxCommon.SuspendLayout();
        tableLayoutPanelCommon.SuspendLayout();
        groupBoxDisc.SuspendLayout();
        tableLayoutPanelDisc.SuspendLayout();
        groupBoxFloorSummary.SuspendLayout();
        tableLayoutPanelFloorSummary.SuspendLayout();
        panelLeftTabHost.SuspendLayout();
        panelPreview.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
        panelMapUnitShell.SuspendLayout();
        panelMapUnitHost.SuspendLayout();
        groupBoxTemplates.SuspendLayout();
        tableLayoutPanelTemplates.SuspendLayout();
        panelTemplateCardsScroll.SuspendLayout();
        panelMapUnitTabHost.SuspendLayout();
        panelInspectorShell.SuspendLayout();
        panelInspectorPanelHost.SuspendLayout();
        panelInspectorTabHost.SuspendLayout();
        SuspendLayout();
        // 
        // splitContainerMain
        // 
        splitContainerMain.Dock = DockStyle.Fill;
        splitContainerMain.Location = new Point(0, 24);
        splitContainerMain.Name = "splitContainerMain";
        // 
        // splitContainerMain.Panel1
        // 
        splitContainerMain.Panel1.Controls.Add(panelSidebarHost);
        // 
        // splitContainerMain.Panel2
        // 
        splitContainerMain.Panel2.Controls.Add(panelPreview);
        splitContainerMain.Panel2.Controls.Add(panelMapUnitShell);
        splitContainerMain.Panel2.Controls.Add(panelInspectorShell);
        splitContainerMain.Size = new Size(1680, 956);
        splitContainerMain.SplitterDistance = 520;
        splitContainerMain.TabIndex = 0;
        // 
        // panelSidebarHost
        // 
        panelSidebarHost.Controls.Add(panelSidebarContentHost);
        panelSidebarHost.Controls.Add(panelLeftTabHost);
        panelSidebarHost.Dock = DockStyle.Fill;
        panelSidebarHost.Location = new Point(0, 0);
        panelSidebarHost.Name = "panelSidebarHost";
        panelSidebarHost.Size = new Size(520, 956);
        panelSidebarHost.TabIndex = 0;
        // 
        // panelSidebarContentHost
        // 
        panelSidebarContentHost.Controls.Add(panelSidebarScroll);
        panelSidebarContentHost.Dock = DockStyle.Fill;
        panelSidebarContentHost.Location = new Point(0, 0);
        panelSidebarContentHost.Name = "panelSidebarContentHost";
        panelSidebarContentHost.Size = new Size(486, 956);
        panelSidebarContentHost.TabIndex = 0;
        // 
        // panelSidebarScroll
        // 
        panelSidebarScroll.AutoScroll = true;
        panelSidebarScroll.Controls.Add(tableLayoutPanelSidebar);
        panelSidebarScroll.Dock = DockStyle.Fill;
        panelSidebarScroll.Location = new Point(0, 0);
        panelSidebarScroll.Name = "panelSidebarScroll";
        panelSidebarScroll.Size = new Size(486, 956);
        panelSidebarScroll.TabIndex = 0;
        // 
        // tableLayoutPanelSidebar
        // 
        tableLayoutPanelSidebar.AutoSize = true;
        tableLayoutPanelSidebar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanelSidebar.ColumnCount = 1;
        tableLayoutPanelSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelSidebar.Controls.Add(groupBoxCommon, 0, 0);
        tableLayoutPanelSidebar.Controls.Add(groupBoxDisc, 0, 1);
        tableLayoutPanelSidebar.Controls.Add(groupBoxFloorSummary, 0, 2);
        tableLayoutPanelSidebar.Dock = DockStyle.Top;
        tableLayoutPanelSidebar.Location = new Point(0, 0);
        tableLayoutPanelSidebar.Name = "tableLayoutPanelSidebar";
        tableLayoutPanelSidebar.Padding = new Padding(12);
        tableLayoutPanelSidebar.RowCount = 3;
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 276F));
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 252F));
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelSidebar.Size = new Size(465, 1010);
        tableLayoutPanelSidebar.TabIndex = 0;
        // 
        // groupBoxCommon
        // 
        groupBoxCommon.Controls.Add(tableLayoutPanelCommon);
        groupBoxCommon.Dock = DockStyle.Fill;
        groupBoxCommon.Location = new Point(15, 15);
        groupBoxCommon.Name = "groupBoxCommon";
        groupBoxCommon.Size = new Size(435, 270);
        groupBoxCommon.TabIndex = 0;
        groupBoxCommon.TabStop = false;
        groupBoxCommon.Text = "共通設定";
        // 
        // tableLayoutPanelCommon
        // 
        tableLayoutPanelCommon.ColumnCount = 3;
        tableLayoutPanelCommon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        tableLayoutPanelCommon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelCommon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
        tableLayoutPanelCommon.Controls.Add(labelToolkit, 0, 0);
        tableLayoutPanelCommon.Controls.Add(textBoxToolkitPath, 1, 0);
        tableLayoutPanelCommon.Controls.Add(buttonBrowseToolkit, 2, 0);
        tableLayoutPanelCommon.Controls.Add(labelToolkitStatus, 1, 1);
        tableLayoutPanelCommon.Controls.Add(labelDiscRoot, 0, 2);
        tableLayoutPanelCommon.Controls.Add(textBoxDiscRoot, 1, 2);
        tableLayoutPanelCommon.Controls.Add(buttonBrowseDisc, 2, 2);
        tableLayoutPanelCommon.Controls.Add(labelLoadFormat, 0, 3);
        tableLayoutPanelCommon.Controls.Add(textBoxLoadFormat, 1, 3);
        tableLayoutPanelCommon.Controls.Add(labelMode, 0, 4);
        tableLayoutPanelCommon.Controls.Add(comboBoxMode, 1, 4);
        tableLayoutPanelCommon.Dock = DockStyle.Fill;
        tableLayoutPanelCommon.Location = new Point(3, 23);
        tableLayoutPanelCommon.Name = "tableLayoutPanelCommon";
        tableLayoutPanelCommon.RowCount = 6;
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        tableLayoutPanelCommon.Size = new Size(429, 244);
        tableLayoutPanelCommon.TabIndex = 0;
        // 
        // labelToolkit
        // 
        labelToolkit.Anchor = AnchorStyles.Left;
        labelToolkit.AutoSize = true;
        labelToolkit.Location = new Point(3, 7);
        labelToolkit.Name = "labelToolkit";
        labelToolkit.Size = new Size(53, 20);
        labelToolkit.TabIndex = 0;
        labelToolkit.Text = "Toolkit";
        // 
        // textBoxToolkitPath
        // 
        textBoxToolkitPath.Dock = DockStyle.Fill;
        textBoxToolkitPath.Location = new Point(95, 3);
        textBoxToolkitPath.Name = "textBoxToolkitPath";
        textBoxToolkitPath.Size = new Size(249, 27);
        textBoxToolkitPath.TabIndex = 1;
        // 
        // buttonBrowseToolkit
        // 
        buttonBrowseToolkit.Dock = DockStyle.Fill;
        buttonBrowseToolkit.Location = new Point(350, 3);
        buttonBrowseToolkit.Name = "buttonBrowseToolkit";
        buttonBrowseToolkit.Size = new Size(76, 28);
        buttonBrowseToolkit.TabIndex = 2;
        buttonBrowseToolkit.Text = "参照";
        buttonBrowseToolkit.UseVisualStyleBackColor = true;
        buttonBrowseToolkit.Click += buttonBrowseToolkit_Click;
        // 
        // labelToolkitStatus
        // 
        labelToolkitStatus.Anchor = AnchorStyles.Left;
        labelToolkitStatus.AutoSize = true;
        tableLayoutPanelCommon.SetColumnSpan(labelToolkitStatus, 2);
        labelToolkitStatus.Location = new Point(95, 38);
        labelToolkitStatus.Name = "labelToolkitStatus";
        labelToolkitStatus.Size = new Size(173, 20);
        labelToolkitStatus.TabIndex = 3;
        labelToolkitStatus.Text = "Hocotate_Toolkit: 未設定";
        // 
        // labelDiscRoot
        // 
        labelDiscRoot.Anchor = AnchorStyles.Left;
        labelDiscRoot.AutoSize = true;
        labelDiscRoot.Location = new Point(3, 69);
        labelDiscRoot.Name = "labelDiscRoot";
        labelDiscRoot.Size = new Size(54, 20);
        labelDiscRoot.TabIndex = 4;
        labelDiscRoot.Text = "参照先";
        // 
        // textBoxDiscRoot
        // 
        textBoxDiscRoot.Dock = DockStyle.Fill;
        textBoxDiscRoot.Location = new Point(95, 65);
        textBoxDiscRoot.Name = "textBoxDiscRoot";
        textBoxDiscRoot.Size = new Size(249, 27);
        textBoxDiscRoot.TabIndex = 5;
        // 
        // buttonBrowseDisc
        // 
        buttonBrowseDisc.Dock = DockStyle.Fill;
        buttonBrowseDisc.Location = new Point(350, 65);
        buttonBrowseDisc.Name = "buttonBrowseDisc";
        buttonBrowseDisc.Size = new Size(76, 28);
        buttonBrowseDisc.TabIndex = 6;
        buttonBrowseDisc.Text = "参照";
        buttonBrowseDisc.UseVisualStyleBackColor = true;
        buttonBrowseDisc.Click += buttonBrowseDisc_Click;
        // 
        // labelLoadFormat
        // 
        labelLoadFormat.Anchor = AnchorStyles.Left;
        labelLoadFormat.AutoSize = true;
        labelLoadFormat.Location = new Point(3, 103);
        labelLoadFormat.Name = "labelLoadFormat";
        labelLoadFormat.Size = new Size(69, 20);
        labelLoadFormat.TabIndex = 7;
        labelLoadFormat.Text = "読込形式";
        // 
        // textBoxLoadFormat
        // 
        tableLayoutPanelCommon.SetColumnSpan(textBoxLoadFormat, 2);
        textBoxLoadFormat.Dock = DockStyle.Fill;
        textBoxLoadFormat.Location = new Point(95, 99);
        textBoxLoadFormat.Name = "textBoxLoadFormat";
        textBoxLoadFormat.ReadOnly = true;
        textBoxLoadFormat.Size = new Size(331, 27);
        textBoxLoadFormat.TabIndex = 8;
        // 
        // labelMode
        // 
        labelMode.Anchor = AnchorStyles.Left;
        labelMode.AutoSize = true;
        labelMode.Location = new Point(3, 137);
        labelMode.Name = "labelMode";
        labelMode.Size = new Size(41, 20);
        labelMode.TabIndex = 9;
        labelMode.Text = "モード";
        // 
        // comboBoxMode
        // 
        comboBoxMode.Dock = DockStyle.Fill;
        comboBoxMode.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxMode.FormattingEnabled = true;
        comboBoxMode.Items.AddRange(new object[] { "地上マップ", "洞窟ユニット" });
        comboBoxMode.Location = new Point(95, 133);
        comboBoxMode.Name = "comboBoxMode";
        comboBoxMode.Size = new Size(249, 28);
        comboBoxMode.TabIndex = 10;
        comboBoxMode.SelectedIndexChanged += comboBoxMode_SelectedIndexChanged;
        // 
        // groupBoxDisc
        // 
        groupBoxDisc.Controls.Add(tableLayoutPanelDisc);
        groupBoxDisc.Dock = DockStyle.Fill;
        groupBoxDisc.Location = new Point(15, 291);
        groupBoxDisc.Name = "groupBoxDisc";
        groupBoxDisc.Size = new Size(435, 246);
        groupBoxDisc.TabIndex = 1;
        groupBoxDisc.TabStop = false;
        groupBoxDisc.Text = "洞窟参照";
        // 
        // tableLayoutPanelDisc
        // 
        tableLayoutPanelDisc.ColumnCount = 3;
        tableLayoutPanelDisc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        tableLayoutPanelDisc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelDisc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
        tableLayoutPanelDisc.Controls.Add(labelArcPath, 0, 0);
        tableLayoutPanelDisc.Controls.Add(textBoxArcPath, 1, 0);
        tableLayoutPanelDisc.Controls.Add(labelUnitsPath, 0, 1);
        tableLayoutPanelDisc.Controls.Add(textBoxUnitsPath, 1, 1);
        tableLayoutPanelDisc.Controls.Add(labelUnitSet, 0, 2);
        tableLayoutPanelDisc.Controls.Add(textBoxUnitSet, 1, 2);
        tableLayoutPanelDisc.Controls.Add(labelCacheProgress, 1, 3);
        tableLayoutPanelDisc.Controls.Add(progressBarCache, 1, 4);
        tableLayoutPanelDisc.Controls.Add(buttonPrepareCache, 1, 5);
        tableLayoutPanelDisc.Dock = DockStyle.Fill;
        tableLayoutPanelDisc.Location = new Point(3, 23);
        tableLayoutPanelDisc.Name = "tableLayoutPanelDisc";
        tableLayoutPanelDisc.RowCount = 6;
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tableLayoutPanelDisc.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tableLayoutPanelDisc.Size = new Size(429, 220);
        tableLayoutPanelDisc.TabIndex = 0;
        // 
        // labelArcPath
        // 
        labelArcPath.Anchor = AnchorStyles.Left;
        labelArcPath.AutoSize = true;
        labelArcPath.Location = new Point(3, 7);
        labelArcPath.Name = "labelArcPath";
        labelArcPath.Size = new Size(29, 20);
        labelArcPath.TabIndex = 0;
        labelArcPath.Text = "arc";
        // 
        // textBoxArcPath
        // 
        textBoxArcPath.Dock = DockStyle.Fill;
        textBoxArcPath.Location = new Point(95, 3);
        textBoxArcPath.Name = "textBoxArcPath";
        textBoxArcPath.ReadOnly = true;
        textBoxArcPath.Size = new Size(249, 27);
        textBoxArcPath.TabIndex = 1;
        // 
        // labelUnitsPath
        // 
        labelUnitsPath.Anchor = AnchorStyles.Left;
        labelUnitsPath.AutoSize = true;
        labelUnitsPath.Location = new Point(3, 41);
        labelUnitsPath.Name = "labelUnitsPath";
        labelUnitsPath.Size = new Size(40, 20);
        labelUnitsPath.TabIndex = 2;
        labelUnitsPath.Text = "units";
        // 
        // textBoxUnitsPath
        // 
        textBoxUnitsPath.Dock = DockStyle.Fill;
        textBoxUnitsPath.Location = new Point(95, 37);
        textBoxUnitsPath.Name = "textBoxUnitsPath";
        textBoxUnitsPath.ReadOnly = true;
        textBoxUnitsPath.Size = new Size(249, 27);
        textBoxUnitsPath.TabIndex = 3;
        // 
        // labelUnitSet
        // 
        labelUnitSet.Anchor = AnchorStyles.Left;
        labelUnitSet.AutoSize = true;
        labelUnitSet.Location = new Point(3, 75);
        labelUnitSet.Name = "labelUnitSet";
        labelUnitSet.Size = new Size(57, 20);
        labelUnitSet.TabIndex = 4;
        labelUnitSet.Text = "UnitSet";
        // 
        // textBoxUnitSet
        // 
        textBoxUnitSet.Dock = DockStyle.Fill;
        textBoxUnitSet.Location = new Point(95, 71);
        textBoxUnitSet.Name = "textBoxUnitSet";
        textBoxUnitSet.ReadOnly = true;
        textBoxUnitSet.Size = new Size(249, 27);
        textBoxUnitSet.TabIndex = 5;
        // 
        // labelCacheProgress
        // 
        labelCacheProgress.Anchor = AnchorStyles.Left;
        labelCacheProgress.AutoSize = true;
        tableLayoutPanelDisc.SetColumnSpan(labelCacheProgress, 2);
        labelCacheProgress.Location = new Point(95, 106);
        labelCacheProgress.Name = "labelCacheProgress";
        labelCacheProgress.Size = new Size(78, 20);
        labelCacheProgress.TabIndex = 6;
        labelCacheProgress.Text = "待機中です";
        // 
        // progressBarCache
        // 
        tableLayoutPanelDisc.SetColumnSpan(progressBarCache, 2);
        progressBarCache.Dock = DockStyle.Fill;
        progressBarCache.Location = new Point(95, 133);
        progressBarCache.Name = "progressBarCache";
        progressBarCache.Size = new Size(331, 28);
        progressBarCache.TabIndex = 7;
        // 
        // buttonPrepareCache
        // 
        tableLayoutPanelDisc.SetColumnSpan(buttonPrepareCache, 2);
        buttonPrepareCache.Dock = DockStyle.Fill;
        buttonPrepareCache.Location = new Point(95, 167);
        buttonPrepareCache.Name = "buttonPrepareCache";
        buttonPrepareCache.Size = new Size(331, 50);
        buttonPrepareCache.TabIndex = 8;
        buttonPrepareCache.Text = "洞窟ユニットのキャッシュを準備";
        buttonPrepareCache.UseVisualStyleBackColor = true;
        buttonPrepareCache.Click += buttonPrepareCache_Click;
        // 
        // groupBoxFloorSummary
        // 
        groupBoxFloorSummary.Controls.Add(tableLayoutPanelFloorSummary);
        groupBoxFloorSummary.Dock = DockStyle.Fill;
        groupBoxFloorSummary.Location = new Point(15, 543);
        groupBoxFloorSummary.Name = "groupBoxFloorSummary";
        groupBoxFloorSummary.Size = new Size(435, 452);
        groupBoxFloorSummary.TabIndex = 3;
        groupBoxFloorSummary.TabStop = false;
        groupBoxFloorSummary.Text = "Console";
        // 
        // tableLayoutPanelFloorSummary
        // 
        tableLayoutPanelFloorSummary.ColumnCount = 1;
        tableLayoutPanelFloorSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelFloorSummary.Controls.Add(richTextBoxConsole, 0, 0);
        tableLayoutPanelFloorSummary.Dock = DockStyle.Fill;
        tableLayoutPanelFloorSummary.Location = new Point(3, 23);
        tableLayoutPanelFloorSummary.Name = "tableLayoutPanelFloorSummary";
        tableLayoutPanelFloorSummary.RowCount = 1;
        tableLayoutPanelFloorSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelFloorSummary.Size = new Size(429, 426);
        tableLayoutPanelFloorSummary.TabIndex = 0;
        // 
        // richTextBoxConsole
        // 
        richTextBoxConsole.BackColor = Color.Black;
        richTextBoxConsole.BorderStyle = BorderStyle.None;
        richTextBoxConsole.Dock = DockStyle.Fill;
        richTextBoxConsole.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        richTextBoxConsole.ForeColor = Color.White;
        richTextBoxConsole.Location = new Point(3, 3);
        richTextBoxConsole.Name = "richTextBoxConsole";
        richTextBoxConsole.ReadOnly = true;
        richTextBoxConsole.Size = new Size(423, 420);
        richTextBoxConsole.TabIndex = 0;
        richTextBoxConsole.Text = "";
        // 
        // panelLeftTabHost
        // 
        panelLeftTabHost.Controls.Add(buttonToggleLeftPane);
        panelLeftTabHost.Dock = DockStyle.Right;
        panelLeftTabHost.Location = new Point(486, 0);
        panelLeftTabHost.Name = "panelLeftTabHost";
        panelLeftTabHost.Padding = new Padding(2, 16, 2, 16);
        panelLeftTabHost.Size = new Size(34, 956);
        panelLeftTabHost.TabIndex = 1;
        // 
        // buttonToggleLeftPane
        // 
        buttonToggleLeftPane.Dock = DockStyle.Top;
        buttonToggleLeftPane.FlatStyle = FlatStyle.Popup;
        buttonToggleLeftPane.Location = new Point(2, 16);
        buttonToggleLeftPane.Name = "buttonToggleLeftPane";
        buttonToggleLeftPane.Size = new Size(30, 54);
        buttonToggleLeftPane.TabIndex = 0;
        buttonToggleLeftPane.Text = "<";
        buttonToggleLeftPane.UseVisualStyleBackColor = true;
        buttonToggleLeftPane.Click += buttonToggleLeftPane_Click;
        // 
        // panelPreview
        // 
        panelPreview.AutoScroll = true;
        panelPreview.BackColor = Color.FromArgb(232, 227, 213);
        panelPreview.Controls.Add(pictureBoxPreview);
        panelPreview.Dock = DockStyle.Fill;
        panelPreview.Location = new Point(352, 0);
        panelPreview.Name = "panelPreview";
        panelPreview.Padding = new Padding(16);
        panelPreview.Size = new Size(456, 956);
        panelPreview.TabIndex = 0;
        // 
        // pictureBoxPreview
        // 
        pictureBoxPreview.Location = new Point(16, 16);
        pictureBoxPreview.Name = "pictureBoxPreview";
        pictureBoxPreview.Size = new Size(960, 960);
        pictureBoxPreview.SizeMode = PictureBoxSizeMode.AutoSize;
        pictureBoxPreview.TabIndex = 0;
        pictureBoxPreview.TabStop = false;
        // 
        // panelMapUnitShell
        // 
        panelMapUnitShell.Controls.Add(panelMapUnitHost);
        panelMapUnitShell.Controls.Add(panelMapUnitTabHost);
        panelMapUnitShell.Dock = DockStyle.Left;
        panelMapUnitShell.Location = new Point(0, 0);
        panelMapUnitShell.Name = "panelMapUnitShell";
        panelMapUnitShell.Size = new Size(352, 956);
        panelMapUnitShell.TabIndex = 2;
        // 
        // panelMapUnitHost
        // 
        panelMapUnitHost.BackColor = Color.FromArgb(244, 241, 232);
        panelMapUnitHost.Controls.Add(groupBoxTemplates);
        panelMapUnitHost.Dock = DockStyle.Fill;
        panelMapUnitHost.Location = new Point(0, 0);
        panelMapUnitHost.Name = "panelMapUnitHost";
        panelMapUnitHost.Padding = new Padding(12);
        panelMapUnitHost.Size = new Size(318, 956);
        panelMapUnitHost.TabIndex = 0;
        // 
        // groupBoxTemplates
        // 
        groupBoxTemplates.Controls.Add(tableLayoutPanelTemplates);
        groupBoxTemplates.Dock = DockStyle.Fill;
        groupBoxTemplates.Location = new Point(12, 12);
        groupBoxTemplates.Name = "groupBoxTemplates";
        groupBoxTemplates.Size = new Size(294, 932);
        groupBoxTemplates.TabIndex = 2;
        groupBoxTemplates.TabStop = false;
        groupBoxTemplates.Text = "マップユニット表示エリア";
        // 
        // tableLayoutPanelTemplates
        // 
        tableLayoutPanelTemplates.ColumnCount = 1;
        tableLayoutPanelTemplates.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelTemplates.Controls.Add(labelTemplateRoot, 0, 0);
        tableLayoutPanelTemplates.Controls.Add(panelTemplateCardsScroll, 0, 1);
        tableLayoutPanelTemplates.Controls.Add(buttonReloadTemplates, 0, 2);
        tableLayoutPanelTemplates.Dock = DockStyle.Fill;
        tableLayoutPanelTemplates.Location = new Point(3, 23);
        tableLayoutPanelTemplates.Name = "tableLayoutPanelTemplates";
        tableLayoutPanelTemplates.RowCount = 3;
        tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tableLayoutPanelTemplates.Size = new Size(288, 906);
        tableLayoutPanelTemplates.TabIndex = 0;
        // 
        // labelTemplateRoot
        // 
        labelTemplateRoot.AutoEllipsis = true;
        labelTemplateRoot.Dock = DockStyle.Fill;
        labelTemplateRoot.Location = new Point(3, 0);
        labelTemplateRoot.Name = "labelTemplateRoot";
        labelTemplateRoot.Size = new Size(282, 42);
        labelTemplateRoot.TabIndex = 0;
        labelTemplateRoot.Text = "Location: 未検出";
        labelTemplateRoot.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // panelTemplateCardsScroll
        // 
        panelTemplateCardsScroll.AutoScroll = true;
        panelTemplateCardsScroll.BorderStyle = BorderStyle.FixedSingle;
        panelTemplateCardsScroll.Controls.Add(flowLayoutPanelTemplateCards);
        panelTemplateCardsScroll.Dock = DockStyle.Fill;
        panelTemplateCardsScroll.Location = new Point(3, 45);
        panelTemplateCardsScroll.Name = "panelTemplateCardsScroll";
        panelTemplateCardsScroll.Padding = new Padding(6);
        panelTemplateCardsScroll.Size = new Size(282, 820);
        panelTemplateCardsScroll.TabIndex = 1;
        // 
        // flowLayoutPanelTemplateCards
        // 
        flowLayoutPanelTemplateCards.AutoSize = true;
        flowLayoutPanelTemplateCards.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanelTemplateCards.Dock = DockStyle.Top;
        flowLayoutPanelTemplateCards.FlowDirection = FlowDirection.TopDown;
        flowLayoutPanelTemplateCards.Location = new Point(6, 6);
        flowLayoutPanelTemplateCards.Name = "flowLayoutPanelTemplateCards";
        flowLayoutPanelTemplateCards.Size = new Size(268, 0);
        flowLayoutPanelTemplateCards.TabIndex = 0;
        flowLayoutPanelTemplateCards.WrapContents = false;
        // 
        // buttonReloadTemplates
        // 
        buttonReloadTemplates.Dock = DockStyle.Right;
        buttonReloadTemplates.Location = new Point(128, 871);
        buttonReloadTemplates.Name = "buttonReloadTemplates";
        buttonReloadTemplates.Size = new Size(157, 32);
        buttonReloadTemplates.TabIndex = 2;
        buttonReloadTemplates.Text = "テンプレート再読込";
        buttonReloadTemplates.UseVisualStyleBackColor = true;
        buttonReloadTemplates.Click += buttonReloadTemplates_Click;
        // 
        // panelMapUnitTabHost
        // 
        panelMapUnitTabHost.Controls.Add(buttonToggleMapUnitPane);
        panelMapUnitTabHost.Dock = DockStyle.Right;
        panelMapUnitTabHost.Location = new Point(318, 0);
        panelMapUnitTabHost.Name = "panelMapUnitTabHost";
        panelMapUnitTabHost.Padding = new Padding(2, 16, 2, 16);
        panelMapUnitTabHost.Size = new Size(34, 956);
        panelMapUnitTabHost.TabIndex = 1;
        // 
        // buttonToggleMapUnitPane
        // 
        buttonToggleMapUnitPane.Dock = DockStyle.Top;
        buttonToggleMapUnitPane.FlatStyle = FlatStyle.Popup;
        buttonToggleMapUnitPane.Location = new Point(2, 16);
        buttonToggleMapUnitPane.Name = "buttonToggleMapUnitPane";
        buttonToggleMapUnitPane.Size = new Size(30, 54);
        buttonToggleMapUnitPane.TabIndex = 0;
        buttonToggleMapUnitPane.Text = "<";
        buttonToggleMapUnitPane.UseVisualStyleBackColor = true;
        buttonToggleMapUnitPane.Click += buttonToggleMapUnitPane_Click;
        // 
        // panelInspectorShell
        // 
        panelInspectorShell.Controls.Add(panelInspectorPanelHost);
        panelInspectorShell.Controls.Add(panelInspectorTabHost);
        panelInspectorShell.Dock = DockStyle.Right;
        panelInspectorShell.Location = new Point(808, 0);
        panelInspectorShell.Name = "panelInspectorShell";
        panelInspectorShell.Size = new Size(348, 956);
        panelInspectorShell.TabIndex = 1;
        // 
        // panelInspectorPanelHost
        // 
        panelInspectorPanelHost.BackColor = Color.FromArgb(244, 241, 232);
        panelInspectorPanelHost.Controls.Add(panelInspectorContent);
        panelInspectorPanelHost.Dock = DockStyle.Fill;
        panelInspectorPanelHost.Location = new Point(28, 0);
        panelInspectorPanelHost.Name = "panelInspectorPanelHost";
        panelInspectorPanelHost.Padding = new Padding(12);
        panelInspectorPanelHost.Size = new Size(320, 956);
        panelInspectorPanelHost.TabIndex = 0;
        // 
        // panelInspectorContent
        // 
        panelInspectorContent.AutoScroll = true;
        panelInspectorContent.Dock = DockStyle.Fill;
        panelInspectorContent.Location = new Point(12, 12);
        panelInspectorContent.Name = "panelInspectorContent";
        panelInspectorContent.Size = new Size(296, 932);
        panelInspectorContent.TabIndex = 0;
        // 
        // panelInspectorTabHost
        // 
        panelInspectorTabHost.Controls.Add(buttonToggleRightPane);
        panelInspectorTabHost.Dock = DockStyle.Left;
        panelInspectorTabHost.Location = new Point(0, 0);
        panelInspectorTabHost.Name = "panelInspectorTabHost";
        panelInspectorTabHost.Padding = new Padding(2, 16, 2, 16);
        panelInspectorTabHost.Size = new Size(28, 956);
        panelInspectorTabHost.TabIndex = 1;
        // 
        // buttonToggleRightPane
        // 
        buttonToggleRightPane.Dock = DockStyle.Top;
        buttonToggleRightPane.FlatStyle = FlatStyle.Popup;
        buttonToggleRightPane.Location = new Point(2, 16);
        buttonToggleRightPane.Name = "buttonToggleRightPane";
        buttonToggleRightPane.Size = new Size(24, 54);
        buttonToggleRightPane.TabIndex = 0;
        buttonToggleRightPane.Text = ">";
        buttonToggleRightPane.UseVisualStyleBackColor = true;
        buttonToggleRightPane.Click += buttonToggleRightPane_Click;
        // 
        // checkBoxObjDirectView
        // 
        checkBoxObjDirectView.Anchor = AnchorStyles.Left;
        checkBoxObjDirectView.AutoSize = true;
        checkBoxObjDirectView.Location = new Point(95, 160);
        checkBoxObjDirectView.Name = "checkBoxObjDirectView";
        checkBoxObjDirectView.Size = new Size(109, 24);
        checkBoxObjDirectView.TabIndex = 9;
        checkBoxObjDirectView.Text = "OBJ 3D表示";
        checkBoxObjDirectView.UseVisualStyleBackColor = true;
        checkBoxObjDirectView.CheckedChanged += checkBoxObjDirectView_CheckedChanged;
        // 
        // checkBoxSpawnOverlay
        // 
        checkBoxSpawnOverlay.AutoSize = true;
        checkBoxSpawnOverlay.Checked = true;
        checkBoxSpawnOverlay.CheckState = CheckState.Checked;
        checkBoxSpawnOverlay.Location = new Point(3, 3);
        checkBoxSpawnOverlay.Name = "checkBoxSpawnOverlay";
        checkBoxSpawnOverlay.Size = new Size(117, 24);
        checkBoxSpawnOverlay.TabIndex = 9;
        checkBoxSpawnOverlay.Text = "スポーンを表示";
        checkBoxSpawnOverlay.UseVisualStyleBackColor = true;
        checkBoxSpawnOverlay.CheckedChanged += checkBoxSpawnOverlay_CheckedChanged;
        // 
        // checkBoxRouteOverlay
        // 
        checkBoxRouteOverlay.AutoSize = true;
        checkBoxRouteOverlay.Checked = true;
        checkBoxRouteOverlay.CheckState = CheckState.Checked;
        checkBoxRouteOverlay.Location = new Point(3, 33);
        checkBoxRouteOverlay.Name = "checkBoxRouteOverlay";
        checkBoxRouteOverlay.Size = new Size(104, 24);
        checkBoxRouteOverlay.TabIndex = 10;
        checkBoxRouteOverlay.Text = "ルートを表示";
        checkBoxRouteOverlay.UseVisualStyleBackColor = true;
        checkBoxRouteOverlay.CheckedChanged += checkBoxRouteOverlay_CheckedChanged;
        // 
        // flowLayoutPanelRouteEdit
        // 
        flowLayoutPanelRouteEdit.Location = new Point(0, 0);
        flowLayoutPanelRouteEdit.Name = "flowLayoutPanelRouteEdit";
        flowLayoutPanelRouteEdit.Size = new Size(200, 100);
        flowLayoutPanelRouteEdit.TabIndex = 0;
        // 
        // buttonSpawnMoveMode
        // 
        buttonSpawnMoveMode.Location = new Point(3, 3);
        buttonSpawnMoveMode.Name = "buttonSpawnMoveMode";
        buttonSpawnMoveMode.Size = new Size(140, 28);
        buttonSpawnMoveMode.TabIndex = 0;
        buttonSpawnMoveMode.Text = "Spawn移動: OFF";
        buttonSpawnMoveMode.UseVisualStyleBackColor = true;
        buttonSpawnMoveMode.Click += buttonSpawnMoveMode_Click;
        // 
        // buttonSaveLayout
        // 
        buttonSaveLayout.Location = new Point(149, 3);
        buttonSaveLayout.Name = "buttonSaveLayout";
        buttonSaveLayout.Size = new Size(108, 28);
        buttonSaveLayout.TabIndex = 1;
        buttonSaveLayout.Text = "layout保存";
        buttonSaveLayout.UseVisualStyleBackColor = true;
        buttonSaveLayout.Click += buttonSaveLayout_Click;
        // 
        // buttonRouteMoveMode
        // 
        buttonRouteMoveMode.Location = new Point(263, 3);
        buttonRouteMoveMode.Name = "buttonRouteMoveMode";
        buttonRouteMoveMode.Size = new Size(140, 28);
        buttonRouteMoveMode.TabIndex = 2;
        buttonRouteMoveMode.Text = "Waypoint移動: OFF";
        buttonRouteMoveMode.UseVisualStyleBackColor = true;
        buttonRouteMoveMode.Click += buttonRouteMoveMode_Click;
        // 
        // buttonSaveRoute
        // 
        buttonSaveRoute.Location = new Point(3, 37);
        buttonSaveRoute.Name = "buttonSaveRoute";
        buttonSaveRoute.Size = new Size(88, 28);
        buttonSaveRoute.TabIndex = 3;
        buttonSaveRoute.Text = "route保存";
        buttonSaveRoute.UseVisualStyleBackColor = true;
        buttonSaveRoute.Click += buttonSaveRoute_Click;
        // 
        // menuStrip1
        // 
        menuStrip1.ImageScalingSize = new Size(20, 20);
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(1680, 24);
        menuStrip1.TabIndex = 1;
        menuStrip1.Text = "menuStrip1";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1680, 980);
        Controls.Add(splitContainerMain);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        MinimumSize = new Size(1440, 900);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Big Pan Map Editor";
        Load += Form1_Load;
        splitContainerMain.Panel1.ResumeLayout(false);
        splitContainerMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
        splitContainerMain.ResumeLayout(false);
        panelSidebarHost.ResumeLayout(false);
        panelSidebarContentHost.ResumeLayout(false);
        panelSidebarScroll.ResumeLayout(false);
        panelSidebarScroll.PerformLayout();
        tableLayoutPanelSidebar.ResumeLayout(false);
        groupBoxCommon.ResumeLayout(false);
        tableLayoutPanelCommon.ResumeLayout(false);
        tableLayoutPanelCommon.PerformLayout();
        groupBoxDisc.ResumeLayout(false);
        tableLayoutPanelDisc.ResumeLayout(false);
        tableLayoutPanelDisc.PerformLayout();
        groupBoxFloorSummary.ResumeLayout(false);
        tableLayoutPanelFloorSummary.ResumeLayout(false);
        panelLeftTabHost.ResumeLayout(false);
        panelPreview.ResumeLayout(false);
        panelPreview.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
        panelMapUnitShell.ResumeLayout(false);
        panelMapUnitHost.ResumeLayout(false);
        groupBoxTemplates.ResumeLayout(false);
        tableLayoutPanelTemplates.ResumeLayout(false);
        panelTemplateCardsScroll.ResumeLayout(false);
        panelTemplateCardsScroll.PerformLayout();
        panelMapUnitTabHost.ResumeLayout(false);
        panelInspectorShell.ResumeLayout(false);
        panelInspectorPanelHost.ResumeLayout(false);
        panelInspectorTabHost.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
    private MenuStrip menuStrip1;
}
