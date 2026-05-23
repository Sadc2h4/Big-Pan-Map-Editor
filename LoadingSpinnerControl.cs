using System.Drawing.Drawing2D;

namespace PikminUnitEditor;

internal sealed class LoadingSpinnerControl : Control
{
    private readonly System.Windows.Forms.Timer _timer;
    private int _angle;

    public LoadingSpinnerControl()
    {
        BackColor = Color.White;
        DoubleBuffered = true;
        Size = new Size(72, 72);
        _timer = new System.Windows.Forms.Timer { Interval = 70 };
        _timer.Tick += (_, _) =>
        {
            _angle = (_angle + 18) % 360;
            Invalidate();
        };
    }

    //-------------------------------------------------------------------------------
    // スピナーの回転アニメーションを開始する処理
    //-------------------------------------------------------------------------------
    public void Start()
    {
        if (!_timer.Enabled)
        {
            _timer.Start();
        }
    }

    //-------------------------------------------------------------------------------
    // スピナーの回転アニメーションを停止する処理
    //-------------------------------------------------------------------------------
    public void Stop()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }
    }

    //-------------------------------------------------------------------------------
    // ロード中スピナーを描画する処理
    //-------------------------------------------------------------------------------
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bounds = new(8, 8, Width - 16, Height - 16);
        using Pen basePen = new(Color.FromArgb(70, 80, 190, 220), 7f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using Pen activePen = new(Color.FromArgb(235, 44, 190, 245), 7f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        e.Graphics.DrawArc(basePen, bounds, 0, 360);
        e.Graphics.DrawArc(activePen, bounds, _angle, 110);
    }

    //-------------------------------------------------------------------------------
    // 使用中タイマーを破棄する処理
    //-------------------------------------------------------------------------------
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }
}
