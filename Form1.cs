using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PikminUnitEditor;

public partial class Form1 : Form
{
    private const int WmSetRedraw = 0x000B;
    private const int HomeConsoleWidth = 1123;
    private const int HomeConsoleHeight = 714;
    private const int HomeDesignWidth = 1123;
    private const int HomeDesignHeight = 714;
    private const string ApplicationVersionText = "Version : 2.0a";
    private readonly EditorSettingsStore _settingsStore;
    private readonly DataRepository _repository;
    private EditorSettings _settings = new();
    private LayoutFile _currentLayout = new(Array.Empty<LayoutSpawn>());
    private string? _currentLayoutPath;
    private RouteFile _currentRoute = new(new Dictionary<int, RouteWaypoint>());
    private string? _currentRoutePath;
    private WaterboxFile _currentWaterbox = new(0, Array.Empty<WaterboxEntry>());
    private string? _currentWaterboxPath;
    private FieldMapData? _currentFieldMapData;
    private string? _currentPreviewUnitName;
    private string? _currentPreviewImagePath;
    private string? _currentObjPath;
    private string? _currentObjMtlPath;
    private ObjScene? _currentObjScene;
    private RectangleF _currentModelBounds = RectangleF.Empty;
    private UnitDefinition? _currentUnitDefinition;
    private readonly UnitMapView _unitMapView;
    private readonly ObjModelView _objModelView;
    private UnitMapEditMode _currentEditMode = UnitMapEditMode.Navigate;
    private string? _selectedTemplateName;
    private int? _selectedSpawnIndex;
    private int? _selectedRouteWaypointIndex;
    private int? _selectedWaterboxIndex;
    private System.Windows.Forms.Timer? _leftPaneTimer;
    private System.Windows.Forms.Timer? _mapUnitPaneTimer;
    private System.Windows.Forms.Timer? _rightPaneTimer;
    private Label? _labelInspectorSelection;
    private NumericUpDown? _numericFieldDay;
    private Label? _labelFieldActiveFiles;
    private ComboBox? _comboBoxFieldAddFile;
    private ComboBox? _comboBoxFieldAddType;
    private Button? _buttonFieldAddSpawnMode;
    private TextBox? _textBoxFieldObjectRaw;
    private Button? _buttonApplyFieldObjectRaw;
    private Button? _buttonAddSpawn;
    private Button? _buttonDeleteSpawn;
    private Button? _buttonAddWaypoint;
    private Button? _buttonDeleteWaypoint;
    private ComboBox? _comboBoxSpawnType;
    private NumericUpDown? _numericSpawnX;
    private NumericUpDown? _numericSpawnY;
    private NumericUpDown? _numericSpawnZ;
    private NumericUpDown? _numericSpawnAngle;
    private NumericUpDown? _numericSpawnRadius;
    private NumericUpDown? _numericSpawnMinCount;
    private NumericUpDown? _numericSpawnMaxCount;
    private Button? _buttonApplySpawn;
    private GroupBox? _groupBoxSelection;
    private GroupBox? _groupBoxSpawnInspector;
    private Label? _labelWaypointIndex;
    private NumericUpDown? _numericWaypointX;
    private NumericUpDown? _numericWaypointY;
    private NumericUpDown? _numericWaypointZ;
    private NumericUpDown? _numericWaypointRadius;
    private TextBox? _textBoxWaypointLinks;
    private Button? _buttonApplyWaypoint;
    private GroupBox? _groupBoxWaypointInspector;
    private NumericUpDown? _numericWaterboxX1;
    private NumericUpDown? _numericWaterboxY1;
    private NumericUpDown? _numericWaterboxZ1;
    private NumericUpDown? _numericWaterboxX2;
    private NumericUpDown? _numericWaterboxY2;
    private NumericUpDown? _numericWaterboxZ2;
    private Button? _buttonApplyWaterbox;
    private GroupBox? _groupBoxWaterboxInspector;
    private bool _inspectorUpdating;
    private QuickToolTarget _quickToolTarget = QuickToolTarget.Spawn;
    private Panel? _quickToolWindow;
    private Panel? _quickToolGrip;
    private Panel? _quickToolContentPanel;
    private Button? _buttonQuickToolMinimize;
    private Button? _buttonQuickSpawn;
    private Button? _buttonQuickRoute;
    private Button? _buttonQuickWaterbox;
    private Button? _buttonQuickAdd;
    private Button? _buttonQuickRouteDelete;
    private Button? _buttonQuickDelete;
    private Button? _buttonQuickMove;
    private Button? _buttonQuickAngle;
    private Button? _buttonQuickRadius;
    private Button? _buttonQuickConnect;
    private Button? _buttonQuickRoomConnect;
    private Button? _buttonQuickSave;
    private Button? _buttonQuickSaveAll;
    private Panel? _fieldConsoleWindow;
    private Panel? _fieldConsoleGrip;
    private Panel? _fieldConsoleContentPanel;
    private Button? _buttonFieldConsoleMinimize;
    private Button? _buttonBrowsePrimaryReference;
    private Button? _buttonBrowseSecondaryReference;
    private ComboBox? _comboBoxQuickSpawnType;
    private CheckBox? _checkBoxRadiusOverlay;
    private CheckBox? _checkBoxWaterboxOverlay;
    private Button? _buttonPrepareAllUnitCache;
    private TextBox? _textBoxUnitSearch;
    private PictureBox? _pictureBoxModeBanner;
    private GroupBox? _groupBoxReferenceUnit;
    private TextBox? _textBoxReferenceArc;
    private TextBox? _textBoxReferenceUnitCache;
    private TextBox? _textBoxReferenceImageCache;
    private Button? _buttonExportLog;
    private Panel? _homeOverlayPanel;
    private Panel? _homeConsolePanel;
    private TextBox? _textBoxHomeDiscRoot;
    private TextBox? _textBoxHomeToolkitPath;
    private HomeImageControl? _pictureBoxHomeDiscLabel;
    private HomeImageControl? _pictureBoxHomeToolkitLabel;
    private ComboBox? _comboBoxHomeLanguage;
    private Label? _labelHomeStatus;
    private ToolStripMenuItem? _homeMenuItem;
    private ToolStripMenuItem? _manualMenuItem;
    private Panel? _loadingOverlayPanel;
    private Panel? _loadingCardPanel;
    private PictureBox? _pictureBoxLoading;
    private LoadingSpinnerControl? _loadingSpinner;
    private Label? _labelLoadingStatus;
    private FormWindowState _editorWindowState = FormWindowState.Normal;
    private Rectangle _editorWindowBounds = Rectangle.Empty;
    private Size _editorMinimumSize = Size.Empty;
    private bool _editorWindowPlacementStored;
    private ToolTip? _quickToolTip;
    private bool _quickToolDragging;
    private Point _quickToolDragOffset;
    private bool _quickToolMinimized;
    private Size _quickToolExpandedSize;
    private bool _fieldConsoleDragging;
    private Point _fieldConsoleDragOffset;
    private bool _fieldConsoleMinimized;
    private Size _fieldConsoleExpandedSize;
    private bool _leftPaneCollapsed;
    private bool _mapUnitPaneCollapsed;
    private bool _rightPaneCollapsed;
    private int _expandedLeftPaneWidth = 520;
    private int _expandedMapUnitPaneWidth = 352;
    private int _expandedRightPaneWidth = 320;
    private List<string> _currentSummaryLines = new();
    private readonly List<ConsoleEntry> _consoleEntries = new();
    private readonly Dictionary<string, Panel> _templateCardPanels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PictureBox> _templateCardImages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<EditorSnapshot> _undoStack = new();
    private readonly Stack<EditorSnapshot> _redoStack = new();
    private bool _applyingHistory;
    private bool _continuousEditUndoRecorded;
    private string? _currentCaveInfoDirectory;
    private LoadFormatKind _currentLoadFormat = LoadFormatKind.None;
    private string? _currentCacheRootOverride;
    private bool _bulkCacheRunning;
    private bool _previewSceneResetRequired = true;
    private bool _currentPreviewImageIsPretty;
    private int _currentFieldDay;
    private IReadOnlyList<FieldDisplayObjectRef> _fieldDisplayObjectRefs = Array.Empty<FieldDisplayObjectRef>();
    private string? _lastCaveArcPath;
    private string? _lastCaveUnitsPath;
    private string? _lastFieldMapRoot;
    private string? _lastFieldTextsRoot;
    private string? _currentDiscSearchRoot;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private static readonly (int TypeId, string Label)[] SpawnTypeOptions =
    {
        (0, "Teki A"),
        (1, "Teki B"),
        (2, "Item"),
        (5, "Hole/Geyser"),
        (6, "Plant"),
        (7, "Start"),
        (8, "Teki F")
    };

    private static readonly (FieldAddTemplateKind Kind, string Label)[] FieldAddTemplateOptions =
    {
        (FieldAddTemplateKind.Teki, "Teki"),
        (FieldAddTemplateKind.Item, "Item"),
        (FieldAddTemplateKind.Pikmin, "Pikmin"),
        (FieldAddTemplateKind.CaveEntrance, "Cave Entrance")
    };

    public Form1()
    {
        InitializeComponent();
        _settingsStore = new EditorSettingsStore(AppContext.BaseDirectory);
        _repository = new DataRepository(AppContext.BaseDirectory);
        _unitMapView = new UnitMapView { Dock = DockStyle.Fill };
        _objModelView = new ObjModelView { Dock = DockStyle.Fill, Visible = false };
        _unitMapView.LayoutSpawnSelectionChanged += UnitMapView_LayoutSpawnSelectionChanged;
        _unitMapView.LayoutSpawnMoved += UnitMapView_LayoutSpawnMoved;
        _unitMapView.LayoutSpawnAngleChanged += Preview_LayoutSpawnAngleChanged;
        _unitMapView.LayoutSpawnRadiusChanged += Preview_LayoutSpawnRadiusChanged;
        _unitMapView.RouteWaypointSelectionChanged += UnitMapView_RouteWaypointSelectionChanged;
        _unitMapView.RouteWaypointMoved += UnitMapView_RouteWaypointMoved;
        _unitMapView.RouteWaypointRadiusChanged += Preview_RouteWaypointRadiusChanged;
        _unitMapView.RouteWaypointLinked += UnitMapView_RouteWaypointLinked;
        _unitMapView.RouteWaypointLinkDeleted += UnitMapView_RouteWaypointLinkDeleted;
        _unitMapView.WaterboxSelectionChanged += Preview_WaterboxSelectionChanged;
        _unitMapView.WaterboxMoved += Preview_WaterboxMoved;
        _unitMapView.WaterboxResized += Preview_WaterboxResized;
        _unitMapView.OverlayDragStarted += Preview_OverlayDragStarted;
        _unitMapView.OverlayDragEnded += Preview_OverlayDragEnded;
        _unitMapView.MapPointPlacementRequested += Preview_MapPointPlacementRequested;
        _unitMapView.MapPointDeletionRequested += Preview_MapPointDeletionRequested;
        _objModelView.LayoutSpawnSelectionChanged += ObjModelView_LayoutSpawnSelectionChanged;
        _objModelView.LayoutSpawnMoved += ObjModelView_LayoutSpawnMoved;
        _objModelView.LayoutSpawnAngleChanged += Preview_LayoutSpawnAngleChanged;
        _objModelView.LayoutSpawnRadiusChanged += Preview_LayoutSpawnRadiusChanged;
        _objModelView.RouteWaypointSelectionChanged += ObjModelView_RouteWaypointSelectionChanged;
        _objModelView.RouteWaypointMoved += ObjModelView_RouteWaypointMoved;
        _objModelView.RouteWaypointRadiusChanged += Preview_RouteWaypointRadiusChanged;
        _objModelView.RouteWaypointLinked += ObjModelView_RouteWaypointLinked;
        _objModelView.RouteWaypointLinkDeleted += ObjModelView_RouteWaypointLinkDeleted;
        _objModelView.WaterboxSelectionChanged += Preview_WaterboxSelectionChanged;
        _objModelView.WaterboxMoved += Preview_WaterboxMoved;
        _objModelView.WaterboxResized += Preview_WaterboxResized;
        _objModelView.WaterboxHeightMoved += Preview_WaterboxHeightMoved;
        _objModelView.OverlayDragStarted += Preview_OverlayDragStarted;
        _objModelView.OverlayDragEnded += Preview_OverlayDragEnded;
        _objModelView.MapPointPlacementRequested += Preview_MapPointPlacementRequested;
        _objModelView.MapPointDeletionRequested += Preview_MapPointDeletionRequested;
        panelPreview.Controls.Add(_objModelView);
        panelPreview.Controls.Add(_unitMapView);
        _unitMapView.BringToFront();
        ConfigureDragAndDrop();
        BuildCollapsibleShells();
        ConfigureSidebarLayout();
        BuildModeBanner();
        BuildReferenceUnitPanel();
        BuildInspectorPanel();
        BuildQuickToolWindow();
        BuildConsoleLogButton();
        BuildAllUnitCacheButton();
        BuildModeReferenceBrowseButtons();
        RelocateEditButtonsToInspector();
        ApplyApplicationTheme();
        BuildFieldGeneratorConsoleWindow();
        BuildHomeScreen();
        BuildLoadingOverlay();
    }

    //-------------------------------------------------------------------------------
    // アプリ全体の配色とフォントを統一するテーマ適用処理
    //-------------------------------------------------------------------------------
    private void ApplyApplicationTheme()
    {
        Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

        // サイドバー
        panelSidebarScroll.BackColor = Color.FromArgb(243, 245, 250);
        tableLayoutPanelSidebar.BackColor = Color.FromArgb(243, 245, 250);
        groupBoxCommon.ForeColor = Color.FromArgb(30, 42, 68);
        groupBoxCommon.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        groupBoxDisc.ForeColor = Color.FromArgb(30, 42, 68);
        groupBoxDisc.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        groupBoxFloorSummary.ForeColor = Color.FromArgb(30, 42, 68);
        groupBoxFloorSummary.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);

        // GroupBox 内 label フォントを通常サイズへ
        tableLayoutPanelCommon.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        tableLayoutPanelDisc.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

        // コンソール
        richTextBoxConsole.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        richTextBoxConsole.BackColor = Color.FromArgb(18, 22, 30);
        richTextBoxConsole.ForeColor = Color.FromArgb(190, 210, 240);

        // 参照ボタン (Browse 系) をフラットに
        StyleActionButton(buttonBrowseToolkit, Color.FromArgb(230, 234, 242), Color.FromArgb(170, 185, 210));
        StyleActionButton(buttonBrowseDisc, Color.FromArgb(230, 234, 242), Color.FromArgb(170, 185, 210));
        if (_buttonBrowsePrimaryReference is not null)
        {
            StyleActionButton(_buttonBrowsePrimaryReference, Color.FromArgb(230, 234, 242), Color.FromArgb(170, 185, 210));
        }

        if (_buttonBrowseSecondaryReference is not null)
        {
            StyleActionButton(_buttonBrowseSecondaryReference, Color.FromArgb(230, 234, 242), Color.FromArgb(170, 185, 210));
        }

        // 右インスペクタ背景
        panelInspectorPanelHost.BackColor = Color.FromArgb(240, 243, 250);
        panelInspectorContent.BackColor = Color.FromArgb(240, 243, 250);

        // 右グリップ/タブ
        panelInspectorTabHost.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleRightPane.ForeColor = Color.FromArgb(210, 220, 240);
        buttonToggleRightPane.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleRightPane.FlatStyle = FlatStyle.Flat;
        buttonToggleRightPane.FlatAppearance.BorderSize = 0;

        // 左グリップ/タブ
        panelLeftTabHost.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleLeftPane.ForeColor = Color.FromArgb(210, 220, 240);
        buttonToggleLeftPane.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleLeftPane.FlatStyle = FlatStyle.Flat;
        buttonToggleLeftPane.FlatAppearance.BorderSize = 0;

        // マップユニット折りたたみタブ
        panelMapUnitTabHost.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleMapUnitPane.ForeColor = Color.FromArgb(210, 220, 240);
        buttonToggleMapUnitPane.BackColor = Color.FromArgb(55, 65, 90);
        buttonToggleMapUnitPane.FlatStyle = FlatStyle.Flat;
        buttonToggleMapUnitPane.FlatAppearance.BorderSize = 0;

        // 動的グループのテーマは追加生成後に適用
        ApplyDynamicGroupTheme();
    }

    //-------------------------------------------------------------------------------
    // 動的生成した GroupBox / Inspector グループのスタイルを適用する処理
    //-------------------------------------------------------------------------------
    private void ApplyDynamicGroupTheme()
    {
        if (_groupBoxSpawnInspector is not null)
        {
            _groupBoxSpawnInspector.ForeColor = Color.FromArgb(30, 42, 68);
            _groupBoxSpawnInspector.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _groupBoxSpawnInspector.BackColor = Color.FromArgb(240, 243, 250);
        }

        if (_groupBoxWaypointInspector is not null)
        {
            _groupBoxWaypointInspector.ForeColor = Color.FromArgb(30, 42, 68);
            _groupBoxWaypointInspector.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _groupBoxWaypointInspector.BackColor = Color.FromArgb(240, 243, 250);
        }

        if (_groupBoxReferenceUnit is not null)
        {
            _groupBoxReferenceUnit.ForeColor = Color.FromArgb(30, 42, 68);
            _groupBoxReferenceUnit.Font = new Font("Yu Gothic UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        }
    }

    //-------------------------------------------------------------------------------
    // アクションボタンにフラットスタイルを適用する処理
    //-------------------------------------------------------------------------------
    private static void StyleActionButton(Button button, Color backColor, Color borderColor)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = backColor;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;
    }

    //-------------------------------------------------------------------------------
    // 読み込み対象のドラッグアンドドロップ受付を設定する処理
    //-------------------------------------------------------------------------------
    private void ConfigureDragAndDrop()
    {
        AllowDrop = true;
        panelPreview.AllowDrop = true;
        textBoxDiscRoot.AllowDrop = true;
        _unitMapView.AllowDrop = true;
        _objModelView.AllowDrop = true;

        DragEnter += LoadTarget_DragEnter;
        DragDrop += LoadTarget_DragDrop;
        panelPreview.DragEnter += LoadTarget_DragEnter;
        panelPreview.DragDrop += LoadTarget_DragDrop;
        panelPreview.Resize += panelPreview_Resize;
        textBoxDiscRoot.DragEnter += LoadTarget_DragEnter;
        textBoxDiscRoot.DragDrop += LoadTarget_DragDrop;
        _unitMapView.DragEnter += LoadTarget_DragEnter;
        _unitMapView.DragDrop += LoadTarget_DragDrop;
        _objModelView.DragEnter += LoadTarget_DragEnter;
        _objModelView.DragDrop += LoadTarget_DragDrop;
    }

    //-------------------------------------------------------------------------------
    // 起動時に表示するホーム画面を作成する処理
    //-------------------------------------------------------------------------------
    private void BuildHomeScreen()
    {
        Icon? homeIcon = LoadHomeIcon("BigPan_icon_trim_20260522_161519.ico");
        if (homeIcon is not null)
        {
            Icon = homeIcon;
        }

        _homeMenuItem = new ToolStripMenuItem("ホーム");
        _homeMenuItem.Click += (_, _) => ShowHomeScreen();
        _manualMenuItem = new ToolStripMenuItem("マニュアル");
        _manualMenuItem.Click += (_, _) => ShowEditorManualWindow();
        menuStrip1.Items.Add(_homeMenuItem);
        menuStrip1.Items.Add(_manualMenuItem);

        _homeOverlayPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(6, 17, 35),
            AutoScroll = true,
            Visible = false
        };
        _homeOverlayPanel.Resize += (_, _) => LayoutHomeConsolePanel();

        _homeConsolePanel = new Panel
        {
            Size = new Size(HomeConsoleWidth, HomeConsoleHeight),
            BackgroundImage = LoadHomeImage("MainConsoleBG.png"),
            BackgroundImageLayout = ImageLayout.Stretch
        };
        _homeOverlayPanel.Controls.Add(_homeConsolePanel);

        _labelHomeStatus = new Label
        {
            AutoSize = false,
            BackColor = Color.FromArgb(6, 17, 35),
            ForeColor = Color.FromArgb(75, 205, 245),
            Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _homeConsolePanel.Controls.Add(_labelHomeStatus);

        _pictureBoxHomeDiscLabel = CreateHomeLabelImage("EditISO.png");
        _pictureBoxHomeToolkitLabel = CreateHomeLabelImage("EditHokotateToolKit.png");
        _textBoxHomeDiscRoot = CreateHomeTextBox();
        _textBoxHomeToolkitPath = CreateHomeTextBox();
        _comboBoxHomeLanguage = CreateHomeLanguageComboBox();
        Button discBrowseButton = CreateHomeSmallButton(buttonHomeBrowseDisc_Click);
        Button toolkitBrowseButton = CreateHomeSmallButton(buttonHomeBrowseToolkit_Click);
        Button caveButton = CreateHomeImageButton("Cave_gen_editor_button.png", (_, _) => EnterEditorFromHome(EditorMode.Cave));
        Button fieldButton = CreateHomeImageButton("field_gen_editor_button.png", (_, _) => EnterEditorFromHome(EditorMode.Field));
        Button toolkitButton = CreateHomeImageButton("HokotateToolKit_button.png", (_, _) => OpenHocotateToolkitPage());
        Button manualButton = CreateHomeImageButton("Manual_button.png", (_, _) => ShowEditorManualWindow());

        _homeConsolePanel.Controls.Add(_pictureBoxHomeDiscLabel);
        _homeConsolePanel.Controls.Add(_pictureBoxHomeToolkitLabel);
        _homeConsolePanel.Controls.Add(_textBoxHomeDiscRoot);
        _homeConsolePanel.Controls.Add(_textBoxHomeToolkitPath);
        _homeConsolePanel.Controls.Add(_comboBoxHomeLanguage);
        _homeConsolePanel.Controls.Add(discBrowseButton);
        _homeConsolePanel.Controls.Add(toolkitBrowseButton);
        _homeConsolePanel.Controls.Add(caveButton);
        _homeConsolePanel.Controls.Add(fieldButton);
        _homeConsolePanel.Controls.Add(toolkitButton);
        _homeConsolePanel.Controls.Add(manualButton);

        PlaceHomeControls(discBrowseButton, toolkitBrowseButton, caveButton, fieldButton, toolkitButton, manualButton);
        RegisterHomeDropTargets(_homeOverlayPanel);
        Controls.Add(_homeOverlayPanel);
        _homeOverlayPanel.BringToFront();
    }

    //-------------------------------------------------------------------------------
    // ホーム画面上のコントロールへ編集対象パスのドロップ受付を設定する処理
    //-------------------------------------------------------------------------------
    private void RegisterHomeDropTargets(Control root)
    {
        root.AllowDrop = true;
        root.DragEnter += HomeLoadTarget_DragEnter;
        root.DragDrop += HomeLoadTarget_DragDrop;
        foreach (Control child in root.Controls)
        {
            RegisterHomeDropTargets(child);
        }
    }

    //-------------------------------------------------------------------------------
    // 長時間処理中に画面中央へ表示するロードオーバーレイを作成する処理
    //-------------------------------------------------------------------------------
    private void BuildLoadingOverlay()
    {
        _loadingOverlayPanel = new Panel
        {
            Size = new Size(216, 174),
            BackColor = Color.White,
            Visible = false
        };
        _loadingOverlayPanel.Resize += (_, _) => LayoutLoadingOverlay();

        _loadingCardPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        _loadingCardPanel.Paint += (_, e) =>
        {
            using Pen borderPen = new(Color.FromArgb(8, 40, 56), 3f);
            e.Graphics.DrawRectangle(borderPen, 1, 1, _loadingCardPanel.Width - 3, _loadingCardPanel.Height - 3);
        };

        _pictureBoxLoading = new PictureBox
        {
            Size = new Size(118, 94),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        _pictureBoxLoading.Image = LoadLoadingImage();

        _loadingSpinner = new LoadingSpinnerControl();
        _loadingSpinner.BackColor = Color.White;

        _labelLoadingStatus = new Label
        {
            AutoSize = false,
            Height = 46,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Black,
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold, GraphicsUnit.Point)
        };

        _loadingCardPanel.Controls.Add(_pictureBoxLoading);
        _loadingCardPanel.Controls.Add(_loadingSpinner);
        _loadingCardPanel.Controls.Add(_labelLoadingStatus);
        _loadingOverlayPanel.Controls.Add(_loadingCardPanel);
        Controls.Add(_loadingOverlayPanel);
        _loadingOverlayPanel.BringToFront();
        Resize += (_, _) => LayoutLoadingOverlay();
        LayoutLoadingOverlay();
    }

    //-------------------------------------------------------------------------------
    // ロード用画像を埋め込みリソースから読み込む処理
    //-------------------------------------------------------------------------------
    private static Image? LoadLoadingImage()
    {
        return EmbeddedImageCatalog.LoadImageWithAnimation("loading", "Loading.gif") ??
            EmbeddedImageCatalog.LoadImageWithAnimation("loading", "Loading.png") ??
            EmbeddedImageCatalog.LoadImageWithAnimation("loading", "Loading.avif");
    }

    //-------------------------------------------------------------------------------
    // ロードオーバーレイ内の表示位置を調整する処理
    //-------------------------------------------------------------------------------
    private void LayoutLoadingOverlay()
    {
        if (_loadingOverlayPanel is null ||
            _loadingCardPanel is null ||
            _pictureBoxLoading is null ||
            _loadingSpinner is null ||
            _labelLoadingStatus is null)
        {
            return;
        }

        _loadingOverlayPanel.Left = Math.Max(0, (ClientSize.Width - _loadingOverlayPanel.Width) / 2);
        _loadingOverlayPanel.Top = Math.Max(0, (ClientSize.Height - _loadingOverlayPanel.Height) / 2);

        bool hasLoadingImage = _pictureBoxLoading.Image is not null;
        Control activeVisual = hasLoadingImage ? _pictureBoxLoading : _loadingSpinner;
        activeVisual.Left = (_loadingCardPanel.Width - activeVisual.Width) / 2;
        activeVisual.Top = 18;
        _pictureBoxLoading.Visible = hasLoadingImage;
        _loadingSpinner.Visible = !hasLoadingImage;
        _labelLoadingStatus.SetBounds(12, 114, _loadingCardPanel.Width - 24, 48);
    }

    //-------------------------------------------------------------------------------
    // ロードオーバーレイの表示状態と文言を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateLoadingOverlay(bool isBusy, string statusText)
    {
        if (_loadingOverlayPanel is null ||
            _loadingSpinner is null ||
            _labelLoadingStatus is null)
        {
            return;
        }

        _labelLoadingStatus.Text = FormatLoadingOverlayText(statusText);
        _loadingOverlayPanel.Visible = isBusy;
        if (isBusy)
        {
            _loadingOverlayPanel.BringToFront();
            _loadingSpinner.Start();
        }
        else
        {
            _loadingSpinner.Stop();
        }

        _loadingOverlayPanel.Refresh();
    }

    //-------------------------------------------------------------------------------
    // 同期的な一覧構築中にもロードアニメーションを進める処理
    //-------------------------------------------------------------------------------
    private void PumpLoadingOverlayAnimation()
    {
        if (_loadingOverlayPanel?.Visible != true)
        {
            return;
        }

        _loadingOverlayPanel.Refresh();
        Application.DoEvents();
    }

    //-------------------------------------------------------------------------------
    // ロードオーバーレイ用の表示文言を英語へ整形する処理
    //-------------------------------------------------------------------------------
    private static string FormatLoadingOverlayText(string statusText)
    {
        if (string.IsNullOrWhiteSpace(statusText))
        {
            return "Preparing...";
        }

        if (!ContainsJapaneseLoadingKeyword(statusText) &&
            statusText.IndexOf(':') < 0 &&
            statusText.Contains("...", StringComparison.Ordinal))
        {
            return statusText;
        }

        string targetName = ExtractLoadingTargetName(statusText);
        string actionText =
            statusText.Contains("保存", StringComparison.Ordinal) ? "Saving..." :
            statusText.Contains("読み込", StringComparison.Ordinal) || statusText.Contains("Loading", StringComparison.OrdinalIgnoreCase) ? "Loading..." :
            statusText.Contains("探索", StringComparison.Ordinal) || statusText.Contains("確認", StringComparison.Ordinal) ? "Checking source..." :
            statusText.Contains("キャッシュ", StringComparison.Ordinal) || statusText.Contains("cache", StringComparison.OrdinalIgnoreCase) ? "Creating cache..." :
            "Preparing...";

        return string.IsNullOrWhiteSpace(targetName)
            ? actionText
            : $"{actionText}{Environment.NewLine}{targetName}";
    }

    //-------------------------------------------------------------------------------
    // ロード文言に日本語の状態キーワードが含まれるか確認する処理
    //-------------------------------------------------------------------------------
    private static bool ContainsJapaneseLoadingKeyword(string statusText)
    {
        return statusText.Contains("保存", StringComparison.Ordinal) ||
            statusText.Contains("読み込", StringComparison.Ordinal) ||
            statusText.Contains("探索", StringComparison.Ordinal) ||
            statusText.Contains("確認", StringComparison.Ordinal) ||
            statusText.Contains("キャッシュ", StringComparison.Ordinal);
    }

    //-------------------------------------------------------------------------------
    // ロードオーバーレイに表示する対象名を進捗文言から抽出する処理
    //-------------------------------------------------------------------------------
    private static string ExtractLoadingTargetName(string statusText)
    {
        int separatorIndex = statusText.IndexOf(':');
        if (separatorIndex < 0 || separatorIndex >= statusText.Length - 1)
        {
            return string.Empty;
        }

        string targetName = statusText[(separatorIndex + 1)..].Trim();
        int progressIndex = targetName.LastIndexOf(" (", StringComparison.Ordinal);
        if (progressIndex > 0)
        {
            targetName = targetName[..progressIndex].Trim();
        }

        return targetName;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面内の固定配置コントロールを背景画像に合わせる処理
    //-------------------------------------------------------------------------------
    private void PlaceHomeControls(Button discBrowseButton, Button toolkitBrowseButton, Button caveButton, Button fieldButton, Button toolkitButton, Button manualButton)
    {
        if (_homeConsolePanel is null ||
            _textBoxHomeDiscRoot is null ||
            _textBoxHomeToolkitPath is null ||
            _pictureBoxHomeDiscLabel is null ||
            _pictureBoxHomeToolkitLabel is null ||
            _comboBoxHomeLanguage is null ||
            _labelHomeStatus is null)
        {
            return;
        }

        _labelHomeStatus.Bounds = ScaleHomeBounds(58, 12, 190, 26);
        _comboBoxHomeLanguage.Bounds = ScaleHomeBounds(800, 12, 260, 30);
        _pictureBoxHomeDiscLabel.Bounds = ScaleHomeBounds(228, 220, 471, 27);
        _textBoxHomeDiscRoot.Bounds = ScaleHomeBounds(228, 248, 648, 25);
        discBrowseButton.Bounds = ScaleHomeBounds(888, 244, 34, 34);
        _pictureBoxHomeToolkitLabel.Bounds = ScaleHomeBounds(228, 286, 471, 27);
        _textBoxHomeToolkitPath.Bounds = ScaleHomeBounds(228, 318, 648, 25);
        toolkitBrowseButton.Bounds = ScaleHomeBounds(888, 314, 34, 34);
        caveButton.Bounds = ScaleHomeBounds(228, 392, 315, 98);
        fieldButton.Bounds = ScaleHomeBounds(582, 392, 315, 98);
        toolkitButton.Bounds = ScaleHomeBounds(228, 516, 315, 98);
        manualButton.Bounds = ScaleHomeBounds(582, 516, 315, 98);
    }

    //-------------------------------------------------------------------------------
    // ホーム画面の背景原寸座標を現在表示サイズへ変換する処理
    //-------------------------------------------------------------------------------
    private static Rectangle ScaleHomeBounds(int x, int y, int width, int height)
    {
        float scaleX = HomeConsoleWidth / (float)HomeDesignWidth;
        float scaleY = HomeConsoleHeight / (float)HomeDesignHeight;
        return new Rectangle(
            (int)MathF.Round(x * scaleX),
            (int)MathF.Round(y * scaleY),
            (int)MathF.Round(width * scaleX),
            (int)MathF.Round(height * scaleY));
    }

    //-------------------------------------------------------------------------------
    // ホーム画面の中央コンソールをフォームサイズへ合わせて配置する処理
    //-------------------------------------------------------------------------------
    private void LayoutHomeConsolePanel()
    {
        if (_homeOverlayPanel is null || _homeConsolePanel is null)
        {
            return;
        }

        int width = HomeConsoleWidth;
        int height = HomeConsoleHeight;

        _homeConsolePanel.Size = new Size(width, height);
        _homeConsolePanel.Location = new Point(
            Math.Max(0, (_homeOverlayPanel.ClientSize.Width - width) / 2),
            Math.Max(0, (_homeOverlayPanel.ClientSize.Height - height) / 2));
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用の入力欄を作成する処理
    //-------------------------------------------------------------------------------
    private static TextBox CreateHomeTextBox()
    {
        return new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(9, 25, 48),
            ForeColor = Color.FromArgb(93, 207, 248),
            Font = new Font("Yu Gothic UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用の言語選択コンボボックスを作成する処理
    //-------------------------------------------------------------------------------
    private ComboBox CreateHomeLanguageComboBox()
    {
        ComboBox comboBox = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(9, 25, 48),
            ForeColor = Color.FromArgb(93, 207, 248),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
        };
        comboBox.Items.Add(new LanguageItem("ja-JP", "日本語"));
        comboBox.Items.Add(new LanguageItem("en", "English"));
        comboBox.SelectedIndexChanged += comboBoxHomeLanguage_SelectedIndexChanged;
        return comboBox;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用のラベル画像を作成する処理
    //-------------------------------------------------------------------------------
    private HomeImageControl CreateHomeLabelImage(string imageName)
    {
        return new HomeImageControl
        {
            Image = LoadHomeImage(imageName)
        };
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用の小型画像ボタンを作成する処理
    //-------------------------------------------------------------------------------
    private Button CreateHomeSmallButton(EventHandler onClick)
    {
        Button button = CreateHomeImageButton("FileIcon.png", onClick);
        button.BackColor = Color.FromArgb(6, 17, 35);
        return button;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用の画像ボタンを作成する処理
    //-------------------------------------------------------------------------------
    private Button CreateHomeImageButton(string imageName, EventHandler onClick)
    {
        Button button = new()
        {
            FlatStyle = FlatStyle.Flat,
            BackgroundImage = LoadHomeImage(imageName),
            BackgroundImageLayout = ImageLayout.Stretch,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 80, 120);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 65, 100);
        button.Click += onClick;
        return button;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面素材の画像を読み込む処理
    //-------------------------------------------------------------------------------
    private static Image? LoadHomeImage(string fileName)
    {
        Image? embeddedImage = EmbeddedImageCatalog.LoadImage("ホーム画面用素材", fileName);
        if (embeddedImage is not null)
        {
            return embeddedImage;
        }

        string path = ResolveHomeAssetPath(fileName);
        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面素材のアイコンを読み込む処理
    //-------------------------------------------------------------------------------
    private static Icon? LoadHomeIcon(string fileName)
    {
        Icon? embeddedIcon = EmbeddedImageCatalog.LoadIcon("ホーム画面用素材", fileName);
        if (embeddedIcon is not null)
        {
            return embeddedIcon;
        }

        string path = ResolveHomeAssetPath(fileName);
        return File.Exists(path) ? new Icon(path) : null;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面素材の実パスを取得する処理
    //-------------------------------------------------------------------------------
    private static string ResolveHomeAssetPath(string fileName)
    {
        return FindFileInParentDirectories("ホーム画面用素材", fileName) ??
            Path.Combine(Environment.CurrentDirectory, "ホーム画面用素材", fileName);
    }

    //-------------------------------------------------------------------------------
    // ホーム画面の表示内容を現在設定へ同期する処理
    //-------------------------------------------------------------------------------
    private void RefreshHomeScreen()
    {
        if (_textBoxHomeDiscRoot is not null)
        {
            _textBoxHomeDiscRoot.Text = textBoxDiscRoot.Text;
        }

        if (_textBoxHomeToolkitPath is not null)
        {
            _textBoxHomeToolkitPath.Text = textBoxToolkitPath.Text;
        }

        if (_labelHomeStatus is not null)
        {
            _labelHomeStatus.Text = ApplicationVersionText;
            _labelHomeStatus.ForeColor = Color.FromArgb(90, 245, 185);
        }

        if (_comboBoxHomeLanguage is not null)
        {
            SelectHomeLanguageItem(_settings.Language);
        }
    }

    //-------------------------------------------------------------------------------
    // ホーム画面の言語選択を現在設定へ同期する処理
    //-------------------------------------------------------------------------------
    private void SelectHomeLanguageItem(string languageCode)
    {
        if (_comboBoxHomeLanguage is null)
        {
            return;
        }

        for (int i = 0; i < _comboBoxHomeLanguage.Items.Count; i++)
        {
            if (_comboBoxHomeLanguage.Items[i] is LanguageItem item &&
                item.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                _comboBoxHomeLanguage.SelectedIndex = i;
                return;
            }
        }

        _comboBoxHomeLanguage.SelectedIndex = 0;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面を表示する処理
    //-------------------------------------------------------------------------------
    private void ShowHomeScreen()
    {
        if (_homeOverlayPanel is null)
        {
            return;
        }

        StoreEditorWindowPlacement();
        ApplyHomeWindowSize();
        RefreshHomeScreen();
        _homeOverlayPanel.Visible = true;
        _homeOverlayPanel.BringToFront();
        LayoutHomeConsolePanel();
    }

    //-------------------------------------------------------------------------------
    // ホーム画面から指定モードのエディターへ移動する処理
    //-------------------------------------------------------------------------------
    private async void EnterEditorFromHome(EditorMode mode)
    {
        if (_textBoxHomeDiscRoot is not null)
        {
            textBoxDiscRoot.Text = _textBoxHomeDiscRoot.Text;
        }

        if (_textBoxHomeToolkitPath is not null)
        {
            textBoxToolkitPath.Text = _textBoxHomeToolkitPath.Text;
        }

        _settings.DiscRoot = textBoxDiscRoot.Text;
        _settings.ToolkitPath = textBoxToolkitPath.Text;
        comboBoxMode.SelectedIndex = mode == EditorMode.Cave ? 1 : 0;
        SaveSettings();
        ApplyToolkitState(showWarning: false);
        _homeOverlayPanel!.Visible = false;
        RestoreEditorWindowPlacement();
        ConfigureEditorModeUi();
        await LoadEditorSourcesWithBusyAsync("Loading editor...");
    }

    //-------------------------------------------------------------------------------
    // ホーム画面の言語選択変更を設定へ保存して UI へ反映する処理
    //-------------------------------------------------------------------------------
    private void comboBoxHomeLanguage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_comboBoxHomeLanguage?.SelectedItem is not LanguageItem item)
        {
            return;
        }

        _settings.Language = item.Code;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(item.Code);
        SaveSettings();
        ApplyLanguageToUi();
    }

    //-------------------------------------------------------------------------------
    // 選択中の言語設定をホームとメニューへ反映する処理
    //-------------------------------------------------------------------------------
    private void ApplyLanguageToUi()
    {
        if (_homeMenuItem is not null)
        {
            _homeMenuItem.Text = Localize("MenuHome");
        }

        if (_manualMenuItem is not null)
        {
            _manualMenuItem.Text = Localize("MenuManual");
        }

        Text = "Big Pan Map Editor";
        groupBoxCommon.Text = Localize("CommonSettings");
        labelToolkit.Text = Localize("Toolkit");
        labelDiscRoot.Text = Localize("ReferenceRoot");
        labelLoadFormat.Text = Localize("LoadFormat");
        labelMode.Text = Localize("Mode");
        buttonBrowseToolkit.Text = Localize("Browse");
        buttonBrowseDisc.Text = Localize("Browse");
        buttonPrepareCache.Text = Localize("PrepareCaveCache");
        buttonReloadTemplates.Text = Localize("ReloadTemplates");
        groupBoxTemplates.Text = Localize("MapListArea");
        if (_buttonPrepareAllUnitCache is not null)
        {
            _buttonPrepareAllUnitCache.Text = Localize("PrepareAllUnitCache");
        }

        if (_buttonExportLog is not null)
        {
            _buttonExportLog.Text = Localize("ExportConsoleLog");
        }

        if (_groupBoxReferenceUnit is not null)
        {
            _groupBoxReferenceUnit.Text = Localize("ReferenceInfo");
        }

        if (_groupBoxSelection is not null)
        {
            _groupBoxSelection.Text = Localize("SelectedPoint");
        }

        if (_groupBoxSpawnInspector is not null)
        {
            _groupBoxSpawnInspector.Text = Localize("SpawnEditor");
        }

        if (_groupBoxWaypointInspector is not null)
        {
            _groupBoxWaypointInspector.Text = Localize("WaypointEditor");
        }

        if (_groupBoxWaterboxInspector is not null)
        {
            _groupBoxWaterboxInspector.Text = Localize("WaterboxEditor");
        }

        if (_labelInspectorSelection is not null && _labelInspectorSelection.Text == "未選択")
        {
            _labelInspectorSelection.Text = Localize("NoSelection");
        }

        groupBoxFloorSummary.Text = Localize("Console");
        checkBoxSpawnOverlay.Text = Localize("ShowSpawn");
        checkBoxRouteOverlay.Text = Localize("ShowRoute");
        if (_checkBoxWaterboxOverlay is not null)
        {
            _checkBoxWaterboxOverlay.Text = Localize("ShowWater");
        }

        if (_quickToolTip is not null)
        {
            if (_quickToolGrip is not null)
            {
                _quickToolTip.SetToolTip(_quickToolGrip, Localize("TipDragMove"));
            }

            if (_fieldConsoleGrip is not null)
            {
                _quickToolTip.SetToolTip(_fieldConsoleGrip, Localize("TipDragMove"));
            }

            if (_comboBoxQuickSpawnType is not null)
            {
                _quickToolTip.SetToolTip(_comboBoxQuickSpawnType, Localize("TipSpawnType"));
            }
        }

        UpdateModeComboBoxLanguage();
        ConfigureEditorModeUi();
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
    }

    //-------------------------------------------------------------------------------
    // 現在言語に対応する表示文字列を取得する処理
    //-------------------------------------------------------------------------------
    private string Localize(string key)
    {
        bool english = _settings.Language.Equals("en", StringComparison.OrdinalIgnoreCase);
        return key switch
        {
            "MenuHome" => english ? "Home" : "ホーム",
            "MenuManual" => english ? "Manual" : "マニュアル",
            "ManualTitle" => english ? "Big Pan Map Editor Manual" : "Big Pan Map Editor マニュアル",
            "TabSetup" => english ? "Setup" : "初期設定",
            "TabBasic" => english ? "Basic" : "基本操作",
            "TabCave" => english ? "Cave" : "洞窟",
            "TabField" => english ? "Field" : "地上",
            "TabViewSave" => english ? "View / Save" : "表示・保存",
            "CommonSettings" => english ? "Common Settings" : "共通設定",
            "Toolkit" => "Toolkit",
            "ReferenceRoot" => english ? "Reference" : "参照先",
            "LoadFormat" => english ? "Load Type" : "読込形式",
            "Mode" => english ? "Mode" : "モード",
            "Browse" => english ? "Browse" : "参照",
            "PrepareCaveCache" => english ? "Prepare cave unit cache" : "洞窟ユニットのキャッシュを準備",
            "ReloadTemplates" => english ? "Reload templates" : "テンプレート再読込",
            "Console" => "Console",
            "ShowSpawn" => english ? "Show spawns" : "スポーンを表示",
            "ShowRoute" => english ? "Show routes" : "ルートを表示",
            "ShowWater" => english ? "Show water" : "水を表示",
            "FieldModeName" => english ? "Field Map" : "地上マップ",
            "CaveModeName" => english ? "Cave Unit" : "洞窟ユニット",
            "CaveReference" => english ? "Cave Reference" : "洞窟参照",
            "FieldReference" => english ? "Field Reference" : "地上参照",
            "Obj3D" => english ? "OBJ 3D View" : "OBJ 3D表示",
            "Field3D" => english ? "Field 3D View" : "地上 3D表示",
            "SpawnMoveOn" => english ? "Spawn move: ON" : "Spawn移動: ON",
            "SpawnMoveOff" => english ? "Spawn move: OFF" : "Spawn移動: OFF",
            "WaypointMoveOn" => english ? "Waypoint move: ON" : "Waypoint移動: ON",
            "WaypointMoveOff" => english ? "Waypoint move: OFF" : "Waypoint移動: OFF",
            "SpawnAddOn" => english ? "Add Spawn: ON" : "Spawn追加: ON",
            "SpawnAdd" => english ? "Add Spawn" : "Spawn追加",
            "WaypointAddOn" => english ? "Add Waypoint: ON" : "Waypoint追加: ON",
            "WaypointAdd" => english ? "Add Waypoint" : "Waypoint追加",
            "MapListArea" => english ? "Map / Unit List" : "マップ・ユニット表示エリア",
            "PrepareAllUnitCache" => english ? "Prepare all unit caches" : "全ユニットのキャッシュを作成",
            "ExportConsoleLog" => english ? "Export console log" : "コンソールログ出力",
            "ReferenceInfo" => english ? "Reference Information" : "参照情報",
            "SelectedPoint" => english ? "Selected Point" : "選択中のポイント",
            "SpawnEditor" => english ? "Spawn Editor" : "Spawn 編集",
            "WaypointEditor" => english ? "Waypoint Editor" : "Waypoint 編集",
            "WaterboxEditor" => english ? "Waterbox Editor" : "Waterbox 編集",
            "NoSelection" => english ? "No selection" : "未選択",
            "MiniController" => english ? "Mini Controller" : "ミニコントローラー",
            "FieldGeneratorConsole" => english ? "Field Generator Console" : "地上 generator コンソール",
            "TipFieldPending" => english ? " (field mode: existing objects only)" : "（地上は既存object編集のみ）",
            "TipFieldSupported" => english ? " (field mode supported)" : "（地上対応）",
            "TipAddSpawn" => english ? "Toggle Spawn add mode" : "Spawn追加モード ON/OFF",
            "TipAddWaypoint" => english ? "Toggle Waypoint add mode" : "Waypoint追加モード ON/OFF",
            "TipAddWaterbox" => english ? "Toggle Waterbox add mode" : "Waterbox追加モード ON/OFF",
            "TipDeleteSpawn" => english ? "Toggle Spawn delete mode" : "Spawn削除モード ON/OFF",
            "TipDeleteWaypoint" => english ? "Toggle Waypoint delete mode" : "Waypoint削除モード ON/OFF",
            "TipDeleteWaterbox" => english ? "Toggle Waterbox delete mode" : "Waterbox削除モード ON/OFF",
            "TipMoveSpawn" => english ? "Toggle Spawn move mode" : "Spawn移動 ON/OFF",
            "TipMoveWaypoint" => english ? "Toggle Waypoint move mode" : "Waypoint移動 ON/OFF",
            "TipMoveWaterbox" => english ? "Toggle Waterbox move mode" : "Waterbox移動 ON/OFF",
            "TipAngleSpawn" => english ? "Toggle Spawn angle edit mode" : "Spawn角度変更 ON/OFF",
            "TipRadiusSpawn" => english ? "Toggle Spawn Radius edit mode" : "Spawn Radius変更 ON/OFF",
            "TipRadiusWaypoint" => english ? "Toggle Waypoint Radius edit mode" : "Waypoint Radius変更 ON/OFF",
            "TipRouteDelete" => english ? "Delete route link" : "ルート削除",
            "TipRouteConnect" => english ? "Connect route waypoints" : "Route接続",
            "TipRoomConnect" => english ? "Move selected Waypoint to nearest connection point" : "選択Waypointを最寄り接続座標へ移動",
            "TipRadiusOverlay" => english ? "Show Spawn / Route Radius circles" : "Spawn / Route の Radius 円を表示",
            "TipSaveField" => english ? "Save field route / generator" : "地上 route/generator 保存",
            "TipSaveLayout" => english ? "Save layout.txt" : "layout.txt 保存",
            "TipSaveRoute" => english ? "Save route.txt" : "route.txt 保存",
            "TipSaveWaterbox" => english ? "Save waterbox.txt" : "waterbox.txt 保存",
            "TipSaveAllField" => english ? "Save field route / generator" : "地上 route/generator を保存",
            "TipSaveAllCave" => english ? "Save layout / route / waterbox" : "layout/route/waterbox をすべて保存",
            "TipDragMove" => english ? "Drag to move" : "ドラッグして移動",
            "TipSpawnType" => english ? "Spawn Type for new Spawn points" : "追加する Spawn Type",
            _ => key
        };
    }

    //-------------------------------------------------------------------------------
    // モード選択コンボボックスの表示言語を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateModeComboBoxLanguage()
    {
        if (comboBoxMode.Items.Count < 2)
        {
            return;
        }

        int selectedIndex = comboBoxMode.SelectedIndex;
        comboBoxMode.Items[0] = Localize("FieldModeName");
        comboBoxMode.Items[1] = Localize("CaveModeName");
        comboBoxMode.SelectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
    }

    //-------------------------------------------------------------------------------
    // エディター画面のウィンドウ位置と状態を保存する処理
    //-------------------------------------------------------------------------------
    private void StoreEditorWindowPlacement()
    {
        if (_homeOverlayPanel is not null && _homeOverlayPanel.Visible)
        {
            return;
        }

        _editorWindowState = WindowState;
        _editorWindowBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        _editorMinimumSize = MinimumSize;
        _editorWindowPlacementStored = true;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面用に背景画像へ合わせたウィンドウサイズへ変更する処理
    //-------------------------------------------------------------------------------
    private void ApplyHomeWindowSize()
    {
        WindowState = FormWindowState.Normal;
        MinimumSize = new Size(HomeConsoleWidth, HomeConsoleHeight + menuStrip1.Height);
        ClientSize = new Size(HomeConsoleWidth, HomeConsoleHeight + menuStrip1.Height);
        CenterToScreen();
    }

    //-------------------------------------------------------------------------------
    // 保存しておいたエディター画面のウィンドウ位置と状態へ戻す処理
    //-------------------------------------------------------------------------------
    private void RestoreEditorWindowPlacement()
    {
        if (!_editorWindowPlacementStored || _editorWindowBounds == Rectangle.Empty)
        {
            return;
        }

        WindowState = FormWindowState.Normal;
        if (_editorMinimumSize != Size.Empty)
        {
            MinimumSize = _editorMinimumSize;
        }

        Bounds = _editorWindowBounds;
        WindowState = _editorWindowState;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面から編集対象ファイルまたはフォルダを選択する処理
    //-------------------------------------------------------------------------------
    private async void buttonHomeBrowseDisc_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog folderDialog = new()
        {
            Description = "ISO / GCR / SZS / ARC を含むフォルダ，または抽出済みディスクデータを選択してください．",
            UseDescriptionForTitle = true
        };
        if (Directory.Exists(textBoxDiscRoot.Text))
        {
            folderDialog.SelectedPath = textBoxDiscRoot.Text;
        }

        if (folderDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await ApplyHomeDiscRootAsync(folderDialog.SelectedPath);
    }

    //-------------------------------------------------------------------------------
    // ホーム画面で選択した編集対象パスを設定へ反映する処理
    //-------------------------------------------------------------------------------
    private async Task ApplyHomeDiscRootAsync(string path)
    {
        if (_textBoxHomeDiscRoot is not null)
        {
            _textBoxHomeDiscRoot.Text = path;
        }

        textBoxDiscRoot.Text = path;
        _settings.DiscRoot = path;
        SaveSettings();
        if (File.Exists(textBoxToolkitPath.Text))
        {
            await ResolveDiscPathsAsync(path);
            RefreshHomeScreen();
        }
        else
        {
            RefreshHomeScreen();
        }
    }

    //-------------------------------------------------------------------------------
    // ホーム画面へ読み込み対象がドラッグされたときに受付可否を表示する処理
    //-------------------------------------------------------------------------------
    private void HomeLoadTarget_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = TryGetDroppedPath(e, out _) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面へドロップされた読み込み対象を ISO/GCR 欄へ反映する処理
    //-------------------------------------------------------------------------------
    private async void HomeLoadTarget_DragDrop(object? sender, DragEventArgs e)
    {
        if (!TryGetDroppedPath(e, out string? droppedPath))
        {
            return;
        }

        await ApplyHomeDiscRootAsync(droppedPath);
    }

    //-------------------------------------------------------------------------------
    // ホーム画面から Hocotate Toolkit の exe を選択する処理
    //-------------------------------------------------------------------------------
    private void buttonHomeBrowseToolkit_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Title = "Hocotate_Toolkit.exe を選択",
            Filter = "Hocotate_Toolkit.exe|Hocotate_Toolkit.exe|Executable (*.exe)|*.exe"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (_textBoxHomeToolkitPath is not null)
        {
            _textBoxHomeToolkitPath.Text = dialog.FileName;
        }

        textBoxToolkitPath.Text = dialog.FileName;
        _settings.ToolkitPath = dialog.FileName;
        SaveSettings();
        ApplyToolkitState(showWarning: false);
        RefreshHomeScreen();
    }

    //-------------------------------------------------------------------------------
    // Hocotate Toolkit の GitHub ページを開く処理
    //-------------------------------------------------------------------------------
    private void OpenHocotateToolkitPage()
    {
        OpenExternalUrl("https://github.com/Sadc2h4/Hocotate-Tool-Kit");
    }

    //-------------------------------------------------------------------------------
    // 外部 URL を既定ブラウザで開く処理
    //-------------------------------------------------------------------------------
    private static void OpenExternalUrl(string url)
    {
        try
        {
            ProcessStartInfo startInfo = new(url)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        catch
        {
        }
    }

    //-------------------------------------------------------------------------------
    // 操作説明用のマニュアルウィンドウを表示する処理
    //-------------------------------------------------------------------------------
    private void ShowEditorManualWindow()
    {
        Form manualForm = new()
        {
            Text = Localize("ManualTitle"),
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(920, 720),
            MinimizeBox = true,
            MaximizeBox = true,
            Icon = Icon
        };

        TabControl manualTabs = new()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Yu Gothic UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
        };

        string language = GetManualLanguage();
        manualTabs.TabPages.Add(CreateManualTab(Localize("TabSetup"), "Setup", language));
        manualTabs.TabPages.Add(CreateManualTab(Localize("TabBasic"), "Basic", language));
        manualTabs.TabPages.Add(CreateManualTab(Localize("TabCave"), "Cave", language));
        manualTabs.TabPages.Add(CreateManualTab(Localize("TabField"), "Field", language));
        manualTabs.TabPages.Add(CreateManualTab(Localize("TabViewSave"), "ViewSave", language));
        manualForm.Controls.Add(manualTabs);
        manualForm.Show(this);
    }

    //-------------------------------------------------------------------------------
    // マニュアルタブページを作成する処理
    //-------------------------------------------------------------------------------
    private TabPage CreateManualTab(string title, string section, string language)
    {
        TabPage tabPage = new(title)
        {
            BackColor = Color.FromArgb(248, 250, 253)
        };
        FlowLayoutPanel manualPanel = CreateManualMarkdownPanel(LoadManualMarkdown(language, section));
        tabPage.Controls.Add(manualPanel);
        return tabPage;
    }

    //-------------------------------------------------------------------------------
    // Markdown 文字列から簡易表示パネルを作成する処理
    //-------------------------------------------------------------------------------
    private FlowLayoutPanel CreateManualMarkdownPanel(string markdown)
    {
        FlowLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 253),
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(22, 20, 22, 20)
        };

        foreach (string line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            AddManualMarkdownLine(panel, line);
        }

        AddManualBottomSpacer(panel);
        panel.SizeChanged += (_, _) => ResizeManualMarkdownControls(panel);
        ResizeManualMarkdownControls(panel);
        return panel;
    }

    //-------------------------------------------------------------------------------
    // Markdown の 1 行を表示コントロールとして追加する処理
    //-------------------------------------------------------------------------------
    private void AddManualMarkdownLine(FlowLayoutPanel panel, string line)
    {
        string trimmed = line.Trim();
        Match imageMatch = Regex.Match(trimmed, @"^!\[(?<alt>[^\]]*)\]\((?<path>[^)]+)\)");
        if (imageMatch.Success)
        {
            AddManualImage(panel, imageMatch.Groups["path"].Value, imageMatch.Groups["alt"].Value);
            return;
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            panel.Controls.Add(new Panel { Height = 6, Width = 1, Margin = Padding.Empty });
            return;
        }

        if (IsManualHorizontalRule(trimmed))
        {
            AddManualHorizontalRule(panel);
            return;
        }

        FontStyle fontStyle = FontStyle.Regular;
        float fontSize = 10.5F;
        Padding margin = new(0, 2, 0, 2);
        string text = trimmed;
        if (trimmed.StartsWith("# ", StringComparison.Ordinal))
        {
            text = trimmed[2..].Trim();
            fontStyle = FontStyle.Bold;
            fontSize = 17F;
            margin = new Padding(0, 0, 0, 12);
        }
        else if (trimmed.StartsWith("## ", StringComparison.Ordinal))
        {
            text = trimmed[3..].Trim();
            fontStyle = FontStyle.Bold;
            fontSize = 13F;
            margin = new Padding(0, 10, 0, 6);
        }
        else if (trimmed.StartsWith("- ", StringComparison.Ordinal))
        {
            text = "・" + trimmed[2..].Trim();
        }

        Label label = new()
        {
            AutoSize = true,
            Font = new Font("Yu Gothic UI", fontSize, fontStyle, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(24, 34, 52),
            Margin = margin,
            Text = text
        };
        panel.Controls.Add(label);
    }

    //-------------------------------------------------------------------------------
    // Markdown の水平線表記か確認する処理
    //-------------------------------------------------------------------------------
    private static bool IsManualHorizontalRule(string line)
    {
        return line.Length >= 3 &&
            (line.All(character => character == '*') ||
             line.All(character => character == '-'));
    }

    //-------------------------------------------------------------------------------
    // Markdown の水平線を表示パネルへ追加する処理
    //-------------------------------------------------------------------------------
    private static void AddManualHorizontalRule(FlowLayoutPanel panel)
    {
        panel.Controls.Add(new Panel
        {
            Height = 1,
            Width = 780,
            BackColor = Color.FromArgb(198, 208, 224),
            Margin = new Padding(0, 12, 0, 12),
            Tag = "ManualHorizontalRule"
        });
    }

    //-------------------------------------------------------------------------------
    // Markdown 画像を表示パネルへ追加する処理
    //-------------------------------------------------------------------------------
    private void AddManualImage(FlowLayoutPanel panel, string imagePath, string altText)
    {
        Image? image = LoadManualImage(imagePath);
        if (image is null)
        {
            panel.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Yu Gothic UI", 10.5F, FontStyle.Italic, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(110, 118, 134),
                Margin = new Padding(0, 4, 0, 8),
                Text = $"[{altText}] {imagePath}"
            });
            return;
        }

        Size imageSize = GetManualImageDisplaySize(image, 780);
        PictureBox pictureBox = new()
        {
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 8, 0, 14),
            Width = imageSize.Width,
            Height = imageSize.Height
        };
        panel.Controls.Add(pictureBox);
    }

    //-------------------------------------------------------------------------------
    // マニュアル画像の左詰め表示サイズを算出する処理
    //-------------------------------------------------------------------------------
    private static Size GetManualImageDisplaySize(Image image, int maxWidth)
    {
        int width = Math.Max(1, Math.Min(maxWidth, image.Width));
        int height = Math.Max(80, Math.Min(420, image.Height * width / Math.Max(image.Width, 1)));
        return new Size(width, height);
    }

    //-------------------------------------------------------------------------------
    // マニュアル末尾までスクロールできるよう余白を追加する処理
    //-------------------------------------------------------------------------------
    private static void AddManualBottomSpacer(FlowLayoutPanel panel)
    {
        panel.Controls.Add(new Panel
        {
            Height = 72,
            Width = 1,
            Margin = Padding.Empty,
            Tag = "ManualBottomSpacer"
        });
    }

    //-------------------------------------------------------------------------------
    // マニュアル表示コントロールの横幅を調整する処理
    //-------------------------------------------------------------------------------
    private static void ResizeManualMarkdownControls(FlowLayoutPanel panel)
    {
        int width = Math.Max(240, panel.ClientSize.Width - panel.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 18);
        foreach (Control control in panel.Controls)
        {
            if (string.Equals(control.Tag as string, "ManualBottomSpacer", StringComparison.Ordinal))
            {
                control.Width = width;
            }
            else if (string.Equals(control.Tag as string, "ManualHorizontalRule", StringComparison.Ordinal))
            {
                control.Width = width;
            }
            else if (control is Label label)
            {
                label.MaximumSize = new Size(width, 0);
            }
            else if (control is PictureBox pictureBox && pictureBox.Image is not null)
            {
                Size imageSize = GetManualImageDisplaySize(pictureBox.Image, width);
                pictureBox.Width = imageSize.Width;
                pictureBox.Height = imageSize.Height;
            }
        }

        panel.PerformLayout();
        int contentBottom = panel.Controls.Cast<Control>()
            .Select(control => control.Bottom + control.Margin.Bottom)
            .DefaultIfEmpty(0)
            .Max();
        panel.AutoScrollMinSize = new Size(0, contentBottom + panel.Padding.Bottom + 8);
    }

    //-------------------------------------------------------------------------------
    // 現在のマニュアル言語コードを取得する処理
    //-------------------------------------------------------------------------------
    private string GetManualLanguage()
    {
        return _settings.Language.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ja";
    }

    //-------------------------------------------------------------------------------
    // マニュアル Markdown を埋め込みリソースから読み込む処理
    //-------------------------------------------------------------------------------
    private string LoadManualMarkdown(string language, string section)
    {
        return TryLoadEmbeddedManualMarkdown(language, section) ??
            BuildManualSectionText(section);
    }

    //-------------------------------------------------------------------------------
    // 埋め込みマニュアル Markdown を読み込む処理
    //-------------------------------------------------------------------------------
    private static string? TryLoadEmbeddedManualMarkdown(string language, string section)
    {
        using Stream? stream = EmbeddedImageCatalog.OpenStream($"manual.{language}", $"{section}.md");
        if (stream is null)
        {
            return null;
        }

        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    //-------------------------------------------------------------------------------
    // マニュアル画像を埋め込みリソースから読み込む処理
    //-------------------------------------------------------------------------------
    private Image? LoadManualImage(string imagePath)
    {
        string? directoryName = Path.GetDirectoryName(imagePath.Replace('\\', '/'))?.Replace('/', '.');
        string fileName = Path.GetFileName(imagePath);
        return string.IsNullOrWhiteSpace(directoryName)
            ? EmbeddedImageCatalog.LoadImage("manual", fileName)
            : EmbeddedImageCatalog.LoadImage($"manual.{directoryName}", fileName);
    }

    //-------------------------------------------------------------------------------
    // マニュアルタブに表示する説明文を作成する処理
    //-------------------------------------------------------------------------------
    private string BuildManualSectionText(string section)
    {
        bool english = _settings.Language.Equals("en", StringComparison.OrdinalIgnoreCase);
        return (section, english) switch
        {
            ("Setup", true) => "Setup\r\n\r\n- Register an ISO / GCR / extracted root folder from the home screen.\r\n- If ISO / GCR is selected，the tool tries disc extraction with Hocotate_Toolkit.exe.\r\n- A folder that already contains sys/files can be loaded without extraction.\r\n- Cave mode can also load a Mukki/mapunits/arc folder directly.\r\n- Register Hocotate_Toolkit.exe to enable SZS extraction and cache generation.\r\n- Cave mode uses Mukki/mapunits references.\r\n- Field mode uses user/Abe/map and user/Kando/map.\r\n- The Home menu can reopen the home screen after entering the editor.",
            ("Basic", true) => "Basic controls\r\n\r\n- Left click selects Spawn / Waypoint / Waterbox / field objects.\r\n- Use the mini controller to switch between Spawn，Route，and Waterbox.\r\n- Pen toggles click-add mode. Click on the map after enabling it.\r\n- Eraser toggles click-delete mode. Click the target point after enabling it.\r\n- Move toggles drag-move mode. Drag the selected point with the left mouse button.\r\n- Angle is for Spawn direction. In 2D，enable Angle and drag the triangular handle or right-drag from the selected Spawn.\r\n- Radius is for Spawn / Route radius. Enable Radius，then drag from the selected point to the desired radius.\r\n- In 3D view，right-drag is used for camera rotation and selected Spawn angle editing depending on mode.",
            ("Cave", true) => "Cave mode\r\n\r\n- Spawn: add，delete，move，angle，radius，type，and count editing are supported.\r\n- Route: add，delete，move，radius，link add，and link delete are supported.\r\n- Waterbox: add，delete，move，XZ resize，and 3D height movement are supported.\r\n- Room-connect moves the selected waypoint to the nearest unit connection point.\r\n- Save writes the active target. Save All writes layout / route / waterbox and repacks archives when available.",
            ("Field", true) => "Field mode\r\n\r\n- Generator files are filtered by elapsed day.\r\n- Use the field generator console to change day and inspect active generator files.\r\n- Select target generator and object type，then use click-add mode to place new objects.\r\n- Supported add templates are Teki，Item，Pikmin，and Cave Entrance.\r\n- Existing field objects can be selected，moved，rotated，and radius-edited when the raw object has matching fields.\r\n- Raw shows the selected generator object text. Press Apply raw after editing.\r\n- Field save writes route.txt and generator txt files back to the map folder.",
            ("ViewSave", true) => "View / Save\r\n\r\n- Toggle Spawn，Route，Radius，and Waterbox overlays from the mini controller.\r\n- OBJ / Field 3D view helps inspect placement in 3D.\r\n- Floating consoles can be moved by dragging their top grip.\r\n- Use the minimize button on floating consoles when they cover the map.\r\n- Back up target folders before saving complex generator edits.",
            ("Setup", false) => "初期設定\r\n\r\n・ホーム画面で ISO / GCR / 抽出済み root フォルダを登録します．\r\n・ISO / GCR を選択した場合，Hocotate_Toolkit.exe でディスク抽出を試行します．\r\n・sys/files を含む抽出済みフォルダは，抽出なしで読み込めます．\r\n・洞窟モードでは Mukki/mapunits/arc フォルダを直接指定して読み込むこともできます．\r\n・Hocotate_Toolkit.exe を登録すると，SZS 展開やキャッシュ生成が利用できます．\r\n・洞窟モードは Mukki/mapunits を参照します．\r\n・地上モードは user/Abe/map と user/Kando/map を参照します．\r\n・エディターへ入った後も，ホームメニューからホーム画面を再表示できます．",
            ("Basic", false) => "基本操作\r\n\r\n・左クリックで Spawn / Waypoint / Waterbox / 地上 object を選択します．\r\n・ミニコントローラーで Spawn，Route，Waterbox の編集対象を切り替えます．\r\n・ペンはクリック追加モードです．有効にした後，マップ上をクリックして追加します．\r\n・消しゴムはクリック削除モードです．有効にした後，対象ポイントをクリックして削除します．\r\n・移動はドラッグ移動モードです．有効にした後，選択ポイントを左ドラッグします．\r\n・Angle は Spawn の向き編集です．2D では三角ハンドルをドラッグ，または選択 Spawn から右ドラッグします．\r\n・Radius は Spawn / Route の半径編集です．有効にした後，選択ポイントから目的の半径位置までドラッグします．\r\n・3D 表示では，モードにより右ドラッグがカメラ回転または選択 Spawn の角度編集になります．",
            ("Cave", false) => "洞窟モード\r\n\r\n・Spawn は追加，削除，移動，角度，Radius，Type，Min/Max Count の編集に対応しています．\r\n・Route は Waypoint の追加，削除，移動，Radius，接続追加，接続削除に対応しています．\r\n・Waterbox は追加，削除，移動，XZ サイズ変更，3D での高さ移動に対応しています．\r\n・接続座標へ移動ボタンは，選択 Waypoint を最寄りのユニット接続座標へ移動します．\r\n・保存は現在対象を保存します．すべて保存は layout / route / waterbox を保存し，可能な場合はアーカイブへ反映します．",
            ("Field", false) => "地上モード\r\n\r\n・経過日数で有効な generator ファイルだけが表示されます．\r\n・地上 generator コンソールで経過日数と有効 generator を確認できます．\r\n・追加先 generator と追加タイプを選び，クリック追加モードで新規 object を配置します．\r\n・追加テンプレートは Teki，Item，Pikmin，Cave Entrance に対応しています．\r\n・既存 object は選択，移動，角度，Radius 編集に対応しています．ただし raw object 側に対応する項目がない場合は反映できません．\r\n・Raw 欄は選択 object の generator テキストです．編集後は「選択 object raw 反映」を押します．\r\n・保存時は route.txt と generator txt を map フォルダへ上書きします．",
            ("ViewSave", false) => "表示・保存\r\n\r\n・スポーン表示，ルート表示，Radius，水を表示でオーバーレイを切り替えます．\r\n・OBJ 3D表示や地上 3D表示を使うと，3D 上で配置を確認できます．\r\n・フローティングコンソールは上部グリップをドラッグして移動できます．\r\n・画面を覆う場合は，各コンソール上部の最小化ボタンを使用します．\r\n・複雑な generator 編集前には対象フォルダのバックアップを推奨します．",
            _ => string.Empty
        };
    }

    //-------------------------------------------------------------------------------
    // 大元モード別の参照パス変更ボタンを左側へ追加する処理
    //-------------------------------------------------------------------------------
    private void BuildModeReferenceBrowseButtons()
    {
        _buttonBrowsePrimaryReference = new Button
        {
            Dock = DockStyle.Fill,
            Text = "参照",
            UseVisualStyleBackColor = true
        };
        _buttonBrowseSecondaryReference = new Button
        {
            Dock = DockStyle.Fill,
            Text = "参照",
            UseVisualStyleBackColor = true
        };

        _buttonBrowsePrimaryReference.Click += buttonBrowsePrimaryReference_Click;
        _buttonBrowseSecondaryReference.Click += buttonBrowseSecondaryReference_Click;
        tableLayoutPanelDisc.Controls.Add(_buttonBrowsePrimaryReference, 2, 0);
        tableLayoutPanelDisc.Controls.Add(_buttonBrowseSecondaryReference, 2, 1);
    }

    //-------------------------------------------------------------------------------
    // Ctrl+Z / Ctrl+Y のショートカット入力を処理する処理
    //-------------------------------------------------------------------------------
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Z))
        {
            UndoEditorChange();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Y) ||
            keyData == (Keys.Control | Keys.Shift | Keys.Z))
        {
            RedoEditorChange();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    //-------------------------------------------------------------------------------
    // 現在の編集状態を Undo 履歴へ保存する処理
    //-------------------------------------------------------------------------------
    private void RecordUndoSnapshot()
    {
        if (_applyingHistory)
        {
            return;
        }

        _undoStack.Push(CaptureEditorSnapshot());
        _redoStack.Clear();
        while (_undoStack.Count > 100)
        {
            _undoStack.TrimExcess();
            break;
        }
    }

    //-------------------------------------------------------------------------------
    // 現在の編集状態をスナップショット化する処理
    //-------------------------------------------------------------------------------
    private EditorSnapshot CaptureEditorSnapshot()
    {
        LayoutFile layout = new(_currentLayout.Spawns.ToList());
        RouteFile route = new(_currentRoute.Waypoints.ToDictionary(
            entry => entry.Key,
            entry => entry.Value with { Links = entry.Value.Links.ToList() }));
        WaterboxFile waterbox = new(_currentWaterbox.Type, _currentWaterbox.Boxes.ToList());
        return new EditorSnapshot(layout, route, waterbox, _selectedSpawnIndex, _selectedRouteWaypointIndex, _selectedWaterboxIndex);
    }

    //-------------------------------------------------------------------------------
    // スナップショットを現在の編集状態へ反映する処理
    //-------------------------------------------------------------------------------
    private void ApplyEditorSnapshot(EditorSnapshot snapshot)
    {
        _applyingHistory = true;
        try
        {
            _currentLayout = new LayoutFile(snapshot.Layout.Spawns.ToList());
            _currentRoute = new RouteFile(snapshot.Route.Waypoints.ToDictionary(
                entry => entry.Key,
                entry => entry.Value with { Links = entry.Value.Links.ToList() }));
            _currentWaterbox = new WaterboxFile(snapshot.Waterbox.Type, snapshot.Waterbox.Boxes.ToList());
            _selectedSpawnIndex = snapshot.SelectedSpawnIndex;
            _selectedRouteWaypointIndex = snapshot.SelectedWaypointIndex;
            _selectedWaterboxIndex = snapshot.SelectedWaterboxIndex;
            UpdateAllPreviewOverlays();
            SyncSelectedTargets();
            RefreshInspector();
            RefreshUnitSummary();
            UpdateQuickToolWindowState();
        }
        finally
        {
            _applyingHistory = false;
        }
    }

    //-------------------------------------------------------------------------------
    // 編集状態を 1 つ前へ戻す処理
    //-------------------------------------------------------------------------------
    private void UndoEditorChange()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        _redoStack.Push(CaptureEditorSnapshot());
        ApplyEditorSnapshot(_undoStack.Pop());
        AppendLog("Undo: 編集状態を戻しました．");
    }

    //-------------------------------------------------------------------------------
    // Undo した編集状態を 1 つ進める処理
    //-------------------------------------------------------------------------------
    private void RedoEditorChange()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        _undoStack.Push(CaptureEditorSnapshot());
        ApplyEditorSnapshot(_redoStack.Pop());
        AppendLog("Redo: 編集状態を進めました．");
    }

    //-------------------------------------------------------------------------------
    // Undo / Redo 履歴を初期化する処理
    //-------------------------------------------------------------------------------
    private void ClearEditorHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _continuousEditUndoRecorded = false;
    }

    //-------------------------------------------------------------------------------
    // ドラッグ編集の開始時に1回だけ Undo 履歴を保存する処理
    //-------------------------------------------------------------------------------
    private void Preview_OverlayDragStarted(object? sender, EventArgs e)
    {
        if (_continuousEditUndoRecorded)
        {
            return;
        }

        RecordUndoSnapshot();
        _continuousEditUndoRecorded = true;
    }

    //-------------------------------------------------------------------------------
    // ドラッグ編集の終了時に Undo 履歴の連続記録状態を解除する処理
    //-------------------------------------------------------------------------------
    private void Preview_OverlayDragEnded(object? sender, EventArgs e)
    {
        _continuousEditUndoRecorded = false;
    }

    //-------------------------------------------------------------------------------
    // 連続ドラッグ中は追加の Undo 履歴保存を抑制する処理
    //-------------------------------------------------------------------------------
    private void RecordUndoSnapshotForEditChange()
    {
        if (_continuousEditUndoRecorded)
        {
            return;
        }

        RecordUndoSnapshot();
    }

    private void BuildCollapsibleShells()
    {
        splitContainerMain.FixedPanel = FixedPanel.Panel1;
        splitContainerMain.Panel1MinSize = 34;
        splitContainerMain.Panel2MinSize = 720;

        _leftPaneTimer = new System.Windows.Forms.Timer { Interval = 12 };
        _leftPaneTimer.Tick += LeftPaneTimer_Tick;
        _mapUnitPaneTimer = new System.Windows.Forms.Timer { Interval = 12 };
        _mapUnitPaneTimer.Tick += MapUnitPaneTimer_Tick;
        _rightPaneTimer = new System.Windows.Forms.Timer { Interval = 12 };
        _rightPaneTimer.Tick += RightPaneTimer_Tick;
    }

    private void ConfigureSidebarLayout()
    {
        splitContainerMain.SplitterDistance = 520;
        tableLayoutPanelSidebar.RowStyles.Clear();
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 276F));
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 194F));
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
        tableLayoutPanelSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelSidebar.RowCount = 4;
        tableLayoutPanelCommon.RowCount = 6;
        while (tableLayoutPanelCommon.RowStyles.Count < 6)
        {
            tableLayoutPanelCommon.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
        }

        labelDiscRoot.Text = "参照先";
        tableLayoutPanelCommon.RowStyles[3] = new RowStyle(SizeType.Absolute, 34F);
        tableLayoutPanelCommon.RowStyles[4] = new RowStyle(SizeType.Absolute, 34F);
        tableLayoutPanelCommon.RowStyles[5] = new RowStyle(SizeType.Absolute, 70F);

        buttonPrepareCache.Visible = false;
        tableLayoutPanelDisc.Controls.Remove(buttonPrepareCache);
        if (tableLayoutPanelDisc.RowStyles.Count >= 6)
        {
            tableLayoutPanelDisc.RowStyles[5] = new RowStyle(SizeType.Absolute, 0F);
        }

        if (tableLayoutPanelSidebar.GetRow(groupBoxFloorSummary) != 3)
        {
            tableLayoutPanelSidebar.Controls.Remove(groupBoxFloorSummary);
            tableLayoutPanelSidebar.Controls.Add(groupBoxFloorSummary, 0, 3);
        }
    }

    //-------------------------------------------------------------------------------
    // 参照中ユニットの関連パスを表示するエリアを作成する処理
    //-------------------------------------------------------------------------------
    private void BuildReferenceUnitPanel()
    {
        if (_groupBoxReferenceUnit is not null)
        {
            return;
        }

        _groupBoxReferenceUnit = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "参照ユニット情報"
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(6)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
        for (int row = 0; row < 3; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        }

        _textBoxReferenceArc = AddReferencePathRow(layout, 0, "arc");
        _textBoxReferenceUnitCache = AddReferencePathRow(layout, 1, "ユニットキャッシュ");
        _textBoxReferenceImageCache = AddReferencePathRow(layout, 2, "画像キャッシュ");

        _groupBoxReferenceUnit.Controls.Add(layout);
        tableLayoutPanelSidebar.Controls.Add(_groupBoxReferenceUnit, 0, 2);
        RefreshReferenceUnitInfo();
    }

    //-------------------------------------------------------------------------------
    // 参照パス表示の 1 行を作成する処理
    //-------------------------------------------------------------------------------
    private TextBox AddReferencePathRow(TableLayoutPanel layout, int rowIndex, string labelText)
    {
        Label label = new()
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Text = labelText
        };
        TextBox textBox = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true
        };
        Button button = new()
        {
            Dock = DockStyle.Fill,
            Image = LoadQuickToolIcon("Open_icon.png"),
            Text = string.Empty,
            Tag = textBox
        };
        button.Click += ReferencePathOpenButton_Click;

        layout.Controls.Add(label, 0, rowIndex);
        layout.Controls.Add(textBox, 1, rowIndex);
        layout.Controls.Add(button, 2, rowIndex);
        return textBox;
    }

    //-------------------------------------------------------------------------------
    // 参照ユニット情報のパス表示を更新する処理
    //-------------------------------------------------------------------------------
    private void RefreshReferenceUnitInfo()
    {
        string? unitName = GetSelectedTemplateName();
        string? cacheRoot = GetCurrentCacheRoot();
        string? arcUnitPath;
        string? unitCachePath;
        string? imageCachePath;

        if (GetCurrentMode() == EditorMode.Field)
        {
            string? fieldMapRoot = GetCurrentFieldMapRoot();
            arcUnitPath = !string.IsNullOrWhiteSpace(unitName) && !string.IsNullOrWhiteSpace(fieldMapRoot)
                ? Path.Combine(fieldMapRoot, unitName)
                : fieldMapRoot;
            unitCachePath = !string.IsNullOrWhiteSpace(unitName) && cacheRoot is not null
                ? Path.Combine(cacheRoot, "地上キャッシュ", unitName)
                : cacheRoot is null ? null : Path.Combine(cacheRoot, "地上キャッシュ");
            imageCachePath = cacheRoot is not null
                ? Path.Combine(cacheRoot, "地上画像キャッシュ")
                : null;
        }
        else
        {
            arcUnitPath = !string.IsNullOrWhiteSpace(unitName) && Directory.Exists(textBoxArcPath.Text)
                ? Path.Combine(textBoxArcPath.Text, unitName)
                : null;
            unitCachePath = !string.IsNullOrWhiteSpace(unitName) && cacheRoot is not null
                ? Path.Combine(cacheRoot, "ユニットキャッシュ", unitName)
                : null;
            imageCachePath = cacheRoot is not null
                ? Path.Combine(cacheRoot, "画像キャッシュ")
                : null;
        }

        SetReferencePath(_textBoxReferenceArc, arcUnitPath);
        SetReferencePath(_textBoxReferenceUnitCache, unitCachePath);
        SetReferencePath(_textBoxReferenceImageCache, imageCachePath);
    }

    //-------------------------------------------------------------------------------
    // 埋め込みリソースから pretty 画像ストリームを返すプロバイダーを生成する処理
    //-------------------------------------------------------------------------------
    private static Func<string, Stream?> CreatePrettyImageProvider()
    {
        return unitName =>
            EmbeddedImageCatalog.OpenStream("pretty", $"{unitName}.png");
    }

    //-------------------------------------------------------------------------------
    // 指定ユニットの pretty 画像が埋め込みリソースに存在するかを確認する処理
    //-------------------------------------------------------------------------------
    private static bool HasEmbeddedPrettyImage(string unitName)
    {
        using Stream? stream = CreatePrettyImageProvider()(unitName);
        return stream is not null;
    }

    //-------------------------------------------------------------------------------
    // 現在の arc からキャッシュ親フォルダを取得する処理
    //-------------------------------------------------------------------------------
    private string? GetCurrentCacheRoot()
    {
        if (!string.IsNullOrWhiteSpace(_currentCacheRootOverride) &&
            Directory.Exists(_currentCacheRootOverride))
        {
            return _currentCacheRootOverride;
        }

        if (!Directory.Exists(textBoxArcPath.Text))
        {
            return null;
        }

        return Path.GetDirectoryName(textBoxArcPath.Text.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? textBoxArcPath.Text;
    }

    //-------------------------------------------------------------------------------
    // 現在ユニットに対応する caveinfo 側の参照パスを推定する処理
    //-------------------------------------------------------------------------------
    private string? ResolveCurrentCaveInfoReferencePath(string? unitName)
    {
        if (string.IsNullOrWhiteSpace(_currentCaveInfoDirectory))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(unitName))
        {
            string unitDirectory = Path.Combine(_currentCaveInfoDirectory, unitName);
            if (Directory.Exists(unitDirectory))
            {
                return unitDirectory;
            }
        }

        return _currentCaveInfoDirectory;
    }

    //-------------------------------------------------------------------------------
    // 参照パス表示欄へ存在状態付きでパスを設定する処理
    //-------------------------------------------------------------------------------
    private static void SetReferencePath(TextBox? textBox, string? path)
    {
        if (textBox is null)
        {
            return;
        }

        bool exists = !string.IsNullOrWhiteSpace(path) && (Directory.Exists(path) || File.Exists(path));
        textBox.Text = string.IsNullOrWhiteSpace(path) ? "-" : path;
        textBox.BackColor = exists ? Color.Honeydew : Color.MistyRose;
    }

    //-------------------------------------------------------------------------------
    // 参照パス行のフォルダを開く処理
    //-------------------------------------------------------------------------------
    private void ReferencePathOpenButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button { Tag: TextBox textBox })
        {
            return;
        }

        string path = textBox.Text;
        if (string.IsNullOrWhiteSpace(path) || path == "-")
        {
            return;
        }

        string? openPath = File.Exists(path)
            ? Path.GetDirectoryName(path) ?? path
            : path;
        openPath = ResolveExistingDirectory(openPath);
        if (string.IsNullOrWhiteSpace(openPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = openPath,
            UseShellExecute = true
        });
    }

    //-------------------------------------------------------------------------------
    // 指定パス自身または親階層から存在するフォルダを取得する処理
    //-------------------------------------------------------------------------------
    private static string? ResolveExistingDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return path;
        }

        DirectoryInfo? directory = Directory.GetParent(path);
        while (directory is not null)
        {
            if (directory.Exists)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 左側モードエリアのバナー表示を作成する処理
    //-------------------------------------------------------------------------------
    private void BuildModeBanner()
    {
        if (_pictureBoxModeBanner is not null)
        {
            return;
        }

        _pictureBoxModeBanner = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 0),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(245, 243, 236)
        };
        tableLayoutPanelCommon.Controls.Add(_pictureBoxModeBanner, 1, 5);
        tableLayoutPanelCommon.SetColumnSpan(_pictureBoxModeBanner, 2);
        UpdateModeBanner();
    }

    //-------------------------------------------------------------------------------
    // 現在の簡易操作対象に合わせて左側バナーを更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateModeBanner()
    {
        if (_pictureBoxModeBanner is null)
        {
            return;
        }

        _pictureBoxModeBanner.Visible = GetCurrentMode() == EditorMode.Cave;
        string fileName = _quickToolTarget switch
        {
            QuickToolTarget.Spawn => "Layout_editor_banner.png",
            QuickToolTarget.Route => "Route_editor_banner.png",
            _ => "WaterBox_editor_banner.png"
        };
        string? imagePath = FindFileInParentDirectories("ボタン用アイコン", fileName);
        string tag = imagePath ?? $"embedded:{fileName}";
        if (string.Equals(_pictureBoxModeBanner.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Image? bannerImage = EmbeddedImageCatalog.LoadImage("ボタン用アイコン", fileName);
        if (bannerImage is null && imagePath is not null)
        {
            bannerImage = LoadImageCloneFromFile(imagePath);
        }

        if (bannerImage is null)
        {
            _pictureBoxModeBanner.Image = null;
            return;
        }

        _pictureBoxModeBanner.Image?.Dispose();
        _pictureBoxModeBanner.Image = bannerImage;
        _pictureBoxModeBanner.Tag = tag;
    }

    private void BuildInspectorPanel()
    {
        panelInspectorContent.Controls.Clear();

        TableLayoutPanel inspectorLayout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1
        };
        inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panelInspectorContent.Controls.Add(inspectorLayout);

        _groupBoxSelection = new GroupBox
        {
            Dock = DockStyle.Top,
            Text = Localize("SelectedPoint"),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        FlowLayoutPanel selectionLayout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };
        _labelInspectorSelection = new Label
        {
            AutoSize = true,
            Text = "未選択",
            Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold)
        };
        selectionLayout.Controls.Add(_labelInspectorSelection);
        _groupBoxSelection.Controls.Add(selectionLayout);

        _groupBoxSpawnInspector = BuildSpawnInspector();
        _groupBoxWaypointInspector = BuildWaypointInspector();
        _groupBoxWaterboxInspector = BuildWaterboxInspector();

        inspectorLayout.Controls.Add(_groupBoxSelection);
        inspectorLayout.Controls.Add(_groupBoxSpawnInspector);
        inspectorLayout.Controls.Add(_groupBoxWaypointInspector);
        inspectorLayout.Controls.Add(_groupBoxWaterboxInspector);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator の日数条件を編集する UI を作成する処理
    //-------------------------------------------------------------------------------
    private GroupBox BuildFieldGeneratorInspector()
    {
        GroupBox groupBox = new()
        {
            Dock = DockStyle.Top,
            Text = "地上 generator 条件",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _numericFieldDay = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 999,
            Increment = 1
        };
        _numericFieldDay.ValueChanged += numericFieldDay_ValueChanged;
        _labelFieldActiveFiles = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            MaximumSize = new Size(220, 0),
            Text = "-"
        };

        AddInspectorRow(layout, 0, "経過日数", _numericFieldDay);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left, AutoSize = true, Text = "対象" }, 0, 1);
        layout.Controls.Add(_labelFieldActiveFiles, 1, 1);
        _textBoxFieldObjectRaw = new TextBox
        {
            Dock = DockStyle.Top,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Height = 220
        };
        _buttonApplyFieldObjectRaw = CreateActionButton("選択 object raw 反映", buttonApplyFieldObjectRaw_Click);
        _buttonApplyFieldObjectRaw.Dock = DockStyle.Top;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left, AutoSize = true, Text = "Raw" }, 0, 2);
        layout.Controls.Add(_textBoxFieldObjectRaw, 1, 2);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        layout.Controls.Add(_buttonApplyFieldObjectRaw, 1, 3);
        groupBox.Controls.Add(layout);
        return groupBox;
    }

    //-------------------------------------------------------------------------------
    // 地上モード専用の generator 操作コンソールを作成する処理
    //-------------------------------------------------------------------------------
    private void BuildFieldGeneratorConsoleWindow()
    {
        if (_fieldConsoleWindow is not null)
        {
            return;
        }

        _fieldConsoleWindow = new Panel
        {
            Size = new Size(660, 460),
            Location = new Point(64, 188),
            Padding = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(238, 241, 248),
            Visible = false
        };
        _fieldConsoleExpandedSize = _fieldConsoleWindow.Size;
        _fieldConsoleWindow.MouseDown += FieldConsoleWindow_MouseDown;
        _fieldConsoleWindow.MouseMove += FieldConsoleWindow_MouseMove;
        _fieldConsoleWindow.MouseUp += FieldConsoleWindow_MouseUp;

        _fieldConsoleGrip = new Panel
        {
            Dock = DockStyle.Top,
            Height = 28,
            Cursor = Cursors.SizeAll,
            BackColor = Color.FromArgb(196, 205, 224)
        };
        _fieldConsoleGrip.Paint += FieldConsoleGrip_Paint;
        _fieldConsoleGrip.MouseDown += FieldConsoleWindow_MouseDown;
        _fieldConsoleGrip.MouseMove += FieldConsoleWindow_MouseMove;
        _fieldConsoleGrip.MouseUp += FieldConsoleWindow_MouseUp;
        _quickToolTip?.SetToolTip(_fieldConsoleGrip, Localize("TipDragMove"));
        _buttonFieldConsoleMinimize = CreateWindowMinimizeButton(FieldConsoleMinimizeButton_Click);
        _fieldConsoleGrip.Controls.Add(_buttonFieldConsoleMinimize);

        _fieldConsoleContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(238, 241, 248)
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        _numericFieldDay = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 999,
            Increment = 1
        };
        _numericFieldDay.ValueChanged += numericFieldDay_ValueChanged;
        _labelFieldActiveFiles = new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = false,
            Text = "-"
        };
        FlowLayoutPanel addRow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _comboBoxFieldAddFile = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 210,
            Margin = new Padding(0, 3, 6, 0)
        };
        _comboBoxFieldAddType = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 160,
            Margin = new Padding(0, 3, 6, 0)
        };
        foreach ((FieldAddTemplateKind kind, string label) in FieldAddTemplateOptions)
        {
            _comboBoxFieldAddType.Items.Add(new FieldAddTemplateItem(kind, label));
        }

        if (_comboBoxFieldAddType.Items.Count > 0)
        {
            _comboBoxFieldAddType.SelectedIndex = 0;
        }

        _buttonFieldAddSpawnMode = CreateActionButton("クリック追加", buttonAddSpawn_Click);
        _buttonFieldAddSpawnMode.Width = 110;
        _buttonFieldAddSpawnMode.Height = 30;
        _buttonFieldAddSpawnMode.Margin = new Padding(0, 3, 0, 0);
        addRow.Controls.Add(_comboBoxFieldAddFile);
        addRow.Controls.Add(_comboBoxFieldAddType);
        addRow.Controls.Add(_buttonFieldAddSpawnMode);
        _textBoxFieldObjectRaw = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 9.5F, FontStyle.Regular, GraphicsUnit.Point)
        };
        _buttonApplyFieldObjectRaw = CreateActionButton("選択 object raw 反映", buttonApplyFieldObjectRaw_Click);
        _buttonApplyFieldObjectRaw.Dock = DockStyle.Left;
        _buttonApplyFieldObjectRaw.Width = 180;

        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left, AutoSize = true, Text = "経過日数" }, 0, 0);
        layout.Controls.Add(_numericFieldDay, 1, 0);
        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true, Text = "対象" }, 0, 1);
        layout.Controls.Add(_labelFieldActiveFiles, 1, 1);
        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left, AutoSize = true, Text = "追加" }, 0, 2);
        layout.Controls.Add(addRow, 1, 2);
        layout.Controls.Add(new Label { Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true, Text = "Raw" }, 0, 3);
        layout.Controls.Add(_textBoxFieldObjectRaw, 1, 3);
        layout.Controls.Add(_buttonApplyFieldObjectRaw, 1, 4);

        _fieldConsoleContentPanel.Controls.Add(layout);
        _fieldConsoleWindow.Controls.Add(_fieldConsoleContentPanel);
        _fieldConsoleWindow.Controls.Add(_fieldConsoleGrip);
        panelPreview.Controls.Add(_fieldConsoleWindow);
        _fieldConsoleWindow.BringToFront();
    }

    //-------------------------------------------------------------------------------
    // Console 近くへログ出力ボタンを配置する処理
    //-------------------------------------------------------------------------------
    private void BuildConsoleLogButton()
    {
        if (_buttonExportLog is not null)
        {
            return;
        }

        _buttonExportLog = CreateActionButton("コンソールログ出力", buttonExportLog_Click);
        _buttonExportLog.Dock = DockStyle.Right;
        _buttonExportLog.Width = 168;

        tableLayoutPanelFloorSummary.RowCount = 2;
        tableLayoutPanelFloorSummary.RowStyles.Clear();
        tableLayoutPanelFloorSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelFloorSummary.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tableLayoutPanelFloorSummary.Controls.Add(_buttonExportLog, 0, 1);
    }

    //-------------------------------------------------------------------------------
    // マップユニット表示エリアへ全ユニットキャッシュ作成ボタンと検索欄を配置する処理
    //-------------------------------------------------------------------------------
    private void BuildAllUnitCacheButton()
    {
        if (_buttonPrepareAllUnitCache is not null)
        {
            return;
        }

        tableLayoutPanelTemplates.Controls.Remove(labelTemplateRoot);
        _buttonPrepareAllUnitCache = new Button
        {
            Dock = DockStyle.Fill,
            Text = "全ユニットのキャッシュを作成",
            UseVisualStyleBackColor = true
        };
        _buttonPrepareAllUnitCache.Click += buttonPrepareAllUnitCache_Click;
        tableLayoutPanelTemplates.Controls.Add(_buttonPrepareAllUnitCache, 0, 0);

        if (_textBoxUnitSearch is null)
        {
            tableLayoutPanelTemplates.RowCount = 4;
            tableLayoutPanelTemplates.RowStyles.Clear();
            tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTemplates.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanelTemplates.SetRow(panelTemplateCardsScroll, 2);
            tableLayoutPanelTemplates.SetRow(buttonReloadTemplates, 3);

            _textBoxUnitSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4),
                PlaceholderText = "ユニット名で検索"
            };
            _textBoxUnitSearch.TextChanged += textBoxUnitSearch_TextChanged;
            tableLayoutPanelTemplates.Controls.Add(_textBoxUnitSearch, 0, 1);
        }
    }

    private void RelocateEditButtonsToInspector()
    {
        flowLayoutPanelRouteEdit.Controls.Clear();
        flowLayoutPanelRouteEdit.Visible = false;
    }

    private GroupBox BuildSpawnInspector()
    {
        GroupBox groupBox = new()
        {
            Dock = DockStyle.Top,
            Text = "Spawn 編集",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        TableLayoutPanel layout = CreateInspectorTable();
        _buttonAddSpawn = CreateActionButton("Spawn追加", buttonAddSpawn_Click);
        _buttonDeleteSpawn = CreateActionButton("選択Spawn削除", buttonDeleteSpawn_Click);
        _comboBoxSpawnType = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach ((int typeId, string label) in SpawnTypeOptions)
        {
            _comboBoxSpawnType.Items.Add(new SpawnTypeItem(typeId, label));
        }

        _numericSpawnX = CreateCoordinateEditor();
        _numericSpawnY = CreateCoordinateEditor();
        _numericSpawnZ = CreateCoordinateEditor();
        _numericSpawnAngle = CreateCoordinateEditor();
        _numericSpawnRadius = CreateCoordinateEditor();
        _numericSpawnMinCount = CreateIntegerEditor();
        _numericSpawnMaxCount = CreateIntegerEditor();
        _buttonApplySpawn = CreateActionButton("Spawn反映", buttonApplySpawn_Click);
        AddInspectorRow(layout, 0, "Type", _comboBoxSpawnType);
        AddInspectorRow(layout, 1, "X", _numericSpawnX);
        AddInspectorRow(layout, 2, "Y", _numericSpawnY);
        AddInspectorRow(layout, 3, "Z", _numericSpawnZ);
        AddInspectorRow(layout, 4, "Angle", _numericSpawnAngle);
        AddInspectorRow(layout, 5, "Radius", _numericSpawnRadius);
        AddInspectorRow(layout, 6, "MinCount", _numericSpawnMinCount);
        AddInspectorRow(layout, 7, "MaxCount", _numericSpawnMaxCount);
        layout.Controls.Add(_buttonApplySpawn, 0, 8);
        layout.SetColumnSpan(_buttonApplySpawn, 2);
        _buttonApplySpawn.Visible = false;
        AttachSpawnInspectorAutoApplyEvents();

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    private GroupBox BuildWaypointInspector()
    {
        GroupBox groupBox = new()
        {
            Dock = DockStyle.Top,
            Text = "Waypoint 編集",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        TableLayoutPanel layout = CreateInspectorTable();
        _buttonAddWaypoint = CreateActionButton("Waypoint追加", buttonAddWaypoint_Click);
        _buttonDeleteWaypoint = CreateActionButton("選択Waypoint削除", buttonDeleteWaypoint_Click);
        _labelWaypointIndex = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _numericWaypointX = CreateCoordinateEditor();
        _numericWaypointY = CreateCoordinateEditor();
        _numericWaypointZ = CreateCoordinateEditor();
        _numericWaypointRadius = CreateCoordinateEditor();
        _textBoxWaypointLinks = new TextBox
        {
            Dock = DockStyle.Fill
        };
        _buttonApplyWaypoint = CreateActionButton("Waypoint反映", buttonApplyWaypoint_Click);
        AddInspectorRow(layout, 0, "Index", _labelWaypointIndex);
        AddInspectorRow(layout, 1, "X", _numericWaypointX);
        AddInspectorRow(layout, 2, "Y", _numericWaypointY);
        AddInspectorRow(layout, 3, "Z", _numericWaypointZ);
        AddInspectorRow(layout, 4, "Radius", _numericWaypointRadius);
        AddInspectorRow(layout, 5, "Links", _textBoxWaypointLinks);
        layout.Controls.Add(_buttonApplyWaypoint, 0, 6);
        layout.SetColumnSpan(_buttonApplyWaypoint, 2);
        _buttonApplyWaypoint.Visible = false;
        AttachWaypointInspectorAutoApplyEvents();

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    private GroupBox BuildWaterboxInspector()
    {
        GroupBox groupBox = new()
        {
            Dock = DockStyle.Top,
            Text = "Waterbox 編集",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        TableLayoutPanel layout = CreateInspectorTable();
        _numericWaterboxX1 = CreateCoordinateEditor();
        _numericWaterboxY1 = CreateCoordinateEditor();
        _numericWaterboxZ1 = CreateCoordinateEditor();
        _numericWaterboxX2 = CreateCoordinateEditor();
        _numericWaterboxY2 = CreateCoordinateEditor();
        _numericWaterboxZ2 = CreateCoordinateEditor();
        _buttonApplyWaterbox = CreateActionButton("Waterbox反映", buttonApplyWaterbox_Click);
        AddInspectorRow(layout, 0, "X1", _numericWaterboxX1);
        AddInspectorRow(layout, 1, "Y1", _numericWaterboxY1);
        AddInspectorRow(layout, 2, "Z1", _numericWaterboxZ1);
        AddInspectorRow(layout, 3, "X2", _numericWaterboxX2);
        AddInspectorRow(layout, 4, "Y2", _numericWaterboxY2);
        AddInspectorRow(layout, 5, "Z2", _numericWaterboxZ2);
        layout.Controls.Add(_buttonApplyWaterbox, 0, 6);
        layout.SetColumnSpan(_buttonApplyWaterbox, 2);
        _buttonApplyWaterbox.Visible = false;
        AttachWaterboxInspectorAutoApplyEvents();

        groupBox.Controls.Add(layout);
        return groupBox;
    }

    //-------------------------------------------------------------------------------
    // Spawn 編集欄の変更を即時反映するイベントを設定する処理
    //-------------------------------------------------------------------------------
    private void AttachSpawnInspectorAutoApplyEvents()
    {
        if (_comboBoxSpawnType is not null)
        {
            _comboBoxSpawnType.SelectedIndexChanged += SpawnInspectorValueChanged;
        }

        foreach (NumericUpDown? editor in new[]
        {
            _numericSpawnX,
            _numericSpawnY,
            _numericSpawnZ,
            _numericSpawnAngle,
            _numericSpawnRadius,
            _numericSpawnMinCount,
            _numericSpawnMaxCount
        })
        {
            if (editor is not null)
            {
                editor.ValueChanged += SpawnInspectorValueChanged;
            }
        }
    }

    //-------------------------------------------------------------------------------
    // Waypoint 編集欄の変更を即時反映するイベントを設定する処理
    //-------------------------------------------------------------------------------
    private void AttachWaypointInspectorAutoApplyEvents()
    {
        foreach (NumericUpDown? editor in new[]
        {
            _numericWaypointX,
            _numericWaypointY,
            _numericWaypointZ,
            _numericWaypointRadius
        })
        {
            if (editor is not null)
            {
                editor.ValueChanged += WaypointInspectorValueChanged;
            }
        }

        if (_textBoxWaypointLinks is not null)
        {
            _textBoxWaypointLinks.Leave += WaypointInspectorValueChanged;
        }
    }

    //-------------------------------------------------------------------------------
    // Waterbox 編集欄の変更を即時反映するイベントを設定する処理
    //-------------------------------------------------------------------------------
    private void AttachWaterboxInspectorAutoApplyEvents()
    {
        foreach (NumericUpDown? editor in new[]
        {
            _numericWaterboxX1,
            _numericWaterboxY1,
            _numericWaterboxZ1,
            _numericWaterboxX2,
            _numericWaterboxY2,
            _numericWaterboxZ2
        })
        {
            if (editor is not null)
            {
                editor.ValueChanged += WaterboxInspectorValueChanged;
            }
        }
    }

    private static TableLayoutPanel CreateInspectorTable()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(6)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static Button CreateActionButton(string text, EventHandler onClick)
    {
        Button button = new()
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = text
        };
        button.Click += onClick;
        return button;
    }

    //-------------------------------------------------------------------------------
    // プレビュー上に表示する簡易操作ミニウィンドウを作成する処理
    //-------------------------------------------------------------------------------
    private void BuildQuickToolWindow()
    {
        _quickToolTip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 350,
            ReshowDelay = 100
        };

        _quickToolWindow = new Panel
        {
            Size = new Size(620, 196),
            Location = new Point(56, 700),
            Padding = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(225, 242, 232)
        };
        _quickToolWindow.MouseDown += QuickToolWindow_MouseDown;
        _quickToolWindow.MouseMove += QuickToolWindow_MouseMove;
        _quickToolWindow.MouseUp += QuickToolWindow_MouseUp;

        _quickToolGrip = new Panel
        {
            Dock = DockStyle.Top,
            Height = 28,
            Margin = Padding.Empty,
            Cursor = Cursors.SizeAll,
            BackColor = Color.FromArgb(196, 224, 207)
        };
        _quickToolGrip.Paint += QuickToolGrip_Paint;
        _quickToolGrip.MouseDown += QuickToolWindow_MouseDown;
        _quickToolGrip.MouseMove += QuickToolWindow_MouseMove;
        _quickToolGrip.MouseUp += QuickToolWindow_MouseUp;
        _quickToolTip.SetToolTip(_quickToolGrip, Localize("TipDragMove"));
        _buttonQuickToolMinimize = CreateWindowMinimizeButton(QuickToolMinimizeButton_Click);
        _quickToolGrip.Controls.Add(_buttonQuickToolMinimize);

        _quickToolContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 8, 8, 8),
            BackColor = Color.FromArgb(225, 242, 232)
        };
        _quickToolContentPanel.MouseDown += QuickToolWindow_MouseDown;
        _quickToolContentPanel.MouseMove += QuickToolWindow_MouseMove;
        _quickToolContentPanel.MouseUp += QuickToolWindow_MouseUp;

        TableLayoutPanel quickToolLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Color.FromArgb(225, 242, 232)
        };
        quickToolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        quickToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        quickToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        quickToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        quickToolLayout.MouseDown += QuickToolWindow_MouseDown;
        quickToolLayout.MouseMove += QuickToolWindow_MouseMove;
        quickToolLayout.MouseUp += QuickToolWindow_MouseUp;

        FlowLayoutPanel optionRow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 0)
        };
        optionRow.MouseDown += QuickToolWindow_MouseDown;
        optionRow.MouseMove += QuickToolWindow_MouseMove;
        optionRow.MouseUp += QuickToolWindow_MouseUp;

        FlowLayoutPanel modeRow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 2, 0, 0)
        };
        modeRow.MouseDown += QuickToolWindow_MouseDown;
        modeRow.MouseMove += QuickToolWindow_MouseMove;
        modeRow.MouseUp += QuickToolWindow_MouseUp;
        _buttonQuickSpawn = CreateQuickModeButton("Spawn", QuickToolTarget.Spawn);
        _buttonQuickRoute = CreateQuickModeButton("Route", QuickToolTarget.Route);
        _buttonQuickWaterbox = CreateQuickModeButton("Waterbox", QuickToolTarget.Waterbox);
        modeRow.Controls.Add(_buttonQuickSpawn);
        modeRow.Controls.Add(_buttonQuickRoute);
        modeRow.Controls.Add(_buttonQuickWaterbox);
        checkBoxObjDirectView.Parent?.Controls.Remove(checkBoxObjDirectView);
        checkBoxObjDirectView.AutoSize = true;
        checkBoxObjDirectView.Margin = new Padding(8, 9, 8, 0);
        modeRow.Controls.Add(checkBoxObjDirectView);
        checkBoxSpawnOverlay.Parent?.Controls.Remove(checkBoxSpawnOverlay);
        checkBoxSpawnOverlay.AutoSize = true;
        checkBoxSpawnOverlay.Margin = new Padding(0, 3, 8, 0);
        checkBoxRouteOverlay.Parent?.Controls.Remove(checkBoxRouteOverlay);
        checkBoxRouteOverlay.AutoSize = true;
        checkBoxRouteOverlay.Margin = new Padding(0, 3, 0, 0);
        _checkBoxRadiusOverlay = new CheckBox
        {
            AutoSize = true,
            Checked = true,
            Text = "Radius",
            Margin = new Padding(8, 3, 0, 0)
        };
        _checkBoxRadiusOverlay.CheckedChanged += (_, _) => UpdateAllPreviewOverlays();
        _checkBoxWaterboxOverlay = new CheckBox
        {
            AutoSize = true,
            Checked = true,
            Text = "水を表示",
            Margin = new Padding(8, 3, 0, 0)
        };
        _checkBoxWaterboxOverlay.CheckedChanged += (_, _) => UpdateAllPreviewOverlays();
        optionRow.Controls.Add(checkBoxSpawnOverlay);
        optionRow.Controls.Add(checkBoxRouteOverlay);
        optionRow.Controls.Add(_checkBoxRadiusOverlay);
        optionRow.Controls.Add(_checkBoxWaterboxOverlay);

        FlowLayoutPanel buttonRow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 8, 0, 0)
        };
        buttonRow.MouseDown += QuickToolWindow_MouseDown;
        buttonRow.MouseMove += QuickToolWindow_MouseMove;
        buttonRow.MouseUp += QuickToolWindow_MouseUp;
        _buttonQuickAdd = CreateQuickIconButton("pen_icon.png", "追加", buttonQuickAdd_Click);
        _buttonQuickRouteDelete = CreateQuickIconButton("Route_Delete_icon.png", "ルート削除", buttonQuickRouteDelete_Click);
        _buttonQuickDelete = CreateQuickIconButton("Eraser_icon.png", "削除", buttonQuickDelete_Click);
        _buttonQuickMove = CreateQuickIconButton("move_icon.png", "移動", buttonQuickMove_Click);
        _buttonQuickAngle = CreateQuickIconButton("angle_icon.png", "角度", buttonQuickAngle_Click);
        _buttonQuickRadius = CreateQuickIconButton("radius_icon.png", "Radius", buttonQuickRadius_Click);
        _buttonQuickConnect = CreateQuickIconButton("Route_icon.png", "接続", buttonQuickConnect_Click);
        _buttonQuickRoomConnect = CreateQuickIconButton("Room_Connect_icon.png", "接続座標へ移動", buttonQuickRoomConnect_Click);
        _buttonQuickSave = CreateQuickIconButton("Save_icon.png", "保存", buttonQuickSave_Click);
        _buttonQuickSaveAll = CreateQuickIconButton("All_Save_icon.png", "すべて保存", buttonQuickSaveAll_Click);
        _comboBoxQuickSpawnType = CreateQuickSpawnTypeComboBox();
        buttonRow.Controls.Add(_buttonQuickMove);
        buttonRow.Controls.Add(_buttonQuickAngle);
        buttonRow.Controls.Add(_buttonQuickRadius);
        buttonRow.Controls.Add(_buttonQuickAdd);
        buttonRow.Controls.Add(_buttonQuickDelete);
        buttonRow.Controls.Add(_buttonQuickConnect);
        buttonRow.Controls.Add(_buttonQuickRouteDelete);
        buttonRow.Controls.Add(_buttonQuickRoomConnect);
        buttonRow.Controls.Add(_buttonQuickSave);
        buttonRow.Controls.Add(_buttonQuickSaveAll);
        buttonRow.Controls.Add(_comboBoxQuickSpawnType);

        quickToolLayout.Controls.Add(modeRow, 0, 0);
        quickToolLayout.Controls.Add(optionRow, 0, 1);
        quickToolLayout.Controls.Add(buttonRow, 0, 2);
        _quickToolContentPanel.Controls.Add(quickToolLayout);
        _quickToolWindow.Controls.Add(_quickToolContentPanel);
        _quickToolWindow.Controls.Add(_quickToolGrip);
        panelPreview.Controls.Add(_quickToolWindow);
        _quickToolExpandedSize = _quickToolWindow.Size;
        _quickToolWindow.BringToFront();
        UpdateQuickToolWindowState();
    }

    //-------------------------------------------------------------------------------
    // Spawn/Route 切替用のミニボタンを作成する処理
    //-------------------------------------------------------------------------------
    private Button CreateQuickModeButton(string text, QuickToolTarget target)
    {
        Button button = new()
        {
            Text = text,
            Width = 94,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 4, 0),
            Tag = target
        };
        button.FlatAppearance.BorderSize = 1;
        button.Click += buttonQuickMode_Click;
        return button;
    }

    //-------------------------------------------------------------------------------
    // アイコン付き簡易操作ボタンを作成する処理
    //-------------------------------------------------------------------------------
    private Button CreateQuickIconButton(string iconFileName, string fallbackText, EventHandler onClick)
    {
        Button button = new()
        {
            Width = 36,
            Height = 46,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 2, 8, 0),
            Text = string.Empty,
            ImageAlign = ContentAlignment.MiddleCenter
        };
        button.FlatAppearance.BorderSize = 1;
        button.Click += onClick;

        Image? icon = LoadQuickToolIcon(iconFileName);
        if (icon is null)
        {
            button.Text = fallbackText;
            button.Font = new Font("Yu Gothic UI", 8F, FontStyle.Bold);
        }
        else
        {
            button.Image = icon;
        }

        return button;
    }

    //-------------------------------------------------------------------------------
    // フローティングウィンドウ用の最小化ボタンを作成する処理
    //-------------------------------------------------------------------------------
    private static Button CreateWindowMinimizeButton(EventHandler onClick)
    {
        Button button = new()
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(24, 22),
            Location = new Point(4, 3),
            FlatStyle = FlatStyle.Flat,
            Text = "-",
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(248, 250, 253),
            ForeColor = Color.FromArgb(30, 42, 68),
            TabStop = false
        };
        button.FlatAppearance.BorderSize = 1;
        button.Click += onClick;
        return button;
    }

    //-------------------------------------------------------------------------------
    // ミニコントローラ用の Spawn Type 選択欄を作成する処理
    //-------------------------------------------------------------------------------
    private ComboBox CreateQuickSpawnTypeComboBox()
    {
        ComboBox comboBox = new()
        {
            Width = 108,
            Height = 28,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 10, 0, 0)
        };

        foreach ((int typeId, string label) in SpawnTypeOptions)
        {
            comboBox.Items.Add(new SpawnTypeItem(typeId, label));
        }

        comboBox.SelectedIndex = Math.Max(0, comboBox.Items.Cast<SpawnTypeItem>().ToList().FindIndex(item => item.TypeId == 7));
        _quickToolTip?.SetToolTip(comboBox, Localize("TipSpawnType"));
        return comboBox;
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ボタン用アイコンを読み込む処理
    //-------------------------------------------------------------------------------
    private static Image? LoadQuickToolIcon(string fileName)
    {
        Image? embeddedSource = EmbeddedImageCatalog.LoadImage("ボタン用アイコン", fileName);
        if (embeddedSource is not null)
        {
            using (embeddedSource)
            {
                return new Bitmap(embeddedSource, new Size(22, 22));
            }
        }

        string? iconPath = FindFileInParentDirectories("ボタン用アイコン", fileName);
        if (iconPath is null)
        {
            return null;
        }

        using Image source = LoadImageCloneFromFile(iconPath);
        return new Bitmap(source, new Size(22, 22));
    }

    //-------------------------------------------------------------------------------
    // 親階層を辿って指定フォルダ内のファイルを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindFileInParentDirectories(string directoryName, string fileName)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, directoryName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        string workingCandidate = Path.Combine(Environment.CurrentDirectory, directoryName, fileName);
        return File.Exists(workingCandidate) ? workingCandidate : null;
    }

    //-------------------------------------------------------------------------------
    // 簡易操作対象を切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickMode_Click(object? sender, EventArgs e)
    {
        if (sender is Button { Tag: QuickToolTarget target })
        {
            _quickToolTarget = target;
            if ((_quickToolTarget == QuickToolTarget.Spawn && !IsSpawnQuickMode(_currentEditMode)) ||
                (_quickToolTarget == QuickToolTarget.Route && !IsRouteQuickMode(_currentEditMode)) ||
                (_quickToolTarget == QuickToolTarget.Waterbox && !IsWaterboxQuickMode(_currentEditMode)))
            {
                _currentEditMode = UnitMapEditMode.Navigate;
                _unitMapView.SetEditMode(_currentEditMode);
                _objModelView.SetEditMode(_currentEditMode);
                UpdateRouteEditUi();
            }

            UpdateQuickToolWindowState();
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から現在対象のポイントを追加する処理
    //-------------------------------------------------------------------------------
    private void buttonQuickAdd_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget == QuickToolTarget.Spawn)
        {
            buttonAddSpawn_Click(sender, e);
        }
        else if (_quickToolTarget == QuickToolTarget.Route)
        {
            buttonAddWaypoint_Click(sender, e);
        }
        else
        {
            buttonAddWaterbox_Click(sender, e);
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から現在対象のポイントを削除する処理
    //-------------------------------------------------------------------------------
    private void buttonQuickDelete_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget == QuickToolTarget.Spawn)
        {
            buttonDeleteSpawn_Click(sender, e);
        }
        else if (_quickToolTarget == QuickToolTarget.Route)
        {
            buttonDeleteWaypoint_Click(sender, e);
        }
        else
        {
            buttonDeleteWaterbox_Click(sender, e);
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から Spawn 角度変更モードを切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickAngle_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget != QuickToolTarget.Spawn || !CanEditSpawnLikePoints())
        {
            return;
        }

        _currentEditMode = _currentEditMode == UnitMapEditMode.RotateSpawn
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.RotateSpawn;

        if (_currentEditMode == UnitMapEditMode.RotateSpawn)
        {
            checkBoxSpawnOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から現在対象の Radius 変更モードを切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickRadius_Click(object? sender, EventArgs e)
    {
        if (!CanEditSpawnLikePoints() && !CanEditRoutePoints())
        {
            return;
        }

        if (_quickToolTarget == QuickToolTarget.Spawn)
        {
            if (!CanEditSpawnLikePoints())
            {
                return;
            }

            _currentEditMode = _currentEditMode == UnitMapEditMode.ResizeSpawnRadius
                ? UnitMapEditMode.Navigate
                : UnitMapEditMode.ResizeSpawnRadius;
            if (_currentEditMode == UnitMapEditMode.ResizeSpawnRadius)
            {
                checkBoxSpawnOverlay.Checked = true;
            }
        }
        else if (_quickToolTarget == QuickToolTarget.Route)
        {
            if (!CanEditRoutePoints())
            {
                return;
            }

            _currentEditMode = _currentEditMode == UnitMapEditMode.ResizeRouteWaypointRadius
                ? UnitMapEditMode.Navigate
                : UnitMapEditMode.ResizeRouteWaypointRadius;
            if (_currentEditMode == UnitMapEditMode.ResizeRouteWaypointRadius)
            {
                checkBoxRouteOverlay.Checked = true;
            }
        }
        else
        {
            return;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から現在対象の移動モードを切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickMove_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget == QuickToolTarget.Spawn)
        {
            buttonSpawnMoveMode_Click(sender, e);
        }
        else if (_quickToolTarget == QuickToolTarget.Route)
        {
            buttonRouteMoveMode_Click(sender, e);
        }
        else
        {
            buttonWaterboxMoveMode_Click(sender, e);
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から route 接続モードを切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickConnect_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget != QuickToolTarget.Route || !CanEditRoutePoints())
        {
            return;
        }

        _currentEditMode = _currentEditMode == UnitMapEditMode.ConnectRouteWaypoint
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.ConnectRouteWaypoint;

        if (_currentEditMode == UnitMapEditMode.ConnectRouteWaypoint)
        {
            checkBoxRouteOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から route 削除モードを切り替える処理
    //-------------------------------------------------------------------------------
    private void buttonQuickRouteDelete_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget != QuickToolTarget.Route || !CanEditRoutePoints())
        {
            return;
        }

        _currentEditMode = _currentEditMode == UnitMapEditMode.DeleteRouteLink
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.DeleteRouteLink;

        if (_currentEditMode == UnitMapEditMode.DeleteRouteLink)
        {
            checkBoxRouteOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から選択 Waypoint を最寄りのマップ接続座標へ移動する処理
    //-------------------------------------------------------------------------------
    private void buttonQuickRoomConnect_Click(object? sender, EventArgs e)
    {
        MoveSelectedWaypointToNearestDoorPoint();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から現在対象の編集ファイルを保存する処理
    //-------------------------------------------------------------------------------
    private void buttonQuickSave_Click(object? sender, EventArgs e)
    {
        if (_quickToolTarget == QuickToolTarget.Spawn)
        {
            buttonSaveLayout_Click(sender, e);
        }
        else
        {
            buttonSaveRoute_Click(sender, e);
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作から layout/route/waterbox をまとめて保存する処理
    //-------------------------------------------------------------------------------
    private async void buttonQuickSaveAll_Click(object? sender, EventArgs e)
    {
        await SaveCurrentUnitArchivesAsync();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウのドラッグ開始を処理する処理
    //-------------------------------------------------------------------------------
    private void QuickToolWindow_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _quickToolDragging = true;
        if (_quickToolWindow is not null)
        {
            Control source = sender as Control ?? _quickToolWindow;
            _quickToolDragOffset = _quickToolWindow.PointToClient(source.PointToScreen(e.Location));
            _quickToolWindow.Capture = true;
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウの最小化状態を切り替える処理
    //-------------------------------------------------------------------------------
    private void QuickToolMinimizeButton_Click(object? sender, EventArgs e)
    {
        if (_quickToolWindow is null || _quickToolGrip is null || _quickToolContentPanel is null)
        {
            return;
        }

        _quickToolMinimized = !_quickToolMinimized;
        if (_quickToolMinimized)
        {
            _quickToolExpandedSize = _quickToolWindow.Size;
            _quickToolContentPanel.Visible = false;
            _quickToolWindow.Size = new Size(240, 42);
        }
        else
        {
            _quickToolWindow.Size = _quickToolExpandedSize.Width > 0 ? _quickToolExpandedSize : new Size(620, 196);
            _quickToolContentPanel.Visible = true;
        }

        UpdateFloatingWindowMinimizeButtons();
        ClampQuickToolWindowToPreview();
        _quickToolGrip.Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウのドラッグ移動を処理する処理
    //-------------------------------------------------------------------------------
    private void QuickToolWindow_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_quickToolDragging || _quickToolWindow is null)
        {
            return;
        }

        Control source = sender as Control ?? _quickToolWindow;
        Point parentPoint = panelPreview.PointToClient(source.PointToScreen(e.Location));
        int x = Math.Clamp(parentPoint.X - _quickToolDragOffset.X, 0, Math.Max(0, panelPreview.ClientSize.Width - _quickToolWindow.Width));
        int y = Math.Clamp(parentPoint.Y - _quickToolDragOffset.Y, 0, Math.Max(0, panelPreview.ClientSize.Height - _quickToolWindow.Height));
        _quickToolWindow.Location = new Point(x, y);
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウの位置をプレビュー領域内に制限する処理
    //-------------------------------------------------------------------------------
    private void ClampQuickToolWindowToPreview()
    {
        if (_quickToolWindow is null)
        {
            return;
        }

        int x = Math.Clamp(_quickToolWindow.Left, 0, Math.Max(0, panelPreview.ClientSize.Width - _quickToolWindow.Width));
        int y = Math.Clamp(_quickToolWindow.Top, 0, Math.Max(0, panelPreview.ClientSize.Height - _quickToolWindow.Height));
        _quickToolWindow.Location = new Point(x, y);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールの位置をプレビュー領域内に制限する処理
    //-------------------------------------------------------------------------------
    private void ClampFieldConsoleWindowToPreview()
    {
        if (_fieldConsoleWindow is null)
        {
            return;
        }

        int x = Math.Clamp(_fieldConsoleWindow.Left, 0, Math.Max(0, panelPreview.ClientSize.Width - _fieldConsoleWindow.Width));
        int y = Math.Clamp(_fieldConsoleWindow.Top, 0, Math.Max(0, panelPreview.ClientSize.Height - _fieldConsoleWindow.Height));
        _fieldConsoleWindow.Location = new Point(x, y);
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウのドラッグ終了を処理する処理
    //-------------------------------------------------------------------------------
    private void QuickToolWindow_MouseUp(object? sender, MouseEventArgs e)
    {
        _quickToolDragging = false;
        if (_quickToolWindow is not null)
        {
            _quickToolWindow.Capture = false;
        }
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールのドラッグ開始を処理する処理
    //-------------------------------------------------------------------------------
    private void FieldConsoleWindow_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _fieldConsoleWindow is null)
        {
            return;
        }

        Control source = sender as Control ?? _fieldConsoleWindow;
        _fieldConsoleDragging = true;
        _fieldConsoleDragOffset = _fieldConsoleWindow.PointToClient(source.PointToScreen(e.Location));
        _fieldConsoleWindow.Capture = true;
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールのドラッグ移動を処理する処理
    //-------------------------------------------------------------------------------
    private void FieldConsoleWindow_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_fieldConsoleDragging || _fieldConsoleWindow is null)
        {
            return;
        }

        Control source = sender as Control ?? _fieldConsoleWindow;
        Point parentPoint = panelPreview.PointToClient(source.PointToScreen(e.Location));
        int x = Math.Clamp(parentPoint.X - _fieldConsoleDragOffset.X, 0, Math.Max(0, panelPreview.ClientSize.Width - _fieldConsoleWindow.Width));
        int y = Math.Clamp(parentPoint.Y - _fieldConsoleDragOffset.Y, 0, Math.Max(0, panelPreview.ClientSize.Height - _fieldConsoleWindow.Height));
        _fieldConsoleWindow.Location = new Point(x, y);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールのドラッグ終了を処理する処理
    //-------------------------------------------------------------------------------
    private void FieldConsoleWindow_MouseUp(object? sender, MouseEventArgs e)
    {
        _fieldConsoleDragging = false;
        if (_fieldConsoleWindow is not null)
        {
            _fieldConsoleWindow.Capture = false;
        }
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールの最小化状態を切り替える処理
    //-------------------------------------------------------------------------------
    private void FieldConsoleMinimizeButton_Click(object? sender, EventArgs e)
    {
        if (_fieldConsoleWindow is null || _fieldConsoleContentPanel is null)
        {
            return;
        }

        _fieldConsoleMinimized = !_fieldConsoleMinimized;
        if (_fieldConsoleMinimized)
        {
            _fieldConsoleExpandedSize = _fieldConsoleWindow.Size;
            _fieldConsoleContentPanel.Visible = false;
            _fieldConsoleWindow.Size = new Size(240, 42);
        }
        else
        {
            _fieldConsoleWindow.Size = _fieldConsoleExpandedSize.Width > 0 ? _fieldConsoleExpandedSize : new Size(660, 460);
            _fieldConsoleContentPanel.Visible = true;
        }

        UpdateFloatingWindowMinimizeButtons();
        ClampFieldConsoleWindowToPreview();
        _fieldConsoleGrip?.Invalidate();
    }

    //-------------------------------------------------------------------------------
    // フローティングウィンドウの最小化ボタン表示を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateFloatingWindowMinimizeButtons()
    {
        if (_buttonQuickToolMinimize is not null)
        {
            _buttonQuickToolMinimize.Text = _quickToolMinimized ? "+" : "-";
        }

        if (_buttonFieldConsoleMinimize is not null)
        {
            _buttonFieldConsoleMinimize.Text = _fieldConsoleMinimized ? "+" : "-";
        }
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウのグリップ見出しを描画する処理
    //-------------------------------------------------------------------------------
    private void QuickToolGrip_Paint(object? sender, PaintEventArgs e)
    {
        string title = Localize("MiniController");
        string text = _quickToolMinimized ? $"{title}  ..." : title;
        using Font font = new("Yu Gothic UI", 9F, FontStyle.Bold);
        TextRenderer.DrawText(
            e.Graphics,
            text,
            font,
            new Rectangle(8, 4, Math.Max(0, e.ClipRectangle.Width - 42), 20),
            Color.FromArgb(30, 42, 68),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator コンソールのグリップ見出しを描画する処理
    //-------------------------------------------------------------------------------
    private void FieldConsoleGrip_Paint(object? sender, PaintEventArgs e)
    {
        string title = Localize("FieldGeneratorConsole");
        string text = _fieldConsoleMinimized ? $"{title}  ..." : title;
        using Font font = new("Yu Gothic UI", 9F, FontStyle.Bold);
        if (_buttonFieldConsoleMinimize is not null && _fieldConsoleGrip is not null)
        {
            _buttonFieldConsoleMinimize.Location = new Point(Math.Max(4, _fieldConsoleGrip.Width - _buttonFieldConsoleMinimize.Width - 4), 3);
        }

        TextRenderer.DrawText(
            e.Graphics,
            text,
            font,
            new Rectangle(8, 4, Math.Max(0, e.ClipRectangle.Width - 42), 20),
            Color.FromArgb(30, 42, 68),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    //-------------------------------------------------------------------------------
    // 選択されたポイント種別に合わせて簡易操作ミニウィンドウを切り替える処理
    //-------------------------------------------------------------------------------
    private void SwitchQuickToolTargetFromSelection(QuickToolTarget target)
    {
        if (_currentEditMode != UnitMapEditMode.Navigate || _quickToolTarget == target)
        {
            return;
        }

        _quickToolTarget = target;
        UpdateQuickToolWindowState();
    }

    //-------------------------------------------------------------------------------
    // 簡易操作ミニウィンドウの表示状態と色を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateQuickToolWindowState()
    {
        if (_quickToolWindow is null ||
            _quickToolGrip is null ||
            _quickToolContentPanel is null ||
            _buttonQuickSpawn is null ||
            _buttonQuickRoute is null ||
            _buttonQuickWaterbox is null ||
            _buttonQuickAdd is null ||
            _buttonQuickRouteDelete is null ||
            _buttonQuickDelete is null ||
            _buttonQuickMove is null ||
            _buttonQuickAngle is null ||
            _buttonQuickRadius is null ||
            _buttonQuickConnect is null ||
            _buttonQuickRoomConnect is null ||
            _buttonQuickSave is null ||
            _buttonQuickSaveAll is null ||
            _comboBoxQuickSpawnType is null ||
            _checkBoxRadiusOverlay is null ||
            _checkBoxWaterboxOverlay is null ||
            _buttonQuickToolMinimize is null)
        {
            return;
        }

        bool isCave = GetCurrentMode() == EditorMode.Cave;
        bool isField = GetCurrentMode() == EditorMode.Field;
        bool hasFieldMap = isField && _currentFieldMapData is not null;
        bool isSpawn = _quickToolTarget == QuickToolTarget.Spawn;
        bool isRoute = _quickToolTarget == QuickToolTarget.Route;
        bool isWaterbox = _quickToolTarget == QuickToolTarget.Waterbox;

        Color baseColor = isSpawn
            ? Color.FromArgb(224, 242, 232)
            : isRoute ? Color.FromArgb(224, 235, 248) : Color.FromArgb(222, 240, 246);
        Color activeColor = isSpawn
            ? Color.FromArgb(77, 151, 105)
            : isRoute ? Color.FromArgb(67, 126, 183) : Color.FromArgb(0, 128, 168);
        _quickToolWindow.BackColor = baseColor;
        _quickToolContentPanel.BackColor = baseColor;
        _quickToolGrip.BackColor = isSpawn
            ? Color.FromArgb(196, 224, 207)
            : isRoute ? Color.FromArgb(196, 213, 232) : Color.FromArgb(188, 223, 233);
        _buttonQuickToolMinimize.Location = new Point(Math.Max(4, _quickToolGrip.Width - _buttonQuickToolMinimize.Width - 4), 3);
        UpdateFloatingWindowMinimizeButtons();
        _quickToolGrip.Invalidate();
        ApplyQuickModeButtonStyle(_buttonQuickSpawn, isSpawn, Color.FromArgb(77, 151, 105));
        ApplyQuickModeButtonStyle(_buttonQuickRoute, isRoute, Color.FromArgb(67, 126, 183));
        ApplyQuickModeButtonStyle(_buttonQuickWaterbox, isWaterbox, Color.FromArgb(0, 128, 168));

        _buttonQuickAdd.Enabled = isCave ||
            (isField && hasFieldMap && (isSpawn || isRoute));
        _buttonQuickRouteDelete.Visible = isRoute;
        _buttonQuickRouteDelete.Enabled = CanEditRoutePoints() && isRoute;
        _buttonQuickDelete.Enabled = isCave ||
            (isField && hasFieldMap && (isSpawn || isRoute));
        _buttonQuickMove.Enabled = isCave ||
            (isField && hasFieldMap && (isSpawn || isRoute));
        _buttonQuickAngle.Visible = isSpawn;
        _buttonQuickAngle.Enabled = CanEditSpawnLikePoints() && isSpawn;
        _buttonQuickRadius.Visible = isSpawn || isRoute;
        _buttonQuickRadius.Enabled = (CanEditSpawnLikePoints() && isSpawn) ||
            (CanEditRoutePoints() && isRoute);
        _buttonQuickConnect.Visible = isRoute;
        _buttonQuickConnect.Enabled = CanEditRoutePoints() && isRoute && _currentRoute.Waypoints.Count >= 2;
        _buttonQuickRoomConnect.Visible = isRoute;
        _buttonQuickRoomConnect.Enabled = isCave &&
            isRoute &&
            _selectedRouteWaypointIndex is not null &&
            _currentUnitDefinition is not null &&
            _currentUnitDefinition.Doors.Count > 0;
        string? targetPath = isSpawn ? _currentLayoutPath : isRoute ? _currentRoutePath : _currentWaterboxPath;
        _buttonQuickSave.Enabled = (isCave && !string.IsNullOrWhiteSpace(targetPath)) ||
            (isField && hasFieldMap && (isSpawn || isRoute));
        _buttonQuickSaveAll.Enabled = (isCave && HasAnyUnitSavePath()) ||
            (isField && hasFieldMap);
        _comboBoxQuickSpawnType.Visible = isSpawn;
        _comboBoxQuickSpawnType.Enabled = isCave && isSpawn;
        checkBoxObjDirectView.Enabled = isCave || _currentObjScene is not null;
        checkBoxSpawnOverlay.Enabled = isCave || hasFieldMap;
        checkBoxRouteOverlay.Enabled = isCave || hasFieldMap;
        _checkBoxRadiusOverlay.Enabled = isCave || hasFieldMap;
        _checkBoxWaterboxOverlay.Enabled = isCave || hasFieldMap;
        _buttonQuickAdd.BackColor = IsCurrentQuickAddModeActive()
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickAdd.ForeColor = IsCurrentQuickAddModeActive() ? Color.White : Color.Black;
        _buttonQuickMove.BackColor = IsCurrentQuickMoveModeActive()
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickMove.ForeColor = IsCurrentQuickMoveModeActive() ? Color.White : Color.Black;
        _buttonQuickAngle.BackColor = _currentEditMode == UnitMapEditMode.RotateSpawn
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickAngle.ForeColor = _currentEditMode == UnitMapEditMode.RotateSpawn ? Color.White : Color.Black;
        _buttonQuickRadius.BackColor =
            (_currentEditMode == UnitMapEditMode.ResizeSpawnRadius && isSpawn) ||
            (_currentEditMode == UnitMapEditMode.ResizeRouteWaypointRadius && isRoute)
                ? activeColor
                : Color.FromArgb(250, 250, 250);
        _buttonQuickRadius.ForeColor =
            (_currentEditMode == UnitMapEditMode.ResizeSpawnRadius && isSpawn) ||
            (_currentEditMode == UnitMapEditMode.ResizeRouteWaypointRadius && isRoute)
                ? Color.White
                : Color.Black;
        _buttonQuickDelete.BackColor = IsCurrentQuickDeleteModeActive()
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickDelete.ForeColor = IsCurrentQuickDeleteModeActive() ? Color.White : Color.Black;
        _buttonQuickConnect.BackColor = _currentEditMode == UnitMapEditMode.ConnectRouteWaypoint
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickConnect.ForeColor = _currentEditMode == UnitMapEditMode.ConnectRouteWaypoint ? Color.White : Color.Black;
        _buttonQuickRouteDelete.BackColor = _currentEditMode == UnitMapEditMode.DeleteRouteLink
            ? activeColor
            : Color.FromArgb(250, 250, 250);
        _buttonQuickRouteDelete.ForeColor = _currentEditMode == UnitMapEditMode.DeleteRouteLink ? Color.White : Color.Black;
        _buttonQuickRoomConnect.BackColor = Color.FromArgb(250, 250, 250);
        _buttonQuickRoomConnect.ForeColor = Color.Black;
        _buttonQuickSaveAll.Visible = true;
        _buttonQuickSaveAll.BackColor = Color.FromArgb(250, 250, 250);
        _buttonQuickSaveAll.ForeColor = Color.Black;

        string fieldPendingSuffix = isField && !((isSpawn || isRoute) && hasFieldMap) ? Localize("TipFieldPending") : string.Empty;
        string fieldMoveSuffix = isField && (isSpawn || isRoute) ? Localize("TipFieldSupported") : fieldPendingSuffix;
        _quickToolTip?.SetToolTip(_buttonQuickAdd, Localize(isSpawn ? "TipAddSpawn" : isRoute ? "TipAddWaypoint" : "TipAddWaterbox") + fieldPendingSuffix);
        _quickToolTip?.SetToolTip(_buttonQuickRouteDelete, Localize("TipRouteDelete"));
        _quickToolTip?.SetToolTip(_buttonQuickRoomConnect, Localize("TipRoomConnect"));
        _quickToolTip?.SetToolTip(_buttonQuickDelete, Localize(isSpawn ? "TipDeleteSpawn" : isRoute ? "TipDeleteWaypoint" : "TipDeleteWaterbox") + fieldPendingSuffix);
        _quickToolTip?.SetToolTip(_buttonQuickMove, Localize(isSpawn ? "TipMoveSpawn" : isRoute ? "TipMoveWaypoint" : "TipMoveWaterbox") + fieldMoveSuffix);
        _quickToolTip?.SetToolTip(_buttonQuickAngle, Localize("TipAngleSpawn"));
        _quickToolTip?.SetToolTip(_buttonQuickRadius, Localize(isSpawn ? "TipRadiusSpawn" : "TipRadiusWaypoint"));
        _quickToolTip?.SetToolTip(_checkBoxRadiusOverlay, Localize("TipRadiusOverlay"));
        _quickToolTip?.SetToolTip(_buttonQuickConnect, Localize("TipRouteConnect"));
        _quickToolTip?.SetToolTip(_buttonQuickSave, isField ? Localize("TipSaveField") : isSpawn ? Localize("TipSaveLayout") : isRoute ? Localize("TipSaveRoute") : Localize("TipSaveWaterbox"));
        _quickToolTip?.SetToolTip(_buttonQuickSaveAll, isField ? Localize("TipSaveAllField") : Localize("TipSaveAllCave"));
        _quickToolWindow.Visible = isCave || hasFieldMap;
        _quickToolWindow.BringToFront();
        if (_fieldConsoleWindow is not null)
        {
            _fieldConsoleWindow.Visible = hasFieldMap;
            if (hasFieldMap)
            {
                _fieldConsoleWindow.BringToFront();
            }
        }
        UpdateModeBanner();
    }

    //-------------------------------------------------------------------------------
    // 現在ユニットに保存可能な編集ファイルがあるか判定する処理
    //-------------------------------------------------------------------------------
    private bool HasAnyUnitSavePath()
    {
        return !string.IsNullOrWhiteSpace(_currentLayoutPath) ||
            !string.IsNullOrWhiteSpace(_currentRoutePath) ||
            !string.IsNullOrWhiteSpace(_currentWaterboxPath);
    }

    //-------------------------------------------------------------------------------
    // 現在モードで Spawn 相当ポイントを編集できるか判定する処理
    //-------------------------------------------------------------------------------
    private bool CanEditSpawnLikePoints()
    {
        return GetCurrentMode() == EditorMode.Cave ||
            (GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null);
    }

    //-------------------------------------------------------------------------------
    // 現在モードで Route ポイントを編集できるか判定する処理
    //-------------------------------------------------------------------------------
    private bool CanEditRoutePoints()
    {
        return GetCurrentMode() == EditorMode.Cave ||
            (GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null);
    }

    //-------------------------------------------------------------------------------
    // 簡易操作の対象切替ボタンの色を更新する処理
    //-------------------------------------------------------------------------------
    private static void ApplyQuickModeButtonStyle(Button button, bool active, Color activeColor)
    {
        button.BackColor = active ? activeColor : Color.FromArgb(250, 250, 250);
        button.ForeColor = active ? Color.White : Color.Black;
    }

    //-------------------------------------------------------------------------------
    // 現在対象の移動モードが有効か判定する処理
    //-------------------------------------------------------------------------------
    private bool IsCurrentQuickMoveModeActive()
    {
        return _quickToolTarget switch
        {
            QuickToolTarget.Spawn => _currentEditMode == UnitMapEditMode.MoveSpawn,
            QuickToolTarget.Route => _currentEditMode == UnitMapEditMode.MoveRouteWaypoint,
            QuickToolTarget.Waterbox => _currentEditMode == UnitMapEditMode.MoveWaterbox,
            _ => false
        };
    }

    //-------------------------------------------------------------------------------
    // 現在対象の追加モードが有効か判定する処理
    //-------------------------------------------------------------------------------
    private bool IsCurrentQuickAddModeActive()
    {
        return _quickToolTarget switch
        {
            QuickToolTarget.Spawn => _currentEditMode == UnitMapEditMode.AddSpawn,
            QuickToolTarget.Route => _currentEditMode == UnitMapEditMode.AddRouteWaypoint,
            QuickToolTarget.Waterbox => _currentEditMode == UnitMapEditMode.AddWaterbox,
            _ => false
        };
    }

    //-------------------------------------------------------------------------------
    // 現在対象の削除モードが有効か判定する処理
    //-------------------------------------------------------------------------------
    private bool IsCurrentQuickDeleteModeActive()
    {
        return _quickToolTarget switch
        {
            QuickToolTarget.Spawn => _currentEditMode == UnitMapEditMode.DeleteSpawn,
            QuickToolTarget.Route => _currentEditMode == UnitMapEditMode.DeleteRouteWaypoint,
            QuickToolTarget.Waterbox => _currentEditMode == UnitMapEditMode.DeleteWaterbox,
            _ => false
        };
    }

    //-------------------------------------------------------------------------------
    // 指定編集モードが Spawn 系の簡易操作か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsSpawnQuickMode(UnitMapEditMode mode)
    {
        return mode == UnitMapEditMode.Navigate ||
            mode == UnitMapEditMode.AddSpawn ||
            mode == UnitMapEditMode.DeleteSpawn ||
            mode == UnitMapEditMode.MoveSpawn ||
            mode == UnitMapEditMode.RotateSpawn ||
            mode == UnitMapEditMode.ResizeSpawnRadius;
    }

    //-------------------------------------------------------------------------------
    // 指定編集モードが Route 系の簡易操作か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsRouteQuickMode(UnitMapEditMode mode)
    {
        return mode == UnitMapEditMode.Navigate ||
            mode == UnitMapEditMode.AddRouteWaypoint ||
            mode == UnitMapEditMode.DeleteRouteWaypoint ||
            mode == UnitMapEditMode.MoveRouteWaypoint ||
            mode == UnitMapEditMode.ConnectRouteWaypoint ||
            mode == UnitMapEditMode.DeleteRouteLink ||
            mode == UnitMapEditMode.ResizeRouteWaypointRadius;
    }

    //-------------------------------------------------------------------------------
    // 指定編集モードが Waterbox 系の簡易操作か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsWaterboxQuickMode(UnitMapEditMode mode)
    {
        return mode == UnitMapEditMode.Navigate ||
            mode == UnitMapEditMode.AddWaterbox ||
            mode == UnitMapEditMode.DeleteWaterbox ||
            mode == UnitMapEditMode.MoveWaterbox;
    }

    private static NumericUpDown CreateCoordinateEditor()
    {
        NumericUpDown editor = new()
        {
            DecimalPlaces = 3,
            Increment = 1,
            Minimum = -1000000,
            Maximum = 1000000,
            Dock = DockStyle.Fill,
            ThousandsSeparator = false
        };
        return editor;
    }

    private static NumericUpDown CreateIntegerEditor()
    {
        NumericUpDown editor = new()
        {
            DecimalPlaces = 0,
            Increment = 1,
            Minimum = 0,
            Maximum = 9999,
            Dock = DockStyle.Fill
        };
        return editor;
    }

    private static void AddInspectorRow(TableLayoutPanel layout, int rowIndex, string labelText, Control editor)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        Label label = new()
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Text = labelText
        };
        layout.Controls.Add(label, 0, rowIndex);
        layout.Controls.Add(editor, 1, rowIndex);
    }

    private void buttonToggleLeftPane_Click(object? sender, EventArgs e)
    {
        if (_leftPaneTimer is null)
        {
            return;
        }

        if (!_leftPaneCollapsed)
        {
            _expandedLeftPaneWidth = Math.Max(splitContainerMain.SplitterDistance, 320);
        }

        _leftPaneTimer.Start();
    }

    private void buttonToggleRightPane_Click(object? sender, EventArgs e)
    {
        if (_rightPaneTimer is null)
        {
            return;
        }

        if (!_rightPaneCollapsed)
        {
            _expandedRightPaneWidth = Math.Max(panelInspectorShell.Width, 308);
        }

        _rightPaneTimer.Start();
    }

    private void buttonToggleMapUnitPane_Click(object? sender, EventArgs e)
    {
        if (_mapUnitPaneTimer is null)
        {
            return;
        }

        if (!_mapUnitPaneCollapsed)
        {
            _expandedMapUnitPaneWidth = Math.Max(panelMapUnitShell.Width, 260);
        }

        _mapUnitPaneTimer.Start();
    }

    private void LeftPaneTimer_Tick(object? sender, EventArgs e)
    {
        int current = splitContainerMain.SplitterDistance;
        int collapsedWidth = 28;
        int target = _leftPaneCollapsed ? _expandedLeftPaneWidth : collapsedWidth;
        int step = 36;

        if (Math.Abs(current - target) <= step)
        {
            splitContainerMain.SplitterDistance = target;
            _leftPaneCollapsed = !_leftPaneCollapsed;
            panelSidebarContentHost.Visible = !_leftPaneCollapsed;
            buttonToggleLeftPane.Text = _leftPaneCollapsed ? ">" : "<";

            _leftPaneTimer?.Stop();
            return;
        }

        if (!_leftPaneCollapsed)
        {
            panelSidebarContentHost.Visible = false;
        }

        splitContainerMain.SplitterDistance = current + (current < target ? step : -step);
    }

    private void RightPaneTimer_Tick(object? sender, EventArgs e)
    {
        int current = panelInspectorShell.Width;
        int collapsedWidth = panelInspectorTabHost.Width;
        int target = _rightPaneCollapsed ? _expandedRightPaneWidth : collapsedWidth;
        int step = 28;

        if (Math.Abs(current - target) <= step)
        {
            panelInspectorShell.Width = target;
            _rightPaneCollapsed = !_rightPaneCollapsed;
            panelInspectorPanelHost.Visible = !_rightPaneCollapsed;
            panelInspectorContent.Visible = !_rightPaneCollapsed;
            buttonToggleRightPane.Text = _rightPaneCollapsed ? "<" : ">";

            _rightPaneTimer?.Stop();
            return;
        }

        if (!_rightPaneCollapsed)
        {
            panelInspectorContent.Visible = false;
        }

        panelInspectorShell.Width = current + (current < target ? step : -step);
        panelInspectorPanelHost.Visible = true;
    }

    private void MapUnitPaneTimer_Tick(object? sender, EventArgs e)
    {
        int current = panelMapUnitShell.Width;
        int collapsedWidth = panelMapUnitTabHost.Width;
        int target = _mapUnitPaneCollapsed ? _expandedMapUnitPaneWidth : collapsedWidth;
        int step = 28;

        if (Math.Abs(current - target) <= step)
        {
            panelMapUnitShell.Width = target;
            _mapUnitPaneCollapsed = !_mapUnitPaneCollapsed;
            panelMapUnitHost.Visible = !_mapUnitPaneCollapsed;
            buttonToggleMapUnitPane.Text = _mapUnitPaneCollapsed ? ">" : "<";

            _mapUnitPaneTimer?.Stop();
            return;
        }

        if (!_mapUnitPaneCollapsed)
        {
            panelMapUnitHost.Visible = false;
        }

        panelMapUnitShell.Width = current + (current < target ? step : -step);
        panelMapUnitHost.Visible = true;
    }

    //-------------------------------------------------------------------------------
    // アプリ起動時に設定と参照先を初期化する処理
    //-------------------------------------------------------------------------------
    private void Form1_Load(object sender, EventArgs e)
    {
        _settings = _settingsStore.Load();

        if (string.IsNullOrWhiteSpace(_settings.ToolkitPath))
        {
            _settings.ToolkitPath = _repository.TryFindDefaultToolkitPath() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_settings.DiscRoot))
        {
            _settings.DiscRoot = _repository.TryFindDefaultDiscRootPath() ?? string.Empty;
        }

        textBoxToolkitPath.Text = _settings.ToolkitPath;
        textBoxDiscRoot.Text = _settings.DiscRoot;
        _settings.UseObjDirectView = false;
        checkBoxObjDirectView.Checked = false;
        comboBoxMode.SelectedIndex = _settings.LastMode switch
        {
            "Cave" => 1,
            _ => 0
        };
        SelectHomeLanguageItem(_settings.Language);
        ApplyLanguageToUi();
        SetLoadFormat(LoadFormatKind.None, null);

        ConfigureEditorModeUi();
        ApplyToolkitState(showWarning: true);
        LoadLocationProfiles();

        if (Directory.Exists(textBoxDiscRoot.Text) || File.Exists(textBoxDiscRoot.Text))
        {
            _ = ResolveDiscPathsAsync(textBoxDiscRoot.Text);
        }
        else
        {
            RefreshPreview();
        }

        ShowHomeScreen();
    }

    private void buttonAddSpawn_Click(object? sender, EventArgs e)
    {
        if (!CanEditSpawnLikePoints())
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Spawn;
        _currentEditMode = _currentEditMode == UnitMapEditMode.AddSpawn
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.AddSpawn;
        if (_currentEditMode == UnitMapEditMode.AddSpawn)
        {
            checkBoxSpawnOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    private void buttonDeleteSpawn_Click(object? sender, EventArgs e)
    {
        if (!CanEditSpawnLikePoints())
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Spawn;
        _currentEditMode = _currentEditMode == UnitMapEditMode.DeleteSpawn
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.DeleteSpawn;
        if (_currentEditMode == UnitMapEditMode.DeleteSpawn)
        {
            checkBoxSpawnOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 指定 index の Spawn を削除する処理
    //-------------------------------------------------------------------------------
    private void DeleteSpawnAt(int spawnIndex)
    {
        if (spawnIndex < 0 || spawnIndex >= _currentLayout.Spawns.Count)
        {
            return;
        }

        if (GetCurrentMode() == EditorMode.Field)
        {
            DeleteFieldObjectAt(spawnIndex);
            return;
        }

        RecordUndoSnapshot();
        List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
        updatedSpawns.RemoveAt(spawnIndex);
        _currentLayout = new LayoutFile(updatedSpawns);
        _selectedSpawnIndex = null;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        AppendLog($"spawn 削除: index={spawnIndex}");
    }

    private void buttonApplySpawn_Click(object? sender, EventArgs e)
    {
        ApplySpawnInspectorValues();
    }

    //-------------------------------------------------------------------------------
    // Spawn 編集欄の変更を現在の選択 Spawn へ反映する処理
    //-------------------------------------------------------------------------------
    private bool ApplySpawnInspectorValues()
    {
        if (_inspectorUpdating ||
            _selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _currentLayout.Spawns.Count ||
            _comboBoxSpawnType?.SelectedItem is not SpawnTypeItem typeItem ||
            _numericSpawnX is null ||
            _numericSpawnY is null ||
            _numericSpawnZ is null ||
            _numericSpawnAngle is null ||
            _numericSpawnRadius is null ||
            _numericSpawnMinCount is null ||
            _numericSpawnMaxCount is null)
        {
            return false;
        }

        List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
        RecordUndoSnapshot();
        updatedSpawns[_selectedSpawnIndex.Value] = new LayoutSpawn(
            typeItem.TypeId,
            typeItem.Label,
            (float)_numericSpawnX.Value,
            (float)_numericSpawnY.Value,
            (float)_numericSpawnZ.Value,
            (float)_numericSpawnAngle.Value,
            Math.Max(0f, (float)_numericSpawnRadius.Value),
            (int)_numericSpawnMinCount.Value,
            Math.Max((int)_numericSpawnMinCount.Value, (int)_numericSpawnMaxCount.Value));
        _currentLayout = new LayoutFile(updatedSpawns);
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        return true;
    }

    //-------------------------------------------------------------------------------
    // Spawn 編集欄の変更イベントから即時反映を実行する処理
    //-------------------------------------------------------------------------------
    private void SpawnInspectorValueChanged(object? sender, EventArgs e)
    {
        ApplySpawnInspectorValues();
    }

    private void buttonAddWaypoint_Click(object? sender, EventArgs e)
    {
        if (!CanEditRoutePoints())
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Route;
        _currentEditMode = _currentEditMode == UnitMapEditMode.AddRouteWaypoint
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.AddRouteWaypoint;
        if (_currentEditMode == UnitMapEditMode.AddRouteWaypoint)
        {
            checkBoxRouteOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    private void buttonDeleteWaypoint_Click(object? sender, EventArgs e)
    {
        if (!CanEditRoutePoints())
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Route;
        _currentEditMode = _currentEditMode == UnitMapEditMode.DeleteRouteWaypoint
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.DeleteRouteWaypoint;
        if (_currentEditMode == UnitMapEditMode.DeleteRouteWaypoint)
        {
            checkBoxRouteOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    private void buttonAddWaterbox_Click(object? sender, EventArgs e)
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Waterbox;
        _currentEditMode = _currentEditMode == UnitMapEditMode.AddWaterbox
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.AddWaterbox;
        if (_currentEditMode == UnitMapEditMode.AddWaterbox && _checkBoxWaterboxOverlay is not null)
        {
            _checkBoxWaterboxOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    private void buttonDeleteWaterbox_Click(object? sender, EventArgs e)
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Waterbox;
        _currentEditMode = _currentEditMode == UnitMapEditMode.DeleteWaterbox
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.DeleteWaterbox;
        if (_currentEditMode == UnitMapEditMode.DeleteWaterbox && _checkBoxWaterboxOverlay is not null)
        {
            _checkBoxWaterboxOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    private void buttonWaterboxMoveMode_Click(object? sender, EventArgs e)
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            return;
        }

        _quickToolTarget = QuickToolTarget.Waterbox;
        _currentEditMode = _currentEditMode == UnitMapEditMode.MoveWaterbox
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.MoveWaterbox;
        if (_currentEditMode == UnitMapEditMode.MoveWaterbox && _checkBoxWaterboxOverlay is not null)
        {
            _checkBoxWaterboxOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 指定 index の Waypoint を削除する処理
    //-------------------------------------------------------------------------------
    private void DeleteWaypointAt(int waypointIndex)
    {
        if (!_currentRoute.Waypoints.ContainsKey(waypointIndex))
        {
            return;
        }

        RecordUndoSnapshotForEditChange();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .Where(entry => entry.Key != waypointIndex)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value with
                {
                    Links = entry.Value.Links.Where(link => link != waypointIndex).ToList()
                });

        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = null;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        AppendLog($"waypoint 削除: index={waypointIndex}");
    }

    //-------------------------------------------------------------------------------
    // 指定座標へ Waypoint を追加する処理
    //-------------------------------------------------------------------------------
    private void AddRouteWaypointAt(float x, float z)
    {
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        RouteWaypoint newWaypoint = CreateDefaultWaypointAt(x, z);
        RecordUndoSnapshot();
        updatedWaypoints[newWaypoint.Index] = newWaypoint;
        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = newWaypoint.Index;
        _selectedSpawnIndex = null;
        checkBoxRouteOverlay.Checked = true;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
        AppendLog($"waypoint 追加: index={newWaypoint.Index}, x={newWaypoint.X:0.###}, y={newWaypoint.Y:0.###}, z={newWaypoint.Z:0.###}");
    }

    //-------------------------------------------------------------------------------
    // 選択中 Waypoint を最寄りのマップ接続座標へ移動する処理
    //-------------------------------------------------------------------------------
    private void MoveSelectedWaypointToNearestDoorPoint()
    {
        if (_selectedRouteWaypointIndex is null ||
            !_currentRoute.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint))
        {
            AppendLog("接続座標補正: Waypoint が選択されていません．");
            return;
        }

        if (_currentUnitDefinition is null || _currentUnitDefinition.Doors.Count == 0)
        {
            AppendLog("接続座標補正: ユニット定義のドア情報が見つかりません．");
            return;
        }

        DoorSnapPoint nearest = GetDoorSnapPoints(_currentUnitDefinition)
            .OrderBy(point => GetSquaredDistance(waypoint.X, waypoint.Z, point.X, point.Z))
            .First();
        float movedY = GetGroundHeightOrFallback(nearest.X, nearest.Z, waypoint.Y);

        RecordUndoSnapshot();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        updatedWaypoints[waypoint.Index] = waypoint with
        {
            X = nearest.X,
            Y = movedY,
            Z = nearest.Z
        };
        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = waypoint.Index;
        _selectedSpawnIndex = null;
        checkBoxRouteOverlay.Checked = true;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
        AppendLog($"接続座標補正: waypoint={waypoint.Index}, door={nearest.DoorIndex}, x={nearest.X:0.###}, z={nearest.Z:0.###}");
    }

    //-------------------------------------------------------------------------------
    // ユニット定義のドア情報からローカル接続座標一覧を作成する処理
    //-------------------------------------------------------------------------------
    private static IReadOnlyList<DoorSnapPoint> GetDoorSnapPoints(UnitDefinition unitDefinition)
    {
        List<DoorSnapPoint> points = new();
        foreach (DoorDefinition door in unitDefinition.Doors)
        {
            points.Add(CreateDoorSnapPoint(unitDefinition, door));
        }

        return points;
    }

    //-------------------------------------------------------------------------------
    // CaveGen の doorPos と同じ基準でドアのローカル接続座標を算出する処理
    //-------------------------------------------------------------------------------
    private static DoorSnapPoint CreateDoorSnapPoint(UnitDefinition unitDefinition, DoorDefinition door)
    {
        float cellX = 0f;
        float cellZ = 0f;
        switch (door.Direction)
        {
            case 0:
                cellX = door.Offset + 0.5f;
                cellZ = 0f;
                break;
            case 1:
                cellX = unitDefinition.Width;
                cellZ = door.Offset + 0.5f;
                break;
            case 2:
                cellX = door.Offset + 0.5f;
                cellZ = unitDefinition.Height;
                break;
            case 3:
                cellX = 0f;
                cellZ = door.Offset + 0.5f;
                break;
        }

        float x = (cellX - (unitDefinition.Width * 0.5f)) * 170f;
        float z = (cellZ - (unitDefinition.Height * 0.5f)) * 170f;
        return new DoorSnapPoint(door.Index, x, z);
    }

    //-------------------------------------------------------------------------------
    // 2点間距離の二乗を取得する処理
    //-------------------------------------------------------------------------------
    private static float GetSquaredDistance(float firstX, float firstZ, float secondX, float secondZ)
    {
        float diffX = firstX - secondX;
        float diffZ = firstZ - secondZ;
        return (diffX * diffX) + (diffZ * diffZ);
    }

    private void buttonApplyWaypoint_Click(object? sender, EventArgs e)
    {
        ApplyWaypointInspectorValues(showWarning: true);
    }

    private void buttonApplyWaterbox_Click(object? sender, EventArgs e)
    {
        ApplyWaterboxInspectorValues();
    }

    //-------------------------------------------------------------------------------
    // Waterbox 編集欄の変更を現在の選択 Waterbox へ反映する処理
    //-------------------------------------------------------------------------------
    private bool ApplyWaterboxInspectorValues()
    {
        if (_inspectorUpdating ||
            _selectedWaterboxIndex is null ||
            _selectedWaterboxIndex.Value < 0 ||
            _selectedWaterboxIndex.Value >= _currentWaterbox.Boxes.Count ||
            _numericWaterboxX1 is null ||
            _numericWaterboxY1 is null ||
            _numericWaterboxZ1 is null ||
            _numericWaterboxX2 is null ||
            _numericWaterboxY2 is null ||
            _numericWaterboxZ2 is null)
        {
            return false;
        }

        List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
        RecordUndoSnapshot();
        boxes[_selectedWaterboxIndex.Value] = new WaterboxEntry(
            (float)_numericWaterboxX1.Value,
            (float)_numericWaterboxY1.Value,
            (float)_numericWaterboxZ1.Value,
            (float)_numericWaterboxX2.Value,
            (float)_numericWaterboxY2.Value,
            (float)_numericWaterboxZ2.Value);
        _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        return true;
    }

    //-------------------------------------------------------------------------------
    // Waterbox 編集欄の変更イベントから即時反映を実行する処理
    //-------------------------------------------------------------------------------
    private void WaterboxInspectorValueChanged(object? sender, EventArgs e)
    {
        ApplyWaterboxInspectorValues();
    }

    //-------------------------------------------------------------------------------
    // 地上 generator の経過日数変更を表示中 object に反映する処理
    //-------------------------------------------------------------------------------
    private void numericFieldDay_ValueChanged(object? sender, EventArgs e)
    {
        if (_inspectorUpdating || _numericFieldDay is null)
        {
            return;
        }

        CommitCurrentFieldLayoutToMapData();
        _currentFieldDay = (int)_numericFieldDay.Value;
        RefreshFieldGeneratorDisplayObjects(resetSelection: true);
        UpdateAllPreviewOverlays(resetView: true);
        RefreshInspector();
        RefreshUnitSummary();
        AppendLog($"地上 generator 日数変更: day={_currentFieldDay}");
    }

    //-------------------------------------------------------------------------------
    // raw editor の内容を選択中の地上 generator object へ反映する処理
    //-------------------------------------------------------------------------------
    private void buttonApplyFieldObjectRaw_Click(object? sender, EventArgs e)
    {
        if (_currentFieldMapData is null ||
            _textBoxFieldObjectRaw is null ||
            !TryGetSelectedFieldObjectRef(out FieldDisplayObjectRef objectRef))
        {
            return;
        }

        try
        {
            CommitCurrentFieldLayoutToMapData();
            List<FieldGeneratorFile> generatorFiles = _currentFieldMapData.GeneratorFiles.ToList();
            generatorFiles[objectRef.FileIndex] = FieldGeneratorParser.ReplaceObjectRawText(
                generatorFiles[objectRef.FileIndex],
                objectRef.ObjectIndex,
                _textBoxFieldObjectRaw.Text);
            _currentFieldMapData = _currentFieldMapData with
            {
                GeneratorFiles = generatorFiles
            };
            RefreshFieldGeneratorDisplayObjects(resetSelection: false);
            UpdateAllPreviewOverlays();
            RefreshInspector();
            RefreshUnitSummary();
            AppendLog($"地上 object raw 反映: {_currentFieldMapData.GeneratorFiles[objectRef.FileIndex].DisplayName} #{objectRef.ObjectIndex}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"raw object の反映に失敗しました．\n{ex.Message}", "地上 object raw", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AppendLog($"地上 object raw 反映失敗: {ex.Message}");
        }
    }

    //-------------------------------------------------------------------------------
    // Waypoint 編集欄の変更を現在の選択 Waypoint へ反映する処理
    //-------------------------------------------------------------------------------
    private bool ApplyWaypointInspectorValues(bool showWarning)
    {
        if (_inspectorUpdating ||
            _selectedRouteWaypointIndex is null ||
            !_currentRoute.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint) ||
            _numericWaypointX is null ||
            _numericWaypointY is null ||
            _numericWaypointZ is null ||
            _numericWaypointRadius is null ||
            _textBoxWaypointLinks is null)
        {
            return false;
        }

        List<int> links;
        try
        {
            links = ParseWaypointLinks(_textBoxWaypointLinks.Text, waypoint.Index);
        }
        catch (Exception ex)
        {
            if (showWarning)
            {
                MessageBox.Show(this, ex.Message, "Waypoint Links", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        RecordUndoSnapshot();
        updatedWaypoints[waypoint.Index] = waypoint with
        {
            X = (float)_numericWaypointX.Value,
            Y = (float)_numericWaypointY.Value,
            Z = (float)_numericWaypointZ.Value,
            Radius = Math.Max(0f, (float)_numericWaypointRadius.Value),
            Links = links
        };
        _currentRoute = new RouteFile(updatedWaypoints);
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        return true;
    }

    //-------------------------------------------------------------------------------
    // Waypoint 編集欄の変更イベントから即時反映を実行する処理
    //-------------------------------------------------------------------------------
    private void WaypointInspectorValueChanged(object? sender, EventArgs e)
    {
        ApplyWaypointInspectorValues(showWarning: false);
    }

    private LayoutSpawn CreateDefaultSpawn()
    {
        PointF point = GetDefaultPlacementPoint();
        return CreateDefaultSpawnAt(point.X, point.Y);
    }

    private RouteWaypoint CreateDefaultWaypoint()
    {
        PointF point = GetDefaultPlacementPoint();
        return CreateDefaultWaypointAt(point.X, point.Y);
    }

    //-------------------------------------------------------------------------------
    // 指定座標へ配置する既定 Spawn を作成する処理
    //-------------------------------------------------------------------------------
    private LayoutSpawn CreateDefaultSpawnAt(float x, float z)
    {
        float y = GetGroundHeightOrFallback(x, z, 0f);
        SpawnTypeItem typeItem = GetSelectedQuickSpawnType();
        return new LayoutSpawn(typeItem.TypeId, typeItem.Label, x, y, z, 0f, 48f, 1, 1);
    }

    //-------------------------------------------------------------------------------
    // ミニコントローラで選択中の Spawn Type を取得する処理
    //-------------------------------------------------------------------------------
    private SpawnTypeItem GetSelectedQuickSpawnType()
    {
        if (_comboBoxQuickSpawnType?.SelectedItem is SpawnTypeItem typeItem)
        {
            return typeItem;
        }

        return new SpawnTypeItem(7, "Start");
    }

    //-------------------------------------------------------------------------------
    // 指定座標へ配置する既定 Waypoint を作成する処理
    //-------------------------------------------------------------------------------
    private RouteWaypoint CreateDefaultWaypointAt(float x, float z)
    {
        int newIndex = 0;
        while (_currentRoute.Waypoints.ContainsKey(newIndex))
        {
            newIndex++;
        }

        float y = GetGroundHeightOrFallback(x, z, 0f);
        return new RouteWaypoint(newIndex, Array.Empty<int>(), x, y, z, 32f);
    }

    private PointF GetDefaultPlacementPoint()
    {
        if (_selectedSpawnIndex is not null &&
            _selectedSpawnIndex.Value >= 0 &&
            _selectedSpawnIndex.Value < _currentLayout.Spawns.Count)
        {
            LayoutSpawn spawn = _currentLayout.Spawns[_selectedSpawnIndex.Value];
            return new PointF(spawn.X + 32f, spawn.Z + 32f);
        }

        if (_selectedRouteWaypointIndex is not null &&
            _currentRoute.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint))
        {
            return new PointF(waypoint.X + 32f, waypoint.Z + 32f);
        }

        if (_currentModelBounds.Width > 0f && _currentModelBounds.Height > 0f)
        {
            return new PointF(
                _currentModelBounds.Left + (_currentModelBounds.Width * 0.5f),
                _currentModelBounds.Top + (_currentModelBounds.Height * 0.5f));
        }

        return PointF.Empty;
    }

    private static List<int> ParseWaypointLinks(string text, int selfIndex)
    {
        List<int> links = new();
        foreach (string token in text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int value = int.Parse(token, CultureInfo.InvariantCulture);
            if (value == selfIndex || links.Contains(value))
            {
                continue;
            }

            links.Add(value);
        }

        return links;
    }

    private void UpdateMapOverlays(bool resetView = false)
    {
        bool showRadius = _checkBoxRadiusOverlay?.Checked != false;
        _unitMapView.SetRadiusVisible(showRadius);
        _unitMapView.SetViewTitle(GetCurrentMode() == EditorMode.Field ? "地上マップモード" : "洞窟ユニットモード");
        _unitMapView.SetUseFieldObjectIcons(GetCurrentMode() == EditorMode.Field);
        _unitMapView.SetRouteColorHeights(BuildRouteColorHeights(_currentRoute));
        LayoutFile layout = checkBoxSpawnOverlay.Checked
            ? _currentLayout
            : new LayoutFile(Array.Empty<LayoutSpawn>());
        RouteFile route = checkBoxRouteOverlay.Checked
            ? _currentRoute
            : new RouteFile(new Dictionary<int, RouteWaypoint>());
        WaterboxFile waterbox = _checkBoxWaterboxOverlay?.Checked == true
            ? _currentWaterbox
            : new WaterboxFile(_currentWaterbox.Type, Array.Empty<WaterboxEntry>());

        if (resetView)
        {
            using Image? image = !string.IsNullOrWhiteSpace(_currentPreviewImagePath) && File.Exists(_currentPreviewImagePath)
                ? LoadImageCloneFromFile(_currentPreviewImagePath)
                : null;
            if (_currentPreviewImageIsPretty && image is not null)
            {
                image.RotateFlip(RotateFlipType.Rotate180FlipX);
            }

            RectangleF imageBounds = _currentPreviewImageIsPretty && image is not null
                ? GetPrettyImageDisplayBounds(_currentModelBounds, _currentUnitDefinition, layout, route, image.Size)
                : _currentModelBounds;
            _unitMapView.SetScene(image, imageBounds, layout, route, waterbox);
            _unitMapView.SetEditMode(_currentEditMode);
        }
        else
        {
            _unitMapView.UpdateOverlayData(layout, route, waterbox);
        }

        if (_selectedSpawnIndex is not null)
        {
            _unitMapView.SelectSpawn(_selectedSpawnIndex);
        }
        else if (_selectedRouteWaypointIndex is not null)
        {
            _unitMapView.SelectRouteWaypoint(_selectedRouteWaypointIndex);
        }
        else if (_selectedWaterboxIndex is not null)
        {
            _unitMapView.SelectWaterbox(_selectedWaterboxIndex);
        }
    }

    //-------------------------------------------------------------------------------
    // 2D/3D の両プレビューへ overlay データを反映する処理
    //-------------------------------------------------------------------------------
    private void UpdateAllPreviewOverlays(bool resetView = false)
    {
        UpdateMapOverlays(resetView);
        UpdateObjDirectView(resetView);
    }

    //-------------------------------------------------------------------------------
    // 現在の OBJ シーンを 3D ビューへ反映する処理
    //-------------------------------------------------------------------------------
    private void UpdateObjDirectView(bool resetView = false)
    {
        bool showRadius = _checkBoxRadiusOverlay?.Checked != false;
        _objModelView.SetRadiusVisible(showRadius);
        _objModelView.SetUseFieldObjectIcons(GetCurrentMode() == EditorMode.Field);
        _objModelView.SetRouteColorHeights(BuildRouteColorHeights(_currentRoute));
        LayoutFile layout = checkBoxSpawnOverlay.Checked
            ? _currentLayout
            : new LayoutFile(Array.Empty<LayoutSpawn>());
        RouteFile route = checkBoxRouteOverlay.Checked
            ? _currentRoute
            : new RouteFile(new Dictionary<int, RouteWaypoint>());
        WaterboxFile waterbox = _checkBoxWaterboxOverlay?.Checked == true
            ? _currentWaterbox
            : new WaterboxFile(_currentWaterbox.Type, Array.Empty<WaterboxEntry>());
        _objModelView.SetScene(_currentObjScene, _currentPreviewUnitName, layout, route, waterbox, resetView);
        SyncSelectedTargets();
    }

    //-------------------------------------------------------------------------------
    // 現在の選択状態を 2D/3D 両ビューへ同期する処理
    //-------------------------------------------------------------------------------
    private void SyncSelectedTargets()
    {
        if (_selectedSpawnIndex is not null)
        {
            _unitMapView.SelectSpawn(_selectedSpawnIndex);
            _objModelView.SelectSpawn(_selectedSpawnIndex);
            return;
        }

        if (_selectedRouteWaypointIndex is not null)
        {
            _unitMapView.SelectRouteWaypoint(_selectedRouteWaypointIndex);
            _objModelView.SelectRouteWaypoint(_selectedRouteWaypointIndex);
            return;
        }

        if (_selectedWaterboxIndex is not null)
        {
            _unitMapView.SelectWaterbox(_selectedWaterboxIndex);
            _objModelView.SelectWaterbox(_selectedWaterboxIndex);
            return;
        }

        _unitMapView.SelectSpawn(null);
        _unitMapView.SelectRouteWaypoint(null);
        _unitMapView.SelectWaterbox(null);
        _objModelView.SelectSpawn(null);
        _objModelView.SelectRouteWaypoint(null);
        _objModelView.SelectWaterbox(null);
    }

    //-------------------------------------------------------------------------------
    // route 矢印の色分けに使う route 定義上の Y 値を作成する処理
    //-------------------------------------------------------------------------------
    private static Dictionary<int, float> BuildRouteColorHeights(RouteFile route)
    {
        Dictionary<int, float> heights = new();
        foreach (RouteWaypoint waypoint in route.Waypoints.Values)
        {
            heights[waypoint.Index] = waypoint.Y;
        }

        return heights;
    }

    private void RefreshInspector()
    {
        if (_labelInspectorSelection is null ||
            _groupBoxSpawnInspector is null ||
            _groupBoxWaypointInspector is null ||
            _groupBoxWaterboxInspector is null)
        {
            return;
        }

        _inspectorUpdating = true;
        try
        {
            RouteWaypoint? waypoint = null;
            bool hasSpawn = _selectedSpawnIndex is not null &&
                _selectedSpawnIndex.Value >= 0 &&
                _selectedSpawnIndex.Value < _currentLayout.Spawns.Count;
            bool hasWaypoint = _selectedRouteWaypointIndex is not null &&
                _currentRoute.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out waypoint);
            bool hasWaterbox = _selectedWaterboxIndex is not null &&
                _selectedWaterboxIndex.Value >= 0 &&
                _selectedWaterboxIndex.Value < _currentWaterbox.Boxes.Count;

            if (_numericFieldDay is not null)
            {
                _numericFieldDay.Enabled = GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null;
                SetNumericValue(_numericFieldDay, _currentFieldDay);
            }

            if (_labelFieldActiveFiles is not null)
            {
                _labelFieldActiveFiles.Text = GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null
                    ? BuildActiveFieldGeneratorFileText(_currentFieldMapData.GeneratorFiles, _currentFieldDay)
                    : "-";
            }

            RefreshFieldAddControls();

            bool hasFieldObject = TryGetSelectedFieldObjectRef(out FieldDisplayObjectRef selectedFieldObjectRef);
            if (_textBoxFieldObjectRaw is not null)
            {
                _textBoxFieldObjectRaw.Enabled = hasFieldObject;
                _textBoxFieldObjectRaw.Text = hasFieldObject ? GetSelectedFieldObjectRawText() : string.Empty;
            }

            if (_buttonApplyFieldObjectRaw is not null)
            {
                _buttonApplyFieldObjectRaw.Enabled = hasFieldObject;
            }

            _groupBoxSpawnInspector.Enabled = GetCurrentMode() == EditorMode.Cave;
            _groupBoxWaypointInspector.Enabled = GetCurrentMode() == EditorMode.Cave;
            _groupBoxWaterboxInspector.Enabled = GetCurrentMode() == EditorMode.Cave;

            if (hasSpawn)
            {
                LayoutSpawn spawn = _currentLayout.Spawns[_selectedSpawnIndex!.Value];
                _labelInspectorSelection.Text = GetCurrentMode() == EditorMode.Field && hasFieldObject && _currentFieldMapData is not null
                    ? $"Object #{_selectedSpawnIndex.Value} / {spawn.TypeLabel} / {_currentFieldMapData.GeneratorFiles[selectedFieldObjectRef.FileIndex].DisplayName}"
                    : $"Spawn #{_selectedSpawnIndex.Value} / {spawn.TypeLabel}";
                SelectSpawnType(spawn.TypeId);
                SetNumericValue(_numericSpawnX, spawn.X);
                SetNumericValue(_numericSpawnY, spawn.Y);
                SetNumericValue(_numericSpawnZ, spawn.Z);
                SetNumericValue(_numericSpawnAngle, spawn.Angle);
                SetNumericValue(_numericSpawnRadius, spawn.Radius);
                SetNumericValue(_numericSpawnMinCount, spawn.MinCount);
                SetNumericValue(_numericSpawnMaxCount, spawn.MaxCount);
            }
            else
            {
                _labelInspectorSelection.Text = hasWaypoint
                    ? $"Waypoint #{waypoint!.Index}"
                    : hasWaterbox ? $"Waterbox #{_selectedWaterboxIndex!.Value}" : "未選択";
            }

            if (hasWaypoint)
            {
                _labelWaypointIndex!.Text = waypoint!.Index.ToString(CultureInfo.InvariantCulture);
                SetNumericValue(_numericWaypointX, waypoint.X);
                SetNumericValue(_numericWaypointY, waypoint.Y);
                SetNumericValue(_numericWaypointZ, waypoint.Z);
                SetNumericValue(_numericWaypointRadius, waypoint.Radius);
                _textBoxWaypointLinks!.Text = string.Join(", ", waypoint.Links.OrderBy(link => link));
            }
            else
            {
                _labelWaypointIndex!.Text = "-";
                _textBoxWaypointLinks!.Text = string.Empty;
            }

            if (hasWaterbox)
            {
                WaterboxEntry box = _currentWaterbox.Boxes[_selectedWaterboxIndex!.Value];
                SetNumericValue(_numericWaterboxX1, box.X1);
                SetNumericValue(_numericWaterboxY1, box.Y1);
                SetNumericValue(_numericWaterboxZ1, box.Z1);
                SetNumericValue(_numericWaterboxX2, box.X2);
                SetNumericValue(_numericWaterboxY2, box.Y2);
                SetNumericValue(_numericWaterboxZ2, box.Z2);
            }
            else
            {
                SetNumericValue(_numericWaterboxX1, 0f);
                SetNumericValue(_numericWaterboxY1, 0f);
                SetNumericValue(_numericWaterboxZ1, 0f);
                SetNumericValue(_numericWaterboxX2, 0f);
                SetNumericValue(_numericWaterboxY2, 0f);
                SetNumericValue(_numericWaterboxZ2, 0f);
            }

            _buttonDeleteSpawn!.Enabled = hasSpawn;
            _buttonApplySpawn!.Enabled = hasSpawn;
            _comboBoxSpawnType!.Enabled = hasSpawn;
            _numericSpawnX!.Enabled = hasSpawn;
            _numericSpawnY!.Enabled = hasSpawn;
            _numericSpawnZ!.Enabled = hasSpawn;
            _numericSpawnAngle!.Enabled = hasSpawn;
            _numericSpawnRadius!.Enabled = hasSpawn;
            _numericSpawnMinCount!.Enabled = hasSpawn;
            _numericSpawnMaxCount!.Enabled = hasSpawn;

            _buttonDeleteWaypoint!.Enabled = hasWaypoint;
            _buttonApplyWaypoint!.Enabled = hasWaypoint;
            _numericWaypointX!.Enabled = hasWaypoint;
            _numericWaypointY!.Enabled = hasWaypoint;
            _numericWaypointZ!.Enabled = hasWaypoint;
            _numericWaypointRadius!.Enabled = hasWaypoint;
            _textBoxWaypointLinks!.Enabled = hasWaypoint;

            _buttonApplyWaterbox!.Enabled = hasWaterbox;
            _numericWaterboxX1!.Enabled = hasWaterbox;
            _numericWaterboxY1!.Enabled = hasWaterbox;
            _numericWaterboxZ1!.Enabled = hasWaterbox;
            _numericWaterboxX2!.Enabled = hasWaterbox;
            _numericWaterboxY2!.Enabled = hasWaterbox;
            _numericWaterboxZ2!.Enabled = hasWaterbox;
        }
        finally
        {
            _inspectorUpdating = false;
        }

        UpdateQuickToolWindowState();
    }

    private void SelectSpawnType(int typeId)
    {
        if (_comboBoxSpawnType is null)
        {
            return;
        }

        for (int i = 0; i < _comboBoxSpawnType.Items.Count; i++)
        {
            if (_comboBoxSpawnType.Items[i] is SpawnTypeItem item && item.TypeId == typeId)
            {
                _comboBoxSpawnType.SelectedIndex = i;
                return;
            }
        }

        _comboBoxSpawnType.SelectedIndex = 0;
    }

    private static void SetNumericValue(NumericUpDown? control, float value)
    {
        if (control is null)
        {
            return;
        }

        decimal clamped = Math.Clamp((decimal)value, control.Minimum, control.Maximum);
        control.Value = clamped;
    }

    private static void SetNumericValue(NumericUpDown? control, int value)
    {
        if (control is null)
        {
            return;
        }

        decimal clamped = Math.Clamp(value, (int)control.Minimum, (int)control.Maximum);
        control.Value = clamped;
    }

    //-------------------------------------------------------------------------------
    // Hocotate Toolkit の参照先を選択する処理
    //-------------------------------------------------------------------------------
    private void buttonBrowseToolkit_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Title = "Hocotate_Toolkit.exe を選択",
            Filter = "Hocotate_Toolkit.exe|Hocotate_Toolkit.exe|Executable (*.exe)|*.exe"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        textBoxToolkitPath.Text = dialog.FileName;
        _settings.ToolkitPath = dialog.FileName;
        SaveSettings();
        ApplyToolkitState(showWarning: false);
    }

    //-------------------------------------------------------------------------------
    // ディスク参照先を選択して必要データを再探索する処理
    //-------------------------------------------------------------------------------
    private async void buttonBrowseDisc_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Pikmin 2 のディスクデータ，または ISO/GCR を含むフォルダを選択してください．",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        textBoxDiscRoot.Text = dialog.SelectedPath;
        _settings.DiscRoot = dialog.SelectedPath;
        SaveSettings();
        await ResolveDiscPathsAsync(dialog.SelectedPath);
    }

    //-------------------------------------------------------------------------------
    // 現在モードの主参照フォルダを選択する処理
    //-------------------------------------------------------------------------------
    private void buttonBrowsePrimaryReference_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = GetCurrentMode() == EditorMode.Cave
                ? "洞窟 arc フォルダを選択してください．"
                : "地上 map フォルダを選択してください．",
            UseDescriptionForTitle = true
        };

        if (Directory.Exists(textBoxArcPath.Text))
        {
            dialog.SelectedPath = textBoxArcPath.Text;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (GetCurrentMode() == EditorMode.Cave)
        {
            _lastCaveArcPath = dialog.SelectedPath;
        }
        else
        {
            _lastFieldMapRoot = dialog.SelectedPath;
        }

        ApplyModeReferencePaths();
        LoadPreviewSources();
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // 現在モードの副参照フォルダを選択する処理
    //-------------------------------------------------------------------------------
    private void buttonBrowseSecondaryReference_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = GetCurrentMode() == EditorMode.Cave
                ? "洞窟 units フォルダを選択してください．"
                : "地上 Kando/map フォルダを選択してください．",
            UseDescriptionForTitle = true
        };

        if (Directory.Exists(textBoxUnitsPath.Text))
        {
            dialog.SelectedPath = textBoxUnitsPath.Text;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (GetCurrentMode() == EditorMode.Cave)
        {
            _lastCaveUnitsPath = dialog.SelectedPath;
        }
        else
        {
            _lastFieldTextsRoot = dialog.SelectedPath;
        }

        ApplyModeReferencePaths();
        LoadPreviewSources();
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // 読み込み対象がドラッグされたときに受付可否を表示する処理
    //-------------------------------------------------------------------------------
    private void LoadTarget_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = TryGetDroppedPath(e, out _) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    //-------------------------------------------------------------------------------
    // ドロップされた読み込み対象を現在の参照先として読み込む処理
    //-------------------------------------------------------------------------------
    private async void LoadTarget_DragDrop(object? sender, DragEventArgs e)
    {
        if (!TryGetDroppedPath(e, out string? droppedPath))
        {
            return;
        }

        if (IsHomeScreenVisible())
        {
            await ApplyHomeDiscRootAsync(droppedPath);
            return;
        }

        textBoxDiscRoot.Text = droppedPath;
        _settings.DiscRoot = droppedPath;
        SaveSettings();
        await ResolveDiscPathsAsync(droppedPath);
    }

    //-------------------------------------------------------------------------------
    // ホーム画面が前面表示中か判定する処理
    //-------------------------------------------------------------------------------
    private bool IsHomeScreenVisible()
    {
        return _homeOverlayPanel is { Visible: true };
    }

    //-------------------------------------------------------------------------------
    // プレビュー領域のサイズ変更時に簡易操作ミニウィンドウを表示範囲内へ戻す処理
    //-------------------------------------------------------------------------------
    private void panelPreview_Resize(object? sender, EventArgs e)
    {
        ClampQuickToolWindowToPreview();
        ClampFieldConsoleWindowToPreview();
    }

    //-------------------------------------------------------------------------------
    // ドラッグデータから最初の有効なファイルまたはフォルダを取得する処理
    //-------------------------------------------------------------------------------
    private static bool TryGetDroppedPath(DragEventArgs e, out string path)
    {
        path = string.Empty;
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
        {
            return false;
        }

        path = paths.FirstOrDefault(candidate => Directory.Exists(candidate) || File.Exists(candidate)) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(path);
    }

    //-------------------------------------------------------------------------------
    // モード切替時にテンプレートとプレビューを更新する処理
    //-------------------------------------------------------------------------------
    private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        _currentEditMode = UnitMapEditMode.Navigate;
        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        _settings.LastMode = GetCurrentMode().ToString();
        SaveSettings();
        ConfigureEditorModeUi();
        LoadLocationProfiles();
        LoadPreviewSources();
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // ルート表示切替時にプレビューを更新する処理
    //-------------------------------------------------------------------------------
    private void checkBoxRouteOverlay_CheckedChanged(object sender, EventArgs e)
    {
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // スポーン表示切替時にプレビューを更新する処理
    //-------------------------------------------------------------------------------
    private void checkBoxSpawnOverlay_CheckedChanged(object? sender, EventArgs e)
    {
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // OBJ 直接表示の切替時にプレビューを更新する処理
    //-------------------------------------------------------------------------------
    private void checkBoxObjDirectView_CheckedChanged(object? sender, EventArgs e)
    {
        SaveSettings();
        ConfigureEditorModeUi();
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // floor 選択時に何もしない互換用処理
    //-------------------------------------------------------------------------------
    private void comboBoxFloor_SelectedIndexChanged(object? sender, EventArgs e)
    {
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // caveinfo ファイル切替時に floor 一覧を再読込する処理
    //-------------------------------------------------------------------------------
    private void comboBoxCaveInfoFile_SelectedIndexChanged(object? sender, EventArgs e)
    {
        RefreshPreview();
    }

    //-------------------------------------------------------------------------------
    // object template 一覧を手動で再読込する処理
    //-------------------------------------------------------------------------------
    private async void buttonReloadTemplates_Click(object sender, EventArgs e)
    {
        await LoadEditorSourcesWithBusyAsync("Reloading templates...");
    }

    //-------------------------------------------------------------------------------
    // 一覧選択時に洞窟ユニットのレイアウト・ルートを読込する処理
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    // 洞窟ユニットのキャッシュ生成を実行する処理
    //-------------------------------------------------------------------------------
    private async void buttonPrepareCache_Click(object sender, EventArgs e)
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            AppendLog("キャッシュ生成は洞窟ユニットモードでのみ実行できます．");
            return;
        }

        if (!Directory.Exists(textBoxArcPath.Text))
        {
            AppendLog("arc フォルダが見つからないため，キャッシュ生成を中断しました．");
            return;
        }

        string? selectedUnitName = GetSelectedTemplateName();
        if (selectedUnitName is null)
        {
            AppendLog("ユニットが未選択のため，キャッシュ生成を中断しました．");
            return;
        }

        try
        {
            SetBusyState(true, "キャッシュ生成中です");
            UnitCacheEntry cacheEntry = await EnsureSelectedUnitCacheAsync(selectedUnitName);
            AppendLog($"キャッシュ生成完了: unit={cacheEntry.UnitName}, obj={(cacheEntry.ObjPath is null ? "-" : "OK")}, layout={(cacheEntry.LayoutPath is null ? "-" : "OK")}, route={(cacheEntry.RoutePath is null ? "-" : "OK")}");
            RefreshTemplateCardImage(cacheEntry.UnitName, cacheEntry.PreviewImagePath);
            await LoadSelectedPreviewAssetsAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"キャッシュ生成失敗: {ex.Message}");
        }
        finally
        {
            SetBusyState(false, "待機中です");
            RefreshPreview();
        }
    }

    //-------------------------------------------------------------------------------
    // 全ユニットのキャッシュ生成をバックグラウンドで実行する処理
    //-------------------------------------------------------------------------------
    private async void buttonPrepareAllUnitCache_Click(object? sender, EventArgs e)
    {
        if (GetCurrentMode() != EditorMode.Cave || _currentLoadFormat != LoadFormatKind.DiscExtractData)
        {
            AppendLog("全ユニットキャッシュはディスク抽出データ参照時のみ実行できます．");
            return;
        }

        if (!Directory.Exists(textBoxArcPath.Text))
        {
            AppendLog("arc フォルダが見つからないため，全ユニットキャッシュを中断しました．");
            return;
        }

        List<string> unitNames = GetVisibleUnitNamesForCache();
        if (unitNames.Count == 0)
        {
            AppendLog("キャッシュ対象のユニットが見つかりません．");
            return;
        }

        DialogResult result = MessageBox.Show(
            this,
            $"全 {unitNames.Count} ユニットのキャッシュを作成します．\n処理には非常に時間がかかる場合があります．実行しますか？",
            "全ユニットキャッシュ作成",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bulkCacheRunning = true;
            panelMapUnitHost.Enabled = false;
            panelTemplateCardsScroll.Enabled = false;
            flowLayoutPanelTemplateCards.Enabled = false;
            SetBusyState(true, "全ユニットキャッシュ生成中です");
            UnitAssetCacheService cacheService = new(textBoxArcPath.Text, new Dictionary<string, string>(), textBoxToolkitPath.Text, GetCurrentCacheRoot(), CreatePrettyImageProvider());
            progressBarCache.Minimum = 0;
            progressBarCache.Maximum = Math.Max(unitNames.Count, 1);
            progressBarCache.Value = 0;

            await Task.Run(() =>
            {
                for (int index = 0; index < unitNames.Count; index++)
                {
                    string unitName = unitNames[index];
                    BeginInvoke(new Action(() =>
                    {
                        labelCacheProgress.Text = $"全ユニットキャッシュ生成中: {unitName} ({index + 1}/{unitNames.Count})";
                        progressBarCache.Value = Math.Clamp(index, 0, progressBarCache.Maximum);
                    }));

                    UnitCacheEntry cacheEntry = cacheService.EnsureUnitCache(unitName, replacePreviewWithPrettyImage: true);
                    BeginInvoke(new Action(() =>
                    {
                        RefreshTemplateCardImage(cacheEntry.UnitName, cacheEntry.PreviewImagePath);
                        progressBarCache.Value = Math.Clamp(index + 1, 0, progressBarCache.Maximum);
                    }));
                }
            });

            AppendLog($"全ユニットキャッシュ生成完了: {unitNames.Count} 件");
        }
        catch (Exception ex)
        {
            AppendLog($"全ユニットキャッシュ生成失敗: {ex.Message}");
        }
        finally
        {
            _bulkCacheRunning = false;
            panelMapUnitHost.Enabled = true;
            panelTemplateCardsScroll.Enabled = true;
            flowLayoutPanelTemplateCards.Enabled = true;
            SetBusyState(false, "待機中です");
            RefreshReferenceUnitInfo();
            RefreshPreview();
        }
    }

    //-------------------------------------------------------------------------------
    // キャッシュ対象にする現在表示中ユニット名の一覧を取得する処理
    //-------------------------------------------------------------------------------
    private List<string> GetVisibleUnitNamesForCache()
    {
        List<string> names = _templateCardPanels.Keys
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (names.Count > 0)
        {
            return names;
        }

        return Directory.Exists(textBoxArcPath.Text)
            ? Directory.GetDirectories(textBoxArcPath.Text)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : new List<string>();
    }

    //-------------------------------------------------------------------------------
    // Toolkit の有効状態を UI に反映する処理
    //-------------------------------------------------------------------------------
    private void ApplyToolkitState(bool showWarning)
    {
        bool ready = File.Exists(textBoxToolkitPath.Text);
        labelToolkitStatus.Text = ready ? "Hocotate_Toolkit: OK" : "Hocotate_Toolkit: 未設定";
        labelToolkitStatus.ForeColor = ready ? Color.DarkGreen : Color.DarkRed;
        buttonBrowseDisc.Enabled = ready;
        buttonPrepareCache.Enabled = ready && GetCurrentMode() == EditorMode.Cave;
        if (_buttonPrepareAllUnitCache is not null)
        {
            _buttonPrepareAllUnitCache.Enabled = ready && GetCurrentMode() == EditorMode.Cave && _currentLoadFormat == LoadFormatKind.DiscExtractData;
        }

        if (!ready && showWarning)
        {
            MessageBox.Show(
                this,
                "Hocotate_Toolkit.exe の参照先が未設定か無効です．\n先にツールの参照先を設定してください．",
                "Toolkit Missing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    //-------------------------------------------------------------------------------
    // Location フォルダからモード対応テンプレート一覧を読み込む処理
    //-------------------------------------------------------------------------------
    private void LoadLocationProfiles()
    {
        string? locationRoot = _repository.TryFindDefaultLocationRoot();
        if (string.IsNullOrWhiteSpace(locationRoot) || !Directory.Exists(locationRoot))
        {
            labelTemplateRoot.Text = "Location: 未検出";
            return;
        }

        EditorMode mode = GetCurrentMode();
        string targetProfileName = mode == EditorMode.Field ? "01_Field" : "00_Cave";
        string profileRoot = Path.Combine(locationRoot, targetProfileName);
        labelTemplateRoot.Text = $"Location: {profileRoot}";

        string templateDir = Path.Combine(profileRoot, "object_templates");
        if (!Directory.Exists(templateDir))
        {
            return;
        }

        int templateIndex = 0;
        foreach (string file in Directory.GetFiles(templateDir, "*.txt").OrderBy(Path.GetFileName))
        {
            AddTemplateListItem(Path.GetFileName(file) ?? file, null);
            templateIndex++;
            if (templateIndex % 8 == 0)
            {
                PumpLoadingOverlayAnimation();
            }
        }

    }

    //-------------------------------------------------------------------------------
    // ディスク参照先から arc と units と caveinfo を探索する処理
    //-------------------------------------------------------------------------------
    private async Task ResolveDiscPathsAsync(string selectedPath)
    {
        try
        {
            SetBusyState(true, "ディスク参照を確認しています");

            if (await TryLoadDirectObjAsync(selectedPath))
            {
                return;
            }

            if (await TryLoadDirectUnitArchiveDirectoryAsync(selectedPath))
            {
                return;
            }

            string selectedRoot = GetLoadRootDirectory(selectedPath);
            bool hasDiscExtractFolders = Directory.Exists(Path.Combine(selectedRoot, "sys")) &&
                Directory.Exists(Path.Combine(selectedRoot, "files"));
            bool isArcFilesFolder = IsArcFilesFolder(selectedRoot);
            bool hasFieldMapFolders = FieldAssetLocator.ResolveFieldMapRoot(selectedRoot) is not null;
            if (!hasDiscExtractFolders && !isArcFilesFolder && !hasFieldMapFolders)
            {
                AppendLog("sys/files が見つからないため，ディスク展開を試行します．");
                HocotateToolkitService toolkit = new(textBoxToolkitPath.Text);
                string message = await Task.Run(() =>
                {
                    return toolkit.TryExtractDiscImage(selectedPath, out string extractMessage)
                        ? extractMessage
                        : extractMessage;
                });
                AppendLog(message);
                hasDiscExtractFolders = Directory.Exists(Path.Combine(selectedRoot, "sys")) &&
                    Directory.Exists(Path.Combine(selectedRoot, "files"));
            }

            string searchRoot = isArcFilesFolder ? selectedRoot : GetDiscSearchRoot(selectedRoot);
            _currentDiscSearchRoot = searchRoot;
            AppendLog($"探索ルート: {searchRoot}");

            SetBusyState(true, "ディスク内の参照先を探索しています");
            string? arcPath = await Task.Run(() => ResolveCaveArcRootFromSearchRoot(searchRoot));
            string? unitsDir = await Task.Run(() => ResolveCaveUnitsRootFromSearchRoot(searchRoot));
            string? fieldMapRoot = await Task.Run(() => FieldAssetLocator.ResolveFieldMapRoot(searchRoot));
            string? fieldTextsRoot = await Task.Run(() => FieldAssetLocator.ResolveFieldTextsRoot(searchRoot));
            _currentCaveInfoDirectory = await Task.Run(() => RecursiveFinder.FindDirectoryContainingFile(searchRoot, "caveinfo.txt"));
            _lastCaveArcPath = arcPath;
            _lastCaveUnitsPath = unitsDir;
            _lastFieldMapRoot = fieldMapRoot;
            _lastFieldTextsRoot = fieldTextsRoot;
            if (isArcFilesFolder && !string.IsNullOrWhiteSpace(arcPath) && string.IsNullOrWhiteSpace(fieldMapRoot))
            {
                comboBoxMode.SelectedIndex = 1;
            }

            if (GetCurrentMode() == EditorMode.Field && !string.IsNullOrWhiteSpace(fieldMapRoot))
            {
                SetPathStatus(textBoxArcPath, fieldMapRoot);
                SetPathStatus(textBoxUnitsPath, fieldTextsRoot);
                textBoxUnitSet.Text = "init/default/plants/loop/nonloop";
            }
            else
            {
                SetPathStatus(textBoxArcPath, arcPath);
                SetPathStatus(textBoxUnitsPath, unitsDir);
                textBoxUnitSet.Text = "arc 直参照";
            }

            LoadFormatKind resolvedFormat = hasDiscExtractFolders
                ? LoadFormatKind.DiscExtractData
                : hasFieldMapFolders ? LoadFormatKind.DiscExtractData
                : isArcFilesFolder ? LoadFormatKind.ArcFilesFolder : LoadFormatKind.None;
            SetLoadFormat(resolvedFormat, resolvedFormat == LoadFormatKind.ArcFilesFolder ? selectedRoot : null);
            RefreshReferenceUnitInfo();

            AppendLog($"探索結果: arc={arcPath ?? "-"}, units={unitsDir ?? "-"}, fieldMap={fieldMapRoot ?? "-"}, fieldTexts={fieldTextsRoot ?? "-"}");

            _currentSummaryLines = GetCurrentMode() == EditorMode.Field
                ? new List<string> { "地上マップ一覧を表示します．" }
                : new List<string> { "arc 配下のユニット一覧を直接表示します．" };
            RefreshConsoleOutput();
            LoadPreviewSources();
            RefreshPreview();
        }
        catch (Exception ex)
        {
            AppendLog($"ディスク参照失敗: {ex.Message}");
        }
        finally
        {
            SetBusyState(false, "待機中です");
        }
    }

    //-------------------------------------------------------------------------------
    // OBJ ファイルを単体のプレビュー対象として読み込む処理
    //-------------------------------------------------------------------------------
    private async Task<bool> TryLoadDirectObjAsync(string selectedPath)
    {
        if (!File.Exists(selectedPath) ||
            !string.Equals(Path.GetExtension(selectedPath), ".obj", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        SetBusyState(true, "OBJ を直接読み込んでいます");
        ObjScene? scene = await Task.Run(() => LoadObjSceneFromPath(selectedPath));
        string unitName = Path.GetFileNameWithoutExtension(selectedPath);
        string? mtlPath = FindObjMtlPath(selectedPath);

        comboBoxMode.SelectedIndex = 1;
        checkBoxObjDirectView.Checked = true;
        SetLoadFormat(LoadFormatKind.DirectFiles, Path.GetDirectoryName(selectedPath));
        _currentPreviewUnitName = unitName;
        _currentPreviewImagePath = null;
        _currentPreviewImageIsPretty = false;
        _currentObjPath = selectedPath;
        _currentObjMtlPath = mtlPath;
        _currentObjScene = scene;
        _currentLayoutPath = null;
        _currentRoutePath = null;
        _currentModelBounds = LoadModelBounds(scene);
        _currentLayout = new LayoutFile(Array.Empty<LayoutSpawn>());
        _currentRoute = new RouteFile(new Dictionary<int, RouteWaypoint>());
        _selectedSpawnIndex = null;
        _selectedRouteWaypointIndex = null;
        _previewSceneResetRequired = true;
        ClearEditorHistory();
        _currentCaveInfoDirectory = null;
        _currentDiscSearchRoot = Path.GetDirectoryName(selectedPath);

        SetPathStatus(textBoxArcPath, Path.GetDirectoryName(selectedPath));
        SetPathStatus(textBoxUnitsPath, null);
        textBoxUnitSet.Text = "OBJ 直接参照";
        labelTemplateRoot.Text = $"OBJ: {selectedPath}";
        _currentSummaryLines = new List<string> { "OBJ ファイルのみを直接表示しています．" };
        RefreshConsoleOutput();
        RefreshUnitSummary();
        RefreshReferenceUnitInfo();
        RefreshPreview();
        AppendLog($"OBJ 直接読込: obj={selectedPath}, mtl={(mtlPath is null ? "-" : mtlPath)}");
        return true;
    }

    //-------------------------------------------------------------------------------
    // arc.szs と texts.szs を直下に持つユニットフォルダを読み込む処理
    //-------------------------------------------------------------------------------
    private async Task<bool> TryLoadDirectUnitArchiveDirectoryAsync(string selectedPath)
    {
        if (!IsDirectUnitArchiveDirectory(selectedPath))
        {
            return false;
        }

        string normalizedPath = selectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string unitName = Path.GetFileName(normalizedPath);
        string arcRoot = Path.GetDirectoryName(normalizedPath) ?? selectedPath;
        AppendLog($"直下 arc.szs/texts.szs を検出: unit={unitName}");

        comboBoxMode.SelectedIndex = 1;
        SetLoadFormat(LoadFormatKind.DirectFiles, normalizedPath);
        _currentDiscSearchRoot = arcRoot;
        _lastCaveArcPath = arcRoot;
        _lastCaveUnitsPath = null;
        _lastFieldMapRoot = null;
        _lastFieldTextsRoot = null;
        SetPathStatus(textBoxArcPath, arcRoot);
        SetPathStatus(textBoxUnitsPath, selectedPath);
        textBoxUnitSet.Text = "arc.szs/texts.szs 直参照";
        _currentCaveInfoDirectory = null;
        _currentSummaryLines = new List<string> { $"{unitName} の arc.szs/texts.szs を直接展開して表示します．" };
        RefreshConsoleOutput();
        RefreshReferenceUnitInfo();
        LoadPreviewSources();
        SelectTemplateCard(unitName, loadPreview: false);
        await LoadSelectedPreviewAssetsAsync();
        RefreshPreview();
        return true;
    }

    //-------------------------------------------------------------------------------
    // 展開後に優先して探索するディスクルートを決める処理
    //-------------------------------------------------------------------------------
    private static string GetDiscSearchRoot(string selectedPath)
    {
        string filesRoot = Path.Combine(selectedPath, "files");
        return Directory.Exists(filesRoot) ? filesRoot : selectedPath;
    }

    //-------------------------------------------------------------------------------
    // ファイル指定時に検索と展開先の基準フォルダを取得する処理
    //-------------------------------------------------------------------------------
    private static string GetLoadRootDirectory(string selectedPath)
    {
        if (Directory.Exists(selectedPath))
        {
            return selectedPath;
        }

        return Path.GetDirectoryName(selectedPath) ?? selectedPath;
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダ直下に arc.szs と texts.szs が揃っているか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsDirectUnitArchiveDirectory(string selectedPath)
    {
        return Directory.Exists(selectedPath) &&
            File.Exists(Path.Combine(selectedPath, "arc.szs")) &&
            File.Exists(Path.Combine(selectedPath, "texts.szs"));
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダが arc/files 形式または洞窟 arc 直下形式か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsArcFilesFolder(string selectedPath)
    {
        if (!Directory.Exists(selectedPath))
        {
            return false;
        }

        if (IsCaveArcRoot(selectedPath))
        {
            return true;
        }

        return Directory.GetDirectories(selectedPath)
            .Any(path => string.Equals(Path.GetFileName(path), "arc", StringComparison.OrdinalIgnoreCase)) &&
            Directory.GetDirectories(selectedPath)
            .Any(path => string.Equals(Path.GetFileName(path), "files", StringComparison.OrdinalIgnoreCase));
    }

    //-------------------------------------------------------------------------------
    // OBJ と同階層にある対応 MTL ファイルを推定する処理
    //-------------------------------------------------------------------------------
    private static string? FindObjMtlPath(string objPath)
    {
        string sameNameMtl = Path.ChangeExtension(objPath, ".mtl");
        if (File.Exists(sameNameMtl))
        {
            return sameNameMtl;
        }

        string viewObjMtl = objPath + ".mtl";
        if (File.Exists(viewObjMtl))
        {
            return viewObjMtl;
        }

        string? directory = Path.GetDirectoryName(objPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return null;
        }

        return Directory.GetFiles(directory, "*.mtl", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName)
            .FirstOrDefault();
    }

    //-------------------------------------------------------------------------------
    // モードに応じて不要な入力欄を表示切替する処理
    //-------------------------------------------------------------------------------
    private void ConfigureEditorModeUi()
    {
        ApplyModeReferencePaths();
        bool isCave = GetCurrentMode() == EditorMode.Cave;
        groupBoxDisc.Text = isCave ? Localize("CaveReference") : Localize("FieldReference");
        labelArcPath.Text = isCave ? "arc" : "map";
        labelUnitsPath.Text = isCave ? "units" : "texts";
        labelUnitSet.Text = isCave ? "UnitSet" : "gen";
        labelUnitsPath.Visible = isCave;
        textBoxUnitsPath.Visible = isCave;
        labelUnitSet.Visible = isCave;
        textBoxUnitSet.Visible = isCave;
        if (!isCave)
        {
            labelUnitsPath.Visible = true;
            textBoxUnitsPath.Visible = true;
            labelUnitSet.Visible = true;
            textBoxUnitSet.Visible = true;
        }

        checkBoxObjDirectView.Visible = true;
        checkBoxObjDirectView.Text = isCave ? Localize("Obj3D") : Localize("Field3D");
        if (_buttonBrowsePrimaryReference is not null)
        {
            _buttonBrowsePrimaryReference.Text = Localize("Browse");
        }

        if (_buttonBrowseSecondaryReference is not null)
        {
            _buttonBrowseSecondaryReference.Text = Localize("Browse");
        }

        groupBoxFloorSummary.Text = Localize("Console");
        flowLayoutPanelRouteEdit.Visible = false;
        UpdateRouteEditUi();
        RefreshInspector();
    }

    //-------------------------------------------------------------------------------
    // 現在の大元モードに合わせて参照パス欄を切り替える処理
    //-------------------------------------------------------------------------------
    private void ApplyModeReferencePaths()
    {
        if (GetCurrentMode() == EditorMode.Cave)
        {
            string? caveArcRoot = GetCurrentCaveArcRoot();
            string? caveUnitsRoot = GetCurrentCaveUnitsRoot();
            _lastCaveArcPath = caveArcRoot;
            _lastCaveUnitsPath = caveUnitsRoot;
            SetPathStatus(textBoxArcPath, caveArcRoot);
            SetPathStatus(textBoxUnitsPath, caveUnitsRoot);
            textBoxUnitSet.Text = "arc 直参照";
            return;
        }

        string? fieldMapRoot = GetCurrentFieldMapRoot();
        string? fieldTextsRoot = GetCurrentFieldTextsRoot();
        SetPathStatus(textBoxArcPath, fieldMapRoot);
        SetPathStatus(textBoxUnitsPath, fieldTextsRoot);
        textBoxUnitSet.Text = "init/default/plants/loop/nonloop";
    }

    //-------------------------------------------------------------------------------
    // パス存在状態をテキストボックスへ反映する処理
    //-------------------------------------------------------------------------------
    private static void SetPathStatus(TextBox box, string? path)
    {
        bool exists = !string.IsNullOrWhiteSpace(path) && (Directory.Exists(path) || File.Exists(path));
        box.Text = exists ? path! : "-Not Found-";
        box.BackColor = exists ? Color.Honeydew : Color.MistyRose;
    }

    //-------------------------------------------------------------------------------
    // 現在の読込形式を UI と内部状態へ反映する処理
    //-------------------------------------------------------------------------------
    private void SetLoadFormat(LoadFormatKind formatKind, string? cacheRootOverride)
    {
        _currentLoadFormat = formatKind;
        _currentCacheRootOverride = cacheRootOverride;
        textBoxLoadFormat.Text = GetLoadFormatLabel(formatKind);
        textBoxLoadFormat.BackColor = formatKind == LoadFormatKind.None ? Color.MistyRose : Color.Honeydew;
        UpdateMapUnitPaneAvailability();
        if (_buttonPrepareAllUnitCache is not null)
        {
            _buttonPrepareAllUnitCache.Enabled = File.Exists(textBoxToolkitPath.Text) && GetCurrentMode() == EditorMode.Cave && formatKind == LoadFormatKind.DiscExtractData;
        }
    }

    //-------------------------------------------------------------------------------
    // 読込形式の表示名を取得する処理
    //-------------------------------------------------------------------------------
    private static string GetLoadFormatLabel(LoadFormatKind formatKind)
    {
        return formatKind switch
        {
            LoadFormatKind.DiscExtractData => "ディスク抽出データ",
            LoadFormatKind.ArcFilesFolder => "arc / Files フォルダ",
            LoadFormatKind.DirectFiles => "個別ファイル / 単体ユニット",
            _ => "未読込"
        };
    }

    //-------------------------------------------------------------------------------
    // 読込形式に応じてマップユニット表示エリアを切り替える処理
    //-------------------------------------------------------------------------------
    private void UpdateMapUnitPaneAvailability()
    {
        bool enabled = _currentLoadFormat == LoadFormatKind.DiscExtractData;
        panelMapUnitShell.Visible = enabled;
        panelMapUnitHost.Visible = enabled && !_mapUnitPaneCollapsed;
    }

    //-------------------------------------------------------------------------------
    // 現在の編集モードを取得する処理
    //-------------------------------------------------------------------------------
    private EditorMode GetCurrentMode()
    {
        return comboBoxMode.SelectedIndex == 1 ? EditorMode.Cave : EditorMode.Field;
    }

    //-------------------------------------------------------------------------------
    // 洞窟モードで OBJ 直接表示を使うか判定する処理
    //-------------------------------------------------------------------------------
    private bool IsObjDirectViewEnabled()
    {
        return GetCurrentMode() == EditorMode.Cave && checkBoxObjDirectView.Checked;
    }

    //-------------------------------------------------------------------------------
    // floor 情報の表示用テキストを組み立てる処理
    //-------------------------------------------------------------------------------
    private static string FormatFloorInfo(FloorInfo floor)
    {
        string[] order =
        {
            "f000", "f001", "f002", "f003", "f004", "f014", "f005", "f006",
            "f007", "f008", "f009", "f00A", "f010", "f011", "f012", "f013",
            "f015", "f016", "f017"
        };

        Dictionary<string, string> labels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["f000"] = "開始階層",
            ["f001"] = "終了階層",
            ["f002"] = "敵最大数",
            ["f003"] = "お宝最大数",
            ["f004"] = "ゲート最大数",
            ["f014"] = "Cap最大数",
            ["f005"] = "ルーム数",
            ["f006"] = "通路率",
            ["f007"] = "帰還噴水",
            ["f008"] = "使用UnitSet",
            ["f009"] = "ライト",
            ["f00A"] = "VRBOX",
            ["f010"] = "穴隠し岩",
            ["f011"] = "f011",
            ["f012"] = "f012",
            ["f013"] = "隠し床",
            ["f015"] = "Version",
            ["f016"] = "BlackManTimer",
            ["f017"] = "f017"
        };

        return string.Join(
            Environment.NewLine,
            order.Where(key => floor.Properties.ContainsKey(key))
                .Select(key => $"{labels[key]}: {floor.Properties[key]}"));
    }

    //-------------------------------------------------------------------------------
    // プレビュー画像を現在の状態から描き直す処理
    //-------------------------------------------------------------------------------
    private void RefreshPreview()
    {
        if (GetCurrentMode() == EditorMode.Cave)
        {
            pictureBoxPreview.Visible = false;
            bool directViewEnabled = IsObjDirectViewEnabled();
            bool wasDirectViewVisible = _objModelView.Visible;
            bool wasMapViewVisible = _unitMapView.Visible;
            _objModelView.Visible = directViewEnabled;
            _unitMapView.Visible = !directViewEnabled;

            if (directViewEnabled)
            {
                bool resetView = _previewSceneResetRequired || !wasDirectViewVisible;
                UpdateObjDirectView(resetView);
                _objModelView.BringToFront();
            }
            else
            {
                bool resetView = _previewSceneResetRequired || !wasMapViewVisible;
                UpdateMapOverlays(resetView);
                _unitMapView.BringToFront();
            }

            _previewSceneResetRequired = false;
            UpdateQuickToolWindowState();
            RefreshInspector();
            return;
        }

        pictureBoxPreview.Visible = false;
        bool directFieldViewEnabled = _currentObjScene is not null && checkBoxObjDirectView.Checked;
        _objModelView.Visible = directFieldViewEnabled;
        _unitMapView.Visible = !directFieldViewEnabled;
        bool resetFieldView = _previewSceneResetRequired;
        if (directFieldViewEnabled)
        {
            UpdateObjDirectView(resetFieldView);
            _objModelView.BringToFront();
        }
        else
        {
            UpdateMapOverlays(resetFieldView);
            _unitMapView.BringToFront();
        }

        _previewSceneResetRequired = false;
        UpdateQuickToolWindowState();
        RefreshInspector();
    }

    //-------------------------------------------------------------------------------
    // 現在のモード情報を簡易プレビュー画像として描画する処理
    //-------------------------------------------------------------------------------
    private Bitmap BuildPreviewBitmap()
    {
        Bitmap bitmap = new(960, 960);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.Clear(Color.FromArgb(248, 246, 239));

        using Pen gridPen = new(Color.FromArgb(224, 219, 205), 1f);
        for (int x = 80; x <= 880; x += 80)
        {
            graphics.DrawLine(gridPen, x, 80, x, 880);
        }

        for (int y = 80; y <= 880; y += 80)
        {
            graphics.DrawLine(gridPen, 80, y, 880, y);
        }

        using SolidBrush titleBrush = new(Color.FromArgb(44, 62, 80));
        using Font titleFont = new("Yu Gothic UI", 20f, FontStyle.Bold);
        using Font bodyFont = new("Yu Gothic UI", 11f, FontStyle.Regular);
        using Font badgeFont = new("Yu Gothic UI", 10f, FontStyle.Bold);
        graphics.DrawString(
            GetCurrentMode() == EditorMode.Field ? "地上マップモード" : "洞窟ユニットモード",
            titleFont,
            titleBrush,
            new PointF(80, 24));

        Rectangle badgeRect = new(700, 24, 180, 36);
        using GraphicsPath badgePath = CreateRoundedRectangle(badgeRect, 16);
        using SolidBrush badgeBrush = new(checkBoxRouteOverlay.Checked ? Color.FromArgb(68, 137, 26) : Color.FromArgb(149, 117, 205));
        graphics.FillPath(badgeBrush, badgePath);
        graphics.DrawString(checkBoxRouteOverlay.Checked ? "Route Overlay: ON" : "Route Overlay: OFF", badgeFont, Brushes.White, new PointF(718, 33));

        string template = GetSelectedTemplateName() ?? "(template 未選択)";
        string bodyText = GetCurrentMode() == EditorMode.Field
            ? $"Template: {template}{Environment.NewLine}Field object template を読み込める状態です．"
            : $"Unit: {template}{Environment.NewLine}arc 配下のユニットを直接参照します．";
        graphics.DrawString(bodyText, bodyFont, titleBrush, new RectangleF(88, 100, 600, 120));

        DrawSamplePins(graphics);

        if (checkBoxRouteOverlay.Checked)
        {
            DrawSampleRoutes(graphics);
        }

        return bitmap;
    }

    //-------------------------------------------------------------------------------
    // 洞窟ユニットの route/layout を使ってプレビュー画像を描画する処理
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------
    // マップピンの簡易表示を描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawSamplePins(Graphics graphics)
    {
        Point[] points =
        {
            new(280, 300),
            new(600, 260),
            new(360, 560),
            new(700, 620)
        };

        Color[] colors =
        {
            Color.FromArgb(230, 74, 25),
            Color.FromArgb(46, 125, 50),
            Color.FromArgb(2, 136, 209),
            Color.FromArgb(251, 192, 45)
        };

        using Font labelFont = new("Yu Gothic UI", 9f, FontStyle.Bold);
        for (int i = 0; i < points.Length; i++)
        {
            Rectangle pinRect = new(points[i].X - 16, points[i].Y - 16, 32, 32);
            using SolidBrush fillBrush = new(colors[i]);
            graphics.FillEllipse(fillBrush, pinRect);
            graphics.DrawEllipse(Pens.White, pinRect);
            graphics.DrawString((i + 1).ToString(), labelFont, Brushes.White, new PointF(points[i].X - 6, points[i].Y - 8));
        }
    }

    //-------------------------------------------------------------------------------
    // route オーバーレイの簡易表示を描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawSampleRoutes(Graphics graphics)
    {
        using Pen routePen = new(Color.FromArgb(211, 47, 47), 4f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        Point[] path =
        {
            new(280, 300),
            new(340, 380),
            new(360, 560),
            new(520, 610),
            new(700, 620)
        };

        graphics.DrawLines(routePen, path);
    }

    //-------------------------------------------------------------------------------
    // 角丸矩形のパスを作成する処理
    //-------------------------------------------------------------------------------
    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        GraphicsPath path = new();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    //-------------------------------------------------------------------------------
    // コンソール欄へログメッセージを追記する処理
    //-------------------------------------------------------------------------------
    private void AppendLog(string message)
    {
        ConsoleColorKind color = DetectConsoleColor(message);
        _consoleEntries.Add(new ConsoleEntry($"[{DateTime.Now:HH:mm:ss}] {message}", color));
        if (_consoleEntries.Count > 200)
        {
            _consoleEntries.RemoveAt(0);
        }

        RefreshConsoleOutput();
    }

    //-------------------------------------------------------------------------------
    // Summary とログを単一コンソール表示へ反映する処理
    //-------------------------------------------------------------------------------
    private void RefreshConsoleOutput()
    {
        richTextBoxConsole.SuspendLayout();
        if (richTextBoxConsole.IsHandleCreated)
        {
            SendMessage(richTextBoxConsole.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
        }

        try
        {
            richTextBoxConsole.Clear();
            richTextBoxConsole.SelectionColor = Color.White;
            richTextBoxConsole.SelectionFont = new Font(richTextBoxConsole.Font, FontStyle.Bold);
            richTextBoxConsole.AppendText("[STATUS]" + Environment.NewLine);
            richTextBoxConsole.SelectionFont = richTextBoxConsole.Font;

            foreach (string line in _currentSummaryLines)
            {
                AppendConsoleLine(line, DetectConsoleColor(line));
            }

            richTextBoxConsole.AppendText(Environment.NewLine);
            richTextBoxConsole.SelectionColor = Color.White;
            richTextBoxConsole.SelectionFont = new Font(richTextBoxConsole.Font, FontStyle.Bold);
            richTextBoxConsole.AppendText("[LOG]" + Environment.NewLine);
            richTextBoxConsole.SelectionFont = richTextBoxConsole.Font;

            foreach (ConsoleEntry entry in _consoleEntries)
            {
                AppendConsoleLine(entry.Text, entry.ColorKind);
            }

            richTextBoxConsole.SelectionStart = richTextBoxConsole.TextLength;
            richTextBoxConsole.ScrollToCaret();
        }
        finally
        {
            if (richTextBoxConsole.IsHandleCreated)
            {
                SendMessage(richTextBoxConsole.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                richTextBoxConsole.Invalidate();
            }

            richTextBoxConsole.ResumeLayout();
        }
    }

    //-------------------------------------------------------------------------------
    // コンソールへ色付き 1 行を追加する処理
    //-------------------------------------------------------------------------------
    private void AppendConsoleLine(string text, ConsoleColorKind colorKind)
    {
        richTextBoxConsole.SelectionColor = colorKind switch
        {
            ConsoleColorKind.Ok => Color.LimeGreen,
            ConsoleColorKind.Error => Color.OrangeRed,
            ConsoleColorKind.Info => Color.DeepSkyBlue,
            _ => Color.White
        };
        richTextBoxConsole.AppendText(text + Environment.NewLine);
    }

    //-------------------------------------------------------------------------------
    // 表示文言からコンソール色を推定する処理
    //-------------------------------------------------------------------------------
    private static ConsoleColorKind DetectConsoleColor(string text)
    {
        if (Regex.IsMatch(text, "OK|完了|保存完了|成功", RegexOptions.IgnoreCase))
        {
            return ConsoleColorKind.Ok;
        }

        if (Regex.IsMatch(text, "失敗|NG|見つからない|中断|未設定|Not Found|error", RegexOptions.IgnoreCase))
        {
            return ConsoleColorKind.Error;
        }

        if (Regex.IsMatch(text, "Unit:|Image:|obj:|layout:|route:|mode:|view:|Selected", RegexOptions.IgnoreCase))
        {
            return ConsoleColorKind.Info;
        }

        return ConsoleColorKind.Normal;
    }

    //-------------------------------------------------------------------------------
    // キャッシュ生成進捗を UI に反映する処理
    //-------------------------------------------------------------------------------
    private void UpdateCacheProgress(CacheProgressInfo progress)
    {
        labelCacheProgress.Text = progress.TotalUnits <= 0
            ? progress.Stage
            : $"{progress.Stage}: {progress.UnitName} ({progress.CompletedUnits}/{progress.TotalUnits})";
        UpdateLoadingOverlay(true, labelCacheProgress.Text);
        progressBarCache.Maximum = Math.Max(progress.TotalUnits, 1);
        progressBarCache.Value = Math.Clamp(progress.CompletedUnits, progressBarCache.Minimum, progressBarCache.Maximum);
        labelCacheProgress.Refresh();
        progressBarCache.Refresh();
        Application.DoEvents();
    }

    //-------------------------------------------------------------------------------
    // 処理中の操作可否をまとめて切り替える処理
    //-------------------------------------------------------------------------------
    private void SetBusyState(bool isBusy, string statusText)
    {
        buttonBrowseToolkit.Enabled = !isBusy;
        buttonBrowseDisc.Enabled = !isBusy && File.Exists(textBoxToolkitPath.Text);
        buttonPrepareCache.Enabled = !isBusy && File.Exists(textBoxToolkitPath.Text) && GetCurrentMode() == EditorMode.Cave;
        if (_buttonPrepareAllUnitCache is not null)
        {
            _buttonPrepareAllUnitCache.Enabled = !isBusy && File.Exists(textBoxToolkitPath.Text) && GetCurrentMode() == EditorMode.Cave && _currentLoadFormat == LoadFormatKind.DiscExtractData;
        }
        buttonReloadTemplates.Enabled = !isBusy;
        comboBoxMode.Enabled = !isBusy;
        checkBoxObjDirectView.Enabled = GetCurrentMode() == EditorMode.Cave || _currentObjScene is not null;
        checkBoxSpawnOverlay.Enabled = !isBusy;
        checkBoxRouteOverlay.Enabled = !isBusy;
        buttonSpawnMoveMode.Enabled = !isBusy && CanEditSpawnLikePoints();
        buttonSaveLayout.Enabled = !isBusy &&
            ((GetCurrentMode() == EditorMode.Cave && !string.IsNullOrWhiteSpace(_currentLayoutPath)) ||
             (GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null));
        buttonRouteMoveMode.Enabled = !isBusy && CanEditRoutePoints();
        buttonSaveRoute.Enabled = !isBusy &&
            ((GetCurrentMode() == EditorMode.Cave && !string.IsNullOrWhiteSpace(_currentRoutePath)) ||
             (GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null));
        labelCacheProgress.Text = statusText;
        if (!isBusy)
        {
            progressBarCache.Value = 0;
        }

        UpdateLoadingOverlay(isBusy, statusText);
        UpdateQuickToolWindowState();
        labelCacheProgress.Refresh();
        progressBarCache.Refresh();
        Application.DoEvents();
    }

    private string? GetSelectedTemplateName()
    {
        return _selectedTemplateName;
    }

    private void AddTemplateListItem(string displayName, string? imagePath)
    {
        Panel cardPanel = new()
        {
            Width = 244,
            Height = 54,
            Margin = new Padding(2, 2, 2, 5),
            Padding = new Padding(4),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(252, 251, 247),
            Tag = displayName,
            Cursor = Cursors.Hand
        };

        PictureBox pictureBox = new()
        {
            Width = 72,
            Height = 44,
            Location = new Point(4, 4),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(236, 233, 224),
            BorderStyle = BorderStyle.FixedSingle,
            Tag = displayName,
            Cursor = Cursors.Hand,
            Image = LoadTemplateCardImage(imagePath)
        };

        Label nameLabel = new()
        {
            AutoEllipsis = true,
            Location = new Point(84, 6),
            Size = new Size(150, 40),
            Font = new Font("Yu Gothic UI", 8.5F, FontStyle.Bold),
            Text = displayName,
            Tag = displayName,
            Cursor = Cursors.Hand
        };

        void bindClick(Control control)
        {
            control.Click += TemplateCard_Click;
        }

        bindClick(cardPanel);
        bindClick(pictureBox);
        bindClick(nameLabel);

        cardPanel.Controls.Add(pictureBox);
        cardPanel.Controls.Add(nameLabel);
        flowLayoutPanelTemplateCards.Controls.Add(cardPanel);
        _templateCardPanels[displayName] = cardPanel;
        _templateCardImages[displayName] = pictureBox;
    }

    private string? TryGetCachedPreviewImagePath(string unitName)
    {
        string? cacheRoot = GetCurrentCacheRoot();
        if (cacheRoot is null)
        {
            return null;
        }

        string imagePath = Path.Combine(cacheRoot, "画像キャッシュ", unitName + ".png");
        return File.Exists(imagePath) ? imagePath : null;
    }

    //-------------------------------------------------------------------------------
    // 地上マップのキャッシュ済みプレビュー画像パスを取得する処理
    //-------------------------------------------------------------------------------
    private string? TryGetCachedFieldPreviewImagePath(string mapName)
    {
        string? cacheRoot = GetCurrentCacheRoot();
        if (cacheRoot is null)
        {
            return null;
        }

        string imagePath = Path.Combine(cacheRoot, "地上画像キャッシュ", mapName + ".png");
        return File.Exists(imagePath) ? imagePath : null;
    }

    private Image LoadTemplateCardImage(string? imagePath)
    {
        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
        {
            try
            {
                using Image source = LoadImageCloneFromFile(imagePath);
                return BuildTemplateCardThumbnail(source);
            }
            catch
            {
                return BuildTemplatePlaceholderImage();
            }
        }

        return BuildTemplatePlaceholderImage();
    }

    //-------------------------------------------------------------------------------
    // ファイルロックを残さず画像を読み込む処理
    //-------------------------------------------------------------------------------
    private static Image LoadImageCloneFromFile(string imagePath)
    {
        using FileStream stream = new(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using Image source = Image.FromStream(stream);
        return new Bitmap(source);
    }

    private void RefreshTemplateCardImage(string itemKey, string? imagePath)
    {
        if (!_templateCardImages.TryGetValue(itemKey, out PictureBox? pictureBox))
        {
            return;
        }

        pictureBox.Image?.Dispose();
        pictureBox.Image = LoadTemplateCardImage(imagePath);
    }

    private void TemplateCard_Click(object? sender, EventArgs e)
    {
        if (_bulkCacheRunning)
        {
            AppendLog("全ユニットキャッシュ生成中はユニット選択を変更できません．");
            return;
        }

        if (sender is not Control control)
        {
            return;
        }

        string? unitName = control.Tag as string;
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return;
        }

        SelectTemplateCard(unitName);
    }

    //-------------------------------------------------------------------------------
    // ユニット検索テキストの変更を一覧表示へ反映する処理
    //-------------------------------------------------------------------------------
    private void textBoxUnitSearch_TextChanged(object? sender, EventArgs e)
    {
        ApplyUnitSearchFilter();
    }

    //-------------------------------------------------------------------------------
    // ユニット一覧カードを検索文字列で絞り込む処理
    //-------------------------------------------------------------------------------
    private void ApplyUnitSearchFilter()
    {
        string filter = _textBoxUnitSearch?.Text.Trim() ?? string.Empty;
        flowLayoutPanelTemplateCards.SuspendLayout();
        foreach ((string key, Panel panel) in _templateCardPanels)
        {
            panel.Visible = string.IsNullOrWhiteSpace(filter) ||
                key.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
        flowLayoutPanelTemplateCards.ResumeLayout();
    }

    private void SelectTemplateCard(string unitName, bool loadPreview = true)
    {
        _selectedTemplateName = unitName;

        foreach ((string key, Panel panel) in _templateCardPanels)
        {
            bool isSelected = string.Equals(key, unitName, StringComparison.OrdinalIgnoreCase);
            panel.BackColor = isSelected ? Color.FromArgb(233, 241, 255) : Color.FromArgb(252, 251, 247);
            panel.Padding = isSelected ? new Padding(5) : new Padding(6);
        }

        if (!loadPreview)
        {
            return;
        }

        _ = LoadSelectedTemplateWithBusyAsync();
    }

    //-------------------------------------------------------------------------------
    // 選択中テンプレートの読み込みをロード表示付きで実行する処理
    //-------------------------------------------------------------------------------
    private async Task LoadSelectedTemplateWithBusyAsync()
    {
        string statusText = GetCurrentMode() == EditorMode.Field
            ? "地上マップを読み込んでいます"
            : "洞窟ユニットを読み込んでいます";

        try
        {
            SetBusyState(true, statusText);
            await Task.Yield();
            await LoadSelectedPreviewAssetsAsync();
            RefreshPreview();
        }
        catch (Exception ex)
        {
            AppendLog($"読込失敗: {ex.Message}");
        }
        finally
        {
            SetBusyState(false, "待機中です");
        }
    }

    private static Bitmap BuildTemplateCardThumbnail(Image source)
    {
        Bitmap bitmap = new(72, 44);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(245, 243, 236));
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle targetBounds = new(3, 3, 66, 38);
        Size fitted = GetAspectFitSize(source.Size, targetBounds.Size);
        Rectangle drawRect = new(
            targetBounds.X + ((targetBounds.Width - fitted.Width) / 2),
            targetBounds.Y + ((targetBounds.Height - fitted.Height) / 2),
            fitted.Width,
            fitted.Height);
        graphics.DrawImage(source, drawRect);
        graphics.DrawRectangle(Pens.SlateGray, targetBounds);
        return bitmap;
    }

    private static Bitmap BuildTemplatePlaceholderImage()
    {
        Bitmap bitmap = new(96, 58);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(238, 235, 228));
        using Pen borderPen = new(Color.FromArgb(120, 122, 126), 1f);
        graphics.DrawRectangle(borderPen, 4, 3, 88, 50);
        using Font font = new("Yu Gothic UI", 8f, FontStyle.Bold);
        using SolidBrush brush = new(Color.FromArgb(96, 96, 96));
        StringFormat format = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString("NO IMAGE", font, brush, new RectangleF(4, 3, 88, 50), format);
        return bitmap;
    }

    private static Size GetAspectFitSize(Size original, Size target)
    {
        if (original.Width <= 0 || original.Height <= 0)
        {
            return target;
        }

        float scale = Math.Min((float)target.Width / original.Width, (float)target.Height / original.Height);
        return new Size(
            Math.Max(1, (int)Math.Round(original.Width * scale)),
            Math.Max(1, (int)Math.Round(original.Height * scale)));
    }

    //-------------------------------------------------------------------------------
    // 現在の設定を json に保存する処理
    //-------------------------------------------------------------------------------
    private void SaveSettings()
    {
        _settings.ToolkitPath = textBoxToolkitPath.Text;
        _settings.DiscRoot = textBoxDiscRoot.Text;
        _settings.LastMode = GetCurrentMode().ToString();
        _settings.UseObjDirectView = false;
        _settingsStore.Save(_settings);
    }

    //-------------------------------------------------------------------------------
    // 現在モードに応じた左一覧を設定する処理
    //-------------------------------------------------------------------------------
    private async Task LoadEditorSourcesWithBusyAsync(string statusText)
    {
        try
        {
            SetBusyState(true, statusText);
            await Task.Yield();
            await Task.Delay(80);
            LoadLocationProfiles();
            PumpLoadingOverlayAnimation();
            LoadPreviewSources();
            RefreshPreview();
        }
        catch (Exception ex)
        {
            AppendLog($"テンプレート読込失敗: {ex.Message}");
        }
        finally
        {
            SetBusyState(false, "待機中です");
        }
    }

    //-------------------------------------------------------------------------------
    // 現在モードに応じた左一覧を設定する処理
    //-------------------------------------------------------------------------------
    private void LoadPreviewSources()
    {
        flowLayoutPanelTemplateCards.SuspendLayout();
        foreach (Control control in flowLayoutPanelTemplateCards.Controls)
        {
            control.Dispose();
        }
        flowLayoutPanelTemplateCards.Controls.Clear();
        flowLayoutPanelTemplateCards.ResumeLayout();
        PumpLoadingOverlayAnimation();
        _templateCardPanels.Clear();
        _templateCardImages.Clear();
        _selectedTemplateName = null;
        _currentPreviewUnitName = null;
        _currentPreviewImagePath = null;
        _currentObjPath = null;
        _currentObjMtlPath = null;
        _currentObjScene = null;
        _currentUnitDefinition = null;
        _currentLayoutPath = null;
        _currentRoutePath = null;
        _currentWaterboxPath = null;
        _currentFieldMapData = null;
        _currentLayout = new LayoutFile(Array.Empty<LayoutSpawn>());
        _currentRoute = new RouteFile(new Dictionary<int, RouteWaypoint>());
        _currentWaterbox = new WaterboxFile(0, Array.Empty<WaterboxEntry>());
        _fieldDisplayObjectRefs = Array.Empty<FieldDisplayObjectRef>();
        _selectedSpawnIndex = null;
        _selectedRouteWaypointIndex = null;
        _selectedWaterboxIndex = null;
        _previewSceneResetRequired = true;

        if (GetCurrentMode() == EditorMode.Cave)
        {
            string? caveArcRoot = GetCurrentCaveArcRoot();
            string? caveUnitsRoot = GetCurrentCaveUnitsRoot();
            SetPathStatus(textBoxArcPath, caveArcRoot);
            SetPathStatus(textBoxUnitsPath, caveUnitsRoot);
            labelTemplateRoot.Text = $"arc: {textBoxArcPath.Text}";
            if (Directory.Exists(textBoxArcPath.Text))
            {
                int unitIndex = 0;
                foreach (string dir in Directory.GetDirectories(textBoxArcPath.Text).OrderBy(Path.GetFileName))
                {
                    string unitName = Path.GetFileName(dir);
                    AddTemplateListItem(unitName, TryGetCachedPreviewImagePath(unitName));
                    unitIndex++;
                    if (unitIndex % 8 == 0)
                    {
                        PumpLoadingOverlayAnimation();
                    }
                }
            }
        }
        else
        {
            string? fieldMapRoot = GetCurrentFieldMapRoot();
            string? fieldTextsRoot = GetCurrentFieldTextsRoot();
            _lastFieldMapRoot = fieldMapRoot;
            _lastFieldTextsRoot = fieldTextsRoot;
            SetPathStatus(textBoxArcPath, fieldMapRoot);
            SetPathStatus(textBoxUnitsPath, fieldTextsRoot);
            textBoxUnitSet.Text = "init/default/plants/loop/nonloop";
            labelTemplateRoot.Text = string.IsNullOrWhiteSpace(fieldMapRoot) ? "map: 未検出" : $"map: {fieldMapRoot}";
            if (!string.IsNullOrWhiteSpace(fieldMapRoot) && Directory.Exists(fieldMapRoot))
            {
                int mapIndex = 0;
                foreach (string dir in Directory.GetDirectories(fieldMapRoot).OrderBy(Path.GetFileName))
                {
                    string mapName = Path.GetFileName(dir);
                    AddTemplateListItem(mapName, TryGetCachedFieldPreviewImagePath(mapName));
                    mapIndex++;
                    if (mapIndex % 8 == 0)
                    {
                        PumpLoadingOverlayAnimation();
                    }
                }
            }
        }

        PumpLoadingOverlayAnimation();
        ApplyUnitSearchFilter();
        RefreshPreview();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 選択中の一覧項目から route/layout を読込する処理
    //-------------------------------------------------------------------------------
    private async Task LoadSelectedPreviewAssetsAsync()
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            await LoadSelectedFieldMapPreviewAssetsAsync();
            return;
        }

        string? unitName = GetSelectedTemplateName();
        _currentPreviewUnitName = unitName;
        _currentPreviewImagePath = null;
        _currentPreviewImageIsPretty = false;
        _currentObjPath = null;
        _currentObjMtlPath = null;
        _currentObjScene = null;
        _currentLayoutPath = null;
        _currentRoutePath = null;
        _currentWaterboxPath = null;
        _currentModelBounds = RectangleF.Empty;
        _currentUnitDefinition = null;
        _currentLayout = new LayoutFile(Array.Empty<LayoutSpawn>());
        _currentRoute = new RouteFile(new Dictionary<int, RouteWaypoint>());
        _currentWaterbox = new WaterboxFile(0, Array.Empty<WaterboxEntry>());
        _selectedSpawnIndex = null;
        _selectedRouteWaypointIndex = null;
        _selectedWaterboxIndex = null;
        _previewSceneResetRequired = true;

        if (string.IsNullOrWhiteSpace(unitName) || !Directory.Exists(textBoxArcPath.Text))
        {
            RefreshReferenceUnitInfo();
            RefreshUnitSummary();
            return;
        }

        UnitCacheEntry cacheEntry = await EnsureSelectedUnitCacheAsync(unitName);
        _currentPreviewImagePath = cacheEntry.PreviewImagePath;
        _currentPreviewImageIsPretty = HasEmbeddedPrettyImage(unitName);
        _currentObjPath = cacheEntry.ObjPath;
        _currentObjMtlPath = cacheEntry.MtlPath;
        _currentObjScene = LoadObjScene(cacheEntry);
        _currentModelBounds = LoadModelBounds(_currentObjScene);
        _currentUnitDefinition = TryLoadUnitDefinition(unitName);
        _currentLayout = LayoutParser.ParseFile(cacheEntry.LayoutPath ?? string.Empty);
        _currentRoute = RouteParser.ParseFile(cacheEntry.RoutePath ?? string.Empty);
        _currentWaterbox = WaterboxParser.ParseFile(cacheEntry.WaterboxPath ?? string.Empty);
        _currentLayoutPath = cacheEntry.LayoutPath;
        _currentRoutePath = cacheEntry.RoutePath;
        _currentWaterboxPath = cacheEntry.WaterboxPath ?? CreateWaterboxPathFromCache(cacheEntry);
        ClearEditorHistory();
        RefreshReferenceUnitInfo();
        RefreshUnitSummary();
        AppendLog($"preview 読込: unit={unitName}, image={(cacheEntry.PreviewImagePath is null ? "-" : "OK")}, obj={(cacheEntry.ObjPath is null ? "-" : "OK")}, layout={Path.GetFileName(cacheEntry.LayoutPath) ?? "-"}, route={Path.GetFileName(cacheEntry.RoutePath) ?? "-"}, waterbox={Path.GetFileName(_currentWaterboxPath) ?? "-"}");
    }

    //-------------------------------------------------------------------------------
    // 選択中の地上マップから route と generator 概要を読み込む処理
    //-------------------------------------------------------------------------------
    private async Task LoadSelectedFieldMapPreviewAssetsAsync()
    {
        string? mapName = GetSelectedTemplateName();
        string? fieldMapRoot = GetCurrentFieldMapRoot();
        string? fieldTextsRoot = GetCurrentFieldTextsRoot();
        _currentPreviewUnitName = mapName;
        _currentPreviewImagePath = null;
        _currentPreviewImageIsPretty = false;
        _currentObjPath = null;
        _currentObjMtlPath = null;
        _currentObjScene = null;
        _currentModelBounds = RectangleF.Empty;
        _currentUnitDefinition = null;
        _currentLayoutPath = null;
        _currentRoutePath = null;
        _currentWaterboxPath = null;
        _currentLayout = new LayoutFile(Array.Empty<LayoutSpawn>());
        _currentRoute = new RouteFile(new Dictionary<int, RouteWaypoint>());
        _currentWaterbox = new WaterboxFile(0, Array.Empty<WaterboxEntry>());
        _currentFieldMapData = null;
        _fieldDisplayObjectRefs = Array.Empty<FieldDisplayObjectRef>();
        _selectedSpawnIndex = null;
        _selectedRouteWaypointIndex = null;
        _selectedWaterboxIndex = null;
        _previewSceneResetRequired = true;

        if (string.IsNullOrWhiteSpace(mapName) ||
            string.IsNullOrWhiteSpace(fieldMapRoot) ||
            !Directory.Exists(Path.Combine(fieldMapRoot, mapName)))
        {
            RefreshUnitSummary();
            return;
        }

        _currentFieldMapData = FieldMapLoader.Load(fieldMapRoot, fieldTextsRoot, mapName);
        _currentRoute = _currentFieldMapData.Route;
        _currentRoutePath = Path.Combine(_currentFieldMapData.MapDirectory, "route.txt");
        RefreshFieldGeneratorDisplayObjects(resetSelection: true);
        _currentLayoutPath = string.Join("; ", _currentFieldMapData.GeneratorFiles.Select(file => file.DisplayName));
        _currentWaterboxPath = _currentFieldMapData.TextsArchiveDirectory is null
            ? null
            : Path.Combine(_currentFieldMapData.TextsArchiveDirectory, "texts.szs");
        _currentModelBounds = GetFieldMapDisplayBounds(_currentLayout, _currentRoute);

        FieldMapCacheEntry? cacheEntry = await TryEnsureFieldMapCacheAsync(mapName, fieldMapRoot, fieldTextsRoot);
        if (cacheEntry is not null)
        {
            _currentPreviewImagePath = cacheEntry.PreviewImagePath;
            _currentObjPath = cacheEntry.ObjPath;
            _currentObjMtlPath = cacheEntry.MtlPath;
            _currentObjScene = !string.IsNullOrWhiteSpace(cacheEntry.ObjPath) && File.Exists(cacheEntry.ObjPath)
                ? LoadObjSceneFromPath(cacheEntry.ObjPath, cacheEntry.MtlPath)
                : null;
            if (_currentObjScene is not null)
            {
                _currentModelBounds = LoadModelBounds(_currentObjScene);
            }

            if (!string.IsNullOrWhiteSpace(cacheEntry.WaterboxPath))
            {
                _currentWaterbox = WaterboxParser.ParseFile(cacheEntry.WaterboxPath);
                _currentWaterboxPath = cacheEntry.WaterboxPath;
            }

            if (_currentPreviewImagePath is null && _currentObjPath is null)
            {
                AppendLog($"地上キャッシュ確認: {cacheEntry.CacheDirectory} に表示用 OBJ/画像が見つかりません．Hocotate_Toolkit 設定または arc.szs 展開結果を確認してください．");
            }
        }

        RefreshTemplateCardImage(mapName, _currentPreviewImagePath);
        ClearEditorHistory();
        RefreshReferenceUnitInfo();
        RefreshUnitSummary();
        AppendLog($"地上マップ読込: map={mapName}, image={(_currentPreviewImagePath is null ? "-" : "OK")}, obj={(_currentObjPath is null ? "-" : "OK")}, route={_currentRoute.Waypoints.Count}, generators={_currentLayout.Spawns.Count}, waterbox={_currentWaterbox.Boxes.Count}");
    }

    //-------------------------------------------------------------------------------
    // 地上マップの表示用キャッシュを生成する処理
    //-------------------------------------------------------------------------------
    private async Task<FieldMapCacheEntry?> TryEnsureFieldMapCacheAsync(string mapName, string fieldMapRoot, string? fieldTextsRoot)
    {
        try
        {
            if (!File.Exists(textBoxToolkitPath.Text))
            {
                AppendLog("地上キャッシュ生成: Hocotate_Toolkit.exe が未設定のため，arc.szs/texts.szs の展開はスキップされます．");
            }

            FieldMapCacheService cacheService = new(fieldMapRoot, fieldTextsRoot, textBoxToolkitPath.Text, GetCurrentCacheRoot());
            progressBarCache.Minimum = 0;
            progressBarCache.Maximum = 4;
            progressBarCache.Value = 0;
            return await Task.Run(() =>
            {
                return cacheService.EnsureFieldMapCache(mapName, (name, completed, total) =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        UpdateCacheProgress(new CacheProgressInfo(name, completed, total, "地上キャッシュ準備中"));
                    }));
                });
            });
        }
        catch (Exception ex)
        {
            AppendLog($"地上キャッシュ生成失敗: {ex.Message}");
            return null;
        }
    }

    //-------------------------------------------------------------------------------
    // waterbox が未作成のユニット用にキャッシュ上の保存先を作成する処理
    //-------------------------------------------------------------------------------
    private static string CreateWaterboxPathFromCache(UnitCacheEntry cacheEntry)
    {
        return Path.Combine(cacheEntry.CacheDirectory, "texts", "waterbox.txt");
    }

    //-------------------------------------------------------------------------------
    // 現在設定から地上マップルートを取得する処理
    //-------------------------------------------------------------------------------
    private string? GetCurrentFieldMapRoot()
    {
        if (!string.IsNullOrWhiteSpace(_lastFieldMapRoot) &&
            Directory.Exists(_lastFieldMapRoot) &&
            IsPathInCurrentDiscSearchRoot(_lastFieldMapRoot))
        {
            return _lastFieldMapRoot;
        }

        if (Directory.Exists(textBoxArcPath.Text) && IsFieldMapRoot(textBoxArcPath.Text))
        {
            return textBoxArcPath.Text;
        }

        string selectedRoot = GetLoadRootDirectory(textBoxDiscRoot.Text);
        string searchRoot = GetDiscSearchRoot(selectedRoot);
        return FieldAssetLocator.ResolveFieldMapRoot(searchRoot);
    }

    //-------------------------------------------------------------------------------
    // 現在設定から地上 texts アーカイブルートを取得する処理
    //-------------------------------------------------------------------------------
    private string? GetCurrentFieldTextsRoot()
    {
        if (!string.IsNullOrWhiteSpace(_lastFieldTextsRoot) &&
            Directory.Exists(_lastFieldTextsRoot) &&
            IsPathInCurrentDiscSearchRoot(_lastFieldTextsRoot))
        {
            return _lastFieldTextsRoot;
        }

        if (Directory.Exists(textBoxUnitsPath.Text) && IsFieldTextsRoot(textBoxUnitsPath.Text))
        {
            return textBoxUnitsPath.Text;
        }

        string selectedRoot = GetLoadRootDirectory(textBoxDiscRoot.Text);
        string searchRoot = GetDiscSearchRoot(selectedRoot);
        return FieldAssetLocator.ResolveFieldTextsRoot(searchRoot);
    }

    //-------------------------------------------------------------------------------
    // 現在設定から洞窟 arc ルートを取得する処理
    //-------------------------------------------------------------------------------
    private string? GetCurrentCaveArcRoot()
    {
        if (!string.IsNullOrWhiteSpace(_lastCaveArcPath) &&
            Directory.Exists(_lastCaveArcPath) &&
            IsPathInCurrentDiscSearchRoot(_lastCaveArcPath))
        {
            return _lastCaveArcPath;
        }

        if (Directory.Exists(textBoxArcPath.Text) && IsCaveArcRoot(textBoxArcPath.Text))
        {
            return textBoxArcPath.Text;
        }

        string selectedRoot = GetLoadRootDirectory(textBoxDiscRoot.Text);
        string? arcRoot = ResolveCaveArcRootFromSearchRoot(GetDiscSearchRoot(selectedRoot));
        return arcRoot;
    }

    //-------------------------------------------------------------------------------
    // 現在設定から洞窟 units ルートを取得する処理
    //-------------------------------------------------------------------------------
    private string? GetCurrentCaveUnitsRoot()
    {
        if (!string.IsNullOrWhiteSpace(_lastCaveUnitsPath) &&
            Directory.Exists(_lastCaveUnitsPath) &&
            IsPathInCurrentDiscSearchRoot(_lastCaveUnitsPath))
        {
            return _lastCaveUnitsPath;
        }

        if (Directory.Exists(textBoxUnitsPath.Text) && IsCaveUnitsRoot(textBoxUnitsPath.Text))
        {
            return textBoxUnitsPath.Text;
        }

        string selectedRoot = GetLoadRootDirectory(textBoxDiscRoot.Text);
        string searchRoot = GetDiscSearchRoot(selectedRoot);
        return ResolveCaveUnitsRootFromSearchRoot(searchRoot);
    }

    //-------------------------------------------------------------------------------
    // パスが現在読み込み中のディスク探索ルート配下か判定する処理
    //-------------------------------------------------------------------------------
    private bool IsPathInCurrentDiscSearchRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(_currentDiscSearchRoot) || !Directory.Exists(_currentDiscSearchRoot))
        {
            return true;
        }

        try
        {
            string root = Path.GetFullPath(_currentDiscSearchRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string target = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return target.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------------
    // 探索ルートから洞窟ユニット用 arc フォルダを解決する処理
    //-------------------------------------------------------------------------------
    private static string? ResolveCaveArcRootFromSearchRoot(string searchRoot)
    {
        if (string.IsNullOrWhiteSpace(searchRoot) || !Directory.Exists(searchRoot))
        {
            return null;
        }

        string direct = Path.Combine(searchRoot, "user", "Mukki", "mapunits", "arc");
        if (IsCaveArcRoot(direct))
        {
            return direct;
        }

        if (IsCaveArcRoot(searchRoot))
        {
            return searchRoot;
        }

        try
        {
            return Directory.EnumerateDirectories(searchRoot, "arc", SearchOption.AllDirectories)
                .Where(IsCaveArcRoot)
                .OrderByDescending(IsMukkiMapUnitsPath)
                .ThenBy(path => path.Length)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    //-------------------------------------------------------------------------------
    // 探索ルートから洞窟ユニット定義 units フォルダを解決する処理
    //-------------------------------------------------------------------------------
    private static string? ResolveCaveUnitsRootFromSearchRoot(string searchRoot)
    {
        if (string.IsNullOrWhiteSpace(searchRoot) || !Directory.Exists(searchRoot))
        {
            return null;
        }

        string direct = Path.Combine(searchRoot, "user", "Mukki", "mapunits", "units");
        if (IsCaveUnitsRoot(direct))
        {
            return direct;
        }

        if (IsCaveUnitsRoot(searchRoot))
        {
            return searchRoot;
        }

        string? allUnitsDir = RecursiveFinder.FindDirectoryContainingFile(searchRoot, "all_units.txt");
        if (IsCaveUnitsRoot(allUnitsDir ?? string.Empty))
        {
            return allUnitsDir;
        }

        try
        {
            return Directory.EnumerateDirectories(searchRoot, "units", SearchOption.AllDirectories)
                .Where(IsCaveUnitsRoot)
                .OrderByDescending(IsMukkiMapUnitsPath)
                .ThenBy(path => path.Length)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    //-------------------------------------------------------------------------------
    // パスが Mukki/mapunits 配下か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsMukkiMapUnitsPath(string path)
    {
        string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        string marker = Path.Combine("user", "Mukki", "mapunits") + Path.DirectorySeparatorChar;
        return normalized.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダが地上 map ルートか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsFieldMapRoot(string path)
    {
        return Directory.Exists(Path.Combine(path, "forest")) &&
            File.Exists(Path.Combine(path, "forest", "route.txt"));
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダが地上 texts ルートか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsFieldTextsRoot(string path)
    {
        return Directory.Exists(Path.Combine(path, "forest")) &&
            File.Exists(Path.Combine(path, "forest", "texts.szs"));
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダが洞窟 arc ルートか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsCaveArcRoot(string path)
    {
        return Directory.Exists(path) &&
            Directory.GetDirectories(path).Any(dir => File.Exists(Path.Combine(dir, "arc.szs")));
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダが洞窟 units ルートか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsCaveUnitsRoot(string path)
    {
        return Directory.Exists(path) &&
            (File.Exists(Path.Combine(path, "all_units.txt")) ||
             Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly).Length > 0);
    }

    //-------------------------------------------------------------------------------
    // 現在の日数条件に一致する地上 generator object を表示用ポイントへ変換する処理
    //-------------------------------------------------------------------------------
    private void RefreshFieldGeneratorDisplayObjects(bool resetSelection)
    {
        if (_currentFieldMapData is null)
        {
            _fieldDisplayObjectRefs = Array.Empty<FieldDisplayObjectRef>();
            _currentLayout = new LayoutFile(Array.Empty<LayoutSpawn>());
            return;
        }

        _currentLayout = BuildDisplayLayoutFromFieldGenerators(
            _currentFieldMapData.GeneratorFiles,
            _currentFieldDay,
            out IReadOnlyList<FieldDisplayObjectRef> refs);
        _fieldDisplayObjectRefs = refs;
        if (resetSelection)
        {
            _selectedSpawnIndex = null;
        }
    }

    //-------------------------------------------------------------------------------
    // 地上 generator object を日数条件つきで既存のポイント表示へ変換する処理
    //-------------------------------------------------------------------------------
    private static LayoutFile BuildDisplayLayoutFromFieldGenerators(
        IReadOnlyList<FieldGeneratorFile> generatorFiles,
        int day,
        out IReadOnlyList<FieldDisplayObjectRef> refs)
    {
        List<LayoutSpawn> spawns = new();
        List<FieldDisplayObjectRef> objectRefs = new();
        for (int fileIndex = 0; fileIndex < generatorFiles.Count; fileIndex++)
        {
            FieldGeneratorFile generatorFile = generatorFiles[fileIndex];
            if (!IsFieldGeneratorFileActive(generatorFile, day, generatorFiles))
            {
                continue;
            }

            for (int objectIndex = 0; objectIndex < generatorFile.Objects.Count; objectIndex++)
            {
                FieldGeneratorObject fieldObject = generatorFile.Objects[objectIndex];
                int typeId = FieldGeneratorParser.ToDisplayTypeId(fieldObject);
                spawns.Add(new LayoutSpawn(
                    typeId,
                    fieldObject.ObjectLabel,
                    fieldObject.X,
                    fieldObject.Y,
                    fieldObject.Z,
                    fieldObject.Angle,
                    fieldObject.Radius,
                    1,
                    1));
                objectRefs.Add(new FieldDisplayObjectRef(fileIndex, objectIndex));
            }
        }

        refs = objectRefs;
        return new LayoutFile(spawns);
    }

    //-------------------------------------------------------------------------------
    // 指定 generator ファイルが現在日数で有効か判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsFieldGeneratorFileActive(FieldGeneratorFile generatorFile, int day, IReadOnlyList<FieldGeneratorFile> allFiles)
    {
        if (generatorFile.Scope == FieldGeneratorScope.Root)
        {
            return true;
        }

        if (generatorFile.StartDay is null || generatorFile.EndDay is null)
        {
            return false;
        }

        if (generatorFile.Scope == FieldGeneratorScope.NonLoop)
        {
            return day >= generatorFile.StartDay.Value && day <= generatorFile.EndDay.Value;
        }

        int minLoopDay = allFiles
            .Where(file => file.Scope == FieldGeneratorScope.Loop && file.StartDay is not null)
            .Select(file => file.StartDay!.Value)
            .DefaultIfEmpty(generatorFile.StartDay.Value)
            .Min();
        int maxLoopDay = allFiles
            .Where(file => file.Scope == FieldGeneratorScope.Loop && file.EndDay is not null)
            .Select(file => file.EndDay!.Value)
            .DefaultIfEmpty(generatorFile.EndDay.Value)
            .Max();
        int loopSpan = Math.Max(1, maxLoopDay - minLoopDay + 1);
        if (day < minLoopDay)
        {
            return false;
        }

        int loopDay = minLoopDay + ((day - minLoopDay) % loopSpan);
        return loopDay >= generatorFile.StartDay.Value && loopDay <= generatorFile.EndDay.Value;
    }

    //-------------------------------------------------------------------------------
    // 現在日数で有効な地上 generator ファイル名を表示用に組み立てる処理
    //-------------------------------------------------------------------------------
    private static string BuildActiveFieldGeneratorFileText(IReadOnlyList<FieldGeneratorFile> generatorFiles, int day)
    {
        List<string> activeNames = generatorFiles
            .Where(file => IsFieldGeneratorFileActive(file, day, generatorFiles))
            .Select(file => file.DisplayName)
            .ToList();
        return activeNames.Count == 0 ? "-" : string.Join(Environment.NewLine, activeNames);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 追加用コントロールの候補を現在日数に合わせる処理
    //-------------------------------------------------------------------------------
    private void RefreshFieldAddControls()
    {
        if (_comboBoxFieldAddFile is null ||
            _comboBoxFieldAddType is null ||
            _buttonFieldAddSpawnMode is null)
        {
            return;
        }

        bool canAdd = GetCurrentMode() == EditorMode.Field && _currentFieldMapData is not null;
        _comboBoxFieldAddFile.Enabled = canAdd;
        _comboBoxFieldAddType.Enabled = canAdd;
        _buttonFieldAddSpawnMode.Enabled = canAdd;
        if (!canAdd)
        {
            _comboBoxFieldAddFile.Items.Clear();
            return;
        }

        int? selectedFileIndex = _comboBoxFieldAddFile.SelectedItem is FieldGeneratorFileItem selectedItem
            ? selectedItem.FileIndex
            : null;
        List<FieldGeneratorFileItem> items = new();
        for (int fileIndex = 0; fileIndex < _currentFieldMapData!.GeneratorFiles.Count; fileIndex++)
        {
            FieldGeneratorFile generatorFile = _currentFieldMapData.GeneratorFiles[fileIndex];
            if (IsFieldGeneratorFileActive(generatorFile, _currentFieldDay, _currentFieldMapData.GeneratorFiles))
            {
                items.Add(new FieldGeneratorFileItem(fileIndex, generatorFile.DisplayName));
            }
        }

        if (items.Count == 0)
        {
            for (int fileIndex = 0; fileIndex < _currentFieldMapData.GeneratorFiles.Count; fileIndex++)
            {
                FieldGeneratorFile generatorFile = _currentFieldMapData.GeneratorFiles[fileIndex];
                if (generatorFile.Scope == FieldGeneratorScope.Root)
                {
                    items.Add(new FieldGeneratorFileItem(fileIndex, generatorFile.DisplayName));
                }
            }
        }

        _comboBoxFieldAddFile.BeginUpdate();
        try
        {
            _comboBoxFieldAddFile.Items.Clear();
            foreach (FieldGeneratorFileItem item in items)
            {
                _comboBoxFieldAddFile.Items.Add(item);
            }
        }
        finally
        {
            _comboBoxFieldAddFile.EndUpdate();
        }

        int selectedIndex = selectedFileIndex is null
            ? -1
            : items.FindIndex(item => item.FileIndex == selectedFileIndex.Value);
        _comboBoxFieldAddFile.SelectedIndex = items.Count == 0 ? -1 : Math.Max(0, selectedIndex);
    }

    //-------------------------------------------------------------------------------
    // 表示用ポイントの編集内容を地上 generator 構造へ反映する処理
    //-------------------------------------------------------------------------------
    private FieldMapData ApplyDisplayLayoutToFieldGenerators(FieldMapData fieldMapData, LayoutFile layout)
    {
        List<FieldGeneratorFile> generatorFiles = fieldMapData.GeneratorFiles.ToList();
        for (int layoutIndex = 0; layoutIndex < layout.Spawns.Count && layoutIndex < _fieldDisplayObjectRefs.Count; layoutIndex++)
        {
            FieldDisplayObjectRef objectRef = _fieldDisplayObjectRefs[layoutIndex];
            if (objectRef.FileIndex < 0 ||
                objectRef.FileIndex >= generatorFiles.Count ||
                objectRef.ObjectIndex < 0 ||
                objectRef.ObjectIndex >= generatorFiles[objectRef.FileIndex].Objects.Count)
            {
                continue;
            }

            FieldGeneratorFile generatorFile = generatorFiles[objectRef.FileIndex];
            List<FieldGeneratorObject> objects = generatorFile.Objects.ToList();
            FieldGeneratorObject fieldObject = objects[objectRef.ObjectIndex];
            LayoutSpawn spawn = layout.Spawns[layoutIndex];
            objects[objectRef.ObjectIndex] = fieldObject with
            {
                X = spawn.X,
                Y = spawn.Y,
                Z = spawn.Z,
                Angle = spawn.Angle,
                Radius = Math.Max(0f, spawn.Radius)
            };
            generatorFiles[objectRef.FileIndex] = generatorFile with
            {
                Objects = objects
            };
        }

        return fieldMapData with
        {
            GeneratorFiles = generatorFiles
        };
    }

    //-------------------------------------------------------------------------------
    // 現在表示中の地上 object 編集内容をメモリ上の地上マップへ反映する処理
    //-------------------------------------------------------------------------------
    private void CommitCurrentFieldLayoutToMapData()
    {
        if (_currentFieldMapData is null || _fieldDisplayObjectRefs.Count == 0)
        {
            return;
        }

        _currentFieldMapData = ApplyDisplayLayoutToFieldGenerators(_currentFieldMapData, _currentLayout);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator へクリック位置を使って object を追加する処理
    //-------------------------------------------------------------------------------
    private void AddFieldObjectAt(float x, float z)
    {
        if (_currentFieldMapData is null ||
            _comboBoxFieldAddFile?.SelectedItem is not FieldGeneratorFileItem fileItem ||
            _comboBoxFieldAddType?.SelectedItem is not FieldAddTemplateItem templateItem)
        {
            AppendLog("地上 object 追加失敗: 追加先 generator または追加タイプが未選択です．");
            return;
        }

        if (fileItem.FileIndex < 0 || fileItem.FileIndex >= _currentFieldMapData.GeneratorFiles.Count)
        {
            return;
        }

        try
        {
            CommitCurrentFieldLayoutToMapData();
            float y = GetGroundHeightOrFallback(x, z, 0f);
            FieldGeneratorFile targetFile = _currentFieldMapData.GeneratorFiles[fileItem.FileIndex];
            int newObjectIndex = targetFile.Objects.Count;
            string rawText = BuildFieldObjectTemplate(templateItem.Kind, x, y, z);
            List<FieldGeneratorFile> generatorFiles = _currentFieldMapData.GeneratorFiles.ToList();
            generatorFiles[fileItem.FileIndex] = FieldGeneratorParser.AddObjectRawText(targetFile, rawText);
            _currentFieldMapData = _currentFieldMapData with
            {
                GeneratorFiles = generatorFiles
            };

            RefreshFieldGeneratorDisplayObjects(resetSelection: false);
            _selectedSpawnIndex = FindFieldDisplayIndex(fileItem.FileIndex, newObjectIndex);
            _selectedRouteWaypointIndex = null;
            checkBoxSpawnOverlay.Checked = true;
            UpdateAllPreviewOverlays();
            SyncSelectedTargets();
            RefreshInspector();
            RefreshUnitSummary();
            AppendLog($"地上 object 追加: file={targetFile.DisplayName}, type={templateItem.Label}, index={newObjectIndex}, x={x:0.###}, y={y:0.###}, z={z:0.###}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"地上 object の追加に失敗しました．\n{ex.Message}", "地上 object 追加", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AppendLog($"地上 object 追加失敗: {ex.Message}");
        }
    }

    //-------------------------------------------------------------------------------
    // 表示中の地上 object を元 generator から削除する処理
    //-------------------------------------------------------------------------------
    private void DeleteFieldObjectAt(int layoutIndex)
    {
        if (_currentFieldMapData is null ||
            layoutIndex < 0 ||
            layoutIndex >= _fieldDisplayObjectRefs.Count)
        {
            return;
        }

        FieldDisplayObjectRef objectRef = _fieldDisplayObjectRefs[layoutIndex];
        if (objectRef.FileIndex < 0 ||
            objectRef.FileIndex >= _currentFieldMapData.GeneratorFiles.Count ||
            objectRef.ObjectIndex < 0 ||
            objectRef.ObjectIndex >= _currentFieldMapData.GeneratorFiles[objectRef.FileIndex].Objects.Count)
        {
            return;
        }

        try
        {
            CommitCurrentFieldLayoutToMapData();
            FieldGeneratorFile targetFile = _currentFieldMapData.GeneratorFiles[objectRef.FileIndex];
            List<FieldGeneratorFile> generatorFiles = _currentFieldMapData.GeneratorFiles.ToList();
            generatorFiles[objectRef.FileIndex] = FieldGeneratorParser.RemoveObject(targetFile, objectRef.ObjectIndex);
            _currentFieldMapData = _currentFieldMapData with
            {
                GeneratorFiles = generatorFiles
            };

            RefreshFieldGeneratorDisplayObjects(resetSelection: true);
            UpdateAllPreviewOverlays();
            SyncSelectedTargets();
            RefreshInspector();
            RefreshUnitSummary();
            AppendLog($"地上 object 削除: file={targetFile.DisplayName}, index={objectRef.ObjectIndex}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"地上 object の削除に失敗しました．\n{ex.Message}", "地上 object 削除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AppendLog($"地上 object 削除失敗: {ex.Message}");
        }
    }

    //-------------------------------------------------------------------------------
    // generator 参照に対応する表示 index を取得する処理
    //-------------------------------------------------------------------------------
    private int? FindFieldDisplayIndex(int fileIndex, int objectIndex)
    {
        for (int i = 0; i < _fieldDisplayObjectRefs.Count; i++)
        {
            FieldDisplayObjectRef objectRef = _fieldDisplayObjectRefs[i];
            if (objectRef.FileIndex == fileIndex && objectRef.ObjectIndex == objectIndex)
            {
                return i;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 地上 generator へ追加する object テンプレートを作成する処理
    //-------------------------------------------------------------------------------
    private static string BuildFieldObjectTemplate(FieldAddTemplateKind kind, float x, float y, float z)
    {
        string position = $"{ToFieldNumber(x)} {ToFieldNumber(y)} {ToFieldNumber(z)} # Position";
        return kind switch
        {
            FieldAddTemplateKind.Item => string.Join(Environment.NewLine, BuildFieldItemTemplateLines(position)),
            FieldAddTemplateKind.Pikmin => string.Join(Environment.NewLine, BuildFieldPikminTemplateLines(position)),
            FieldAddTemplateKind.CaveEntrance => string.Join(Environment.NewLine, BuildFieldCaveTemplateLines(position)),
            _ => string.Join(Environment.NewLine, BuildFieldTekiTemplateLines(position))
        };
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 用の teki テンプレート行を作成する処理
    //-------------------------------------------------------------------------------
    private static IEnumerable<string> BuildFieldTekiTemplateLines(string position)
    {
        yield return "# Codex added teki";
        yield return "{";
        yield return "\t{v0.3} # Version";
        yield return "\t0 # Reserved";
        yield return "\t0 # Days till resurrection";
        yield return "\t67 111 100 101 120 32 84 101 107 105 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 # Name";
        yield return $"\t{position}";
        yield return "\t0.0 0.0 0.0 # Offset";
        yield return "\t{teki} {0005} 49 # Teki identifier";
        yield return "\t0 # teki_birth_type";
        yield return "\t1 # teki_num";
        yield return "\t0.0 # face direction";
        yield return "\t1 # 0:point 1:circle";
        yield return "\t100.0 # appear radius";
        yield return "\t0.0 # enemy size";
        yield return "\t0 # treasure item code";
        yield return "\t3 # Pellet color";
        yield return "\t1 # Pellet size";
        yield return "\t1 # Pellet Min";
        yield return "\t8 # Pellet Max";
        yield return "\t0.0 # Pellet Min";
        yield return "\t{????} # Version";
        yield return "\t{";
        yield return "\t\t{_eof}";
        yield return "\t}";
        yield return "}";
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 用の item テンプレート行を作成する処理
    //-------------------------------------------------------------------------------
    private static IEnumerable<string> BuildFieldItemTemplateLines(string position)
    {
        yield return "# Codex added item";
        yield return "{";
        yield return "\t{v0.1} # Version";
        yield return "\t11 # Reserved";
        yield return "\t0 # Days till resurrection";
        yield return "\t67 111 100 101 120 32 73 116 101 109 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 # Name";
        yield return $"\t{position}";
        yield return "\t0.0 0.0 0.0 # Offset";
        yield return "\t{pelt} {0000}";
        yield return "\t{";
        yield return "\t\t0 # Pellet";
        yield return "\t\t0.0 0.0 0.0 # Rotation";
        yield return "\t\t{0000} # Local version";
        yield return "\t\t0 1 # Pellet type and size";
        yield return "\t}";
        yield return "\t{";
        yield return "\t\t{_eof}";
        yield return "\t}";
        yield return "}";
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 用の Pikmin テンプレート行を作成する処理
    //-------------------------------------------------------------------------------
    private static IEnumerable<string> BuildFieldPikminTemplateLines(string position)
    {
        yield return "# Codex added Pikmin";
        yield return "{";
        yield return "\t{v0.3} # Version";
        yield return "\t0 # Reserved";
        yield return "\t0 # Days till resurrection";
        yield return "\t67 111 100 101 120 32 80 105 107 109 105 110 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 # Name";
        yield return $"\t{position}";
        yield return "\t0.0 0.0 0.0 # Offset";
        yield return "\t{piki} {0001}";
        yield return "\t{";
        yield return "\t\t{p000} 4 1";
        yield return "\t\t{p001} 4 1";
        yield return "\t\t{p002} 4 0";
        yield return "\t\t{_eof}";
        yield return "\t}";
        yield return "}";
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 用の cave entrance テンプレート行を作成する処理
    //-------------------------------------------------------------------------------
    private static IEnumerable<string> BuildFieldCaveTemplateLines(string position)
    {
        yield return "# Codex added cave entrance";
        yield return "{";
        yield return "\t{v0.1} # Version";
        yield return "\t0 # Reserved";
        yield return "\t0 # Days till resurrection";
        yield return "\t67 111 100 101 120 32 67 97 118 101 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 # Name";
        yield return $"\t{position}";
        yield return "\t0.0 0.0 0.0 # Offset";
        yield return "\t{item} {0002} # cave";
        yield return "\t{";
        yield return "\t\t{cave} # item id";
        yield return "\t\t0.0 0.0 0.0 # rotation";
        yield return "\t\t{0002} # item local version";
        yield return "\t\ttutorial_1.txt";
        yield return "\t\tunits.txt";
        yield return "\t\t{c_01} # id";
        yield return "\t\t{";
        yield return "\t\t\t{_eof}";
        yield return "\t\t}";
        yield return "\t}";
        yield return "\t{";
        yield return "\t\t{_eof}";
        yield return "\t}";
        yield return "}";
    }

    //-------------------------------------------------------------------------------
    // generator ファイルへ書く実数文字列を作成する処理
    //-------------------------------------------------------------------------------
    private static string ToFieldNumber(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    //-------------------------------------------------------------------------------
    // 選択中の表示 object に対応する地上 generator 参照を取得する処理
    //-------------------------------------------------------------------------------
    private bool TryGetSelectedFieldObjectRef(out FieldDisplayObjectRef objectRef)
    {
        objectRef = new FieldDisplayObjectRef(-1, -1);
        if (GetCurrentMode() != EditorMode.Field ||
            _selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _fieldDisplayObjectRefs.Count)
        {
            return false;
        }

        FieldDisplayObjectRef candidate = _fieldDisplayObjectRefs[_selectedSpawnIndex.Value];
        if (_currentFieldMapData is null ||
            candidate.FileIndex < 0 ||
            candidate.FileIndex >= _currentFieldMapData.GeneratorFiles.Count ||
            candidate.ObjectIndex < 0 ||
            candidate.ObjectIndex >= _currentFieldMapData.GeneratorFiles[candidate.FileIndex].Objects.Count)
        {
            return false;
        }

        objectRef = candidate;
        return true;
    }

    //-------------------------------------------------------------------------------
    // 選択中の地上 generator object の raw テキストを取得する処理
    //-------------------------------------------------------------------------------
    private string GetSelectedFieldObjectRawText()
    {
        if (_currentFieldMapData is null || !TryGetSelectedFieldObjectRef(out FieldDisplayObjectRef objectRef))
        {
            return string.Empty;
        }

        FieldMapData previewData = ApplyDisplayLayoutToFieldGenerators(_currentFieldMapData, _currentLayout);
        return FieldGeneratorParser.GetObjectRawText(
            previewData.GeneratorFiles[objectRef.FileIndex],
            objectRef.ObjectIndex);
    }

    //-------------------------------------------------------------------------------
    // 地上マップ表示用の範囲を route と generator から計算する処理
    //-------------------------------------------------------------------------------
    private static RectangleF GetFieldMapDisplayBounds(LayoutFile layout, RouteFile route)
    {
        List<float> xs = new();
        List<float> zs = new();
        xs.AddRange(layout.Spawns.Select(spawn => spawn.X));
        zs.AddRange(layout.Spawns.Select(spawn => spawn.Z));
        xs.AddRange(route.Waypoints.Values.Select(waypoint => waypoint.X));
        zs.AddRange(route.Waypoints.Values.Select(waypoint => waypoint.Z));

        if (xs.Count == 0 || zs.Count == 0)
        {
            return RectangleF.Empty;
        }

        float minX = xs.Min();
        float maxX = xs.Max();
        float minZ = zs.Min();
        float maxZ = zs.Max();
        float marginX = Math.Max((maxX - minX) * 0.12f, 128f);
        float marginZ = Math.Max((maxZ - minZ) * 0.12f, 128f);
        return RectangleF.FromLTRB(minX - marginX, minZ - marginZ, maxX + marginX, maxZ + marginZ);
    }

    //-------------------------------------------------------------------------------
    // 選択ユニットのキャッシュ生成を進捗付きで実行する処理
    //-------------------------------------------------------------------------------
    private async Task<UnitCacheEntry> EnsureSelectedUnitCacheAsync(string unitName)
    {
        UnitAssetCacheService cacheService = new(textBoxArcPath.Text, new Dictionary<string, string>(), textBoxToolkitPath.Text, GetCurrentCacheRoot(), CreatePrettyImageProvider());
        progressBarCache.Minimum = 0;
        progressBarCache.Maximum = 4;
        progressBarCache.Value = 0;
        return await Task.Run(() =>
        {
            return cacheService.EnsureUnitCache(unitName, (name, completed, total) =>
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateCacheProgress(new CacheProgressInfo(name, completed, total, "キャッシュ準備中"));
                }));
            });
        });
    }

    //-------------------------------------------------------------------------------
    // obj から地形のワールド範囲を取得する処理
    //-------------------------------------------------------------------------------
    private static ObjScene? LoadObjScene(UnitCacheEntry cacheEntry)
    {
        if (!string.IsNullOrWhiteSpace(cacheEntry.ObjPath) &&
            File.Exists(cacheEntry.ObjPath))
        {
            return LoadObjSceneFromPath(cacheEntry.ObjPath, cacheEntry.MtlPath);
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // OBJ パスと任意の MTL パスから 3D シーンを読み込む処理
    //-------------------------------------------------------------------------------
    private static ObjScene? LoadObjSceneFromPath(string objPath, string? mtlPath = null)
    {
        if (string.IsNullOrWhiteSpace(objPath) || !File.Exists(objPath))
        {
            return null;
        }

        string? resolvedMtlPath = !string.IsNullOrWhiteSpace(mtlPath) && File.Exists(mtlPath)
            ? mtlPath
            : FindObjMtlPath(objPath);
        return ObjScene.Load(objPath, resolvedMtlPath);
    }

    //-------------------------------------------------------------------------------
    // 読み込み済み obj から地形のワールド範囲を取得する処理
    //-------------------------------------------------------------------------------
    private static RectangleF LoadModelBounds(ObjScene? scene)
    {
        if (scene is not null && scene.Vertices.Count > 0)
        {
            float minX = scene.Vertices.Min(v => v.X);
            float maxX = scene.Vertices.Max(v => v.X);
            float minZ = scene.Vertices.Min(v => v.Z);
            float maxZ = scene.Vertices.Max(v => v.Z);
            return RectangleF.FromLTRB(minX, minZ, maxX, maxZ);
        }

        return new RectangleF(-256, -256, 512, 512);
    }

    //-------------------------------------------------------------------------------
    // 選択ユニットの定義ファイルから dX/dZ 情報を取得する処理
    //-------------------------------------------------------------------------------
    private UnitDefinition? TryLoadUnitDefinition(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return null;
        }

        List<string> candidates = new();
        AddUnitDefinitionCandidates(textBoxUnitsPath.Text, candidates);

        string? caveUnitsRoot = GetCurrentCaveUnitsRoot();
        if (!string.IsNullOrWhiteSpace(caveUnitsRoot))
        {
            AddUnitDefinitionCandidates(caveUnitsRoot, candidates);
        }

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            try
            {
                UnitDefinition? definition = UnitDefinitionParser.ParseMany(candidate)
                    .FirstOrDefault(unit => unit.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase));
                if (definition is not null)
                {
                    return definition;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"unit 定義読込失敗: {Path.GetFileName(candidate)} ({ex.Message})");
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // UnitSet 定義の候補ファイルを追加する処理
    //-------------------------------------------------------------------------------
    private static void AddUnitDefinitionCandidates(string? unitsPath, List<string> candidates)
    {
        if (string.IsNullOrWhiteSpace(unitsPath))
        {
            return;
        }

        if (Directory.Exists(unitsPath))
        {
            candidates.Add(Path.Combine(unitsPath, "all_units.txt"));
            try
            {
                candidates.AddRange(Directory.GetFiles(unitsPath, "*.txt").OrderBy(Path.GetFileName));
            }
            catch
            {
            }
        }
        else if (File.Exists(unitsPath))
        {
            candidates.Add(unitsPath);
        }
    }

    //-------------------------------------------------------------------------------
    // pretty 画像を CaveGen と同じユニット寸法基準へ合わせるための表示範囲を算出する処理
    //-------------------------------------------------------------------------------
    private static RectangleF GetPrettyImageDisplayBounds(RectangleF modelBounds, UnitDefinition? unitDefinition, LayoutFile layout, RouteFile route, Size imageSize)
    {
        float imageAspect = imageSize.Height <= 0
            ? 1f
            : imageSize.Width / (float)imageSize.Height;

        if (unitDefinition is not null && unitDefinition.Width > 0 && unitDefinition.Height > 0)
        {
            float width = unitDefinition.Width * 170f;
            float height = unitDefinition.Height * 170f;
            return new RectangleF(width * -0.5f, height * -0.5f, width, height);
        }

        RectangleF? inferredBounds = TryInferPrettyImageCellBounds(layout, route);
        if (inferredBounds is not null && IsReasonablePrettyImageInferredBounds(inferredBounds.Value, modelBounds))
        {
            return inferredBounds.Value;
        }

        if (modelBounds.Width > 0f && modelBounds.Height > 0f)
        {
            return ExpandBoundsToAspect(modelBounds, imageAspect);
        }

        RectangleF overlayBounds = GetOverlayDataBounds(layout, route);
        if (overlayBounds.Width <= 0f || overlayBounds.Height <= 0f)
        {
            return modelBounds;
        }

        float marginX = Math.Max(overlayBounds.Width * 0.18f, 48f);
        float marginZ = Math.Max(overlayBounds.Height * 0.18f, 48f);
        RectangleF expanded = RectangleF.FromLTRB(
            overlayBounds.Left - marginX,
            overlayBounds.Top - marginZ,
            overlayBounds.Right + marginX,
            overlayBounds.Bottom + marginZ);

        return ExpandBoundsToAspect(expanded, imageAspect);
    }

    //-------------------------------------------------------------------------------
    // route/layout から推定した pretty 表示範囲が異常に大きくないか判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsReasonablePrettyImageInferredBounds(RectangleF inferredBounds, RectangleF modelBounds)
    {
        if (inferredBounds.Width <= 0f || inferredBounds.Height <= 0f)
        {
            return false;
        }

        if (modelBounds.Width <= 0f || modelBounds.Height <= 0f)
        {
            return inferredBounds.Width <= 2048f && inferredBounds.Height <= 2048f;
        }

        float maxWidth = Math.Max(modelBounds.Width * 2.5f, modelBounds.Width + 340f);
        float maxHeight = Math.Max(modelBounds.Height * 2.5f, modelBounds.Height + 340f);
        return inferredBounds.Width <= maxWidth && inferredBounds.Height <= maxHeight;
    }

    //-------------------------------------------------------------------------------
    // UnitSet 定義がない場合に route/layout 座標からセル寸法を推定する処理
    //-------------------------------------------------------------------------------
    private static RectangleF? TryInferPrettyImageCellBounds(LayoutFile layout, RouteFile route)
    {
        List<float> xs = new();
        List<float> zs = new();
        foreach (RouteWaypoint waypoint in route.Waypoints.Values)
        {
            xs.Add(waypoint.X);
            zs.Add(waypoint.Z);
        }

        foreach (LayoutSpawn spawn in layout.Spawns)
        {
            xs.Add(spawn.X);
            zs.Add(spawn.Z);
        }

        if (xs.Count == 0 || zs.Count == 0)
        {
            return null;
        }

        int cellWidth = Math.Max(1, (int)MathF.Ceiling(xs.Max(value => MathF.Abs(value)) * 2f / 170f));
        int cellHeight = Math.Max(1, (int)MathF.Ceiling(zs.Max(value => MathF.Abs(value)) * 2f / 170f));
        float width = cellWidth * 170f;
        float height = cellHeight * 170f;
        return new RectangleF(width * -0.5f, height * -0.5f, width, height);
    }

    //-------------------------------------------------------------------------------
    // route/layout の座標範囲を取得する処理
    //-------------------------------------------------------------------------------
    private static RectangleF GetOverlayDataBounds(LayoutFile layout, RouteFile route)
    {
        List<float> xs = new();
        List<float> zs = new();
        foreach (RouteWaypoint waypoint in route.Waypoints.Values)
        {
            xs.Add(waypoint.X);
            zs.Add(waypoint.Z);
        }

        foreach (LayoutSpawn spawn in layout.Spawns)
        {
            xs.Add(spawn.X - Math.Max(spawn.Radius, 0f));
            xs.Add(spawn.X + Math.Max(spawn.Radius, 0f));
            zs.Add(spawn.Z - Math.Max(spawn.Radius, 0f));
            zs.Add(spawn.Z + Math.Max(spawn.Radius, 0f));
        }

        if (xs.Count == 0 || zs.Count == 0)
        {
            return RectangleF.Empty;
        }

        return RectangleF.FromLTRB(xs.Min(), zs.Min(), xs.Max(), zs.Max());
    }

    //-------------------------------------------------------------------------------
    // 指定範囲を中心基準で指定アスペクト比へ拡張する処理
    //-------------------------------------------------------------------------------
    private static RectangleF ExpandBoundsToAspect(RectangleF bounds, float aspect)
    {
        if (bounds.Width <= 0f || bounds.Height <= 0f || aspect <= 0.0001f)
        {
            return bounds;
        }

        float currentAspect = bounds.Width / bounds.Height;
        if (Math.Abs(currentAspect - aspect) < 0.0001f)
        {
            return bounds;
        }

        float centerX = bounds.Left + (bounds.Width * 0.5f);
        float centerY = bounds.Top + (bounds.Height * 0.5f);
        if (currentAspect < aspect)
        {
            float newWidth = bounds.Height * aspect;
            return new RectangleF(centerX - (newWidth * 0.5f), bounds.Top, newWidth, bounds.Height);
        }

        float newHeight = bounds.Width / aspect;
        return new RectangleF(bounds.Left, centerY - (newHeight * 0.5f), bounds.Width, newHeight);
    }

    //-------------------------------------------------------------------------------
    // waypoint 移動モードの切替を行う処理
    //-------------------------------------------------------------------------------
    private void buttonRouteMoveMode_Click(object? sender, EventArgs e)
    {
        if (!CanEditRoutePoints())
        {
            return;
        }

        _currentEditMode = _currentEditMode == UnitMapEditMode.MoveRouteWaypoint
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.MoveRouteWaypoint;

        if (_currentEditMode == UnitMapEditMode.MoveRouteWaypoint)
        {
            checkBoxRouteOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // spawn 移動モードの切替を行う処理
    //-------------------------------------------------------------------------------
    private void buttonSpawnMoveMode_Click(object? sender, EventArgs e)
    {
        if (!CanEditSpawnLikePoints())
        {
            return;
        }

        _currentEditMode = _currentEditMode == UnitMapEditMode.MoveSpawn
            ? UnitMapEditMode.Navigate
            : UnitMapEditMode.MoveSpawn;

        if (_currentEditMode == UnitMapEditMode.MoveSpawn)
        {
            checkBoxSpawnOverlay.Checked = true;
        }

        _unitMapView.SetEditMode(_currentEditMode);
        _objModelView.SetEditMode(_currentEditMode);
        UpdateRouteEditUi();
        UpdateQuickToolWindowState();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // layout.txt を現在の編集状態で保存する処理
    //-------------------------------------------------------------------------------
    private async void buttonSaveLayout_Click(object? sender, EventArgs e)
    {
        await SaveCurrentUnitArchivesAsync();
    }

    //-------------------------------------------------------------------------------
    // route.txt を現在の編集状態で保存する処理
    //-------------------------------------------------------------------------------
    private async void buttonSaveRoute_Click(object? sender, EventArgs e)
    {
        await SaveCurrentUnitArchivesAsync();
    }

    //-------------------------------------------------------------------------------
    // 現在の layout/route をキャッシュへ保存し元 szs へ反映する処理
    //-------------------------------------------------------------------------------
    private async Task SaveCurrentUnitArchivesAsync()
    {
        if (GetCurrentMode() == EditorMode.Field)
        {
            await SaveCurrentFieldMapFilesAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentPreviewUnitName))
        {
            AppendLog("保存対象ユニットが見つからないため保存できません．");
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentLayoutPath) &&
            string.IsNullOrWhiteSpace(_currentRoutePath) &&
            string.IsNullOrWhiteSpace(_currentWaterboxPath))
        {
            AppendLog("layout/route/waterbox 保存先が見つからないため保存できません．");
            return;
        }

        DialogResult result = MessageBox.Show(
            this,
            "現在の layout/route/waterbox をキャッシュへ保存し，arc.szs と texts.szs を元ユニットへ上書きします．\n続行しますか？",
            "保存確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            AppendLog("保存をキャンセルしました．");
            return;
        }

        try
        {
            SetBusyState(true, "ユニットを保存しています");
            if (!string.IsNullOrWhiteSpace(_currentLayoutPath))
            {
                LayoutSerializer.WriteFile(_currentLayoutPath, _currentLayout);
                AppendLog($"layout キャッシュ保存完了: {_currentLayoutPath}");
            }

            if (!string.IsNullOrWhiteSpace(_currentRoutePath))
            {
                RouteSerializer.WriteFile(_currentRoutePath, _currentRoute);
                AppendLog($"route キャッシュ保存完了: {_currentRoutePath}");
            }

            if (!string.IsNullOrWhiteSpace(_currentWaterboxPath))
            {
                WaterboxSerializer.WriteFile(_currentWaterboxPath, _currentWaterbox);
                AppendLog($"waterbox キャッシュ保存完了: {_currentWaterboxPath}");
            }

            string unitName = _currentPreviewUnitName;
            UnitArchiveRepackResult repackResult = await Task.Run(() =>
            {
                UnitAssetCacheService cacheService = new(
                    textBoxArcPath.Text,
                    new Dictionary<string, string>(),
                    textBoxToolkitPath.Text,
                    GetCurrentCacheRoot(),
                    CreatePrettyImageProvider());
                return cacheService.RepackUnitArchives(unitName);
            });

            AppendLog($"arc.szs 保存完了: {repackResult.ArcArchivePath}");
            AppendLog($"texts.szs 保存完了: {repackResult.TextsArchivePath}");
            MessageBox.Show(this, "layout/route/waterbox を保存し，元ユニットの szs を更新しました．", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog($"ユニット保存失敗: {ex.Message}");
            MessageBox.Show(this, $"保存に失敗しました．\n{ex.Message}", "保存失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusyState(false, "待機中です");
        }
    }

    //-------------------------------------------------------------------------------
    // 現在の地上 route/generator 編集内容を元テキストへ保存する処理
    //-------------------------------------------------------------------------------
    private async Task SaveCurrentFieldMapFilesAsync()
    {
        if (_currentFieldMapData is null)
        {
            AppendLog("保存対象の地上マップが見つからないため保存できません．");
            return;
        }

        DialogResult result = MessageBox.Show(
            this,
            "現在の route と generator の編集内容を地上 map フォルダの txt へ上書きします．\n続行しますか？",
            "地上マップ保存確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            AppendLog("地上マップ保存をキャンセルしました．");
            return;
        }

        try
        {
            SetBusyState(true, "地上マップを保存しています");
            FieldMapData saveData = ApplyDisplayLayoutToFieldGenerators(_currentFieldMapData, _currentLayout);
            await Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(_currentRoutePath))
                {
                    RouteSerializer.WriteFile(_currentRoutePath, _currentRoute);
                }

                foreach (FieldGeneratorFile generatorFile in saveData.GeneratorFiles)
                {
                    FieldGeneratorParser.WriteFile(generatorFile);
                }
            });

            _currentFieldMapData = FieldMapLoader.Load(
                saveData.MapDirectory,
                GetCurrentFieldTextsRoot(),
                saveData.Name);
            RefreshFieldGeneratorDisplayObjects(resetSelection: false);
            _currentRoute = RouteParser.ParseFile(_currentRoutePath ?? string.Empty);
            UpdateAllPreviewOverlays();
            RefreshUnitSummary();
            AppendLog($"地上マップ保存完了: map={saveData.Name}, route={_currentRoute.Waypoints.Count}, generators={_currentLayout.Spawns.Count}");
            MessageBox.Show(this, "地上マップの route/generator を保存しました．", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog($"地上マップ保存失敗: {ex.Message}");
            MessageBox.Show(this, $"地上マップ保存に失敗しました．\n{ex.Message}", "保存失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusyState(false, "待機中です");
        }
    }

    //-------------------------------------------------------------------------------
    // Console の内容を exe と同じ階層へ出力する処理
    //-------------------------------------------------------------------------------
    private void buttonExportLog_Click(object? sender, EventArgs e)
    {
        string unitName = SanitizeFileName(_currentPreviewUnitName ?? "unit");
        string fileName = $"[{DateTime.Now:yyyyMMddHHmmss}]_{unitName}_log.txt";
        string outputPath = Path.Combine(AppContext.BaseDirectory, fileName);
        try
        {
            File.WriteAllText(outputPath, richTextBoxConsole.Text);
            MessageBox.Show(this, $"ログを出力しました．\n{outputPath}", "ログ出力", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"ログ出力に失敗しました．\n{ex.Message}", "ログ出力", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    //-------------------------------------------------------------------------------
    // ファイル名に使えない文字を置換する処理
    //-------------------------------------------------------------------------------
    private static string SanitizeFileName(string text)
    {
        string sanitized = text;
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "unit" : sanitized;
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の waypoint 選択変更をフォームへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_RouteWaypointSelectionChanged(object? sender, RouteWaypointSelectionChangedEventArgs e)
    {
        _selectedRouteWaypointIndex = e.WaypointIndex;
        if (e.WaypointIndex is not null)
        {
            _selectedSpawnIndex = null;
            _selectedWaterboxIndex = null;
        }
        _objModelView.SelectRouteWaypoint(e.WaypointIndex);
        if (e.WaypointIndex is not null)
        {
            _objModelView.SelectSpawn(null);
            _objModelView.SelectWaterbox(null);
            SwitchQuickToolTargetFromSelection(QuickToolTarget.Route);
        }
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の spawn 選択変更をフォームへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_LayoutSpawnSelectionChanged(object? sender, LayoutSpawnSelectionChangedEventArgs e)
    {
        _selectedSpawnIndex = e.SpawnIndex;
        if (e.SpawnIndex is not null)
        {
            _selectedRouteWaypointIndex = null;
            _selectedWaterboxIndex = null;
        }
        _objModelView.SelectSpawn(e.SpawnIndex);
        if (e.SpawnIndex is not null)
        {
            _objModelView.SelectRouteWaypoint(null);
            _objModelView.SelectWaterbox(null);
            SwitchQuickToolTargetFromSelection(QuickToolTarget.Spawn);
        }
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の waypoint 移動量を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_RouteWaypointMoved(object? sender, RouteWaypointMovedEventArgs e)
    {
        if (!_currentRoute.Waypoints.TryGetValue(e.WaypointIndex, out RouteWaypoint? waypoint))
        {
            return;
        }

        float movedX = waypoint.X + e.DeltaX;
        float movedZ = waypoint.Z + e.DeltaZ;
        float movedY = e.MovesHeight
            ? waypoint.Y + e.DeltaY
            : GetGroundHeightOrFallback(movedX, movedZ, waypoint.Y);

        RecordUndoSnapshotForEditChange();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        updatedWaypoints[e.WaypointIndex] = waypoint with
        {
            X = movedX,
            Y = movedY,
            Z = movedZ
        };
        _currentRoute = new RouteFile(updatedWaypoints);
        UpdateAllPreviewOverlays();
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー上のドラッグで変更された Waypoint Radius を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_RouteWaypointRadiusChanged(object? sender, RouteWaypointRadiusChangedEventArgs e)
    {
        if (!_currentRoute.Waypoints.TryGetValue(e.WaypointIndex, out RouteWaypoint? waypoint))
        {
            return;
        }

        float radius = Math.Max(0f, e.Radius);
        if (Math.Abs(waypoint.Radius - radius) < 0.01f)
        {
            return;
        }

        RecordUndoSnapshotForEditChange();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        updatedWaypoints[e.WaypointIndex] = waypoint with
        {
            Radius = radius
        };
        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = e.WaypointIndex;
        _selectedSpawnIndex = null;
        UpdateAllPreviewOverlays();
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の waypoint 接続操作を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_RouteWaypointLinked(object? sender, RouteWaypointLinkedEventArgs e)
    {
        if (e.FromWaypointIndex == e.ToWaypointIndex ||
            !_currentRoute.Waypoints.TryGetValue(e.FromWaypointIndex, out RouteWaypoint? fromWaypoint) ||
            !_currentRoute.Waypoints.ContainsKey(e.ToWaypointIndex))
        {
            return;
        }

        if (fromWaypoint.Links.Contains(e.ToWaypointIndex))
        {
            AppendLog($"route 接続済み: {e.FromWaypointIndex} -> {e.ToWaypointIndex}");
            return;
        }

        RecordUndoSnapshot();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        updatedWaypoints[e.FromWaypointIndex] = fromWaypoint with
        {
            Links = AddRouteLink(fromWaypoint.Links, e.ToWaypointIndex)
        };

        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = e.ToWaypointIndex;
        _selectedSpawnIndex = null;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        AppendLog($"route 接続追加: {e.FromWaypointIndex} -> {e.ToWaypointIndex}");
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の route 削除操作を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_RouteWaypointLinkDeleted(object? sender, RouteWaypointLinkedEventArgs e)
    {
        if (!_currentRoute.Waypoints.TryGetValue(e.FromWaypointIndex, out RouteWaypoint? fromWaypoint) ||
            !fromWaypoint.Links.Contains(e.ToWaypointIndex))
        {
            return;
        }

        RecordUndoSnapshot();
        Dictionary<int, RouteWaypoint> updatedWaypoints = _currentRoute.Waypoints
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        updatedWaypoints[e.FromWaypointIndex] = fromWaypoint with
        {
            Links = fromWaypoint.Links.Where(link => link != e.ToWaypointIndex).ToList()
        };

        _currentRoute = new RouteFile(updatedWaypoints);
        _selectedRouteWaypointIndex = e.FromWaypointIndex;
        _selectedSpawnIndex = null;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshUnitSummary();
        AppendLog($"route 接続削除: {e.FromWaypointIndex} -> {e.ToWaypointIndex}");
    }

    //-------------------------------------------------------------------------------
    // route 接続先を重複なしで追加する処理
    //-------------------------------------------------------------------------------
    private static List<int> AddRouteLink(IReadOnlyList<int> links, int targetIndex)
    {
        List<int> updatedLinks = links.ToList();
        if (!updatedLinks.Contains(targetIndex))
        {
            updatedLinks.Add(targetIndex);
        }

        updatedLinks.Sort();
        return updatedLinks;
    }

    //-------------------------------------------------------------------------------
    // マップビュー側の spawn 移動量を layout データへ反映する処理
    //-------------------------------------------------------------------------------
    private void UnitMapView_LayoutSpawnMoved(object? sender, LayoutSpawnMovedEventArgs e)
    {
        if (e.SpawnIndex < 0 || e.SpawnIndex >= _currentLayout.Spawns.Count)
        {
            return;
        }

        List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
        LayoutSpawn spawn = updatedSpawns[e.SpawnIndex];
        float movedX = spawn.X + e.DeltaX;
        float movedZ = spawn.Z + e.DeltaZ;
        float movedY = GetGroundHeightOrFallback(movedX, movedZ, spawn.Y);
        RecordUndoSnapshotForEditChange();
        updatedSpawns[e.SpawnIndex] = spawn with
        {
            X = movedX,
            Y = movedY,
            Z = movedZ
        };
        _currentLayout = new LayoutFile(updatedSpawns);
        UpdateAllPreviewOverlays();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー上のドラッグで変更された Spawn 角度を layout データへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_LayoutSpawnAngleChanged(object? sender, LayoutSpawnAngleChangedEventArgs e)
    {
        if (e.SpawnIndex < 0 || e.SpawnIndex >= _currentLayout.Spawns.Count)
        {
            return;
        }

        LayoutSpawn spawn = _currentLayout.Spawns[e.SpawnIndex];
        if (Math.Abs(spawn.Angle - e.Angle) < 0.01f)
        {
            return;
        }

        List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
        RecordUndoSnapshotForEditChange();
        updatedSpawns[e.SpawnIndex] = spawn with
        {
            Angle = e.Angle
        };
        _currentLayout = new LayoutFile(updatedSpawns);
        _selectedSpawnIndex = e.SpawnIndex;
        _selectedRouteWaypointIndex = null;
        UpdateAllPreviewOverlays();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー上のドラッグで変更された Spawn Radius を layout データへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_LayoutSpawnRadiusChanged(object? sender, LayoutSpawnRadiusChangedEventArgs e)
    {
        if (e.SpawnIndex < 0 || e.SpawnIndex >= _currentLayout.Spawns.Count)
        {
            return;
        }

        LayoutSpawn spawn = _currentLayout.Spawns[e.SpawnIndex];
        float radius = Math.Max(0f, e.Radius);
        if (Math.Abs(spawn.Radius - radius) < 0.01f)
        {
            return;
        }

        List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
        RecordUndoSnapshotForEditChange();
        updatedSpawns[e.SpawnIndex] = spawn with
        {
            Radius = radius
        };
        _currentLayout = new LayoutFile(updatedSpawns);
        _selectedSpawnIndex = e.SpawnIndex;
        _selectedRouteWaypointIndex = null;
        UpdateAllPreviewOverlays();
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー上のクリック位置へ Spawn または Waypoint を追加する処理
    //-------------------------------------------------------------------------------
    private void Preview_MapPointPlacementRequested(object? sender, MapPointPlacementRequestedEventArgs e)
    {
        if (GetCurrentMode() == EditorMode.Field)
        {
            if (e.EditMode == UnitMapEditMode.AddSpawn)
            {
                AddFieldObjectAt(e.X, e.Z);
                return;
            }

            if (e.EditMode == UnitMapEditMode.AddRouteWaypoint)
            {
                AddRouteWaypointAt(e.X, e.Z);
            }

            return;
        }

        if (GetCurrentMode() != EditorMode.Cave)
        {
            return;
        }

        if (e.EditMode == UnitMapEditMode.AddSpawn)
        {
            List<LayoutSpawn> updatedSpawns = _currentLayout.Spawns.ToList();
            LayoutSpawn newSpawn = CreateDefaultSpawnAt(e.X, e.Z);
            RecordUndoSnapshot();
            updatedSpawns.Add(newSpawn);
            int newSpawnIndex = updatedSpawns.Count - 1;
            _currentLayout = new LayoutFile(updatedSpawns);
            _selectedSpawnIndex = newSpawnIndex;
            _selectedRouteWaypointIndex = null;
            checkBoxSpawnOverlay.Checked = true;
            UpdateAllPreviewOverlays();
            _selectedSpawnIndex = newSpawnIndex;
            _selectedRouteWaypointIndex = null;
            SyncSelectedTargets();
            RefreshUnitSummary();
            AppendLog($"spawn 追加: index={newSpawnIndex}, x={newSpawn.X:0.###}, y={newSpawn.Y:0.###}, z={newSpawn.Z:0.###}");
            return;
        }

        if (e.EditMode == UnitMapEditMode.AddRouteWaypoint)
        {
            AddRouteWaypointAt(e.X, e.Z);
            return;
        }

        if (e.EditMode == UnitMapEditMode.AddWaterbox)
        {
            List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
            WaterboxEntry newBox = CreateDefaultWaterboxAt(e.X, e.Z);
            RecordUndoSnapshot();
            boxes.Add(newBox);
            int newWaterboxIndex = boxes.Count - 1;
            _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
            _selectedWaterboxIndex = newWaterboxIndex;
            _selectedSpawnIndex = null;
            _selectedRouteWaypointIndex = null;
            if (_checkBoxWaterboxOverlay is not null)
            {
                _checkBoxWaterboxOverlay.Checked = true;
            }

            UpdateAllPreviewOverlays();
            SyncSelectedTargets();
            RefreshInspector();
            RefreshUnitSummary();
            AppendLog($"waterbox 追加: index={newWaterboxIndex}, x1={newBox.X1:0.###}, z1={newBox.Z1:0.###}, x2={newBox.X2:0.###}, z2={newBox.Z2:0.###}");
        }
    }

    //-------------------------------------------------------------------------------
    // プレビュー上でクリックされた Spawn または Waypoint を削除する処理
    //-------------------------------------------------------------------------------
    private void Preview_MapPointDeletionRequested(object? sender, MapPointDeletionRequestedEventArgs e)
    {
        if (GetCurrentMode() == EditorMode.Field)
        {
            if (e.EditMode == UnitMapEditMode.DeleteSpawn)
            {
                DeleteFieldObjectAt(e.PointIndex);
                return;
            }

            if (e.EditMode == UnitMapEditMode.DeleteRouteWaypoint)
            {
                DeleteWaypointAt(e.PointIndex);
            }

            return;
        }

        if (GetCurrentMode() != EditorMode.Cave)
        {
            return;
        }

        if (e.EditMode == UnitMapEditMode.DeleteSpawn)
        {
            DeleteSpawnAt(e.PointIndex);
            return;
        }

        if (e.EditMode == UnitMapEditMode.DeleteRouteWaypoint)
        {
            DeleteWaypointAt(e.PointIndex);
            return;
        }

        if (e.EditMode == UnitMapEditMode.DeleteWaterbox)
        {
            DeleteWaterboxAt(e.PointIndex);
        }
    }

    //-------------------------------------------------------------------------------
    // クリック位置を中心に既定サイズの waterbox を作成する処理
    //-------------------------------------------------------------------------------
    private static WaterboxEntry CreateDefaultWaterboxAt(float x, float z)
    {
        const float halfSize = 85f;
        return new WaterboxEntry(x - halfSize, -100f, z - halfSize, x + halfSize, 0f, z + halfSize);
    }

    //-------------------------------------------------------------------------------
    // 指定 index の Waterbox を削除する処理
    //-------------------------------------------------------------------------------
    private void DeleteWaterboxAt(int waterboxIndex)
    {
        if (waterboxIndex < 0 || waterboxIndex >= _currentWaterbox.Boxes.Count)
        {
            return;
        }

        RecordUndoSnapshot();
        List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
        boxes.RemoveAt(waterboxIndex);
        _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
        _selectedWaterboxIndex = null;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
        AppendLog($"waterbox 削除: index={waterboxIndex}");
    }

    private float GetGroundHeightOrFallback(float x, float z, float fallbackY)
    {
        return TryGetGroundHeightFromObj(x, z, fallbackY, out float groundY) ? groundY : fallbackY;
    }

    private bool TryGetGroundHeightFromObj(float x, float z, float fallbackY, out float groundY)
    {
        groundY = 0f;
        if (_currentObjScene is null || _currentObjScene.Vertices.Count == 0 || _currentObjScene.Faces.Count == 0)
        {
            return false;
        }

        List<float> candidateDataHeights = new();
        foreach (ObjFace face in _currentObjScene.Faces)
        {
            if (face.Indices.Count < 3)
            {
                continue;
            }

            ObjVertex a = _currentObjScene.Vertices[face.Indices[0].Vertex];
            for (int i = 1; i + 1 < face.Indices.Count; i++)
            {
                ObjVertex b = _currentObjScene.Vertices[face.Indices[i].Vertex];
                ObjVertex c = _currentObjScene.Vertices[face.Indices[i + 1].Vertex];
                if (!TryGetTriangleHeightAtPoint(a, b, c, x, z, out float triangleY))
                {
                    continue;
                }

                candidateDataHeights.Add(triangleY - _objModelView.OverlayHeightOffset);
            }
        }

        if (candidateDataHeights.Count == 0)
        {
            return false;
        }

        groundY = candidateDataHeights
            .OrderBy(candidateY => Math.Abs(candidateY - fallbackY))
            .First();
        return true;
    }

    private static bool TryGetTriangleHeightAtPoint(ObjVertex a, ObjVertex b, ObjVertex c, float x, float z, out float y)
    {
        y = 0f;

        float denominator = ((b.Z - c.Z) * (a.X - c.X)) + ((c.X - b.X) * (a.Z - c.Z));
        if (Math.Abs(denominator) < 0.0001f)
        {
            return false;
        }

        float w0 = (((b.Z - c.Z) * (x - c.X)) + ((c.X - b.X) * (z - c.Z))) / denominator;
        float w1 = (((c.Z - a.Z) * (x - c.X)) + ((a.X - c.X) * (z - c.Z))) / denominator;
        float w2 = 1f - w0 - w1;
        const float epsilon = 0.0001f;
        if (w0 < -epsilon || w1 < -epsilon || w2 < -epsilon)
        {
            return false;
        }

        y = (a.Y * w0) + (b.Y * w1) + (c.Y * w2);
        return true;
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の waypoint 選択変更をフォームへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_RouteWaypointSelectionChanged(object? sender, RouteWaypointSelectionChangedEventArgs e)
    {
        _selectedRouteWaypointIndex = e.WaypointIndex;
        if (e.WaypointIndex is not null)
        {
            _selectedSpawnIndex = null;
            _selectedWaterboxIndex = null;
        }

        _unitMapView.SelectRouteWaypoint(e.WaypointIndex);
        if (e.WaypointIndex is not null)
        {
            _unitMapView.SelectSpawn(null);
            _unitMapView.SelectWaterbox(null);
            SwitchQuickToolTargetFromSelection(QuickToolTarget.Route);
        }

        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の spawn 選択変更をフォームへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_LayoutSpawnSelectionChanged(object? sender, LayoutSpawnSelectionChangedEventArgs e)
    {
        _selectedSpawnIndex = e.SpawnIndex;
        if (e.SpawnIndex is not null)
        {
            _selectedRouteWaypointIndex = null;
            _selectedWaterboxIndex = null;
        }

        _unitMapView.SelectSpawn(e.SpawnIndex);
        if (e.SpawnIndex is not null)
        {
            _unitMapView.SelectRouteWaypoint(null);
            _unitMapView.SelectWaterbox(null);
            SwitchQuickToolTargetFromSelection(QuickToolTarget.Spawn);
        }

        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー側の waterbox 選択変更をフォームへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_WaterboxSelectionChanged(object? sender, WaterboxSelectionChangedEventArgs e)
    {
        _selectedWaterboxIndex = e.WaterboxIndex;
        if (e.WaterboxIndex is not null)
        {
            _selectedSpawnIndex = null;
            _selectedRouteWaypointIndex = null;
            _unitMapView.SelectSpawn(null);
            _unitMapView.SelectRouteWaypoint(null);
            _objModelView.SelectSpawn(null);
            _objModelView.SelectRouteWaypoint(null);
            SwitchQuickToolTargetFromSelection(QuickToolTarget.Waterbox);
        }

        _unitMapView.SelectWaterbox(e.WaterboxIndex);
        _objModelView.SelectWaterbox(e.WaterboxIndex);
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー側の waterbox 移動量をデータへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_WaterboxMoved(object? sender, WaterboxMovedEventArgs e)
    {
        if (e.WaterboxIndex < 0 || e.WaterboxIndex >= _currentWaterbox.Boxes.Count)
        {
            return;
        }

        List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
        WaterboxEntry box = boxes[e.WaterboxIndex];
        RecordUndoSnapshotForEditChange();
        boxes[e.WaterboxIndex] = box with
        {
            X1 = box.X1 + e.DeltaX,
            Z1 = box.Z1 + e.DeltaZ,
            X2 = box.X2 + e.DeltaX,
            Z2 = box.Z2 + e.DeltaZ
        };
        _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
        _selectedWaterboxIndex = e.WaterboxIndex;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // プレビュー側の waterbox サイズ変更量をデータへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_WaterboxResized(object? sender, WaterboxResizedEventArgs e)
    {
        if (e.WaterboxIndex < 0 || e.WaterboxIndex >= _currentWaterbox.Boxes.Count)
        {
            return;
        }

        List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
        WaterboxEntry box = boxes[e.WaterboxIndex];
        float minX = box.MinX;
        float maxX = box.MaxX;
        float minZ = box.MinZ;
        float maxZ = box.MaxZ;

        if (e.Handle is WaterboxResizeHandle.Left or WaterboxResizeHandle.TopLeft or WaterboxResizeHandle.BottomLeft)
        {
            minX += e.DeltaX;
        }
        if (e.Handle is WaterboxResizeHandle.Right or WaterboxResizeHandle.TopRight or WaterboxResizeHandle.BottomRight)
        {
            maxX += e.DeltaX;
        }
        if (e.Handle is WaterboxResizeHandle.Top or WaterboxResizeHandle.TopLeft or WaterboxResizeHandle.TopRight)
        {
            minZ += e.DeltaZ;
        }
        if (e.Handle is WaterboxResizeHandle.Bottom or WaterboxResizeHandle.BottomLeft or WaterboxResizeHandle.BottomRight)
        {
            maxZ += e.DeltaZ;
        }

        NormalizeMinimumWaterboxSize(ref minX, ref maxX, ref minZ, ref maxZ);
        RecordUndoSnapshotForEditChange();
        boxes[e.WaterboxIndex] = new WaterboxEntry(minX, box.MinY, minZ, maxX, box.MaxY, maxZ);
        _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
        _selectedWaterboxIndex = e.WaterboxIndex;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の waterbox 高さ移動量をデータへ反映する処理
    //-------------------------------------------------------------------------------
    private void Preview_WaterboxHeightMoved(object? sender, WaterboxHeightMovedEventArgs e)
    {
        if (e.WaterboxIndex < 0 || e.WaterboxIndex >= _currentWaterbox.Boxes.Count)
        {
            return;
        }

        List<WaterboxEntry> boxes = _currentWaterbox.Boxes.ToList();
        WaterboxEntry box = boxes[e.WaterboxIndex];
        RecordUndoSnapshotForEditChange();
        boxes[e.WaterboxIndex] = box with
        {
            Y1 = box.Y1 + e.DeltaY,
            Y2 = box.Y2 + e.DeltaY
        };
        _currentWaterbox = new WaterboxFile(_currentWaterbox.Type, boxes);
        _selectedWaterboxIndex = e.WaterboxIndex;
        UpdateAllPreviewOverlays();
        SyncSelectedTargets();
        RefreshInspector();
        RefreshUnitSummary();
    }

    private static void NormalizeMinimumWaterboxSize(ref float minX, ref float maxX, ref float minZ, ref float maxZ)
    {
        const float minSize = 8f;
        if (minX > maxX)
        {
            (minX, maxX) = (maxX, minX);
        }
        if (minZ > maxZ)
        {
            (minZ, maxZ) = (maxZ, minZ);
        }
        if (maxX - minX < minSize)
        {
            float center = (minX + maxX) * 0.5f;
            minX = center - (minSize * 0.5f);
            maxX = center + (minSize * 0.5f);
        }
        if (maxZ - minZ < minSize)
        {
            float center = (minZ + maxZ) * 0.5f;
            minZ = center - (minSize * 0.5f);
            maxZ = center + (minSize * 0.5f);
        }
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の waypoint 移動量を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_RouteWaypointMoved(object? sender, RouteWaypointMovedEventArgs e)
    {
        UnitMapView_RouteWaypointMoved(sender, e);
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の waypoint 接続操作を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_RouteWaypointLinked(object? sender, RouteWaypointLinkedEventArgs e)
    {
        UnitMapView_RouteWaypointLinked(sender, e);
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の route 削除操作を route データへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_RouteWaypointLinkDeleted(object? sender, RouteWaypointLinkedEventArgs e)
    {
        UnitMapView_RouteWaypointLinkDeleted(sender, e);
    }

    //-------------------------------------------------------------------------------
    // 3D ビュー側の spawn 移動量を layout データへ反映する処理
    //-------------------------------------------------------------------------------
    private void ObjModelView_LayoutSpawnMoved(object? sender, LayoutSpawnMovedEventArgs e)
    {
        UnitMapView_LayoutSpawnMoved(sender, e);
    }

    //-------------------------------------------------------------------------------
    // 編集系ボタンの状態を現在モードへ合わせて更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateRouteEditUi()
    {
        bool isCave = GetCurrentMode() == EditorMode.Cave;
        buttonSpawnMoveMode.Visible = isCave;
        buttonSaveLayout.Visible = isCave;
        buttonRouteMoveMode.Visible = isCave;
        buttonSaveRoute.Visible = isCave;
        buttonSpawnMoveMode.Text = _currentEditMode == UnitMapEditMode.MoveSpawn
            ? Localize("SpawnMoveOn")
            : Localize("SpawnMoveOff");
        buttonSpawnMoveMode.BackColor = _currentEditMode == UnitMapEditMode.MoveSpawn
            ? Color.Honeydew
            : SystemColors.Control;
        buttonRouteMoveMode.Text = _currentEditMode == UnitMapEditMode.MoveRouteWaypoint
            ? Localize("WaypointMoveOn")
            : Localize("WaypointMoveOff");
        buttonRouteMoveMode.BackColor = _currentEditMode == UnitMapEditMode.MoveRouteWaypoint
            ? Color.Honeydew
            : SystemColors.Control;
        if (_buttonAddSpawn is not null)
        {
            _buttonAddSpawn.Text = _currentEditMode == UnitMapEditMode.AddSpawn
                ? Localize("SpawnAddOn")
                : Localize("SpawnAdd");
            _buttonAddSpawn.BackColor = _currentEditMode == UnitMapEditMode.AddSpawn
                ? Color.Honeydew
                : SystemColors.Control;
        }

        if (_buttonAddWaypoint is not null)
        {
            _buttonAddWaypoint.Text = _currentEditMode == UnitMapEditMode.AddRouteWaypoint
                ? Localize("WaypointAddOn")
                : Localize("WaypointAdd");
            _buttonAddWaypoint.BackColor = _currentEditMode == UnitMapEditMode.AddRouteWaypoint
                ? Color.Honeydew
                : SystemColors.Control;
        }

        buttonSpawnMoveMode.Enabled = isCave;
        buttonRouteMoveMode.Enabled = isCave;
        buttonSaveLayout.Enabled = isCave && !string.IsNullOrWhiteSpace(_currentLayoutPath);
        buttonSaveRoute.Enabled = isCave && !string.IsNullOrWhiteSpace(_currentRoutePath);
    }

    //-------------------------------------------------------------------------------
    // 現在の選択状態を Unit Summary 欄へ反映する処理
    //-------------------------------------------------------------------------------
    private void RefreshUnitSummary()
    {
        if (GetCurrentMode() != EditorMode.Cave)
        {
            List<string> fieldLines = new();
            fieldLines.Add($"Field Map: {_currentPreviewUnitName ?? "-"}");
            fieldLines.Add($"map: {(_currentFieldMapData?.MapDirectory ?? GetCurrentFieldMapRoot() ?? "-")}");
            fieldLines.Add($"route: {(_currentRoutePath ?? "-")}");
            fieldLines.Add($"texts: {(_currentWaterboxPath ?? "-")}");
            fieldLines.Add($"day: {_currentFieldDay}");
            fieldLines.Add($"generator files: {(_currentFieldMapData?.GeneratorFiles.Count ?? 0)}");
            fieldLines.Add($"active generator objects: {_currentLayout.Spawns.Count}");
            fieldLines.Add($"route waypoints: {_currentRoute.Waypoints.Count}");

            if (_currentFieldMapData is not null)
            {
                fieldLines.Add(string.Empty);
                foreach (FieldGeneratorFile generatorFile in _currentFieldMapData.GeneratorFiles)
                {
                    string activeMark = IsFieldGeneratorFileActive(generatorFile, _currentFieldDay, _currentFieldMapData.GeneratorFiles)
                        ? "* "
                        : "  ";
                    string countText = generatorFile.DeclaredObjectCount > 0
                        ? $"{generatorFile.Objects.Count}/{generatorFile.DeclaredObjectCount}"
                        : generatorFile.Objects.Count.ToString(CultureInfo.InvariantCulture);
                    fieldLines.Add($"{activeMark}{generatorFile.DisplayName}: {countText}");
                }
            }

            fieldLines.Add(string.Empty);
            fieldLines.Add("地上 generator は経過日数に一致するファイルだけを表示・編集します．");
            fieldLines.Add("地上 generator は既存 object の移動，角度，Radius 編集に対応しています．");
            fieldLines.Add("新規追加と削除は generator 型の既定値整備後に有効化します．");

            _currentSummaryLines = fieldLines;
            RefreshConsoleOutput();
            UpdateRouteEditUi();
            RefreshInspector();
            return;
        }

        List<string> lines = new();
        lines.Add($"Unit: {_currentPreviewUnitName ?? "-"}");
        lines.Add($"Image: {_currentPreviewImagePath ?? "-"}");
        lines.Add($"obj: {(_currentObjPath ?? "-")}");
        lines.Add($"layout: {(_currentLayoutPath ?? "-")}");
        lines.Add($"route: {(_currentRoutePath ?? "-")}");
        lines.Add($"waterbox: {(_currentWaterboxPath ?? "-")}");
        lines.Add($"mode: {_currentEditMode}");
        lines.Add($"view: {(IsObjDirectViewEnabled() ? "OBJ 3D" : "TopDown 2D")}");

        if (_selectedSpawnIndex is not null &&
            _selectedSpawnIndex.Value >= 0 &&
            _selectedSpawnIndex.Value < _currentLayout.Spawns.Count)
        {
            LayoutSpawn spawn = _currentLayout.Spawns[_selectedSpawnIndex.Value];
            lines.Add(string.Empty);
            lines.Add($"Selected Spawn: {_selectedSpawnIndex.Value}");
            lines.Add($"Type: {spawn.TypeLabel} ({spawn.TypeId})");
            lines.Add($"X: {spawn.X:0.###}");
            lines.Add($"Y: {spawn.Y:0.###}");
            lines.Add($"Z: {spawn.Z:0.###}");
            lines.Add($"Angle: {spawn.Angle:0.###}");
            lines.Add($"Radius: {spawn.Radius:0.###}");
            lines.Add($"Count: {spawn.MinCount} - {spawn.MaxCount}");
        }

        if (_selectedRouteWaypointIndex is not null &&
            _currentRoute.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint))
        {
            lines.Add(string.Empty);
            lines.Add($"Selected Waypoint: {waypoint.Index}");
            lines.Add($"X: {waypoint.X:0.###}");
            lines.Add($"Y: {waypoint.Y:0.###}");
            lines.Add($"Z: {waypoint.Z:0.###}");
            lines.Add($"Radius: {waypoint.Radius:0.###}");
            lines.Add($"Links: {(waypoint.Links.Count == 0 ? "-" : string.Join(", ", waypoint.Links))}");
        }
        else if (_selectedWaterboxIndex is not null &&
            _selectedWaterboxIndex.Value >= 0 &&
            _selectedWaterboxIndex.Value < _currentWaterbox.Boxes.Count)
        {
            WaterboxEntry box = _currentWaterbox.Boxes[_selectedWaterboxIndex.Value];
            lines.Add(string.Empty);
            lines.Add($"Selected Waterbox: {_selectedWaterboxIndex.Value}");
            lines.Add($"X: {box.MinX:0.###} - {box.MaxX:0.###}");
            lines.Add($"Y: {box.MinY:0.###} - {box.MaxY:0.###}");
            lines.Add($"Z: {box.MinZ:0.###} - {box.MaxZ:0.###}");
        }
        else
        {
            lines.Add(string.Empty);
            lines.Add("Selected Target: -");
            if (IsObjDirectViewEnabled())
            {
                lines.Add("OBJ 3D表示中です．");
                lines.Add("左ドラッグで回転，右ドラッグで移動，ホイールでズームできます．");
            }
            else
            {
                lines.Add("左クリックで spawn / waypoint を選択できます．");
                lines.Add("各移動モード ON 時はドラッグで位置を動かせます．");
            }
        }

        _currentSummaryLines = lines;
        RefreshConsoleOutput();
        UpdateRouteEditUi();
        RefreshInspector();
    }

    private sealed record FloorComboItem(FloorInfo Floor)
    {
        public override string ToString()
        {
            return $"{Floor.FloorIndex + 1:D2}: rooms={Floor.RoomCount}, set={Floor.UnitSetFile}";
        }
    }

    private sealed record SpawnTypeItem(int TypeId, string Label)
    {
        public override string ToString()
        {
            return $"{Label} ({TypeId})";
        }
    }

    private sealed record EditorSnapshot(
        LayoutFile Layout,
        RouteFile Route,
        WaterboxFile Waterbox,
        int? SelectedSpawnIndex,
        int? SelectedWaypointIndex,
        int? SelectedWaterboxIndex);

    private sealed record DoorSnapPoint(int DoorIndex, float X, float Z);

    private sealed record FieldDisplayObjectRef(int FileIndex, int ObjectIndex);

    private sealed record FieldGeneratorFileItem(int FileIndex, string DisplayName)
    {
        public override string ToString()
        {
            return DisplayName;
        }
    }

    private sealed record FieldAddTemplateItem(FieldAddTemplateKind Kind, string Label)
    {
        public override string ToString()
        {
            return Label;
        }
    }

    private sealed record LanguageItem(string Code, string Label)
    {
        public override string ToString()
        {
            return Label;
        }
    }

    private sealed record ConsoleEntry(string Text, ConsoleColorKind ColorKind);

    private enum ConsoleColorKind
    {
        Normal,
        Info,
        Ok,
        Error
    }

    private enum QuickToolTarget
    {
        Spawn,
        Route,
        Waterbox
    }

    private enum LoadFormatKind
    {
        None,
        DiscExtractData,
        ArcFilesFolder,
        DirectFiles
    }

    private enum FieldAddTemplateKind
    {
        Teki,
        Item,
        Pikmin,
        CaveEntrance
    }
}

internal enum EditorMode
{
    Field,
    Cave
}
