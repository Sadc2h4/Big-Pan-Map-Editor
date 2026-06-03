using System.Drawing.Drawing2D;

namespace PikminUnitEditor;

internal sealed class UnitMapView : Control
{
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 25f;
    private const float RouteHitRadiusPixels = 14f;
    private const float SpawnHitRadiusPixels = 14f;
    private const float SpawnAngleHandleDistancePixels = 25f;
    private const float SpawnAngleHandleSizePixels = 10f;
    private const float GridStepWorld = 170f;
    private static Cursor? s_deleteCursor;

    private bool _leftButtonDown;
    private bool _rightButtonDown;
    private bool _draggingRouteWaypoint;
    private bool _draggingSpawn;
    private bool _draggingWaterbox;
    private bool _resizingWaterbox;
    private bool _rotatingSpawn;
    private bool _resizingPointRadius;
    private WaterboxResizeHandle _waterboxResizeHandle;
    private int? _linkingRouteWaypointIndex;
    private PointF _linkPreviewWorldPoint;
    private Point _lastMouse;
    private float _baseScale = 1f;
    private float _zoom = 1f;
    private PointF _panWorld = PointF.Empty;
    private RectangleF _modelBounds = RectangleF.Empty;
    private Image? _terrainImage;
    private LayoutFile _layout = new(Array.Empty<LayoutSpawn>());
    private RouteFile _route = new(new Dictionary<int, RouteWaypoint>());
    private IReadOnlyDictionary<int, float> _routeColorHeights = new Dictionary<int, float>();
    private WaterboxFile _waterbox = new(0, Array.Empty<WaterboxEntry>());
    private UnitMapEditMode _editMode;
    private bool _showRadius = true;
    private bool _useFieldObjectIcons;
    private bool _englishUi;
    private string _viewTitle = "洞窟ユニットモード";
    private int? _selectedRouteWaypointIndex;
    private int? _selectedSpawnIndex;
    private int? _selectedWaterboxIndex;

    public event EventHandler<RouteWaypointSelectionChangedEventArgs>? RouteWaypointSelectionChanged;
    public event EventHandler<RouteWaypointMovedEventArgs>? RouteWaypointMoved;
    public event EventHandler<RouteWaypointLinkedEventArgs>? RouteWaypointLinked;
    public event EventHandler<RouteWaypointLinkedEventArgs>? RouteWaypointLinkDeleted;
    public event EventHandler<LayoutSpawnSelectionChangedEventArgs>? LayoutSpawnSelectionChanged;
    public event EventHandler<LayoutSpawnMovedEventArgs>? LayoutSpawnMoved;
    public event EventHandler<LayoutSpawnAngleChangedEventArgs>? LayoutSpawnAngleChanged;
    public event EventHandler<LayoutSpawnRadiusChangedEventArgs>? LayoutSpawnRadiusChanged;
    public event EventHandler<RouteWaypointRadiusChangedEventArgs>? RouteWaypointRadiusChanged;
    public event EventHandler<WaterboxSelectionChangedEventArgs>? WaterboxSelectionChanged;
    public event EventHandler<WaterboxMovedEventArgs>? WaterboxMoved;
    public event EventHandler<WaterboxResizedEventArgs>? WaterboxResized;
    public event EventHandler? OverlayDragStarted;
    public event EventHandler? OverlayDragEnded;
    public event EventHandler<MapPointPlacementRequestedEventArgs>? MapPointPlacementRequested;
    public event EventHandler<MapPointDeletionRequestedEventArgs>? MapPointDeletionRequested;

