# Big Pan Map Editor
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net&logoColor=white)
![platform](https://img.shields.io/badge/platform-Windows-informational)
![license](https://img.shields.io/badge/license-Freeware-lightgrey)

Big Pan Map Editor は，ゲームキューブ用ソフト『ピクミン2』の洞窟ユニットと地上マップを確認しながら編集するための Windows アプリです．

`pikmin-tools-master` の `pikmingen_editor` と `route_editor` を参考に，3Dモデル，pretty画像，layout，route，waterbox，地上 generator を同じ画面で扱えるようにすることを目的にしています．

## ダウンロード

GitHub の Releases から最新版の `BigPanMapEditor.exe` をダウンロードしてください．

> [!IMPORTANT]
> SZS 展開と再圧縮には `Hocotate_Toolkit.exe` が必要です．ホーム画面で事前に参照先を登録してください．

## 主な機能

- 洞窟ユニット一覧の表示
- 洞窟ユニットの pretty画像，2D配置，3Dモデル表示
- Spawn の追加，削除，移動，角度，Radius，Type，Count 編集
- Route Waypoint の追加，削除，移動，Radius，接続追加，接続削除
- Waterbox の追加，削除，移動，XZ範囲編集，高さ確認
- layout / route / waterbox の保存とアーカイブ反映
- 地上マップ一覧の表示
- 地上 generator object の日数条件表示
- 地上 generator object の追加，削除，移動，角度，Radius，Raw編集
- 地上 route.txt の Waypoint 追加，削除，移動，Radius，接続編集
- 日本語 / English の言語切り替え
- アプリ内マニュアル表示

## 対応する読み込み対象

- ISO / GCR
- 抽出済みディスクフォルダ
- `sys/files` を含むディスク抽出データ
- 洞窟 `user/Mukki/mapunits/arc` フォルダ
- 単体ユニットフォルダ
- 地上 `user/Abe/map` と `user/Kando/map`

ISO / GCR を指定した場合，`Hocotate_Toolkit.exe` を使ってディスク抽出を試行します．抽出済みフォルダや arc フォルダを指定した場合は，抽出処理を省略して直接参照します．

## 基本的な使い方

1. `BigPanMapEditor.exe` を起動します．
2. ホーム画面で ISO / GCR / root フォルダを指定します．
3. `Hocotate_Toolkit.exe` の参照先を指定します．
4. `Cave Gen Editor` または `Field Gen Editor` を選択します．
5. 左側の一覧からユニットまたは地上マップを選択します．
6. ミニコントローラーで Spawn / Route / Waterbox を切り替えて編集します．
7. 保存ボタンで編集内容を対象ファイルへ反映します．

## 洞窟モード

洞窟モードでは `layout.txt`，`route.txt`，`waterbox` を編集できます．保存時はキャッシュ上の編集内容を保存し，可能な場合は `arc.szs` と `texts.szs` へ再圧縮して元ユニットへ反映します．

主な編集対象:

- Spawn
- Route Waypoint
- Route Link
- Waterbox

## 地上モード

地上モードでは `route.txt` と generator 関連ファイルを読み込みます．generator object は経過日数条件によって表示対象が変わるため，地上 generator コンソールで日数と有効ファイルを確認しながら編集します．

主な編集対象:

- generator object
- generator raw text
- route.txt Waypoint
- route.txt Link

## キャッシュについて

3Dモデルやプレビュー画像の表示にはキャッシュを使用します．キャッシュは実行環境に作成され，配布exeには含まれません．

長いキャッシュ生成中は画面中央にロード表示が出ます．大量のユニットキャッシュを作成する場合は時間がかかります．

## マニュアル編集

アプリ内マニュアルは `PikminUnitEditor/manual` 配下の md ファイルからビルド時に埋め込まれます．

開発時にマニュアルを編集する場合は，`PikminUnitEditor/manual_editor.html` を Chrome または Edge で開き，`manual` フォルダを選択して編集してください．画像は `PikminUnitEditor/manual/images` 配下に追加し，md から `![説明](images/file.png)` 形式で参照します．

## 開発環境

- Windows
- .NET 8 SDK
- Windows Forms
- Hocotate Tool Kit

ビルド:

```powershell
dotnet build PikminUnitEditor\BigPanMapEditor.sln
```

単体exe発行:

```powershell
dotnet publish PikminUnitEditor\BigPanMapEditor.csproj -c Release -r win-x64 --self-contained true -o publish\BigPanMapEditor-win-x64 -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false
```

## リポジトリ構成

```text
PikminUnitEditor/
  manual/
  pretty/
  スポーンアイコン/
  ホーム画面用素材/
  ボタン用アイコン/
  確認用画像/
  BigPanMapEditor.csproj
  BigPanMapEditor.sln
```

画像素材は `PikminUnitEditor` 配下へ集約し，ビルド時に埋め込みリソースとして exe に含めます．

## 配布条件

- 本ツールはフリーソフトです．
- 改造や検証用途で自由に利用できます．
- 本ツールの使用により発生した損害について，作者は責任を負いません．

## 免責

本ツールは非公式ツールです．対象データを編集する前に必ずバックアップを作成してください．
