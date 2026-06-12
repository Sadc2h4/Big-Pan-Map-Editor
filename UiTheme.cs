namespace PikminUnitEditor;

//-------------------------------------------------------------------------------
// ホーム画面用素材と統一したアプリ全体の配色テーマ定義
//-------------------------------------------------------------------------------
internal static class UiTheme
{
    // 基本面 (ホーム画面の MainConsoleBG.png に合わせたダークネイビー系)
    public static readonly Color BackgroundDeep = Color.FromArgb(6, 17, 35);    // 最背面
    public static readonly Color PanelBack = Color.FromArgb(10, 25, 48);        // サイドパネル背景
    public static readonly Color SurfaceBack = Color.FromArgb(14, 32, 60);      // カード/グループ背景
    public static readonly Color InputBack = Color.FromArgb(9, 22, 42);         // 入力欄背景
    public static readonly Color HeaderBack = Color.FromArgb(18, 48, 82);       // タブ/ヘッダー背景

    // アクセント (ホーム画面のシアンブルー)
    public static readonly Color AccentCyan = Color.FromArgb(75, 205, 245);     // メインアクセント
    public static readonly Color AccentCyanSoft = Color.FromArgb(93, 207, 248); // 補助アクセント
    public static readonly Color BorderCyan = Color.FromArgb(36, 96, 140);      // 枠線

    // 文字色
    public static readonly Color TextMain = Color.FromArgb(214, 234, 248);      // 基本文字色
    public static readonly Color TextSub = Color.FromArgb(150, 185, 215);       // 補助文字色
    public static readonly Color TextOnLight = Color.FromArgb(16, 42, 70);      // 明色面の文字色

    // ボタン (ダーク面用，ホーム画面のリストボタンと同系)
    public static readonly Color ButtonBack = Color.FromArgb(13, 38, 66);
    public static readonly Color ButtonHover = Color.FromArgb(30, 80, 120);
    public static readonly Color ButtonDown = Color.FromArgb(20, 65, 100);

    // 明色ボタン (ホーム画面の Cave Gen Editor ボタンと同系の水色面)
    public static readonly Color LightButtonBack = Color.FromArgb(191, 224, 240);
    public static readonly Color LightButtonHover = Color.FromArgb(168, 212, 234);
    public static readonly Color LightButtonBorder = Color.FromArgb(41, 171, 226);

    // ミニコントローラーのモード別配色 (ダーク基調 + モード別アクセント)
    public static readonly Color SpawnModeBack = Color.FromArgb(10, 36, 30);
    public static readonly Color SpawnModeHeader = Color.FromArgb(14, 56, 44);
    public static readonly Color SpawnModeAccent = Color.FromArgb(80, 200, 140);
    public static readonly Color RouteModeBack = Color.FromArgb(8, 26, 50);
    public static readonly Color RouteModeHeader = Color.FromArgb(12, 42, 76);
    public static readonly Color RouteModeAccent = Color.FromArgb(75, 170, 245);
    public static readonly Color WaterboxModeBack = Color.FromArgb(6, 32, 42);
    public static readonly Color WaterboxModeHeader = Color.FromArgb(10, 50, 64);
    public static readonly Color WaterboxModeAccent = Color.FromArgb(0, 180, 215);
    public static readonly Color UnitConnectModeBack = Color.FromArgb(40, 16, 24);
    public static readonly Color UnitConnectModeHeader = Color.FromArgb(60, 24, 34);
    public static readonly Color UnitConnectModeAccent = Color.FromArgb(235, 110, 100);

    // 2D/3D プレビュー面
    public static readonly Color CanvasBack = Color.FromArgb(7, 19, 38);        // プレビュー最背面
    public static readonly Color CanvasPaper = Color.FromArgb(10, 26, 50);      // 図面エリア
    public static readonly Color CanvasGrid = Color.FromArgb(22, 52, 84);       // 補助グリッド線
    public static readonly Color CanvasAxis = Color.FromArgb(38, 92, 132);      // 軸線
    public static readonly Color CanvasText = Color.FromArgb(120, 200, 240);    // プレビュー上の情報文字

    // ユニット一覧カード
    public static readonly Color CardBack = Color.FromArgb(14, 32, 60);
    public static readonly Color CardSelectedBack = Color.FromArgb(24, 70, 110);
    public static readonly Color CardThumbnailBack = Color.FromArgb(9, 22, 42);

    //-------------------------------------------------------------------------------
    // ダーク面用のフラットボタンスタイルを適用する処理
    //-------------------------------------------------------------------------------
    public static void StyleDarkButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = ButtonBack;
        button.ForeColor = AccentCyanSoft;
        button.FlatAppearance.BorderColor = BorderCyan;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = ButtonHover;
        button.FlatAppearance.MouseDownBackColor = ButtonDown;
        button.UseVisualStyleBackColor = false;
    }

    //-------------------------------------------------------------------------------
    // ホーム画面ボタン風の明色フラットボタンスタイルを適用する処理
    //-------------------------------------------------------------------------------
    public static void StyleLightButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = LightButtonBack;
        button.ForeColor = TextOnLight;
        button.FlatAppearance.BorderColor = LightButtonBorder;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = LightButtonHover;
        button.FlatAppearance.MouseDownBackColor = LightButtonBorder;
        button.UseVisualStyleBackColor = false;
    }

    //-------------------------------------------------------------------------------
    // 指定コントロール配下の入力系/文字系コントロールへダークテーマを適用する処理
    //-------------------------------------------------------------------------------
    public static void ApplyDarkThemeRecursive(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case UnitMapView:
                case ObjModelView:
                case PictureBox:
                case HomeImageControl:
                    continue;                                       // 描画/画像系は対象外
                case RichTextBox richTextBox:
                    richTextBox.BackColor = InputBack;
                    richTextBox.ForeColor = AccentCyanSoft;
                    break;
                case TextBox textBox:
                    textBox.BackColor = InputBack;
                    textBox.ForeColor = TextMain;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ComboBox comboBox:
                    comboBox.BackColor = InputBack;
                    comboBox.ForeColor = TextMain;
                    comboBox.FlatStyle = FlatStyle.Flat;
                    break;
                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = InputBack;
                    numericUpDown.ForeColor = TextMain;
                    break;
                case Button button:
                    StyleDarkButton(button);
                    break;
                case CheckBox checkBox:
                    checkBox.ForeColor = TextMain;
                    break;
                case RadioButton radioButton:
                    radioButton.ForeColor = TextMain;
                    break;
                case Label label:
                    label.ForeColor = TextMain;
                    break;
                case GroupBox groupBox:
                    groupBox.ForeColor = AccentCyan;
                    groupBox.BackColor = PanelBack;
                    break;
                case TableLayoutPanel or FlowLayoutPanel or Panel:
                    control.BackColor = PanelBack;
                    break;
            }

            if (control.HasChildren)
            {
                ApplyDarkThemeRecursive(control);
            }
        }
    }
}