    public UnitMapView()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(232, 227, 213);
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    //-------------------------------------------------------------------------------
    // 地形画像と実ワールド範囲を設定する処理
    //-------------------------------------------------------------------------------
    public void SetScene(Image? terrainImage, RectangleF modelBounds, LayoutFile layout, RouteFile route, WaterboxFile waterbox, bool resetView = true)
    {
        _terrainImage?.Dispose();
        _terrainImage = terrainImage is null ? null : (Image)terrainImage.Clone();
        _modelBounds = modelBounds;
        _layout = layout;
        _route = route;
        _waterbox = waterbox;
        if (_selectedRouteWaypointIndex is not null &&
            !_route.Waypoints.ContainsKey(_selectedRouteWaypointIndex.Value))
        {
            SetSelectedRouteWaypoint(null);
        }

        if (_selectedSpawnIndex is not null &&
            (_selectedSpawnIndex.Value < 0 || _selectedSpawnIndex.Value >= _layout.Spawns.Count))
        {
            SetSelectedSpawn(null);
        }

        if (_selectedWaterboxIndex is not null &&
            (_selectedWaterboxIndex.Value < 0 || _selectedWaterboxIndex.Value >= _waterbox.Boxes.Count))
        {
            SetSelectedWaterbox(null);
        }

        if (resetView)
        {
            ResetView();
        }

        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // route/layout のみ差し替えて現在の視点状態を維持する処理
    //-------------------------------------------------------------------------------
    public void UpdateOverlayData(LayoutFile layout, RouteFile route, WaterboxFile waterbox)
    {
        _layout = layout;
        _route = route;
        _waterbox = waterbox;
        if (_selectedRouteWaypointIndex is not null &&
            !_route.Waypoints.ContainsKey(_selectedRouteWaypointIndex.Value))
        {
            SetSelectedRouteWaypoint(null);
        }

        if (_selectedSpawnIndex is not null &&
            (_selectedSpawnIndex.Value < 0 || _selectedSpawnIndex.Value >= _layout.Spawns.Count))
        {
            SetSelectedSpawn(null);
        }

        if (_selectedWaterboxIndex is not null &&
            (_selectedWaterboxIndex.Value < 0 || _selectedWaterboxIndex.Value >= _waterbox.Boxes.Count))
        {
            SetSelectedWaterbox(null);
        }

        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 現在の編集モードを切り替える処理
    //-------------------------------------------------------------------------------
    public void SetEditMode(UnitMapEditMode mode)
    {
        _editMode = mode;
        _draggingRouteWaypoint = false;
        _draggingSpawn = false;
        _draggingWaterbox = false;
        _resizingWaterbox = false;
        _rotatingSpawn = false;
        _resizingPointRadius = false;
        _waterboxResizeHandle = WaterboxResizeHandle.None;
        _linkingRouteWaypointIndex = null;
        Cursor = GetCursorForEditMode(mode);
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // Radius 円の表示状態を切り替える処理
    //-------------------------------------------------------------------------------
    public void SetRadiusVisible(bool visible)
    {
        if (_showRadius == visible)
        {
            return;
        }

        _showRadius = visible;
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // route 矢印の色分けに使う表示用 Y 値を設定する処理
    //-------------------------------------------------------------------------------
    public void SetRouteColorHeights(IReadOnlyDictionary<int, float> routeColorHeights)
    {
        _routeColorHeights = routeColorHeights;
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 2D ビュー内に描画する文言の言語を切り替える処理
    //-------------------------------------------------------------------------------
    public void SetLanguage(bool english)
    {
        if (_englishUi == english)
        {
            return;
        }

        _englishUi = english;
        if (string.Equals(_viewTitle, "洞窟ユニットモード", StringComparison.Ordinal) ||
            string.Equals(_viewTitle, "Cave Unit Mode", StringComparison.Ordinal))
        {
            _viewTitle = _englishUi ? "Cave Unit Mode" : "洞窟ユニットモード";
        }

        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 2D ビュー左上に表示するモード名を切り替える処理
    //-------------------------------------------------------------------------------
    public void SetViewTitle(string title)
    {
        string nextTitle = string.IsNullOrWhiteSpace(title)
            ? (_englishUi ? "Cave Unit Mode" : "洞窟ユニットモード")
            : title;
        if (string.Equals(_viewTitle, nextTitle, StringComparison.Ordinal))
        {
            return;
        }

        _viewTitle = nextTitle;
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 地上 object 用アイコンを優先するかを切り替える処理
    //-------------------------------------------------------------------------------
    public void SetUseFieldObjectIcons(bool useFieldObjectIcons)
    {
        if (_useFieldObjectIcons == useFieldObjectIcons)
        {
            return;
        }

        _useFieldObjectIcons = useFieldObjectIcons;
        Invalidate();
    }

    public void SelectRouteWaypoint(int? waypointIndex)
    {
        if (waypointIndex is not null && !_route.Waypoints.ContainsKey(waypointIndex.Value))
        {
            waypointIndex = null;
        }

        SetSelectedRouteWaypoint(waypointIndex);
        if (waypointIndex is not null)
        {
            SetSelectedSpawn(null);
            SetSelectedWaterbox(null);
        }
    }

    public void SelectSpawn(int? spawnIndex)
    {
        if (spawnIndex is not null &&
            (spawnIndex.Value < 0 || spawnIndex.Value >= _layout.Spawns.Count))
        {
            spawnIndex = null;
        }

        SetSelectedSpawn(spawnIndex);
        if (spawnIndex is not null)
        {
            SetSelectedRouteWaypoint(null);
            SetSelectedWaterbox(null);
        }
    }

    //-------------------------------------------------------------------------------
    // 選択中 waterbox を外部から指定する処理
    //-------------------------------------------------------------------------------
    public void SelectWaterbox(int? waterboxIndex)
    {
        if (waterboxIndex is not null &&
            (waterboxIndex.Value < 0 || waterboxIndex.Value >= _waterbox.Boxes.Count))
        {
            waterboxIndex = null;
        }

        SetSelectedWaterbox(waterboxIndex);
        if (waterboxIndex is not null)
        {
            SetSelectedRouteWaypoint(null);
            SetSelectedSpawn(null);
        }
    }

    //-------------------------------------------------------------------------------
    // ビュー操作状態を初期値へ戻す処理
    //-------------------------------------------------------------------------------
    public void ResetView()
    {
        _zoom = 1f;
        _panWorld = PointF.Empty;
        _baseScale = CalculateBaseScale();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _baseScale = CalculateBaseScale();
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _lastMouse = e.Location;
        if (e.Button == MouseButtons.Left)
        {
            _leftButtonDown = true;
            Capture = true;
            if (_editMode == UnitMapEditMode.DeleteRouteLink &&
                HitTestRouteLink(e.Location) is { } hitLink)
            {
                RouteWaypointLinkDeleted?.Invoke(
                    this,
                    new RouteWaypointLinkedEventArgs(hitLink.From, hitLink.To));
                SetSelectedRouteWaypoint(hitLink.From);
                SetSelectedSpawn(null);
                _leftButtonDown = false;
                Capture = false;
                return;
            }

            if (_editMode == UnitMapEditMode.AddSpawn ||
                _editMode == UnitMapEditMode.AddRouteWaypoint ||
                _editMode == UnitMapEditMode.AddWaterbox)
            {
                PointF worldPoint = ScreenToWorld(e.Location);
                MapPointPlacementRequested?.Invoke(
                    this,
                    new MapPointPlacementRequestedEventArgs(_editMode, worldPoint.X, worldPoint.Y));
                _leftButtonDown = false;
                Capture = false;
                return;
            }

            int? hitWaypoint = HitTestRouteWaypoint(e.Location);
            int? hitSpawn = hitWaypoint is null ? HitTestLayoutSpawn(e.Location) : null;
            WaterboxResizeHandle hitWaterboxHandle = HitTestWaterboxHandle(e.Location, out int? hitHandleWaterbox);
            int? hitWaterbox = hitWaypoint is null && hitSpawn is null ? HitTestWaterbox(e.Location) : null;
            if (_editMode == UnitMapEditMode.DeleteRouteWaypoint)
            {
                if (hitWaypoint is not null)
                {
                    MapPointDeletionRequested?.Invoke(
                        this,
                        new MapPointDeletionRequestedEventArgs(_editMode, hitWaypoint.Value));
                }

                _leftButtonDown = false;
                Capture = false;
                return;
            }

            if (_editMode == UnitMapEditMode.DeleteSpawn)
            {
                if (hitSpawn is not null)
                {
                    MapPointDeletionRequested?.Invoke(
                        this,
                        new MapPointDeletionRequestedEventArgs(_editMode, hitSpawn.Value));
                }

                _leftButtonDown = false;
                Capture = false;
                return;
            }

            if (_editMode == UnitMapEditMode.DeleteWaterbox)
            {
                if (hitWaterbox is not null)
                {
                    MapPointDeletionRequested?.Invoke(
                        this,
                        new MapPointDeletionRequestedEventArgs(_editMode, hitWaterbox.Value));
                }

                _leftButtonDown = false;
                Capture = false;
                return;
            }

            if (hitWaypoint is not null)
            {
                SetSelectedRouteWaypoint(hitWaypoint);
                SetSelectedSpawn(null);
                SetSelectedWaterbox(null);
                if (_editMode == UnitMapEditMode.ConnectRouteWaypoint)
                {
                    _linkingRouteWaypointIndex = hitWaypoint;
                    _linkPreviewWorldPoint = ScreenToWorld(e.Location);
                }
                else
                {
                    _resizingPointRadius = _editMode == UnitMapEditMode.ResizeRouteWaypointRadius;
                    _draggingRouteWaypoint = !_resizingPointRadius && _editMode == UnitMapEditMode.MoveRouteWaypoint;
                    if (_draggingRouteWaypoint || _resizingPointRadius)
                    {
                        OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                        if (_resizingPointRadius)
                        {
                            UpdateSelectedRadiusFromScreenPoint(e.Location);
                        }
                    }
                }
            }
            else if (hitSpawn is not null)
            {
                SetSelectedSpawn(hitSpawn);
                SetSelectedRouteWaypoint(null);
                SetSelectedWaterbox(null);
                _resizingPointRadius = _editMode == UnitMapEditMode.ResizeSpawnRadius;
                _draggingSpawn = !_resizingPointRadius && _editMode == UnitMapEditMode.MoveSpawn;
                if (_draggingSpawn || _resizingPointRadius)
                {
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    if (_resizingPointRadius)
                    {
                        UpdateSelectedRadiusFromScreenPoint(e.Location);
                    }
                }
            }
            else if (hitHandleWaterbox is not null &&
                hitWaterboxHandle != WaterboxResizeHandle.None &&
                _editMode == UnitMapEditMode.MoveWaterbox)
            {
                SetSelectedWaterbox(hitHandleWaterbox);
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                _resizingWaterbox = true;
                _waterboxResizeHandle = hitWaterboxHandle;
                OverlayDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (hitWaterbox is not null)
            {
                SetSelectedWaterbox(hitWaterbox);
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                _draggingWaterbox = _editMode == UnitMapEditMode.MoveWaterbox;
                if (_draggingWaterbox)
                {
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                SetSelectedWaterbox(null);
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            _rightButtonDown = true;
            Capture = true;
            int? hitSpawn = HitTestLayoutSpawn(e.Location);
            if (hitSpawn is not null)
            {
                SetSelectedSpawn(hitSpawn);
                SetSelectedRouteWaypoint(null);
                SetSelectedWaterbox(null);
                if (_editMode == UnitMapEditMode.ResizeSpawnRadius)
                {
                    _resizingPointRadius = true;
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSelectedRadiusFromScreenPoint(e.Location);
                }
                else
                {
                    _rotatingSpawn = true;
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSpawnAngleFromScreenPoint(e.Location);
                }
            }
            else
            {
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                SetSelectedWaterbox(null);
                _rotatingSpawn = false;
                _resizingPointRadius = false;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int dx = e.X - _lastMouse.X;
        int dy = e.Y - _lastMouse.Y;

        if (_rightButtonDown && _rotatingSpawn && _selectedSpawnIndex is not null)
        {
            UpdateSpawnAngleFromScreenPoint(e.Location);
        }
        else if (_rightButtonDown && _resizingPointRadius)
        {
            UpdateSelectedRadiusFromScreenPoint(e.Location);
        }
        else if (_leftButtonDown && _linkingRouteWaypointIndex is not null)
        {
            _linkPreviewWorldPoint = ScreenToWorld(e.Location);
            Invalidate();
        }
        else if (_leftButtonDown && _draggingRouteWaypoint && _selectedRouteWaypointIndex is not null)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
                float deltaY = -dy / unitScale;
                if (Math.Abs(deltaY) > 0.0001f)
                {
                    RouteWaypointMoved?.Invoke(
                        this,
                        new RouteWaypointMovedEventArgs(_selectedRouteWaypointIndex.Value, 0f, 0f, deltaY));
                }

                _lastMouse = e.Location;
                return;
            }

            PointF previousWorld = ScreenToWorld(_lastMouse);
            PointF currentWorld = ScreenToWorld(e.Location);
            float deltaX = currentWorld.X - previousWorld.X;
            float deltaZ = currentWorld.Y - previousWorld.Y;
            if (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    if (Math.Abs(deltaX) >= Math.Abs(deltaZ))
                    {
                        deltaZ = 0f;
                    }
                    else
                    {
                        deltaX = 0f;
                    }
                }

                RouteWaypointMoved?.Invoke(this, new RouteWaypointMovedEventArgs(_selectedRouteWaypointIndex.Value, deltaX, deltaZ));
            }
        }
        else if (_leftButtonDown && _draggingSpawn && _selectedSpawnIndex is not null)
        {
            PointF previousWorld = ScreenToWorld(_lastMouse);
            PointF currentWorld = ScreenToWorld(e.Location);
            float deltaX = currentWorld.X - previousWorld.X;
            float deltaZ = currentWorld.Y - previousWorld.Y;
            if (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    if (Math.Abs(deltaX) >= Math.Abs(deltaZ))
                    {
                        deltaZ = 0f;
                    }
                    else
                    {
                        deltaX = 0f;
                    }
                }

                LayoutSpawnMoved?.Invoke(this, new LayoutSpawnMovedEventArgs(_selectedSpawnIndex.Value, deltaX, deltaZ));
            }
        }
        else if (_leftButtonDown && _draggingWaterbox && _selectedWaterboxIndex is not null)
        {
            PointF previousWorld = ScreenToWorld(_lastMouse);
            PointF currentWorld = ScreenToWorld(e.Location);
            float deltaX = currentWorld.X - previousWorld.X;
            float deltaZ = currentWorld.Y - previousWorld.Y;
            if (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f)
            {
                WaterboxMoved?.Invoke(this, new WaterboxMovedEventArgs(_selectedWaterboxIndex.Value, deltaX, deltaZ));
            }
        }
        else if (_leftButtonDown && _resizingWaterbox && _selectedWaterboxIndex is not null)
        {
            PointF previousWorld = ScreenToWorld(_lastMouse);
            PointF currentWorld = ScreenToWorld(e.Location);
            float deltaX = currentWorld.X - previousWorld.X;
            float deltaZ = currentWorld.Y - previousWorld.Y;
            if (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f)
            {
                WaterboxResized?.Invoke(this, new WaterboxResizedEventArgs(_selectedWaterboxIndex.Value, _waterboxResizeHandle, deltaX, deltaZ));
            }
        }
        else if (_leftButtonDown && _rotatingSpawn && _selectedSpawnIndex is not null)
        {
            UpdateSpawnAngleFromScreenPoint(e.Location);
        }
        else if (_leftButtonDown && _resizingPointRadius)
        {
            UpdateSelectedRadiusFromScreenPoint(e.Location);
        }
        else if (_leftButtonDown)
        {
            float scale = Math.Max(_baseScale * _zoom, 0.0001f);
            _panWorld.X += dx / scale;
            _panWorld.Y += dy / scale;
            Invalidate();
        }

        _lastMouse = e.Location;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            bool endedOverlayDrag = _draggingRouteWaypoint || _draggingSpawn || _draggingWaterbox || _resizingWaterbox || _rotatingSpawn || _resizingPointRadius;
            if (_linkingRouteWaypointIndex is not null)
            {
                int? targetWaypoint = HitTestRouteWaypoint(e.Location);
                if (targetWaypoint is not null && targetWaypoint != _linkingRouteWaypointIndex)
                {
                    RouteWaypointLinked?.Invoke(
                        this,
                        new RouteWaypointLinkedEventArgs(_linkingRouteWaypointIndex.Value, targetWaypoint.Value));
                }
            }

            _leftButtonDown = false;
            _draggingRouteWaypoint = false;
            _draggingSpawn = false;
            _draggingWaterbox = false;
            _resizingWaterbox = false;
            _rotatingSpawn = false;
            _resizingPointRadius = false;
            _linkingRouteWaypointIndex = null;
            _waterboxResizeHandle = WaterboxResizeHandle.None;
            if (endedOverlayDrag)
            {
                OverlayDragEnded?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }
        else if (e.Button == MouseButtons.Right)
        {
            bool endedOverlayDrag = _rotatingSpawn || _resizingPointRadius;
            _rightButtonDown = false;
            _rotatingSpawn = false;
            _resizingPointRadius = false;
            if (endedOverlayDrag)
            {
                OverlayDragEnded?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }

        if (!_leftButtonDown && !_rightButtonDown)
        {
            Capture = false;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        float factor = e.Delta > 0 ? 1.15f : (1f / 1.15f);
        _zoom = Math.Clamp(_zoom * factor, MinZoom, MaxZoom);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.Clear(BackColor);

        Rectangle viewport = ClientRectangle;
        if (viewport.Width <= 0 || viewport.Height <= 0)
        {
            return;
        }

        DrawBackdrop(g, viewport);

        using Matrix transform = BuildWorldToScreenMatrix(viewport);
        Matrix original = g.Transform.Clone();
        g.Transform = transform;

        DrawWorldGrid(g);
        DrawTerrain(g);
        DrawWaterbox(g);
        DrawRoute(g, viewport);
        DrawRouteConnectionPreview(g);
        DrawLayout(g, viewport);
        DrawSpawnAngleArrow(g);

        g.Transform = original;
        DrawSpawnIcons(g, transform);
        DrawPointLabels(g, transform);
        original.Dispose();

        DrawOverlay(g);
    }

    private void DrawBackdrop(Graphics g, Rectangle viewport)
    {
        using SolidBrush paperBrush = new(Color.FromArgb(248, 246, 239));
        Rectangle bounds = viewport;
        bounds.Inflate(-12, -12);
        g.FillRectangle(paperBrush, bounds);
    }

    private void DrawTerrain(Graphics g)
    {
        if (_terrainImage is null || _modelBounds.Width <= 0f || _modelBounds.Height <= 0f)
        {
            return;
        }

        PointF[] points =
        {
            new(_modelBounds.Left, _modelBounds.Bottom),
            new(_modelBounds.Right, _modelBounds.Bottom),
            new(_modelBounds.Left, _modelBounds.Top)
        };
        g.DrawImage(_terrainImage, points);
    }

    //-------------------------------------------------------------------------------
    // BigPan_Editor 互換の目安グリッドを地形の下へ描画する処理
    //-------------------------------------------------------------------------------
    private void DrawWorldGrid(Graphics g)
    {
        RectangleF bounds = GetDataBounds();
        float minX = MathF.Floor(bounds.Left / GridStepWorld) * GridStepWorld;
        float maxX = MathF.Ceiling(bounds.Right / GridStepWorld) * GridStepWorld;
        float minZ = MathF.Floor(bounds.Top / GridStepWorld) * GridStepWorld;
        float maxZ = MathF.Ceiling(bounds.Bottom / GridStepWorld) * GridStepWorld;
        float stroke = 1f / Math.Max(_baseScale * _zoom, 0.0001f);

        using Pen gridPen = new(Color.FromArgb(205, 198, 178), stroke);
        using Pen axisPen = new(Color.FromArgb(162, 151, 121), stroke * 1.6f);

        for (float x = minX; x <= maxX + 0.001f; x += GridStepWorld)
        {
            Pen pen = Math.Abs(x) < 0.001f ? axisPen : gridPen;
            g.DrawLine(pen, x, minZ, x, maxZ);
        }

        for (float z = minZ; z <= maxZ + 0.001f; z += GridStepWorld)
        {
            Pen pen = Math.Abs(z) < 0.001f ? axisPen : gridPen;
            g.DrawLine(pen, minX, z, maxX, z);
        }
    }

    private void DrawRoute(Graphics g, Rectangle viewport)
    {
        if (_route.Waypoints.Count == 0)
        {
            return;
        }

        float stroke = 3.2f / Math.Max(_baseScale * _zoom, 0.0001f);
        float outlineStroke = stroke + (3f / Math.Max(_baseScale * _zoom, 0.0001f));
        using Pen routeOutlinePen = new(Color.FromArgb(215, 20, 20, 20), outlineStroke)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using Pen routePen = new(Color.FromArgb(224, 31, 31), stroke)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using Pen arrowOutlinePen = new(Color.FromArgb(235, 20, 20, 20), outlineStroke)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using SolidBrush wpBrush = new(Color.White);
        using Pen wpPen = new(Color.DarkRed, stroke);
        HashSet<(int From, int To)> drawnEdges = new();

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            PointF from = new(waypoint.X, waypoint.Z);
            foreach (int link in waypoint.Links)
            {
                if (!_route.Waypoints.TryGetValue(link, out RouteWaypoint? target))
                {
                    continue;
                }

                (int From, int To) edgeKey = waypoint.Index < link
                    ? (waypoint.Index, link)
                    : (link, waypoint.Index);

                if (drawnEdges.Add(edgeKey))
                {
                    g.DrawLine(routeOutlinePen, from, new PointF(target.X, target.Z));
                    g.DrawLine(routePen, from, new PointF(target.X, target.Z));
                }
            }
        }

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            PointF from = new(waypoint.X, waypoint.Z);
            foreach (int link in waypoint.Links)
            {
                if (!_route.Waypoints.TryGetValue(link, out RouteWaypoint? target))
                {
                    continue;
                }

                PointF to = new(target.X, target.Z);
                DrawRouteArrow(g, arrowOutlinePen, from, to);
                DrawGradientRouteArrow(g, from, to, GetRouteColorHeight(waypoint), GetRouteColorHeight(target), stroke);
            }
        }

        float radius = 7f / Math.Max(_baseScale * _zoom, 0.0001f);
        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            bool isSelected = _selectedRouteWaypointIndex == waypoint.Index;
            if (_showRadius && waypoint.Radius > 0.01f)
            {
                using Pen rangePen = new(Color.FromArgb(230, 255, 132, 0), 2.2f / Math.Max(_baseScale * _zoom, 0.0001f));
                g.DrawEllipse(rangePen, waypoint.X - waypoint.Radius, waypoint.Z - waypoint.Radius, waypoint.Radius * 2f, waypoint.Radius * 2f);
            }

            if (isSelected)
            {
                using Pen selectedPen = new(Color.FromArgb(255, 111, 0), stroke * 1.25f);
                g.FillEllipse(wpBrush, waypoint.X - radius, waypoint.Z - radius, radius * 2f, radius * 2f);
                g.DrawEllipse(selectedPen, waypoint.X - radius, waypoint.Z - radius, radius * 2f, radius * 2f);
            }
            else
            {
                g.FillEllipse(wpBrush, waypoint.X - radius, waypoint.Z - radius, radius * 2f, radius * 2f);
                g.DrawEllipse(wpPen, waypoint.X - radius, waypoint.Z - radius, radius * 2f, radius * 2f);
            }
        }
    }

    //-------------------------------------------------------------------------------
    // route 矢印の色分けに使う Y 値を取得する処理
    //-------------------------------------------------------------------------------
    private float GetRouteColorHeight(RouteWaypoint waypoint)
    {
        return _routeColorHeights.TryGetValue(waypoint.Index, out float height) ? height : waypoint.Y;
    }

    private void DrawLayout(Graphics g, Rectangle viewport)
    {
        if (_layout.Spawns.Count == 0)
        {
            return;
        }

        float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
        for (int index = 0; index < _layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            using SolidBrush fillBrush = new(GetSpawnColor(spawn.TypeId));
            using Pen rangePen = new(Color.FromArgb(230, 255, 132, 0), 2.2f / unitScale);
            if (_useFieldObjectIcons && FieldObjectIconCatalog.GetFootprintSize(spawn.TypeLabel) is SizeF footprintSize)
            {
                Color footprintColor = FieldObjectIconCatalog.GetFootprintColor(spawn.TypeLabel);
                PointF[] footprintPoints = GetOrientedFootprintPoints(spawn, footprintSize);
                using SolidBrush footprintBrush = new(footprintColor);
                using Pen footprintPen = new(Color.FromArgb(Math.Min(220, footprintColor.A + 110), footprintColor.R, footprintColor.G, footprintColor.B), 2.2f / unitScale);
                g.FillPolygon(footprintBrush, footprintPoints);
                g.DrawPolygon(footprintPen, footprintPoints);
            }

            if (_showRadius && spawn.Radius > 0.01f)
            {
                g.DrawEllipse(rangePen, spawn.X - spawn.Radius, spawn.Z - spawn.Radius, spawn.Radius * 2f, spawn.Radius * 2f);
            }
        }
    }

    //-------------------------------------------------------------------------------
    // waterbox の X/Z 矩形を半透明で描画する処理
    //-------------------------------------------------------------------------------
    private void DrawWaterbox(Graphics g)
    {
        if (_waterbox.Boxes.Count == 0)
        {
            return;
        }

        float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
        for (int index = 0; index < _waterbox.Boxes.Count; index++)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            RectangleF bounds = RectangleF.FromLTRB(box.MinX, box.MinZ, box.MaxX, box.MaxZ);
            using SolidBrush fillBrush = new(index == _selectedWaterboxIndex
                ? Color.FromArgb(92, 0, 150, 255)
                : Color.FromArgb(48, 30, 100, 255));
            using Pen edgePen = new(index == _selectedWaterboxIndex
                ? Color.FromArgb(255, 0, 106, 180)
                : Color.FromArgb(180, 30, 100, 255), 2.2f / unitScale);
            g.FillRectangle(fillBrush, bounds);
            g.DrawRectangle(edgePen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            if (index == _selectedWaterboxIndex)
            {
                DrawWaterboxResizeHandles(g, bounds, unitScale);
            }
        }
    }

    //-------------------------------------------------------------------------------
    // Spawn の Angle とサイズから地上 object の向き付き矩形を作成する処理
    //-------------------------------------------------------------------------------
    private static PointF[] GetOrientedFootprintPoints(LayoutSpawn spawn, SizeF size)
    {
        float radians = spawn.Angle * MathF.PI / 180f;
        PointF forward = new(MathF.Sin(radians), MathF.Cos(radians));
        PointF right = new(forward.Y, -forward.X);
        float halfWidth = size.Width * 0.5f;
        float halfLength = size.Height * 0.5f;
        return new[]
        {
            new PointF(spawn.X - (right.X * halfWidth) - (forward.X * halfLength), spawn.Z - (right.Y * halfWidth) - (forward.Y * halfLength)),
            new PointF(spawn.X + (right.X * halfWidth) - (forward.X * halfLength), spawn.Z + (right.Y * halfWidth) - (forward.Y * halfLength)),
            new PointF(spawn.X + (right.X * halfWidth) + (forward.X * halfLength), spawn.Z + (right.Y * halfWidth) + (forward.Y * halfLength)),
            new PointF(spawn.X - (right.X * halfWidth) + (forward.X * halfLength), spawn.Z - (right.Y * halfWidth) + (forward.Y * halfLength))
        };
    }

    //-------------------------------------------------------------------------------
    // 選択中 waterbox のサイズ変更ハンドルを描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawWaterboxResizeHandles(Graphics g, RectangleF bounds, float unitScale)
    {
        float size = 10f / Math.Max(unitScale, 0.0001f);
        using SolidBrush handleBrush = new(Color.White);
        using Pen handlePen = new(Color.FromArgb(255, 0, 106, 180), 1.8f / Math.Max(unitScale, 0.0001f));
        foreach (PointF point in GetWaterboxHandlePoints(bounds).Values)
        {
            RectangleF handleBounds = new(point.X - (size * 0.5f), point.Y - (size * 0.5f), size, size);
            g.FillRectangle(handleBrush, handleBounds);
            g.DrawRectangle(handlePen, handleBounds.X, handleBounds.Y, handleBounds.Width, handleBounds.Height);
        }
    }

    //-------------------------------------------------------------------------------
    // 画像表示モードでスポーン種別アイコンを画面座標に描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSpawnIcons(Graphics g, Matrix worldToScreen)
    {
        if (_layout.Spawns.Count == 0)
        {
            return;
        }

        using Font labelFont = new("Yu Gothic UI", 9f, FontStyle.Bold);
        for (int index = 0; index < _layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            PointF screen = TransformWorldPoint(worldToScreen, spawn.X, spawn.Z);
            bool isSelected = _selectedSpawnIndex == index;
            const float iconSize = 32f;
            RectangleF iconBounds = new(screen.X - iconSize * 0.5f, screen.Y - iconSize * 0.5f, iconSize, iconSize);
            Image? fieldIcon = _useFieldObjectIcons ? FieldObjectIconCatalog.GetIcon(spawn.TypeLabel) : null;
            Image? icon = fieldIcon ?? SpawnIconCatalog.GetIcon(spawn.TypeId);

            if (icon is not null)
            {
                g.DrawImage(icon, iconBounds);
            }
            else
            {
                using SolidBrush fillBrush = new(GetSpawnColor(spawn.TypeId));
                g.FillEllipse(fillBrush, iconBounds);
            }

            using Pen edgePen = new(isSelected ? Color.FromArgb(255, 111, 0) : Color.White, isSelected ? 2.5f : 1.5f);
            if (fieldIcon is not null)
            {
                g.DrawRectangle(edgePen, iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height);
            }
            else
            {
                g.DrawEllipse(edgePen, iconBounds);
            }
            DrawSpawnAngleHandle(g, worldToScreen, spawn, screen, isSelected);
            DrawPointText(g, index.ToString(), new PointF(screen.X + 15f, screen.Y - 17f), Color.White, labelFont);
        }
    }

    //-------------------------------------------------------------------------------
    // スポーン外周の角度変更ハンドルを描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSpawnAngleHandle(Graphics g, Matrix worldToScreen, LayoutSpawn spawn, PointF center, bool isSelected)
    {
        PointF direction = GetSpawnAngleScreenDirection(worldToScreen, spawn);
        PointF[] points = GetSpawnAngleHandlePoints(center, direction);
        using SolidBrush fillBrush = new(isSelected ? Color.FromArgb(255, 111, 0) : Color.FromArgb(255, 150, 32));
        using Pen outlinePen = new(Color.FromArgb(230, 20, 20, 20), 1.6f);
        g.FillPolygon(fillBrush, points);
        g.DrawPolygon(outlinePen, points);
    }

    //-------------------------------------------------------------------------------
    // 画像表示モードで Spawn と Waypoint の番号を画面座標に描画する処理
    //-------------------------------------------------------------------------------
    private void DrawPointLabels(Graphics g, Matrix worldToScreen)
    {
        using Font labelFont = new("Yu Gothic UI", 9f, FontStyle.Bold);

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values.OrderBy(waypoint => waypoint.Index))
        {
            PointF screen = TransformWorldPoint(worldToScreen, waypoint.X, waypoint.Z);
            DrawPointText(g, waypoint.Index.ToString(), new PointF(screen.X + 11f, screen.Y - 17f), Color.White, labelFont);
        }
    }

    //-------------------------------------------------------------------------------
    // 1 点の番号テキストを縁取り付きで描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawPointText(Graphics g, string text, PointF location, Color textColor, Font font)
    {
        using SolidBrush outlineBrush = new(Color.FromArgb(230, 20, 20, 20));
        using SolidBrush textBrush = new(textColor);
        g.DrawString(text, font, outlineBrush, new PointF(location.X - 1f, location.Y));
        g.DrawString(text, font, outlineBrush, new PointF(location.X + 1f, location.Y));
        g.DrawString(text, font, outlineBrush, new PointF(location.X, location.Y - 1f));
        g.DrawString(text, font, outlineBrush, new PointF(location.X, location.Y + 1f));
        g.DrawString(text, font, textBrush, location);
    }

    //-------------------------------------------------------------------------------
    // ワールド座標を現在の画面座標へ変換する処理
    //-------------------------------------------------------------------------------
    private static PointF TransformWorldPoint(Matrix worldToScreen, float x, float z)
    {
        PointF[] points = { new(x, z) };
        worldToScreen.TransformPoints(points);
        return points[0];
    }

    private void DrawOverlay(Graphics g)
    {
        using SolidBrush textBrush = new(Color.FromArgb(44, 62, 80));
        using Font titleFont = new("Yu Gothic UI", 20f, FontStyle.Bold);
        using Font bodyFont = new("Yu Gothic UI", 10f, FontStyle.Regular);
        g.DrawString(_viewTitle, titleFont, textBrush, new PointF(76, 22));

        string info = $"Zoom: {_zoom:0.00}x";
        g.DrawString(info, bodyFont, textBrush, new PointF(78, 60));
        string modeText = _editMode switch
        {
            UnitMapEditMode.MoveRouteWaypoint => "Edit: Move Waypoint",
            UnitMapEditMode.AddRouteWaypoint => "Edit: Add Waypoint",
            UnitMapEditMode.AddSpawn => "Edit: Add Spawn",
            UnitMapEditMode.DeleteRouteWaypoint => "Edit: Delete Waypoint",
            UnitMapEditMode.DeleteSpawn => "Edit: Delete Spawn",
            UnitMapEditMode.ConnectRouteWaypoint => "Edit: Connect Route",
            UnitMapEditMode.MoveSpawn => "Edit: Move Spawn",
            UnitMapEditMode.RotateSpawn => "Edit: Rotate Spawn",
            UnitMapEditMode.ResizeSpawnRadius => "Edit: Resize Spawn Radius",
            UnitMapEditMode.ResizeRouteWaypointRadius => "Edit: Resize Waypoint Radius",
            UnitMapEditMode.AddWaterbox => "Edit: Add Waterbox",
            UnitMapEditMode.DeleteWaterbox => "Edit: Delete Waterbox",
            UnitMapEditMode.MoveWaterbox => "Edit: Move Waterbox",
            _ => "Edit: Navigate"
        };
        g.DrawString(modeText, bodyFont, textBrush, new PointF(78, 82));
        string hintText = GetEditHintText();
        if (!string.IsNullOrEmpty(hintText))
        {
            g.DrawString(hintText, bodyFont, textBrush, new PointF(78, 104));
        }
    }

    //-------------------------------------------------------------------------------
    // 現在の編集モードに応じた操作ヒント文を返す処理
    //-------------------------------------------------------------------------------
    private string GetEditHintText()
    {
        return _editMode switch
        {
            UnitMapEditMode.MoveWaterbox => _englishUi ? "Waterbox: drag the center to move, drag white corner/edge handles to resize" : "Waterbox: 中央ドラッグで移動，四隅/辺の白ハンドルでサイズ変更",
            UnitMapEditMode.AddWaterbox => _englishUi ? "Waterbox: left click to add a default-size waterbox" : "Waterbox: 左クリック位置へ既定サイズで追加",
            UnitMapEditMode.DeleteWaterbox => _englishUi ? "Waterbox: left click a target to delete it" : "Waterbox: 対象を左クリックで削除",
            UnitMapEditMode.MoveRouteWaypoint => _englishUi ? "Waypoint: drag to move, Shift locks the main axis, Ctrl moves Y height" : "Waypoint: ドラッグで移動，Shiftで主軸固定，Ctrlで高さY移動",
            UnitMapEditMode.ConnectRouteWaypoint => _englishUi ? "Route: drag between waypoints to connect them" : "Route: Waypoint同士をドラッグで接続",
            UnitMapEditMode.DeleteRouteLink => _englishUi ? "Route: left click a connection line to delete it" : "Route: 接続線を左クリックで削除",
            UnitMapEditMode.RotateSpawn => _englishUi ? "Spawn: right drag from the selected Spawn to change its angle" : "Spawn: 選択Spawnから右ドラッグ方向へ角度変更",
            UnitMapEditMode.ResizeSpawnRadius => _englishUi ? "Spawn: drag from the selected Spawn to set Radius by distance" : "Spawn: 選択Spawnからドラッグ位置までの距離でRadius変更",
            UnitMapEditMode.ResizeRouteWaypointRadius => _englishUi ? "Waypoint: drag from the selected Waypoint to set Radius by distance" : "Waypoint: 選択Waypointからドラッグ位置までの距離でRadius変更",
            _ => string.Empty
        };
    }

    //-------------------------------------------------------------------------------
    // route 接続ドラッグ中の仮線を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawRouteConnectionPreview(Graphics g)
    {
        if (_linkingRouteWaypointIndex is null ||
            !_route.Waypoints.TryGetValue(_linkingRouteWaypointIndex.Value, out RouteWaypoint? from))
        {
            return;
        }

        float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
        using Pen previewPen = new(Color.FromArgb(220, 25, 118, 210), 2.4f / unitScale)
        {
            DashStyle = DashStyle.Dash
        };
        g.DrawLine(previewPen, new PointF(from.X, from.Z), _linkPreviewWorldPoint);
    }

    private Matrix BuildWorldToScreenMatrix(Rectangle viewport)
    {
        Matrix matrix = new();
        float centerX = viewport.Width * 0.5f;
        float centerY = viewport.Height * 0.5f;
        float worldCenterX = _modelBounds.Width <= 0f ? 0f : _modelBounds.Left + (_modelBounds.Width * 0.5f) + _panWorld.X;
        float worldCenterY = _modelBounds.Height <= 0f ? 0f : _modelBounds.Top + (_modelBounds.Height * 0.5f) + _panWorld.Y;

        matrix.Translate(centerX, centerY);
        matrix.Scale(-_baseScale * _zoom, -_baseScale * _zoom);
        matrix.Translate(-worldCenterX, -worldCenterY);
        return matrix;
    }

    private float CalculateBaseScale()
    {
        RectangleF bounds = GetDataBounds();
        if (bounds.Width <= 0f || bounds.Height <= 0f || Width <= 0 || Height <= 0)
        {
            return 1f;
        }

        float width = Math.Max(Width - 80f, 64f);
        float height = Math.Max(Height - 80f, 64f);
        return Math.Min(width / bounds.Width, height / bounds.Height);
    }

    private RectangleF GetDataBounds()
    {
        if (_modelBounds.Width > 0f && _modelBounds.Height > 0f)
        {
            float marginX = Math.Max(_modelBounds.Width * 0.1f, 32f);
            float marginZ = Math.Max(_modelBounds.Height * 0.1f, 32f);
            return RectangleF.FromLTRB(
                _modelBounds.Left - marginX,
                _modelBounds.Top - marginZ,
                _modelBounds.Right + marginX,
                _modelBounds.Bottom + marginZ);
        }

        List<float> xs = new();
        List<float> zs = new();
        xs.AddRange(_route.Waypoints.Values.Select(w => w.X));
        zs.AddRange(_route.Waypoints.Values.Select(w => w.Z));
        xs.AddRange(_layout.Spawns.Select(s => s.X));
        zs.AddRange(_layout.Spawns.Select(s => s.Z));
        xs.AddRange(_waterbox.Boxes.SelectMany(box => new[] { box.MinX, box.MaxX }));
        zs.AddRange(_waterbox.Boxes.SelectMany(box => new[] { box.MinZ, box.MaxZ }));

        if (xs.Count == 0 || zs.Count == 0)
        {
            return new RectangleF(-256, -256, 512, 512);
        }

        float minX = xs.Min();
        float maxX = xs.Max();
        float minZ = zs.Min();
        float maxZ = zs.Max();
        float fallbackMarginX = Math.Max((maxX - minX) * 0.1f, 32f);
        float fallbackMarginZ = Math.Max((maxZ - minZ) * 0.1f, 32f);
        return RectangleF.FromLTRB(minX - fallbackMarginX, minZ - fallbackMarginZ, maxX + fallbackMarginX, maxZ + fallbackMarginZ);
    }

    private static Color GetSpawnColor(int typeId)
    {
        return typeId switch
        {
            0 => Color.FromArgb(230, 74, 25),
            1 => Color.FromArgb(211, 47, 47),
            2 => Color.FromArgb(255, 179, 0),
            5 => Color.FromArgb(0, 121, 107),
            4 => Color.FromArgb(0, 121, 107),
            6 => Color.FromArgb(67, 160, 71),
            7 => Color.FromArgb(2, 136, 209),
            8 => Color.FromArgb(123, 31, 162),
            _ => Color.FromArgb(96, 125, 139)
        };
    }

    //-------------------------------------------------------------------------------
    // 画面座標から選択中 Spawn の角度を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateSpawnAngleFromScreenPoint(Point location)
    {
        if (_selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _layout.Spawns.Count)
        {
            return;
        }

        LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
        PointF worldPoint = ScreenToWorld(location);
        float deltaX = worldPoint.X - spawn.X;
        float deltaZ = worldPoint.Y - spawn.Z;
        if ((deltaX * deltaX) + (deltaZ * deltaZ) < 0.0001f)
        {
            return;
        }

        float angle = MathF.Atan2(deltaX, deltaZ) * 180f / MathF.PI;
        if (angle < 0f)
        {
            angle += 360f;
        }

        LayoutSpawnAngleChanged?.Invoke(this, new LayoutSpawnAngleChangedEventArgs(_selectedSpawnIndex.Value, angle));
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 画面座標から選択中ポイントの Radius を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateSelectedRadiusFromScreenPoint(Point location)
    {
        PointF worldPoint = ScreenToWorld(location);
        if (_editMode == UnitMapEditMode.ResizeSpawnRadius &&
            _selectedSpawnIndex is not null &&
            _selectedSpawnIndex.Value >= 0 &&
            _selectedSpawnIndex.Value < _layout.Spawns.Count)
        {
            LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
            float radius = DistanceXZ(worldPoint.X, worldPoint.Y, spawn.X, spawn.Z);
            LayoutSpawnRadiusChanged?.Invoke(this, new LayoutSpawnRadiusChangedEventArgs(_selectedSpawnIndex.Value, radius));
            Invalidate();
            return;
        }

        if (_editMode == UnitMapEditMode.ResizeRouteWaypointRadius &&
            _selectedRouteWaypointIndex is not null &&
            _route.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint))
        {
            float radius = DistanceXZ(worldPoint.X, worldPoint.Y, waypoint.X, waypoint.Z);
            RouteWaypointRadiusChanged?.Invoke(this, new RouteWaypointRadiusChangedEventArgs(_selectedRouteWaypointIndex.Value, radius));
            Invalidate();
        }
    }

    //-------------------------------------------------------------------------------
    // X/Z 平面上の距離を計算する処理
    //-------------------------------------------------------------------------------
    private static float DistanceXZ(float x1, float z1, float x2, float z2)
    {
        float deltaX = x1 - x2;
        float deltaZ = z1 - z2;
        return MathF.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
    }

    //-------------------------------------------------------------------------------
    // 選択中 Spawn の角度方向を矢印で描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSpawnAngleArrow(Graphics g)
    {
        if (_editMode != UnitMapEditMode.RotateSpawn ||
            _selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _layout.Spawns.Count)
        {
            return;
        }

        LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
        float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
        float length = 54f / unitScale;
        float radians = spawn.Angle * MathF.PI / 180f;
        PointF start = new(spawn.X, spawn.Z);
        PointF end = new(
            spawn.X + (MathF.Sin(radians) * length),
            spawn.Z + (MathF.Cos(radians) * length));

        using Pen outlinePen = new(Color.FromArgb(235, 20, 20, 20), 5f / unitScale)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using Pen arrowPen = new(Color.FromArgb(255, 111, 0), 3f / unitScale)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        g.DrawLine(outlinePen, start, end);
        g.DrawLine(arrowPen, start, end);
        DrawRouteArrow(g, outlinePen, start, end);
        DrawRouteArrow(g, arrowPen, start, end);
    }

    //-------------------------------------------------------------------------------
    // マウス位置に最も近い waypoint を画面座標から求める処理
    //-------------------------------------------------------------------------------
    private int? HitTestRouteWaypoint(Point location)
    {
        if (_route.Waypoints.Count == 0)
        {
            return null;
        }

        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        int? nearestWaypoint = null;
        float nearestDistanceSquared = RouteHitRadiusPixels * RouteHitRadiusPixels;
        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            PointF screenPoint = WorldToScreen(transform, new PointF(waypoint.X, waypoint.Z));
            float dx = screenPoint.X - location.X;
            float dy = screenPoint.Y - location.Y;
            float distanceSquared = (dx * dx) + (dy * dy);
            if (distanceSquared <= nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestWaypoint = waypoint.Index;
            }
        }

        return nearestWaypoint;
    }

    //-------------------------------------------------------------------------------
    // マウス位置に最も近い spawn を画面座標から求める処理
    //-------------------------------------------------------------------------------
    private int? HitTestLayoutSpawn(Point location)
    {
        if (_layout.Spawns.Count == 0)
        {
            return null;
        }

        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        int? nearestSpawn = null;
        float nearestDistanceSquared = SpawnHitRadiusPixels * SpawnHitRadiusPixels;
        for (int index = 0; index < _layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            PointF screenPoint = WorldToScreen(transform, new PointF(spawn.X, spawn.Z));
            float dx = screenPoint.X - location.X;
            float dy = screenPoint.Y - location.Y;
            float distanceSquared = (dx * dx) + (dy * dy);
            if (distanceSquared <= nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestSpawn = index;
            }
        }

        return nearestSpawn;
    }

    //-------------------------------------------------------------------------------
    // マウス位置にあるスポーン角度ハンドルを画面座標から求める処理
    //-------------------------------------------------------------------------------
    private int? HitTestSpawnAngleHandle(Point location)
    {
        if (_layout.Spawns.Count == 0)
        {
            return null;
        }

        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        for (int index = _layout.Spawns.Count - 1; index >= 0; index--)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            PointF center = WorldToScreen(transform, new PointF(spawn.X, spawn.Z));
            PointF direction = GetSpawnAngleScreenDirection(transform, spawn);
            using GraphicsPath path = new();
            path.AddPolygon(GetSpawnAngleHandlePoints(center, direction));
            if (path.IsVisible(location))
            {
                return index;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // Spawn の Angle から画面上の向きベクトルを作成する処理
    //-------------------------------------------------------------------------------
    private static PointF GetSpawnAngleScreenDirection(Matrix worldToScreen, LayoutSpawn spawn)
    {
        float radians = spawn.Angle * MathF.PI / 180f;
        PointF center = TransformWorldPoint(worldToScreen, spawn.X, spawn.Z);
        PointF directionPoint = TransformWorldPoint(
            worldToScreen,
            spawn.X + MathF.Sin(radians),
            spawn.Z + MathF.Cos(radians));
        float dx = directionPoint.X - center.X;
        float dy = directionPoint.Y - center.Y;
        float length = MathF.Sqrt((dx * dx) + (dy * dy));
        if (length < 0.0001f)
        {
            return new PointF(0f, -1f);
        }

        return new PointF(dx / length, dy / length);
    }

    //-------------------------------------------------------------------------------
    // 画面上の三角形ハンドル頂点を作成する処理
    //-------------------------------------------------------------------------------
    private static PointF[] GetSpawnAngleHandlePoints(PointF center, PointF direction)
    {
        PointF tip = new(
            center.X + (direction.X * (SpawnAngleHandleDistancePixels + SpawnAngleHandleSizePixels * 0.6f)),
            center.Y + (direction.Y * (SpawnAngleHandleDistancePixels + SpawnAngleHandleSizePixels * 0.6f)));
        PointF baseCenter = new(
            center.X + (direction.X * (SpawnAngleHandleDistancePixels - SpawnAngleHandleSizePixels * 0.55f)),
            center.Y + (direction.Y * (SpawnAngleHandleDistancePixels - SpawnAngleHandleSizePixels * 0.55f)));
        PointF perpendicular = new(-direction.Y, direction.X);
        return new[]
        {
            tip,
            new PointF(
                baseCenter.X + (perpendicular.X * SpawnAngleHandleSizePixels * 0.55f),
                baseCenter.Y + (perpendicular.Y * SpawnAngleHandleSizePixels * 0.55f)),
            new PointF(
                baseCenter.X - (perpendicular.X * SpawnAngleHandleSizePixels * 0.55f),
                baseCenter.Y - (perpendicular.Y * SpawnAngleHandleSizePixels * 0.55f))
        };
    }

    //-------------------------------------------------------------------------------
    // マウス位置が含まれる waterbox を画面座標から求める処理
    //-------------------------------------------------------------------------------
    private int? HitTestWaterbox(Point location)
    {
        if (_waterbox.Boxes.Count == 0)
        {
            return null;
        }

        PointF world = ScreenToWorld(location);
        for (int index = _waterbox.Boxes.Count - 1; index >= 0; index--)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            if (world.X >= box.MinX &&
                world.X <= box.MaxX &&
                world.Y >= box.MinZ &&
                world.Y <= box.MaxZ)
            {
                return index;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // マウス位置に最も近い waterbox サイズ変更ハンドルを求める処理
    //-------------------------------------------------------------------------------
    private WaterboxResizeHandle HitTestWaterboxHandle(Point location, out int? waterboxIndex)
    {
        waterboxIndex = null;
        if (_waterbox.Boxes.Count == 0)
        {
            return WaterboxResizeHandle.None;
        }

        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        const float hitSize = 9f;
        for (int index = _waterbox.Boxes.Count - 1; index >= 0; index--)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            RectangleF bounds = RectangleF.FromLTRB(box.MinX, box.MinZ, box.MaxX, box.MaxZ);
            foreach ((WaterboxResizeHandle handle, PointF worldPoint) in GetWaterboxHandlePoints(bounds))
            {
                PointF screenPoint = WorldToScreen(transform, worldPoint);
                if (Math.Abs(screenPoint.X - location.X) <= hitSize &&
                    Math.Abs(screenPoint.Y - location.Y) <= hitSize)
                {
                    waterboxIndex = index;
                    return handle;
                }
            }
        }

        return WaterboxResizeHandle.None;
    }

    private static IReadOnlyDictionary<WaterboxResizeHandle, PointF> GetWaterboxHandlePoints(RectangleF bounds)
    {
        float centerX = bounds.Left + (bounds.Width * 0.5f);
        float centerZ = bounds.Top + (bounds.Height * 0.5f);
        return new Dictionary<WaterboxResizeHandle, PointF>
        {
            [WaterboxResizeHandle.TopLeft] = new(bounds.Left, bounds.Top),
            [WaterboxResizeHandle.Top] = new(centerX, bounds.Top),
            [WaterboxResizeHandle.TopRight] = new(bounds.Right, bounds.Top),
            [WaterboxResizeHandle.Right] = new(bounds.Right, centerZ),
            [WaterboxResizeHandle.BottomRight] = new(bounds.Right, bounds.Bottom),
            [WaterboxResizeHandle.Bottom] = new(centerX, bounds.Bottom),
            [WaterboxResizeHandle.BottomLeft] = new(bounds.Left, bounds.Bottom),
            [WaterboxResizeHandle.Left] = new(bounds.Left, centerZ)
        };
    }

    //-------------------------------------------------------------------------------
    // マウス位置に最も近い route 接続線を画面座標から求める処理
    //-------------------------------------------------------------------------------
    private (int From, int To)? HitTestRouteLink(Point location)
    {
        const float maxDistancePixels = 9f;
        (int From, int To)? nearestLink = null;
        float nearestDistanceSquared = maxDistancePixels * maxDistancePixels;

        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            PointF from = WorldToScreen(transform, new PointF(waypoint.X, waypoint.Z));
            foreach (int link in waypoint.Links)
            {
                if (!_route.Waypoints.TryGetValue(link, out RouteWaypoint? target))
                {
                    continue;
                }

                PointF to = WorldToScreen(transform, new PointF(target.X, target.Z));
                float distanceSquared = DistanceSquaredToSegment(location, from, to);
                if (distanceSquared <= nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearestLink = (waypoint.Index, link);
                }
            }
        }

        return nearestLink;
    }

    //-------------------------------------------------------------------------------
    // route_editor 準拠でルート途中へ矢印を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawRouteArrow(Graphics g, Pen arrowPen, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dz = end.Y - start.Y;
        float length = MathF.Sqrt((dx * dx) + (dz * dz));
        if (length < 0.001f)
        {
            return;
        }

        float unitScale = Math.Max(_baseScale * _zoom, 0.0001f);
        float arrowLength = 15f / unitScale;
        float arrowAngle = 40f * (MathF.PI / 180f);
        PointF center = new(
            (end.X * 0.8f) + (start.X * 0.2f),
            (end.Y * 0.8f) + (start.Y * 0.2f));

        float direction = MathF.Atan2(dz, dx);
        PointF first = RotatePointAroundCenter(
            new PointF(center.X - arrowLength, center.Y),
            center,
            direction + arrowAngle);
        PointF second = RotatePointAroundCenter(
            new PointF(center.X - arrowLength, center.Y),
            center,
            direction - arrowAngle);

        g.DrawLine(arrowPen, center, first);
        g.DrawLine(arrowPen, center, second);
    }

    //-------------------------------------------------------------------------------
    // route の Y 高さに応じたグラデーション矢印を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawGradientRouteArrow(Graphics g, PointF start, PointF end, float startY, float endY, float stroke)
    {
        if (Math.Abs(start.X - end.X) < 0.001f && Math.Abs(start.Y - end.Y) < 0.001f)
        {
            return;
        }

        using LinearGradientBrush brush = new(start, end, GetRouteHeightColor(startY), GetRouteHeightColor(endY));
        using Pen arrowPen = new(brush, stroke)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        DrawRouteArrow(g, arrowPen, start, end);
    }

    //-------------------------------------------------------------------------------
    // route の Y 座標を表示色へ変換する処理
    //-------------------------------------------------------------------------------
    private static Color GetRouteHeightColor(float y)
    {
        const float nearGroundThreshold = 10f;
        const float fullHeightRange = 130f;

        Color groundColor = Color.FromArgb(255, 214, 30);
        if (Math.Abs(y) <= nearGroundThreshold)
        {
            return groundColor;
        }

        float amount = Math.Clamp((Math.Abs(y) - nearGroundThreshold) / fullHeightRange, 0f, 1f);
        Color targetColor = y > 0f
            ? Color.FromArgb(37, 116, 220)
            : Color.FromArgb(220, 38, 38);
        return BlendColor(groundColor, targetColor, amount);
    }

    //-------------------------------------------------------------------------------
    // 2色を指定割合で補間する処理
    //-------------------------------------------------------------------------------
    private static Color BlendColor(Color from, Color to, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        int r = (int)MathF.Round(from.R + ((to.R - from.R) * amount));
        int g = (int)MathF.Round(from.G + ((to.G - from.G) * amount));
        int b = (int)MathF.Round(from.B + ((to.B - from.B) * amount));
        return Color.FromArgb(r, g, b);
    }

    //-------------------------------------------------------------------------------
    // 指定中心を基準に2D座標を回転する処理
    //-------------------------------------------------------------------------------
    private static PointF RotatePointAroundCenter(PointF point, PointF center, float radians)
    {
        float relativeX = point.X - center.X;
        float relativeY = point.Y - center.Y;
        float cosine = MathF.Cos(radians);
        float sine = MathF.Sin(radians);

        return new PointF(
            (relativeX * cosine) - (relativeY * sine) + center.X,
            (relativeX * sine) + (relativeY * cosine) + center.Y);
    }

    //-------------------------------------------------------------------------------
    // 指定画面座標を現在のワールド座標へ逆変換する処理
    //-------------------------------------------------------------------------------
    private PointF ScreenToWorld(Point location)
    {
        using Matrix transform = BuildWorldToScreenMatrix(ClientRectangle);
        transform.Invert();
        PointF[] points = { new(location.X, location.Y) };
        transform.TransformPoints(points);
        return points[0];
    }

    //-------------------------------------------------------------------------------
    // ワールド座標を現在の画面座標へ変換する処理
    //-------------------------------------------------------------------------------
    private static PointF WorldToScreen(Matrix transform, PointF worldPoint)
    {
        PointF[] points = { worldPoint };
        transform.TransformPoints(points);
        return points[0];
    }

    //-------------------------------------------------------------------------------
    // 編集モードに合わせて表示するカーソルを返す処理
    //-------------------------------------------------------------------------------
    private static Cursor GetCursorForEditMode(UnitMapEditMode mode)
    {
        return mode switch
        {
            UnitMapEditMode.Navigate => Cursors.Default,
            UnitMapEditMode.MoveRouteWaypoint or
                UnitMapEditMode.MoveSpawn or
                UnitMapEditMode.MoveWaterbox or
                UnitMapEditMode.DeleteRouteWaypoint or
                UnitMapEditMode.DeleteSpawn or
                UnitMapEditMode.DeleteWaterbox or
                UnitMapEditMode.DeleteRouteLink or
                UnitMapEditMode.AddRouteWaypoint or
                UnitMapEditMode.AddSpawn or
                UnitMapEditMode.AddWaterbox or
                UnitMapEditMode.ConnectRouteWaypoint or
                UnitMapEditMode.RotateSpawn or
                UnitMapEditMode.ResizeSpawnRadius or
                UnitMapEditMode.ResizeRouteWaypointRadius => GetDeleteCursor(),
            _ => Cursors.Default
        };
    }

    //-------------------------------------------------------------------------------
    // 編集モード用の赤い十字カーソルを作成する処理
    //-------------------------------------------------------------------------------
    private static Cursor GetDeleteCursor()
    {
        if (s_deleteCursor is not null)
        {
            return s_deleteCursor;
        }

        Bitmap bitmap = new(32, 32);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using Pen outlinePen = new(Color.White, 5f);
        using Pen crossPen = new(Color.Red, 3f);
        graphics.DrawLine(outlinePen, 16, 2, 16, 30);
        graphics.DrawLine(outlinePen, 2, 16, 30, 16);
        graphics.DrawLine(crossPen, 16, 2, 16, 30);
        graphics.DrawLine(crossPen, 2, 16, 30, 16);
        s_deleteCursor = new Cursor(bitmap.GetHicon());
        return s_deleteCursor;
    }

    private static float DistanceSquaredToSegment(PointF point, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lengthSquared = (dx * dx) + (dy * dy);
        if (lengthSquared < 0.0001f)
        {
            float singleDx = point.X - start.X;
            float singleDy = point.Y - start.Y;
            return (singleDx * singleDx) + (singleDy * singleDy);
        }

        float t = (((point.X - start.X) * dx) + ((point.Y - start.Y) * dy)) / lengthSquared;
        t = Math.Clamp(t, 0f, 1f);
        float closestX = start.X + (dx * t);
        float closestY = start.Y + (dy * t);
        float closestDx = point.X - closestX;
        float closestDy = point.Y - closestY;
        return (closestDx * closestDx) + (closestDy * closestDy);
    }

    //-------------------------------------------------------------------------------
    // 選択中 waypoint を更新してイベント通知する処理
    //-------------------------------------------------------------------------------
    private void SetSelectedRouteWaypoint(int? waypointIndex)
    {
        if (_selectedRouteWaypointIndex == waypointIndex)
        {
            return;
        }

        _selectedRouteWaypointIndex = waypointIndex;
        RouteWaypointSelectionChanged?.Invoke(this, new RouteWaypointSelectionChangedEventArgs(waypointIndex));
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 選択中 spawn を更新してイベント通知する処理
    //-------------------------------------------------------------------------------
    private void SetSelectedSpawn(int? spawnIndex)
    {
        if (_selectedSpawnIndex == spawnIndex)
        {
            return;
        }

        _selectedSpawnIndex = spawnIndex;
        LayoutSpawnSelectionChanged?.Invoke(this, new LayoutSpawnSelectionChangedEventArgs(spawnIndex));
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 選択中 waterbox を更新してイベント通知する処理
    //-------------------------------------------------------------------------------
    private void SetSelectedWaterbox(int? waterboxIndex)
    {
        if (_selectedWaterboxIndex == waterboxIndex)
        {
            return;
        }

        _selectedWaterboxIndex = waterboxIndex;
        WaterboxSelectionChanged?.Invoke(this, new WaterboxSelectionChangedEventArgs(waterboxIndex));
        Invalidate();
    }

}

internal enum UnitMapEditMode
{
    Navigate,
    AddSpawn,
    AddRouteWaypoint,
    DeleteSpawn,
    DeleteRouteWaypoint,
    MoveRouteWaypoint,
    ConnectRouteWaypoint,
    DeleteRouteLink,
    MoveSpawn,
    RotateSpawn,
    ResizeSpawnRadius,
    ResizeRouteWaypointRadius,
    AddWaterbox,
    DeleteWaterbox,
    MoveWaterbox
}

internal enum WaterboxResizeHandle
{
    None,
    Left,
    Right,
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

internal sealed class RouteWaypointSelectionChangedEventArgs(int? waypointIndex) : EventArgs
{
    public int? WaypointIndex { get; } = waypointIndex;
}

internal sealed class RouteWaypointMovedEventArgs : EventArgs
{
    public RouteWaypointMovedEventArgs(int waypointIndex, float deltaX, float deltaZ)
        : this(waypointIndex, deltaX, deltaZ, 0f, false)
    {
    }

    public RouteWaypointMovedEventArgs(int waypointIndex, float deltaX, float deltaZ, float deltaY)
        : this(waypointIndex, deltaX, deltaZ, deltaY, true)
    {
    }

    private RouteWaypointMovedEventArgs(int waypointIndex, float deltaX, float deltaZ, float deltaY, bool movesHeight)
    {
        WaypointIndex = waypointIndex;
        DeltaX = deltaX;
        DeltaY = deltaY;
        DeltaZ = deltaZ;
        MovesHeight = movesHeight;
    }

    public int WaypointIndex { get; }

    public float DeltaX { get; }

    public float DeltaY { get; }

    public float DeltaZ { get; }

    public bool MovesHeight { get; }
}

internal sealed class RouteWaypointLinkedEventArgs(int fromWaypointIndex, int toWaypointIndex) : EventArgs
{
    public int FromWaypointIndex { get; } = fromWaypointIndex;
    public int ToWaypointIndex { get; } = toWaypointIndex;
}

internal sealed class LayoutSpawnSelectionChangedEventArgs(int? spawnIndex) : EventArgs
{
    public int? SpawnIndex { get; } = spawnIndex;
}

internal sealed class LayoutSpawnMovedEventArgs(int spawnIndex, float deltaX, float deltaZ) : EventArgs
{
    public int SpawnIndex { get; } = spawnIndex;

    public float DeltaX { get; } = deltaX;

    public float DeltaZ { get; } = deltaZ;
}

internal sealed class LayoutSpawnAngleChangedEventArgs(int spawnIndex, float angle) : EventArgs
{
    public int SpawnIndex { get; } = spawnIndex;

    public float Angle { get; } = angle;
}

internal sealed class LayoutSpawnRadiusChangedEventArgs(int spawnIndex, float radius) : EventArgs
{
    public int SpawnIndex { get; } = spawnIndex;

    public float Radius { get; } = radius;
}

internal sealed class RouteWaypointRadiusChangedEventArgs(int waypointIndex, float radius) : EventArgs
{
    public int WaypointIndex { get; } = waypointIndex;

    public float Radius { get; } = radius;
}

internal sealed class WaterboxSelectionChangedEventArgs(int? waterboxIndex) : EventArgs
{
    public int? WaterboxIndex { get; } = waterboxIndex;
}

internal sealed class WaterboxMovedEventArgs(int waterboxIndex, float deltaX, float deltaZ) : EventArgs
{
    public int WaterboxIndex { get; } = waterboxIndex;

    public float DeltaX { get; } = deltaX;

    public float DeltaZ { get; } = deltaZ;
}

internal sealed class WaterboxResizedEventArgs(int waterboxIndex, WaterboxResizeHandle handle, float deltaX, float deltaZ) : EventArgs
{
    public int WaterboxIndex { get; } = waterboxIndex;

    public WaterboxResizeHandle Handle { get; } = handle;

    public float DeltaX { get; } = deltaX;

    public float DeltaZ { get; } = deltaZ;
}

internal sealed class WaterboxHeightMovedEventArgs(int waterboxIndex, float deltaY) : EventArgs
{
    public int WaterboxIndex { get; } = waterboxIndex;

    public float DeltaY { get; } = deltaY;
}

internal sealed class MapPointPlacementRequestedEventArgs(UnitMapEditMode editMode, float x, float z) : EventArgs
{
    public UnitMapEditMode EditMode { get; } = editMode;

    public float X { get; } = x;

    public float Z { get; } = z;
}

internal sealed class MapPointDeletionRequestedEventArgs(UnitMapEditMode editMode, int pointIndex) : EventArgs
{
    public UnitMapEditMode EditMode { get; } = editMode;

    public int PointIndex { get; } = pointIndex;
}
