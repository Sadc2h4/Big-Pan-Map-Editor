using System.Drawing.Drawing2D;

namespace PikminUnitEditor;

internal sealed class ObjModelView : Control
{
    private const float MinZoom = 0.05f;
    private const float MaxZoom = 25f;
    private const float TopDownPitch = 1.5608f;
    private const float TopDownYaw = 3.1415927f;
    private const float RouteHitRadiusPixels = 24f;
    private const float SpawnHitRadiusPixels = 24f;
    private const float SpawnAngleHandleDistancePixels = 25f;
    private const float SpawnAngleHandleSizePixels = 10f;
    private static Cursor? s_deleteCursor;

    private ObjScene? _scene;
    private string? _sceneName;
    private ModelBounds _bounds;
    private LayoutFile _layout = new(Array.Empty<LayoutSpawn>());
    private RouteFile _route = new(new Dictionary<int, RouteWaypoint>());
    private IReadOnlyDictionary<int, float> _routeColorHeights = new Dictionary<int, float>();
    private WaterboxFile _waterbox = new(0, Array.Empty<WaterboxEntry>());
    private UnitMapEditMode _editMode;
    private bool _showRadius = true;
    private bool _useFieldObjectIcons;
    private int? _selectedSpawnIndex;
    private int? _selectedRouteWaypointIndex;
    private int? _selectedWaterboxIndex;
    private float _overlayHeightOffset;
    private bool _leftButtonDown;
    private bool _middleButtonDown;
    private bool _rightButtonDown;
    private bool _draggingSpawn;
    private bool _draggingWaypoint;
    private bool _draggingWaterbox;
    private bool _resizingWaterbox;
    private bool _rotatingSpawn;
    private bool _resizingPointRadius;
    private WaterboxResizeHandle _waterboxResizeHandle;
    private int? _linkingRouteWaypointIndex;
    private Point _linkPreviewPoint;
    private Point _lastMousePoint;
    private float _dragPlaneY;
    private float _yaw = TopDownYaw;
    private float _pitch = TopDownPitch;
    private float _zoom = 1.1f;
    private float _panX;
    private float _panY;

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
    public event EventHandler<WaterboxHeightMovedEventArgs>? WaterboxHeightMoved;
    public event EventHandler? OverlayDragStarted;
    public event EventHandler? OverlayDragEnded;
    public event EventHandler<MapPointPlacementRequestedEventArgs>? MapPointPlacementRequested;
    public event EventHandler<MapPointDeletionRequestedEventArgs>? MapPointDeletionRequested;

    public float OverlayHeightOffset => _overlayHeightOffset;

