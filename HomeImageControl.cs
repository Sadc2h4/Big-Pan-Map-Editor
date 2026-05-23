using System.Drawing.Drawing2D;

namespace PikminUnitEditor;

internal sealed class HomeImageControl : Control
{
    public Image? Image { get; set; }

    public HomeImageControl()
    {
        SetStyle(ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw, true);
    }

    //-------------------------------------------------------------------------------
    // 親のホーム背景を描画してから透過PNGを合成する処理
    //-------------------------------------------------------------------------------
    protected override void OnPaint(PaintEventArgs e)
    {
        DrawParentBackground(e.Graphics);
        if (Image is null)
        {
            return;
        }

        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle targetBounds = GetZoomBounds(Image.Size, ClientSize);
        e.Graphics.DrawImage(Image, targetBounds);
    }

    //-------------------------------------------------------------------------------
    // 親コントロールの背景画像を現在コントロール位置に合わせて描画する処理
    //-------------------------------------------------------------------------------
    private void DrawParentBackground(Graphics graphics)
    {
        if (Parent is null)
        {
            graphics.Clear(Color.FromArgb(6, 17, 35));
            return;
        }

        using SolidBrush brush = new(Parent.BackColor);
        graphics.FillRectangle(brush, ClientRectangle);
        if (Parent.BackgroundImage is null)
        {
            return;
        }

        Rectangle destinationBounds = Parent.BackgroundImageLayout == ImageLayout.Stretch
            ? new Rectangle(-Left, -Top, Parent.ClientSize.Width, Parent.ClientSize.Height)
            : new Rectangle(-Left, -Top, Parent.BackgroundImage.Width, Parent.BackgroundImage.Height);
        graphics.DrawImage(Parent.BackgroundImage, destinationBounds);
    }

    //-------------------------------------------------------------------------------
    // 画像をコントロール内へアスペクト比維持で収める範囲を算出する処理
    //-------------------------------------------------------------------------------
    private static Rectangle GetZoomBounds(Size imageSize, Size targetSize)
    {
        if (imageSize.Width <= 0 || imageSize.Height <= 0 || targetSize.Width <= 0 || targetSize.Height <= 0)
        {
            return Rectangle.Empty;
        }

        float scale = Math.Min(targetSize.Width / (float)imageSize.Width, targetSize.Height / (float)imageSize.Height);
        int width = Math.Max(1, (int)MathF.Round(imageSize.Width * scale));
        int height = Math.Max(1, (int)MathF.Round(imageSize.Height * scale));
        return new Rectangle(
            (targetSize.Width - width) / 2,
            (targetSize.Height - height) / 2,
            width,
            height);
    }

    //-------------------------------------------------------------------------------
    // 保持している画像リソースを破棄する処理
    //-------------------------------------------------------------------------------
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Image?.Dispose();
        }

        base.Dispose(disposing);
    }
}