    public ObjModelView()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(232, 227, 213);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
    }

    //-------------------------------------------------------------------------------
    // 読み込み済み OBJ シーンと overlay データをビューへ設定する処理
    //-------------------------------------------------------------------------------
    public void SetScene(ObjScene? scene, string? sceneName, LayoutFile layout, RouteFile route, WaterboxFile waterbox, bool resetView = true)
    {
        _scene = scene;
        _sceneName = sceneName;
        _layout = layout;
        _route = route;
        _waterbox = waterbox;
        _bounds = scene is null ? default : ModelBounds.FromScene(scene);
        _overlayHeightOffset = scene is null ? 0f : EstimateOverlayHeightOffset(scene, layout, route);

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
        _draggingSpawn = false;
        _draggingWaypoint = false;
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

    //-------------------------------------------------------------------------------
    // 選択中 waypoint を外部から指定する処理
    //-------------------------------------------------------------------------------
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

    //-------------------------------------------------------------------------------
    // 選択中 spawn を外部から指定する処理
    //-------------------------------------------------------------------------------
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
    // 視点状態を初期値へ戻す処理
    //-------------------------------------------------------------------------------
    public void ResetView()
    {
        _yaw = TopDownYaw;
        _pitch = TopDownPitch;
        _zoom = 1.1f;
        _panX = 0f;
        _panY = 0f;
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 背景とモデルと overlay を再描画する処理
    //-------------------------------------------------------------------------------
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.Clear(BackColor);

        DrawBackdrop(e.Graphics);

        if (_scene is not null && _scene.Vertices.Count > 0 && _scene.Faces.Count > 0)
        {
            DrawScene(e.Graphics);
        }
        else
        {
            DrawEmptyState(e.Graphics);
            DrawWaterboxOverlay(e.Graphics);
        }

        DrawRouteOverlay(e.Graphics);
        DrawSpawnOverlay(e.Graphics);
        DrawSelectedWaterboxResizeHandles(e.Graphics);
        DrawHud(e.Graphics);
    }

    //-------------------------------------------------------------------------------
    // マウス押下時に選択・移動開始・視点操作開始を行う処理
    //-------------------------------------------------------------------------------
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        _lastMousePoint = e.Location;

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
                if (TryGetPlacementWorldPoint(e.Location, out Vector3 placementPoint))
                {
                    MapPointPlacementRequested?.Invoke(
                        this,
                        new MapPointPlacementRequestedEventArgs(_editMode, placementPoint.X, placementPoint.Z));
                }

                _leftButtonDown = false;
                Capture = false;
                return;
            }

            int? hitWaypoint = HitTestRouteWaypoint(e.Location);
            int? hitSpawn = hitWaypoint is null ? HitTestLayoutSpawn(e.Location) : null;
            int? hitHandleWaterbox = null;
            WaterboxResizeHandle hitWaterboxHandle = hitWaypoint is null && hitSpawn is null && _editMode == UnitMapEditMode.MoveWaterbox
                ? HitTestWaterboxHandle(e.Location, out hitHandleWaterbox)
                : WaterboxResizeHandle.None;
            int? hitWaterbox = hitWaypoint is null && hitSpawn is null && hitWaterboxHandle == WaterboxResizeHandle.None ? HitTestWaterbox(e.Location) : null;
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
                    _linkPreviewPoint = e.Location;
                }
                else if (_editMode == UnitMapEditMode.MoveRouteWaypoint &&
                    _route.Waypoints.TryGetValue(hitWaypoint.Value, out RouteWaypoint? waypoint))
                {
                    _draggingWaypoint = true;
                    _dragPlaneY = GetOverlayWorldY(waypoint.Y);
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                }
                else if (_editMode == UnitMapEditMode.ResizeRouteWaypointRadius &&
                    _route.Waypoints.TryGetValue(hitWaypoint.Value, out RouteWaypoint? radiusWaypoint))
                {
                    _resizingPointRadius = true;
                    _dragPlaneY = GetOverlayWorldY(radiusWaypoint.Y);
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSelectedRadiusFromScreenPoint(e.Location);
                }
            }
            else if (hitSpawn is not null)
            {
                SetSelectedSpawn(hitSpawn);
                SetSelectedRouteWaypoint(null);
                SetSelectedWaterbox(null);
                if (_editMode == UnitMapEditMode.MoveSpawn &&
                    hitSpawn.Value >= 0 &&
                    hitSpawn.Value < _layout.Spawns.Count)
                {
                    _draggingSpawn = true;
                    _dragPlaneY = GetOverlayWorldY(_layout.Spawns[hitSpawn.Value].Y);
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                }
                else if (_editMode == UnitMapEditMode.ResizeSpawnRadius &&
                    hitSpawn.Value >= 0 &&
                    hitSpawn.Value < _layout.Spawns.Count)
                {
                    _resizingPointRadius = true;
                    _dragPlaneY = GetOverlayWorldY(_layout.Spawns[hitSpawn.Value].Y);
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSelectedRadiusFromScreenPoint(e.Location);
                }
            }
            else if (hitWaterbox is not null)
            {
                SetSelectedWaterbox(hitWaterbox);
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                _draggingWaterbox = _editMode == UnitMapEditMode.MoveWaterbox;
                if (hitWaterbox.Value >= 0 && hitWaterbox.Value < _waterbox.Boxes.Count)
                {
                    _dragPlaneY = _waterbox.Boxes[hitWaterbox.Value].MaxY + _overlayHeightOffset;
                    if (_draggingWaterbox)
                    {
                        OverlayDragStarted?.Invoke(this, EventArgs.Empty);
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
                _dragPlaneY = _waterbox.Boxes[hitHandleWaterbox.Value].MaxY + _overlayHeightOffset;
                OverlayDragStarted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                SetSelectedWaterbox(null);
            }
        }
        else if (e.Button == MouseButtons.Middle)
        {
            _middleButtonDown = true;
            Capture = true;
            Cursor = Cursors.Hand;
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
                _dragPlaneY = GetOverlayWorldY(_layout.Spawns[hitSpawn.Value].Y);
                if (_editMode == UnitMapEditMode.ResizeSpawnRadius)
                {
                    _resizingPointRadius = true;
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSelectedRadiusFromScreenPoint(e.Location);
                    Cursor = Cursors.Cross;
                }
                else
                {
                    _rotatingSpawn = true;
                    OverlayDragStarted?.Invoke(this, EventArgs.Empty);
                    UpdateSpawnAngleFromScreenPoint(e.Location);
                    Cursor = Cursors.Cross;
                }
            }
            else
            {
                SetSelectedRouteWaypoint(null);
                SetSelectedSpawn(null);
                SetSelectedWaterbox(null);
                _rotatingSpawn = false;
                _resizingPointRadius = false;
                Cursor = Cursors.SizeAll;
            }
        }
    }

    //-------------------------------------------------------------------------------
    // マウス移動時に視点操作または overlay 移動を行う処理
    //-------------------------------------------------------------------------------
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int diffX = e.X - _lastMousePoint.X;
        int diffY = e.Y - _lastMousePoint.Y;

        if (_rightButtonDown && _rotatingSpawn && _selectedSpawnIndex is not null)
        {
            UpdateSpawnAngleFromScreenPoint(e.Location);
        }
        else if (_rightButtonDown && _resizingPointRadius)
        {
            UpdateSelectedRadiusFromScreenPoint(e.Location);
        }
        else if (_rightButtonDown)
        {
            _yaw += diffX * 0.01f;
            _pitch = Math.Clamp(_pitch + (diffY * 0.01f), -TopDownPitch, TopDownPitch);
            Invalidate();
        }
        else if (_middleButtonDown)
        {
            _panX += diffX;
            _panY += diffY;
            Invalidate();
        }
        else if (_leftButtonDown && _linkingRouteWaypointIndex is not null)
        {
            _linkPreviewPoint = e.Location;
            Invalidate();
        }
        else if (_leftButtonDown && _draggingWaypoint && _selectedRouteWaypointIndex is not null)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                float deltaY = -diffY / Math.Max(_zoom, 0.1f);
                if (Math.Abs(deltaY) > 0.0001f)
                {
                    RouteWaypointMoved?.Invoke(
                        this,
                        new RouteWaypointMovedEventArgs(_selectedRouteWaypointIndex.Value, 0f, 0f, deltaY));
                }

                _lastMousePoint = e.Location;
                return;
            }

            if (_route.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint) &&
                TryGetScreenDragWorldDelta(
                    _lastMousePoint,
                    e.Location,
                    waypoint.X,
                    GetOverlayWorldY(waypoint.Y),
                    waypoint.Z,
                    out float deltaX,
                    out float deltaZ))
            {
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
        }
        else if (_leftButtonDown && _draggingSpawn && _selectedSpawnIndex is not null)
        {
            if (_selectedSpawnIndex.Value >= 0 &&
                _selectedSpawnIndex.Value < _layout.Spawns.Count)
            {
                LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
                if (TryGetScreenDragWorldDelta(
                    _lastMousePoint,
                    e.Location,
                    spawn.X,
                    GetOverlayWorldY(spawn.Y),
                    spawn.Z,
                    out float deltaX,
                    out float deltaZ) &&
                    (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f))
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
        }
        else if (_leftButtonDown && _draggingWaterbox && _selectedWaterboxIndex is not null)
        {
            if (_selectedWaterboxIndex.Value >= 0 &&
                _selectedWaterboxIndex.Value < _waterbox.Boxes.Count)
            {
                WaterboxEntry box = _waterbox.Boxes[_selectedWaterboxIndex.Value];
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    float deltaY = -diffY / Math.Max(_zoom, 0.1f);
                    if (Math.Abs(deltaY) > 0.0001f)
                    {
                        WaterboxHeightMoved?.Invoke(this, new WaterboxHeightMovedEventArgs(_selectedWaterboxIndex.Value, deltaY));
                    }

                    _lastMousePoint = e.Location;
                    return;
                }

                float centerX = (box.MinX + box.MaxX) * 0.5f;
                float centerZ = (box.MinZ + box.MaxZ) * 0.5f;
                if (TryGetScreenDragWorldDelta(
                    _lastMousePoint,
                    e.Location,
                    centerX,
                    box.MaxY + _overlayHeightOffset,
                    centerZ,
                    out float deltaX,
                    out float deltaZ) &&
                    (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f))
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

                    WaterboxMoved?.Invoke(this, new WaterboxMovedEventArgs(_selectedWaterboxIndex.Value, deltaX, deltaZ));
                }
            }
        }
        else if (_leftButtonDown && _resizingWaterbox && _selectedWaterboxIndex is not null)
        {
            if (_selectedWaterboxIndex.Value >= 0 &&
                _selectedWaterboxIndex.Value < _waterbox.Boxes.Count)
            {
                WaterboxEntry box = _waterbox.Boxes[_selectedWaterboxIndex.Value];
                PointF handlePoint = GetWaterboxHandleWorldPoint(box, _waterboxResizeHandle);
                if (TryGetScreenDragWorldDelta(
                    _lastMousePoint,
                    e.Location,
                    handlePoint.X,
                    box.MaxY + _overlayHeightOffset,
                    handlePoint.Y,
                    out float deltaX,
                    out float deltaZ) &&
                    (Math.Abs(deltaX) > 0.0001f || Math.Abs(deltaZ) > 0.0001f))
                {
                    WaterboxResized?.Invoke(this, new WaterboxResizedEventArgs(_selectedWaterboxIndex.Value, _waterboxResizeHandle, deltaX, deltaZ));
                }
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

        _lastMousePoint = e.Location;
    }

    //-------------------------------------------------------------------------------
    // マウス解放時にドラッグ状態を解除する処理
    //-------------------------------------------------------------------------------
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left)
        {
            bool endedOverlayDrag = _draggingSpawn || _draggingWaypoint || _draggingWaterbox || _resizingWaterbox || _rotatingSpawn || _resizingPointRadius;
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
            _draggingSpawn = false;
            _draggingWaypoint = false;
            _draggingWaterbox = false;
            _resizingWaterbox = false;
            _rotatingSpawn = false;
            _resizingPointRadius = false;
            _waterboxResizeHandle = WaterboxResizeHandle.None;
            _linkingRouteWaypointIndex = null;
            if (endedOverlayDrag)
            {
                OverlayDragEnded?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }
        else if (e.Button == MouseButtons.Middle)
        {
            _middleButtonDown = false;
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

        if (!_leftButtonDown && !_middleButtonDown && !_rightButtonDown)
        {
            Capture = false;
            Cursor = GetCursorForEditMode(_editMode);
        }
    }

    //-------------------------------------------------------------------------------
    // ホイール操作でズーム倍率を変更する処理
    //-------------------------------------------------------------------------------
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        float zoomFactor = e.Delta > 0 ? 1.15f : 1f / 1.15f;
        _zoom = Math.Clamp(_zoom * zoomFactor, MinZoom, MaxZoom);
        Invalidate();
    }

    //-------------------------------------------------------------------------------
    // 背景グラデーションと補助線を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawBackdrop(Graphics graphics)
    {
        Rectangle bounds = ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        using LinearGradientBrush brush = new(
            bounds,
            Color.FromArgb(247, 244, 236),
            Color.FromArgb(220, 214, 198),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(brush, bounds);

        using Pen gridPen = new(Color.FromArgb(215, 208, 191), 1f);
        for (int x = 0; x < bounds.Width; x += 64)
        {
            graphics.DrawLine(gridPen, x, 0, x, bounds.Height);
        }

        for (int y = 0; y < bounds.Height; y += 64)
        {
            graphics.DrawLine(gridPen, 0, y, bounds.Width, y);
        }
    }

    //-------------------------------------------------------------------------------
    // シーン未読込時のガイド表示を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawEmptyState(Graphics graphics)
    {
        using Font titleFont = new("Yu Gothic UI", 18f, FontStyle.Bold);
        using Font bodyFont = new("Yu Gothic UI", 10f, FontStyle.Regular);
        using SolidBrush titleBrush = new(Color.FromArgb(56, 70, 80));
        using SolidBrush bodyBrush = new(Color.FromArgb(96, 96, 96));

        graphics.DrawString("OBJ を読み込めませんでした．", titleFont, titleBrush, new PointF(32, 28));
        graphics.DrawString("右ドラッグ: 回転  /  中ドラッグ: 平行移動  /  ホイール: ズーム", bodyFont, bodyBrush, new PointF(32, 84));
    }

    //-------------------------------------------------------------------------------
    // 現在の OBJ シーンを簡易 3D 描画する処理
    //-------------------------------------------------------------------------------
    private void DrawScene(Graphics graphics)
    {
        CameraState camera = GetCameraState();
        Vector3 lightDirection = Vector3.Normalize(new Vector3(-0.45f, 0.85f, 0.35f));
        List<ProjectedTriangle> triangles = new();

        foreach (ObjFace face in _scene!.Faces)
        {
            if (face.Indices.Count < 3)
            {
                continue;
            }

            Vector3 origin = TransformWorldToCamera(_scene.Vertices[face.Indices[0].Vertex]);
            for (int i = 1; i < face.Indices.Count - 1; i++)
            {
                Vector3 second = TransformWorldToCamera(_scene.Vertices[face.Indices[i].Vertex]);
                Vector3 third = TransformWorldToCamera(_scene.Vertices[face.Indices[i + 1].Vertex]);

                if (!TryProjectTriangle(origin, second, third, camera, out PointF[]? points, out float depth))
                {
                    continue;
                }

                Vector3 normal = Vector3.Normalize(Vector3.Cross(second - origin, third - origin));
                float lightStrength = Math.Clamp(0.25f + (MathF.Abs(Vector3.Dot(normal, lightDirection)) * 0.75f), 0.25f, 1f);
                Color baseColor = ResolveFaceColor(face.MaterialName);
                Color shadedColor = ShadeColor(baseColor, lightStrength);
                triangles.Add(new ProjectedTriangle(points!, depth, shadedColor, Color.FromArgb(92, 54, 48, 40), 1f));
            }
        }

        AddWaterboxProjectedPolygons(triangles, camera);

        foreach (ProjectedTriangle triangle in triangles.OrderBy(t => t.Depth))
        {
            using SolidBrush fillBrush = new(triangle.Color);
            using Pen edgePen = new(triangle.EdgeColor, triangle.EdgeWidth);
            graphics.FillPolygon(fillBrush, triangle.Points);
            graphics.DrawPolygon(edgePen, triangle.Points);
        }
    }

    //-------------------------------------------------------------------------------
    // waterbox 上面を地形と同じ深度ソート対象へ追加する処理
    //-------------------------------------------------------------------------------
    private void AddWaterboxProjectedPolygons(List<ProjectedTriangle> triangles, CameraState camera)
    {
        if (_waterbox.Boxes.Count == 0)
        {
            return;
        }

        for (int index = 0; index < _waterbox.Boxes.Count; index++)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            float y = box.MaxY + _overlayHeightOffset;
            Vector3[] cameraPoints =
            {
                TransformWorldToCamera(box.MinX, y, box.MinZ),
                TransformWorldToCamera(box.MaxX, y, box.MinZ),
                TransformWorldToCamera(box.MaxX, y, box.MaxZ),
                TransformWorldToCamera(box.MinX, y, box.MaxZ)
            };
            PointF[] points = new PointF[4];
            float depth = 0f;
            bool projected = true;
            for (int i = 0; i < cameraPoints.Length; i++)
            {
                if (!TryProjectPoint(cameraPoints[i], camera, out points[i], out float pointDepth))
                {
                    projected = false;
                    break;
                }

                depth += pointDepth;
            }

            if (!projected)
            {
                continue;
            }

            depth /= points.Length;
            Color fillColor = index == _selectedWaterboxIndex
                ? Color.FromArgb(92, 0, 150, 255)
                : Color.FromArgb(48, 30, 100, 255);
            Color edgeColor = index == _selectedWaterboxIndex
                ? Color.FromArgb(255, 0, 106, 180)
                : Color.FromArgb(190, 30, 100, 255);
            triangles.Add(new ProjectedTriangle(points, depth, fillColor, edgeColor, 2.2f));
        }
    }

    //-------------------------------------------------------------------------------
    // waterbox の上面矩形を 3D overlay として描画する処理
    //-------------------------------------------------------------------------------
    private void DrawWaterboxOverlay(Graphics graphics)
    {
        if (_waterbox.Boxes.Count == 0)
        {
            return;
        }

        for (int index = 0; index < _waterbox.Boxes.Count; index++)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            float y = box.MaxY + _overlayHeightOffset;
            PointF[] points = new PointF[4];
            if (!TryProjectWorldPoint(box.MinX, y, box.MinZ, out points[0], out _) ||
                !TryProjectWorldPoint(box.MaxX, y, box.MinZ, out points[1], out _) ||
                !TryProjectWorldPoint(box.MaxX, y, box.MaxZ, out points[2], out _) ||
                !TryProjectWorldPoint(box.MinX, y, box.MaxZ, out points[3], out _))
            {
                continue;
            }

            using SolidBrush fillBrush = new(index == _selectedWaterboxIndex
                ? Color.FromArgb(92, 0, 150, 255)
                : Color.FromArgb(48, 30, 100, 255));
            using Pen edgePen = new(index == _selectedWaterboxIndex
                ? Color.FromArgb(255, 0, 106, 180)
                : Color.FromArgb(190, 30, 100, 255), 2.2f);
            graphics.FillPolygon(fillBrush, points);
            graphics.DrawPolygon(edgePen, points);
        }
    }

    //-------------------------------------------------------------------------------
    // 選択中 waterbox のサイズ変更ハンドルを 3D overlay 上に描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSelectedWaterboxResizeHandles(Graphics graphics)
    {
        if (_selectedWaterboxIndex is null ||
            _selectedWaterboxIndex.Value < 0 ||
            _selectedWaterboxIndex.Value >= _waterbox.Boxes.Count)
        {
            return;
        }

        WaterboxEntry box = _waterbox.Boxes[_selectedWaterboxIndex.Value];
        float y = box.MaxY + _overlayHeightOffset;
        using SolidBrush handleBrush = new(Color.White);
        using Pen handlePen = new(Color.FromArgb(255, 0, 106, 180), 1.8f);
        foreach (PointF worldPoint in GetWaterboxHandlePoints(box).Values)
        {
            if (!TryProjectWorldPoint(worldPoint.X, y, worldPoint.Y, out PointF screenPoint, out _))
            {
                continue;
            }

            const float size = 10f;
            RectangleF handleBounds = new(screenPoint.X - (size * 0.5f), screenPoint.Y - (size * 0.5f), size, size);
            graphics.FillRectangle(handleBrush, handleBounds);
            graphics.DrawRectangle(handlePen, handleBounds.X, handleBounds.Y, handleBounds.Width, handleBounds.Height);
        }
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

        const float hitSize = 9f;
        for (int index = _waterbox.Boxes.Count - 1; index >= 0; index--)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            float y = box.MaxY + _overlayHeightOffset;
            foreach ((WaterboxResizeHandle handle, PointF worldPoint) in GetWaterboxHandlePoints(box))
            {
                if (!TryProjectWorldPoint(worldPoint.X, y, worldPoint.Y, out PointF screenPoint, out _))
                {
                    continue;
                }

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

    //-------------------------------------------------------------------------------
    // waterbox の指定ハンドル位置を X/Z 座標で取得する処理
    //-------------------------------------------------------------------------------
    private static PointF GetWaterboxHandleWorldPoint(WaterboxEntry box, WaterboxResizeHandle handle)
    {
        return GetWaterboxHandlePoints(box).TryGetValue(handle, out PointF point)
            ? point
            : new PointF((box.MinX + box.MaxX) * 0.5f, (box.MinZ + box.MaxZ) * 0.5f);
    }

    //-------------------------------------------------------------------------------
    // waterbox の四隅と辺中央のハンドル位置を X/Z 座標で作成する処理
    //-------------------------------------------------------------------------------
    private static IReadOnlyDictionary<WaterboxResizeHandle, PointF> GetWaterboxHandlePoints(WaterboxEntry box)
    {
        float centerX = (box.MinX + box.MaxX) * 0.5f;
        float centerZ = (box.MinZ + box.MaxZ) * 0.5f;
        return new Dictionary<WaterboxResizeHandle, PointF>
        {
            [WaterboxResizeHandle.TopLeft] = new(box.MinX, box.MinZ),
            [WaterboxResizeHandle.Top] = new(centerX, box.MinZ),
            [WaterboxResizeHandle.TopRight] = new(box.MaxX, box.MinZ),
            [WaterboxResizeHandle.Right] = new(box.MaxX, centerZ),
            [WaterboxResizeHandle.BottomRight] = new(box.MaxX, box.MaxZ),
            [WaterboxResizeHandle.Bottom] = new(centerX, box.MaxZ),
            [WaterboxResizeHandle.BottomLeft] = new(box.MinX, box.MaxZ),
            [WaterboxResizeHandle.Left] = new(box.MinX, centerZ)
        };
    }

    //-------------------------------------------------------------------------------
    // route の接続線と waypoint を 3D overlay として描画する処理
    //-------------------------------------------------------------------------------
    private void DrawRouteOverlay(Graphics graphics)
    {
        if (_route.Waypoints.Count == 0)
        {
            return;
        }

        Dictionary<int, PointF> screenPoints = new();
        using Pen routeOutlinePen = new(Color.FromArgb(230, 20, 20, 20), 5f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using Pen routePen = new(Color.FromArgb(235, 224, 31, 31), 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using Pen arrowOutlinePen = new(Color.FromArgb(240, 20, 20, 20), 5f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        HashSet<(int From, int To)> drawnEdges = new();

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (TryProjectWorldPoint(waypoint.X, GetOverlayWorldY(waypoint.Y), waypoint.Z, out PointF point, out _))
            {
                screenPoints[waypoint.Index] = point;
            }
        }

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (!screenPoints.TryGetValue(waypoint.Index, out PointF from))
            {
                continue;
            }

            foreach (int link in waypoint.Links)
            {
                if (!screenPoints.TryGetValue(link, out PointF to) ||
                    !_route.Waypoints.TryGetValue(link, out RouteWaypoint? target))
                {
                    continue;
                }

                (int From, int To) edgeKey = waypoint.Index < link
                    ? (waypoint.Index, link)
                    : (link, waypoint.Index);
                if (drawnEdges.Add(edgeKey))
                {
                    graphics.DrawLine(routeOutlinePen, from, to);
                    graphics.DrawLine(routePen, from, to);
                }
            }
        }

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (!screenPoints.TryGetValue(waypoint.Index, out PointF from))
            {
                continue;
            }

            foreach (int link in waypoint.Links)
            {
                if (!screenPoints.TryGetValue(link, out PointF to) ||
                    !_route.Waypoints.TryGetValue(link, out RouteWaypoint? target))
                {
                    continue;
                }

                DrawScreenRouteArrow(graphics, arrowOutlinePen, from, to);
                DrawGradientScreenRouteArrow(graphics, from, to, GetRouteColorHeight(waypoint), GetRouteColorHeight(target), 3f);
            }
        }

        if (_linkingRouteWaypointIndex is not null &&
            screenPoints.TryGetValue(_linkingRouteWaypointIndex.Value, out PointF previewFrom))
        {
            using Pen previewPen = new(Color.FromArgb(230, 25, 118, 210), 2.5f)
            {
                DashStyle = DashStyle.Dash
            };
            graphics.DrawLine(previewPen, previewFrom, _linkPreviewPoint);
        }

        using Font labelFont = new("Yu Gothic UI", 8f, FontStyle.Bold);
        foreach ((int index, PointF point) in screenPoints.OrderBy(entry => entry.Key))
        {
            bool isSelected = _selectedRouteWaypointIndex == index;
            if (_route.Waypoints.TryGetValue(index, out RouteWaypoint? waypoint))
            {
                if (_showRadius)
                {
                    DrawProjectedRadiusCircle(
                        graphics,
                        waypoint.X,
                        GetOverlayWorldY(waypoint.Y),
                        waypoint.Z,
                        waypoint.Radius,
                        Color.FromArgb(230, 255, 132, 0));
                }
            }

            float radius = isSelected ? 9f : 7f;
            using SolidBrush fillBrush = new(Color.White);
            using Pen edgePen = new(isSelected ? Color.FromArgb(255, 111, 0) : Color.DarkRed, isSelected ? 2f : 1.5f);
            graphics.FillEllipse(fillBrush, point.X - radius, point.Y - radius, radius * 2f, radius * 2f);
            graphics.DrawEllipse(edgePen, point.X - radius, point.Y - radius, radius * 2f, radius * 2f);
            DrawPointText(graphics, index.ToString(), new PointF(point.X + 9f, point.Y - 17f), Color.White, labelFont);
        }
    }

    //-------------------------------------------------------------------------------
    // route 矢印の色分けに使う Y 値を取得する処理
    //-------------------------------------------------------------------------------
    private float GetRouteColorHeight(RouteWaypoint waypoint)
    {
        return _routeColorHeights.TryGetValue(waypoint.Index, out float height) ? height : waypoint.Y;
    }

    //-------------------------------------------------------------------------------
    // spawn の位置と半径を 3D overlay として描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSpawnOverlay(Graphics graphics)
    {
        if (_layout.Spawns.Count == 0)
        {
            return;
        }

        using Font labelFont = new("Yu Gothic UI", 8f, FontStyle.Bold);
        for (int index = 0; index < _layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            if (!TryProjectWorldPoint(spawn.X, GetOverlayWorldY(spawn.Y), spawn.Z, out PointF point, out _))
            {
                continue;
            }

            bool isSelected = _selectedSpawnIndex == index;
            if (_useFieldObjectIcons && FieldObjectIconCatalog.GetFootprintSize(spawn.TypeLabel) is SizeF footprintSize)
            {
                DrawProjectedFieldFootprint(graphics, spawn, footprintSize);
            }

            if (_showRadius)
            {
                DrawProjectedRadiusCircle(
                    graphics,
                    spawn.X,
                    GetOverlayWorldY(spawn.Y),
                    spawn.Z,
                    spawn.Radius,
                    Color.FromArgb(230, 255, 132, 0));
            }

            const float iconSize = 32f;
            RectangleF iconBounds = new(point.X - iconSize * 0.5f, point.Y - iconSize * 0.5f, iconSize, iconSize);
            Image? fieldIcon = _useFieldObjectIcons ? FieldObjectIconCatalog.GetIcon(spawn.TypeLabel) : null;
            Image? icon = fieldIcon ?? SpawnIconCatalog.GetIcon(spawn.TypeId);
            if (icon is not null)
            {
                graphics.DrawImage(icon, iconBounds);
            }
            else
            {
                using SolidBrush fillBrush = new(GetSpawnColor(spawn.TypeId));
                graphics.FillEllipse(fillBrush, iconBounds);
            }

            using Pen edgePen = new(isSelected ? Color.FromArgb(255, 111, 0) : Color.White, isSelected ? 2.5f : 1.5f);
            if (fieldIcon is not null)
            {
                graphics.DrawRectangle(edgePen, iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height);
            }
            else
            {
                graphics.DrawEllipse(edgePen, iconBounds);
            }
            DrawSpawnAngleHandle(graphics, spawn, point, isSelected);
            DrawPointText(graphics, index.ToString(), new PointF(point.X + 15f, point.Y - 17f), Color.White, labelFont);
        }

        DrawSelectedSpawnAngleArrow(graphics);
    }

    //-------------------------------------------------------------------------------
    // 地上 object の向き付き矩形を 3D overlay 上に投影して描画する処理
    //-------------------------------------------------------------------------------
    private void DrawProjectedFieldFootprint(Graphics graphics, LayoutSpawn spawn, SizeF size)
    {
        PointF[] worldPoints = GetOrientedFootprintPoints(spawn, size);
        PointF[] screenPoints = new PointF[worldPoints.Length];
        float worldY = GetOverlayWorldY(spawn.Y);
        for (int i = 0; i < worldPoints.Length; i++)
        {
            if (!TryProjectWorldPoint(worldPoints[i].X, worldY, worldPoints[i].Y, out screenPoints[i], out _))
            {
                return;
            }
        }

        Color footprintColor = FieldObjectIconCatalog.GetFootprintColor(spawn.TypeLabel);
        using SolidBrush brush = new(footprintColor);
        using Pen pen = new(Color.FromArgb(Math.Min(220, footprintColor.A + 110), footprintColor.R, footprintColor.G, footprintColor.B), 1.8f);
        graphics.FillPolygon(brush, screenPoints);
        graphics.DrawPolygon(pen, screenPoints);
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
    // 指定位置の Radius 円を 3D overlay 上に投影して描画する処理
    //-------------------------------------------------------------------------------
    private void DrawProjectedRadiusCircle(Graphics graphics, float x, float y, float z, float radius, Color color)
    {
        if (radius <= 0.01f)
        {
            return;
        }

        const int segmentCount = 48;
        List<PointF> points = new(segmentCount + 1);
        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = (MathF.PI * 2f * i) / segmentCount;
            float px = x + (MathF.Cos(angle) * radius);
            float pz = z + (MathF.Sin(angle) * radius);
            if (!TryProjectWorldPoint(px, y, pz, out PointF projected, out _))
            {
                return;
            }

            points.Add(projected);
        }

        using Pen pen = new(color, 2.2f);
        graphics.DrawLines(pen, points.ToArray());
    }

    //-------------------------------------------------------------------------------
    // スポーン外周の角度変更ハンドルを描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSpawnAngleHandle(Graphics graphics, LayoutSpawn spawn, PointF center, bool isSelected)
    {
        if (!TryGetSpawnAngleScreenDirection(spawn, center, out PointF direction))
        {
            return;
        }

        PointF[] points = GetSpawnAngleHandlePoints(center, direction);
        using SolidBrush fillBrush = new(isSelected ? Color.FromArgb(255, 111, 0) : Color.FromArgb(255, 150, 32));
        using Pen outlinePen = new(Color.FromArgb(230, 20, 20, 20), 1.6f);
        graphics.FillPolygon(fillBrush, points);
        graphics.DrawPolygon(outlinePen, points);
    }

    //-------------------------------------------------------------------------------
    // 選択中 Spawn の角度方向を 3D overlay 上に矢印で描画する処理
    //-------------------------------------------------------------------------------
    private void DrawSelectedSpawnAngleArrow(Graphics graphics)
    {
        if (_editMode != UnitMapEditMode.RotateSpawn ||
            _selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _layout.Spawns.Count)
        {
            return;
        }

        LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
        float radians = spawn.Angle * MathF.PI / 180f;
        float length = Math.Max(spawn.Radius, 80f);
        float worldY = GetOverlayWorldY(spawn.Y);
        if (!TryProjectWorldPoint(spawn.X, worldY, spawn.Z, out PointF start, out _) ||
            !TryProjectWorldPoint(
                spawn.X + (MathF.Sin(radians) * length),
                worldY,
                spawn.Z + (MathF.Cos(radians) * length),
                out PointF end,
                out _))
        {
            return;
        }

        using Pen outlinePen = new(Color.FromArgb(235, 20, 20, 20), 5f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using Pen arrowPen = new(Color.FromArgb(255, 111, 0), 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        graphics.DrawLine(outlinePen, start, end);
        graphics.DrawLine(arrowPen, start, end);
        DrawScreenRouteArrow(graphics, outlinePen, start, end);
        DrawScreenRouteArrow(graphics, arrowPen, start, end);
    }

    //-------------------------------------------------------------------------------
    // 画面座標から選択中 Spawn の角度を更新する処理
    //-------------------------------------------------------------------------------
    private void UpdateSpawnAngleFromScreenPoint(Point location)
    {
        if (_selectedSpawnIndex is null ||
            _selectedSpawnIndex.Value < 0 ||
            _selectedSpawnIndex.Value >= _layout.Spawns.Count ||
            !TryGetPlaneIntersection(location, _dragPlaneY, out Vector3 worldPoint))
        {
            return;
        }

        LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
        float deltaX = worldPoint.X - spawn.X;
        float deltaZ = worldPoint.Z - spawn.Z;
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
        if (!TryGetPlaneIntersection(location, _dragPlaneY, out Vector3 worldPoint))
        {
            return;
        }

        if (_editMode == UnitMapEditMode.ResizeSpawnRadius &&
            _selectedSpawnIndex is not null &&
            _selectedSpawnIndex.Value >= 0 &&
            _selectedSpawnIndex.Value < _layout.Spawns.Count)
        {
            LayoutSpawn spawn = _layout.Spawns[_selectedSpawnIndex.Value];
            float radius = DistanceXZ(worldPoint.X, worldPoint.Z, spawn.X, spawn.Z);
            LayoutSpawnRadiusChanged?.Invoke(this, new LayoutSpawnRadiusChangedEventArgs(_selectedSpawnIndex.Value, radius));
            Invalidate();
            return;
        }

        if (_editMode == UnitMapEditMode.ResizeRouteWaypointRadius &&
            _selectedRouteWaypointIndex is not null &&
            _route.Waypoints.TryGetValue(_selectedRouteWaypointIndex.Value, out RouteWaypoint? waypoint))
        {
            float radius = DistanceXZ(worldPoint.X, worldPoint.Z, waypoint.X, waypoint.Z);
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
    // 1 点の番号テキストを縁取り付きで描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawPointText(Graphics graphics, string text, PointF location, Color textColor, Font font)
    {
        using SolidBrush outlineBrush = new(Color.FromArgb(230, 20, 20, 20));
        using SolidBrush textBrush = new(textColor);
        graphics.DrawString(text, font, outlineBrush, new PointF(location.X - 1f, location.Y));
        graphics.DrawString(text, font, outlineBrush, new PointF(location.X + 1f, location.Y));
        graphics.DrawString(text, font, outlineBrush, new PointF(location.X, location.Y - 1f));
        graphics.DrawString(text, font, outlineBrush, new PointF(location.X, location.Y + 1f));
        graphics.DrawString(text, font, textBrush, location);
    }

    //-------------------------------------------------------------------------------
    // 画面左上のビュー情報を描画する処理
    //-------------------------------------------------------------------------------
    private void DrawHud(Graphics graphics)
    {
        using Font titleFont = new("Yu Gothic UI", 11f, FontStyle.Bold);
        using Font bodyFont = new("Yu Gothic UI", 9f, FontStyle.Regular);
        using SolidBrush boxBrush = new(Color.FromArgb(188, 42, 38, 31));
        using SolidBrush titleBrush = new(Color.White);
        using SolidBrush bodyBrush = new(Color.FromArgb(238, 238, 238));

        string hintText = GetEditHintText();
        Rectangle overlayRect = new(18, 18, 520, string.IsNullOrEmpty(hintText) ? 90 : 112);
        using GraphicsPath path = CreateRoundedRectangle(overlayRect, 12);
        graphics.FillPath(boxBrush, path);

        string title = string.IsNullOrWhiteSpace(_sceneName) ? "OBJ 3D 表示" : $"OBJ 3D 表示: {_sceneName}";
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

        graphics.DrawString(title, titleFont, titleBrush, new PointF(32, 30));
        graphics.DrawString($"Zoom: {_zoom:0.00}x  Pitch: {_pitch:0.00}  Yaw: {_yaw:0.00}  OffsetY: {_overlayHeightOffset:0.##}", bodyFont, bodyBrush, new PointF(32, 56));
        graphics.DrawString($"{modeText} / 左クリック: 選択 / 右ドラッグ: 回転・選択Spawn角度 / 中ドラッグ: 移動", bodyFont, bodyBrush, new PointF(32, 76));
        if (!string.IsNullOrEmpty(hintText))
        {
            graphics.DrawString(hintText, bodyFont, bodyBrush, new PointF(32, 96));
        }
    }

    //-------------------------------------------------------------------------------
    // 現在の編集モードに応じた操作ヒント文を返す処理
    //-------------------------------------------------------------------------------
    private string GetEditHintText()
    {
        return _editMode switch
        {
            UnitMapEditMode.MoveWaterbox => "Waterbox: 中央ドラッグでX/Z移動，四隅/辺の白ハンドルでサイズ変更，Ctrlで高さY移動",
            UnitMapEditMode.AddWaterbox => "Waterbox: 左クリック位置へ既定サイズで追加",
            UnitMapEditMode.DeleteWaterbox => "Waterbox: 対象を左クリックで削除",
            UnitMapEditMode.MoveRouteWaypoint => "Waypoint: ドラッグで移動，Shiftで主軸固定，Ctrlで高さY移動",
            UnitMapEditMode.ConnectRouteWaypoint => "Route: Waypoint同士をドラッグで接続",
            UnitMapEditMode.DeleteRouteLink => "Route: 接続線を左クリックで削除",
            UnitMapEditMode.RotateSpawn => "Spawn: 選択Spawnから右ドラッグ方向へ角度変更",
            UnitMapEditMode.ResizeSpawnRadius => "Spawn: 選択Spawnからドラッグ位置までの距離でRadius変更",
            UnitMapEditMode.ResizeRouteWaypointRadius => "Waypoint: 選択Waypointからドラッグ位置までの距離でRadius変更",
            _ => string.Empty
        };
    }

    //-------------------------------------------------------------------------------
    // ワールド座標をカメラ座標へ変換する処理
    //-------------------------------------------------------------------------------
    private Vector3 TransformWorldToCamera(ObjVertex vertex)
    {
        return TransformWorldToCamera(vertex.X, vertex.Y, vertex.Z);
    }

    //-------------------------------------------------------------------------------
    // ワールド座標をカメラ座標へ変換する処理
    //-------------------------------------------------------------------------------
    private Vector3 TransformWorldToCamera(float x, float y, float z)
    {
        float localX = x - _bounds.CenterX;
        float localY = y - _bounds.CenterY;
        float localZ = z - _bounds.CenterZ;
        return RotateWorldToCamera(new Vector3(localX, localY, localZ));
    }

    //-------------------------------------------------------------------------------
    // カメラ座標をワールド相対座標へ戻す処理
    //-------------------------------------------------------------------------------
    private Vector3 TransformCameraToWorld(Vector3 cameraPoint)
    {
        return InverseRotateToWorld(cameraPoint) + new Vector3(_bounds.CenterX, _bounds.CenterY, _bounds.CenterZ);
    }

    //-------------------------------------------------------------------------------
    // 現在の視点パラメータをまとめて取得する処理
    //-------------------------------------------------------------------------------
    private CameraState GetCameraState()
    {
        float radius = Math.Max(_bounds.Radius, 1f);
        float cameraDistance = radius * 5.5f;
        float baseScale = (Math.Min(ClientSize.Width, ClientSize.Height) * 0.82f) / (radius * 2f);
        float centerX = (ClientSize.Width * 0.5f) + _panX;
        float centerY = (ClientSize.Height * 0.5f) + _panY;
        return new CameraState(cameraDistance, baseScale, centerX, centerY);
    }

    //-------------------------------------------------------------------------------
    // ワールド座標を現在の 2D スクリーン座標へ投影する処理
    //-------------------------------------------------------------------------------
    private bool TryProjectWorldPoint(float x, float y, float z, out PointF projected, out float depth)
    {
        return TryProjectPoint(TransformWorldToCamera(x, y, z), GetCameraState(), out projected, out depth);
    }

    //-------------------------------------------------------------------------------
    // カメラ座標の点をスクリーン座標へ投影する処理
    //-------------------------------------------------------------------------------
    private bool TryProjectPoint(Vector3 point, CameraState camera, out PointF projected, out float depth)
    {
        float denominator = camera.Distance - point.Z;
        if (denominator <= 0.1f)
        {
            projected = default;
            depth = 0f;
            return false;
        }

        float perspective = (camera.Distance / denominator) * _zoom;
        projected = new PointF(
            camera.CenterX + (point.X * camera.BaseScale * perspective),
            camera.CenterY - (point.Y * camera.BaseScale * perspective));
        depth = point.Z;
        return true;
    }

    //-------------------------------------------------------------------------------
    // 変換済み 3 頂点を 2D スクリーン座標へ投影する処理
    //-------------------------------------------------------------------------------
    private bool TryProjectTriangle(
        Vector3 a,
        Vector3 b,
        Vector3 c,
        CameraState camera,
        out PointF[]? points,
        out float depth)
    {
        if (!TryProjectPoint(a, camera, out PointF pa, out float za) ||
            !TryProjectPoint(b, camera, out PointF pb, out float zb) ||
            !TryProjectPoint(c, camera, out PointF pc, out float zc))
        {
            points = null;
            depth = 0f;
            return false;
        }

        points = new[] { pa, pb, pc };
        depth = (za + zb + zc) / 3f;
        return true;
    }

    //-------------------------------------------------------------------------------
    // 画面座標から指定高さ平面との交点を求める処理
    //-------------------------------------------------------------------------------
    private bool TryGetPlaneIntersection(Point screenPoint, float planeY, out Vector3 intersection)
    {
        CameraState camera = GetCameraState();
        float relativeX = (screenPoint.X - camera.CenterX) / (camera.BaseScale * _zoom);
        float relativeY = -(screenPoint.Y - camera.CenterY) / (camera.BaseScale * _zoom);

        Vector3 cameraOrigin = TransformCameraToWorld(new Vector3(0f, 0f, camera.Distance));
        Vector3 pointOnProjectionPlane = TransformCameraToWorld(new Vector3(relativeX, relativeY, 0f));
        Vector3 rayDirection = pointOnProjectionPlane - cameraOrigin;

        if (Math.Abs(rayDirection.Y) < 0.0001f)
        {
            intersection = default;
            return false;
        }

        float t = (planeY - cameraOrigin.Y) / rayDirection.Y;
        if (t < 0f)
        {
            intersection = default;
            return false;
        }

        intersection = cameraOrigin + (rayDirection * t);
        return true;
    }

    //-------------------------------------------------------------------------------
    // 画面座標から追加ポイント用のワールド座標を取得する処理
    //-------------------------------------------------------------------------------
    private bool TryGetPlacementWorldPoint(Point screenPoint, out Vector3 placementPoint)
    {
        if (TryGetSceneIntersection(screenPoint, out placementPoint))
        {
            return true;
        }

        return TryGetPlaneIntersection(screenPoint, _bounds.CenterY, out placementPoint);
    }

    //-------------------------------------------------------------------------------
    // 画面座標のレイと OBJ 面の最寄り交点を取得する処理
    //-------------------------------------------------------------------------------
    private bool TryGetSceneIntersection(Point screenPoint, out Vector3 intersection)
    {
        intersection = default;
        if (_scene is null || _scene.Vertices.Count == 0 || _scene.Faces.Count == 0)
        {
            return false;
        }

        CameraState camera = GetCameraState();
        float relativeX = (screenPoint.X - camera.CenterX) / (camera.BaseScale * _zoom);
        float relativeY = -(screenPoint.Y - camera.CenterY) / (camera.BaseScale * _zoom);
        Vector3 rayOrigin = TransformCameraToWorld(new Vector3(0f, 0f, camera.Distance));
        Vector3 pointOnProjectionPlane = TransformCameraToWorld(new Vector3(relativeX, relativeY, 0f));
        Vector3 rayDirection = Vector3.Normalize(pointOnProjectionPlane - rayOrigin);

        bool found = false;
        float nearestDistance = float.MaxValue;
        foreach (ObjFace face in _scene.Faces)
        {
            if (face.Indices.Count < 3)
            {
                continue;
            }

            ObjVertex first = _scene.Vertices[face.Indices[0].Vertex];
            Vector3 a = new(first.X, first.Y, first.Z);
            for (int i = 1; i + 1 < face.Indices.Count; i++)
            {
                ObjVertex second = _scene.Vertices[face.Indices[i].Vertex];
                ObjVertex third = _scene.Vertices[face.Indices[i + 1].Vertex];
                Vector3 b = new(second.X, second.Y, second.Z);
                Vector3 c = new(third.X, third.Y, third.Z);
                if (!TryIntersectRayTriangle(rayOrigin, rayDirection, a, b, c, out float distance))
                {
                    continue;
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    intersection = rayOrigin + (rayDirection * distance);
                    found = true;
                }
            }
        }

        return found;
    }

    //-------------------------------------------------------------------------------
    // レイと三角形の交差距離を取得する処理
    //-------------------------------------------------------------------------------
    private static bool TryIntersectRayTriangle(Vector3 rayOrigin, Vector3 rayDirection, Vector3 a, Vector3 b, Vector3 c, out float distance)
    {
        distance = 0f;
        const float epsilon = 0.0001f;
        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        Vector3 h = Vector3.Cross(rayDirection, edge2);
        float determinant = Vector3.Dot(edge1, h);
        if (Math.Abs(determinant) < epsilon)
        {
            return false;
        }

        float inverseDeterminant = 1f / determinant;
        Vector3 s = rayOrigin - a;
        float u = inverseDeterminant * Vector3.Dot(s, h);
        if (u < -epsilon || u > 1f + epsilon)
        {
            return false;
        }

        Vector3 q = Vector3.Cross(s, edge1);
        float v = inverseDeterminant * Vector3.Dot(rayDirection, q);
        if (v < -epsilon || u + v > 1f + epsilon)
        {
            return false;
        }

        distance = inverseDeterminant * Vector3.Dot(edge2, q);
        return distance > epsilon;
    }

    //-------------------------------------------------------------------------------
    // スクリーンドラッグ量を現在視点の X/Z ワールド差分へ変換する処理
    //-------------------------------------------------------------------------------
    private bool TryGetScreenDragWorldDelta(
        Point previousScreenPoint,
        Point currentScreenPoint,
        float worldX,
        float worldY,
        float worldZ,
        out float deltaX,
        out float deltaZ)
    {
        const float sampleStep = 32f;

        if (!TryProjectWorldPoint(worldX, worldY, worldZ, out PointF basePoint, out _) ||
            !TryProjectWorldPoint(worldX + sampleStep, worldY, worldZ, out PointF xPoint, out _) ||
            !TryProjectWorldPoint(worldX, worldY, worldZ + sampleStep, out PointF zPoint, out _))
        {
            deltaX = 0f;
            deltaZ = 0f;
            return false;
        }

        float vxX = xPoint.X - basePoint.X;
        float vxY = xPoint.Y - basePoint.Y;
        float vzX = zPoint.X - basePoint.X;
        float vzY = zPoint.Y - basePoint.Y;
        float determinant = (vxX * vzY) - (vxY * vzX);
        if (Math.Abs(determinant) < 0.0001f)
        {
            deltaX = 0f;
            deltaZ = 0f;
            return false;
        }

        float screenDx = currentScreenPoint.X - previousScreenPoint.X;
        float screenDy = currentScreenPoint.Y - previousScreenPoint.Y;

        float localX = ((screenDx * vzY) - (screenDy * vzX)) / determinant;
        float localZ = ((vxX * screenDy) - (vxY * screenDx)) / determinant;
        deltaX = localX * sampleStep;
        deltaZ = localZ * sampleStep;
        return true;
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

        int? nearestWaypoint = null;
        float nearestDistanceSquared = RouteHitRadiusPixels * RouteHitRadiusPixels;
        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (!TryProjectWorldPoint(waypoint.X, GetOverlayWorldY(waypoint.Y), waypoint.Z, out PointF screenPoint, out _))
            {
                continue;
            }

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

        int? nearestSpawn = null;
        float nearestDistanceSquared = SpawnHitRadiusPixels * SpawnHitRadiusPixels;
        for (int index = 0; index < _layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            if (!TryProjectWorldPoint(spawn.X, GetOverlayWorldY(spawn.Y), spawn.Z, out PointF screenPoint, out _))
            {
                continue;
            }

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

        for (int index = _layout.Spawns.Count - 1; index >= 0; index--)
        {
            LayoutSpawn spawn = _layout.Spawns[index];
            if (!TryProjectWorldPoint(spawn.X, GetOverlayWorldY(spawn.Y), spawn.Z, out PointF center, out _) ||
                !TryGetSpawnAngleScreenDirection(spawn, center, out PointF direction))
            {
                continue;
            }

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
    private bool TryGetSpawnAngleScreenDirection(LayoutSpawn spawn, PointF center, out PointF direction)
    {
        float radians = spawn.Angle * MathF.PI / 180f;
        if (!TryProjectWorldPoint(
            spawn.X + MathF.Sin(radians),
            GetOverlayWorldY(spawn.Y),
            spawn.Z + MathF.Cos(radians),
            out PointF directionPoint,
            out _))
        {
            direction = default;
            return false;
        }

        float dx = directionPoint.X - center.X;
        float dy = directionPoint.Y - center.Y;
        float length = MathF.Sqrt((dx * dx) + (dy * dy));
        if (length < 0.0001f)
        {
            direction = new PointF(0f, -1f);
            return true;
        }

        direction = new PointF(dx / length, dy / length);
        return true;
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
    // マウス位置が投影済み waterbox 上面に含まれるか判定する処理
    //-------------------------------------------------------------------------------
    private int? HitTestWaterbox(Point location)
    {
        if (_waterbox.Boxes.Count == 0)
        {
            return null;
        }

        for (int index = _waterbox.Boxes.Count - 1; index >= 0; index--)
        {
            WaterboxEntry box = _waterbox.Boxes[index];
            float y = box.MaxY + _overlayHeightOffset;
            PointF[] points = new PointF[4];
            if (!TryProjectWorldPoint(box.MinX, y, box.MinZ, out points[0], out _) ||
                !TryProjectWorldPoint(box.MaxX, y, box.MinZ, out points[1], out _) ||
                !TryProjectWorldPoint(box.MaxX, y, box.MaxZ, out points[2], out _) ||
                !TryProjectWorldPoint(box.MinX, y, box.MaxZ, out points[3], out _))
            {
                continue;
            }

            using GraphicsPath path = new();
            path.AddPolygon(points);
            if (path.IsVisible(location))
            {
                return index;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // マウス位置に最も近い route 接続線を画面座標から求める処理
    //-------------------------------------------------------------------------------
    private (int From, int To)? HitTestRouteLink(Point location)
    {
        const float maxDistancePixels = 12f;
        (int From, int To)? nearestLink = null;
        float nearestDistanceSquared = maxDistancePixels * maxDistancePixels;

        Dictionary<int, PointF> screenPoints = new();
        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (TryProjectWorldPoint(waypoint.X, GetOverlayWorldY(waypoint.Y), waypoint.Z, out PointF point, out _))
            {
                screenPoints[waypoint.Index] = point;
            }
        }

        foreach (RouteWaypoint waypoint in _route.Waypoints.Values)
        {
            if (!screenPoints.TryGetValue(waypoint.Index, out PointF from))
            {
                continue;
            }

            foreach (int link in waypoint.Links)
            {
                if (!screenPoints.TryGetValue(link, out PointF to))
                {
                    continue;
                }

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
    // overlay 用の spawn 色を typeId から返す処理
    //-------------------------------------------------------------------------------
    private static Color GetSpawnColor(int typeId)
    {
        return typeId switch
        {
            0 => Color.FromArgb(230, 74, 25),
            1 => Color.FromArgb(211, 47, 47),
            2 => Color.FromArgb(255, 179, 0),
            4 => Color.FromArgb(0, 121, 107),
            5 => Color.FromArgb(0, 121, 107),
            6 => Color.FromArgb(67, 160, 71),
            7 => Color.FromArgb(2, 136, 209),
            8 => Color.FromArgb(123, 31, 162),
            _ => Color.FromArgb(96, 125, 139)
        };
    }

    //-------------------------------------------------------------------------------
    // 面のマテリアル名から描画色を決める処理
    //-------------------------------------------------------------------------------
    private static Color ResolveFaceColor(string? materialName)
    {
        if (string.IsNullOrWhiteSpace(materialName))
        {
            return Color.FromArgb(188, 168, 146);
        }

        int hash = materialName.Aggregate(17, (current, ch) => (current * 31) + ch);
        int r = 120 + Math.Abs(hash % 90);
        int g = 118 + Math.Abs((hash / 7) % 82);
        int b = 112 + Math.Abs((hash / 11) % 76);
        return Color.FromArgb(Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
    }

    //-------------------------------------------------------------------------------
    // 明度係数を使って色を陰影付きへ変換する処理
    //-------------------------------------------------------------------------------
    private static Color ShadeColor(Color baseColor, float amount)
    {
        return Color.FromArgb(
            Math.Clamp((int)(baseColor.R * amount), 0, 255),
            Math.Clamp((int)(baseColor.G * amount), 0, 255),
            Math.Clamp((int)(baseColor.B * amount), 0, 255));
    }

    //-------------------------------------------------------------------------------
    // overlay の高さ補正量を route/layout と OBJ 面から推定する処理
    //-------------------------------------------------------------------------------
    private static float EstimateOverlayHeightOffset(ObjScene scene, LayoutFile layout, RouteFile route)
    {
        List<float> samples = new();

        foreach (RouteWaypoint waypoint in route.Waypoints.Values)
        {
            if (TrySampleHeightOffset(scene, waypoint.X, waypoint.Z, waypoint.Y, out float offset))
            {
                samples.Add(offset);
            }
        }

        foreach (LayoutSpawn spawn in layout.Spawns)
        {
            if (TrySampleHeightOffset(scene, spawn.X, spawn.Z, spawn.Y, out float offset))
            {
                samples.Add(offset);
            }
        }

        if (samples.Count < 6)
        {
            return 0f;
        }

        samples.Sort();
        float median = samples[samples.Count / 2];
        return Math.Abs(median) < 1f ? 0f : median;
    }

    //-------------------------------------------------------------------------------
    // route_editor 準拠で画面上のルート途中へ矢印を描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawScreenRouteArrow(Graphics graphics, Pen arrowPen, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float length = MathF.Sqrt((dx * dx) + (dy * dy));
        if (length < 0.001f)
        {
            return;
        }

        const float arrowLength = 18f;
        float arrowAngle = 40f * (MathF.PI / 180f);
        PointF center = new(
            (end.X * 0.8f) + (start.X * 0.2f),
            (end.Y * 0.8f) + (start.Y * 0.2f));

        float direction = MathF.Atan2(dy, dx);
        PointF first = RotatePointAroundCenter(
            new PointF(center.X - arrowLength, center.Y),
            center,
            direction + arrowAngle);
        PointF second = RotatePointAroundCenter(
            new PointF(center.X - arrowLength, center.Y),
            center,
            direction - arrowAngle);

        graphics.DrawLine(arrowPen, center, first);
        graphics.DrawLine(arrowPen, center, second);
    }

    //-------------------------------------------------------------------------------
    // route の Y 高さに応じた 3D overlay 用グラデーション矢印を描画する処理
    //-------------------------------------------------------------------------------
    private static void DrawGradientScreenRouteArrow(Graphics graphics, PointF start, PointF end, float startY, float endY, float stroke)
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
        DrawScreenRouteArrow(graphics, arrowPen, start, end);
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

    private static PointF RotatePointAroundCenter(PointF point, PointF center, float radians)
    {
        float relativeX = point.X - center.X;
        float relativeY = point.Y - center.Y;
        float sine = MathF.Sin(radians);
        float cosine = MathF.Cos(radians);
        return new PointF(
            (relativeX * cosine) - (relativeY * sine) + center.X,
            (relativeX * sine) + (relativeY * cosine) + center.Y);
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

    //-------------------------------------------------------------------------------
    // 指定 XZ の OBJ 面から route/layout とモデルの高さ差分を取得する処理
    //-------------------------------------------------------------------------------
    private static bool TrySampleHeightOffset(ObjScene scene, float x, float z, float y, out float offset)
    {
        List<float> candidateHeights = new();
        foreach (ObjFace face in scene.Faces)
        {
            if (face.Indices.Count < 3)
            {
                continue;
            }

            ObjVertex first = scene.Vertices[face.Indices[0].Vertex];
            for (int i = 1; i + 1 < face.Indices.Count; i++)
            {
                ObjVertex second = scene.Vertices[face.Indices[i].Vertex];
                ObjVertex third = scene.Vertices[face.Indices[i + 1].Vertex];
                if (TryGetTriangleHeightAtPoint(first, second, third, x, z, out float triangleY))
                {
                    candidateHeights.Add(triangleY);
                }
            }
        }

        if (candidateHeights.Count > 0)
        {
            float surfaceY = candidateHeights
                .OrderBy(candidateY => Math.Abs(candidateY - y))
                .First();
            offset = surfaceY - y;
            return true;
        }

        return TrySampleNearestVertexHeightOffset(scene, x, z, y, out offset);
    }

    //-------------------------------------------------------------------------------
    // OBJ 面から高さを取得できない場合に近傍頂点から高さ差分を取得する処理
    //-------------------------------------------------------------------------------
    private static bool TrySampleNearestVertexHeightOffset(ObjScene scene, float x, float z, float y, out float offset)
    {
        ObjVertex? nearestVertex = null;
        float nearestDistanceSquared = float.MaxValue;

        foreach (ObjVertex vertex in scene.Vertices)
        {
            float dx = vertex.X - x;
            float dz = vertex.Z - z;
            float distanceSquared = (dx * dx) + (dz * dz);
            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestVertex = vertex;
            }
        }

        if (nearestVertex is null || nearestDistanceSquared > (64f * 64f))
        {
            offset = 0f;
            return false;
        }

        offset = nearestVertex.Y - y;
        return true;
    }

    //-------------------------------------------------------------------------------
    // XZ 平面上の指定点が三角形内にある場合に補間された高さを取得する処理
    //-------------------------------------------------------------------------------
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

    private float GetOverlayWorldY(float dataY)
    {
        return _overlayHeightOffset + dataY;
    }

    //-------------------------------------------------------------------------------
    // HUD 用の角丸矩形パスを生成する処理
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
    // ワールド相対座標をカメラ座標へ回転する処理
    //-------------------------------------------------------------------------------
    private Vector3 RotateWorldToCamera(Vector3 world)
    {
        float cosYaw = MathF.Cos(_yaw);
        float sinYaw = MathF.Sin(_yaw);
        float yawX = (world.X * cosYaw) + (world.Z * sinYaw);
        float yawZ = (-world.X * sinYaw) + (world.Z * cosYaw);

        float cosPitch = MathF.Cos(_pitch);
        float sinPitch = MathF.Sin(_pitch);
        float pitchY = (world.Y * cosPitch) - (yawZ * sinPitch);
        float pitchZ = (world.Y * sinPitch) + (yawZ * cosPitch);

        return new Vector3(yawX, pitchY, pitchZ);
    }

    //-------------------------------------------------------------------------------
    // カメラ座標をワールド相対座標へ逆回転する処理
    //-------------------------------------------------------------------------------
    private Vector3 InverseRotateToWorld(Vector3 camera)
    {
        float cosPitch = MathF.Cos(_pitch);
        float sinPitch = MathF.Sin(_pitch);
        float yawY = (camera.Y * cosPitch) + (camera.Z * sinPitch);
        float yawZ = (-camera.Y * sinPitch) + (camera.Z * cosPitch);

        float cosYaw = MathF.Cos(_yaw);
        float sinYaw = MathF.Sin(_yaw);
        float worldX = (camera.X * cosYaw) - (yawZ * sinYaw);
        float worldZ = (camera.X * sinYaw) + (yawZ * cosYaw);

        return new Vector3(worldX, yawY, worldZ);
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

    private readonly record struct CameraState(float Distance, float BaseScale, float CenterX, float CenterY);
    private readonly record struct ProjectedTriangle(PointF[] Points, float Depth, Color Color, Color EdgeColor, float EdgeWidth);
    private readonly record struct ModelBounds(float CenterX, float CenterY, float CenterZ, float Radius)
    {
        //-------------------------------------------------------------------------------
        // OBJ 全頂点から中心点と半径を算出する処理
        //-------------------------------------------------------------------------------
        public static ModelBounds FromScene(ObjScene scene)
        {
            float minX = scene.Vertices.Min(v => v.X);
            float minY = scene.Vertices.Min(v => v.Y);
            float minZ = scene.Vertices.Min(v => v.Z);
            float maxX = scene.Vertices.Max(v => v.X);
            float maxY = scene.Vertices.Max(v => v.Y);
            float maxZ = scene.Vertices.Max(v => v.Z);

            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float centerZ = (minZ + maxZ) * 0.5f;
            float radius = MathF.Max(maxX - minX, MathF.Max(maxY - minY, maxZ - minZ)) * 0.6f;
            return new ModelBounds(centerX, centerY, centerZ, MathF.Max(radius, 1f));
        }
    }

    private readonly record struct Vector3(float X, float Y, float Z)
    {
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator *(Vector3 a, float factor)
        {
            return new Vector3(a.X * factor, a.Y * factor, a.Z * factor);
        }

        //-------------------------------------------------------------------------------
        // 2 ベクトルの外積を返す処理
        //-------------------------------------------------------------------------------
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                (a.Y * b.Z) - (a.Z * b.Y),
                (a.Z * b.X) - (a.X * b.Z),
                (a.X * b.Y) - (a.Y * b.X));
        }

        //-------------------------------------------------------------------------------
        // 2 ベクトルの内積を返す処理
        //-------------------------------------------------------------------------------
        public static float Dot(Vector3 a, Vector3 b)
        {
            return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        }

        //-------------------------------------------------------------------------------
        // ベクトルを正規化した結果を返す処理
        //-------------------------------------------------------------------------------
        public static Vector3 Normalize(Vector3 value)
        {
            float length = MathF.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
            if (length <= 0.0001f)
            {
                return new Vector3(0f, 0f, 0f);
            }

            return new Vector3(value.X / length, value.Y / length, value.Z / length);
        }
    }
}
