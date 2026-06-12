# Big Pan Map Editor
<!-- .NET 8 / Windows x64 -->
![.NET](https://img.shields.io/badge/language-.NET%208-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D4?style=flat-square&logo=windows&logoColor=white)
![Architecture](https://img.shields.io/badge/arch-x64-gray?style=flat-square)

<img width="60%" alt="アプリロゴ" src="https://github.com/user-attachments/assets/b1bcce82-0bc4-4ac9-9ac7-793f4c85fa33" />

## Download

<a href="https://github.com/Sadc2h4/Big-Pan-Map-Editor/releases/tag/v2.1a">
  <img
    src="https://raw.githubusercontent.com/Sadc2h4/brand-assets/main/button/Download_Button_1.png"
    alt="Download .zip"
    height="48"
  />
</a>
<br>

## Credits

各機能の処理で参考にしたアプリと作者は以下の通りです．  
勝手に内容を参考にしたことをお詫びするとともに，情報を共有してくださったことに感謝いたします．

----------------------------------------------------------------------------------------------------

Big Pan Map Editor itself is created by `C2H4`.  
The following applications and reference authors were consulted for implementation.

| Area | Reference Application | Reference Author |
|--------|------|------|
| Cave generator editing | `pikmingen_editor` from `pikmin-tools-master` | `Yoshi2` |
| Cave route editing | `route_editor` from `pikmin-tools-master` | `Yoshi2` |
| Field route editing | `route_editor` from `pikmin-tools-master` | `Yoshi2` |

## Overview

Big Pan Map Editor is a Windows application for inspecting and editing the cave units and field maps used in the GameCube title *Pikmin 2*.

It was built by combining the ideas behind `pikmin-tools-master`'s `pikmingen_editor` and `route_editor`, with the goal of handling 3D models, pretty images, `layout`, `route`, `waterbox`, and field generator data in one place.

----------------------------------------------------------------------------------------------------

Big Pan Map Editor は，ゲームキューブ用ソフト『ピクミン2』の洞窟ユニットと地上マップを確認しながら編集するための Windows アプリです．

`pikmin-tools-master` の `pikmingen_editor` と `route_editor` を参考に，3Dモデル，pretty画像，layout，route，waterbox，地上 generator を同じ画面で扱えるようにすることを目的にしています．

## Supported Inputs / 対応する読み込み対象

- ISO / GCR files
- Extracted disc folders
- Disc extraction data containing `sys/files`
- Cave `user/Mukki/mapunits/arc` folders
- Standalone cave unit folders
- Field `user/Abe/map` and `user/Kando/map` folders

When ISO / GCR is selected, the app attempts disc extraction through `Hocotate_Toolkit.exe`. If you point the app at an extracted folder or an `arc` folder, it reads the data directly without extraction.

----------------------------------------------------------------------------------------------------

- ISO / GCR
- 抽出済みディスクフォルダ
- `sys/files` を含むディスク抽出データ
- 洞窟 `user/Mukki/mapunits/arc` フォルダ
- 単体ユニットフォルダ
- 地上 `user/Abe/map` と `user/Kando/map`

ISO / GCR を指定した場合，`Hocotate_Toolkit.exe` を使ってディスク抽出を試行します．抽出済みフォルダや arc フォルダを指定した場合は，抽出処理を省略して直接参照します．

## Features / 主な機能

- Cave unit list browsing
- Cave unit pretty image preview, 2D placement view, and 3D model view
- Cave 3D view source selection: `texts/grid` collision view or `arc visual`
- Spawn editing: add, delete, move, angle, radius, type, and count
- Route waypoint editing: add, delete, move, radius, link add, and link delete
- Waterbox editing: add, delete, move, XZ range editing, and height inspection
- Save support for `layout`, `route`, and `waterbox`; cave saves update `texts.szs` only
- Keyboard shortcuts: Ctrl+S / Ctrl+Shift+S save, Ctrl+Z undo, Ctrl+Y / Ctrl+Shift+Z redo
- Field map list browsing
- Elapsed-day filtering for field generator objects
- Field generator object editing: add, delete, move, angle, radius, and raw text editing
- Field `route.txt` waypoint editing: add, delete, move, radius, and link editing
- Japanese / English language switching
- In-app manual viewer
 
----------------------------------------------------------------------------------------------------

- 洞窟ユニット一覧の表示
- 洞窟ユニットの pretty画像，2D配置，3Dモデル表示
- 洞窟 3D 表示ソース選択: `texts/grid` collision 表示または `arc visual`
- Spawn の追加，削除，移動，角度，Radius，Type，Count 編集
- Route Waypoint の追加，削除，移動，Radius，接続追加，接続削除
- Waterbox の追加，削除，移動，XZ範囲編集，高さ確認
- layout / route / waterbox の保存と `texts.szs` 反映
- ショートカット: Ctrl+S / Ctrl+Shift+S 保存，Ctrl+Z Undo，Ctrl+Y / Ctrl+Shift+Z Redo
- 地上マップ一覧の表示
- 地上 generator object の日数条件表示
- 地上 generator object の追加，削除，移動，角度，Radius，Raw編集
- 地上 route.txt の Waypoint 追加，削除，移動，Radius，接続編集
- 日本語 / English の言語切り替え
- アプリ内マニュアル表示

## Usage / 基本的な使い方

1. Launch `BigPanMapEditor.exe`.
2. Register the ISO / GCR / root folder on the home screen.
3. Register the path to `Hocotate_Toolkit.exe`.
4. Choose `Cave Gen Editor` or `Field Gen Editor`.
5. Select a unit or field map from the list on the left.
6. Use the mini controller to switch between Spawn, Route, and Waterbox editing.
7. Save the changes back to the target files.

----------------------------------------------------------------------------------------------------

1. `BigPanMapEditor.exe` を起動します．
2. ホーム画面で ISO / GCR / root フォルダを指定します．
3. `Hocotate_Toolkit.exe` の参照先を指定します．
4. `Cave Gen Editor` または `Field Gen Editor` を選択します．
5. 左側の一覧からユニットまたは地上マップを選択します．
6. ミニコントローラーで Spawn / Route / Waterbox を切り替えて編集します．
7. 保存ボタンで編集内容を対象ファイルへ反映します．

## Cave Mode / 洞窟モード

Cave mode edits `layout.txt`, `route.txt`, and `waterbox` data. When saving, the editor stores the cached edits and repacks them back into `texts.szs`. `arc.szs` is read for the optional `arc visual` 3D view, but it is not modified by current cave spawn / route / waterbox saves.

Main targets:

- Spawn
- Route Waypoints
- Route Links
- Waterboxes

----------------------------------------------------------------------------------------------------

洞窟モードでは `layout.txt`，`route.txt`，`waterbox` を編集できます．保存時はキャッシュ上の編集内容を保存し，`texts.szs` へ再圧縮して元ユニットへ反映します．`arc visual` 表示では `arc.szs` を読み込みますが，現在の Spawn / Route / Waterbox 保存では `arc.szs` は変更しません．

主な編集対象:

- Spawn
- Route Waypoints
- Route Links
- Waterboxes

## Field Mode / 地上モード

Field mode loads `route.txt` and generator-related files. Because generator objects are controlled by elapsed-day conditions, the Field Generator Console is used to inspect the current day and the active generator files.

Main targets:
- Generator objects
- Generator raw text
- `route.txt` waypoints
- `route.txt` links

----------------------------------------------------------------------------------------------------

地上モードでは `route.txt` と generator 関連ファイルを読み込みます．generator object は経過日数条件によって表示対象が変わるため，地上 generator コンソールで日数と有効ファイルを確認しながら編集します．

主な編集対象:
- generator object
- generator raw text
- route.txt Waypoint
- route.txt Link

## Cache / キャッシュについて

3D models and preview images use cache data. Cache files are created in the runtime environment and are not bundled into the release exe.
Long cache jobs display a centered loading dialog. Building cache for many units can take a while.

----------------------------------------------------------------------------------------------------

3Dモデルやプレビュー画像の表示にはキャッシュを使用します．キャッシュは実行環境に作成され，配布exeには含まれません．
長いキャッシュ生成中は画面中央にロード表示が出ます．大量のユニットキャッシュを作成する場合は時間がかかります．

## Version 2.0b Notes / v2.0b 変更点

- Cave saves now update `texts.szs` only, regardless of the selected 3D view source.
- `arc visual` still reads `arc.szs` for display, but generated raw display assets are no longer written back during normal saves.
- Spawn drag movement supports Shift axis locking in both 2D and 3D views.
- Added save hotkeys: Ctrl+S and Ctrl+Shift+S.
- The default cave 3D source remains `texts/grid`, which uses `grid.bin` collision data for display.

----------------------------------------------------------------------------------------------------

- 洞窟保存は，3D 表示ソースに関係なく `texts.szs` のみ更新するようにしました．
- `arc visual` は表示用に `arc.szs` を読み込みますが，通常保存では生成された raw 表示アセットを書き戻しません．
- Spawn のドラッグ移動で，2D / 3D とも Shift による主軸固定に対応しました．
- 保存ホットキー Ctrl+S / Ctrl+Shift+S を追加しました．
- 洞窟 3D 表示の既定は引き続き `texts/grid` で，`grid.bin` の collision 情報を表示に使用します．

## Manual Editing / マニュアル編集

The in-app manual is embedded at build time from the markdown files under `PikminUnitEditor/manual`.
To edit the manual during development, open `PikminUnitEditor/manual_editor.html` in Chrome or Edge, select the `manual` folder, and edit the markdown files there. Add images under `PikminUnitEditor/manual/images` and reference them from markdown as `![description](images/file.png)`.

----------------------------------------------------------------------------------------------------

アプリ内マニュアルは `PikminUnitEditor/manual` 配下の md ファイルからビルド時に埋め込まれます．
開発時にマニュアルを編集する場合は，`PikminUnitEditor/manual_editor.html` を Chrome または Edge で開き，`manual` フォルダを選択して編集してください．画像は `PikminUnitEditor/manual/images` 配下に追加し，md から `![説明](images/file.png)` 形式で参照します．

## Development / 開発環境

- Windows
- .NET 8 SDK
- Windows Forms
- Hocotate Tool Kit

Build:

```powershell
dotnet build PikminUnitEditor\BigPanMapEditor.sln
```

Single-file publish:

```powershell
dotnet publish PikminUnitEditor\BigPanMapEditor.csproj -c Release -r win-x64 --self-contained true -o publish\BigPanMapEditor-win-x64 -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false
```

## Repository Layout / リポジトリ構成

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

Image assets are collected under `PikminUnitEditor` and embedded into the exe at build time.

画像素材は `PikminUnitEditor` 配下へ集約し，ビルド時に埋め込みリソースとして exe に含めます．

## Distribution / 配布条件

- This tool is freeware.
- It may be used freely for modification and validation purposes.
- The author is not responsible for any damage caused by the use of this tool.

- 本ツールはフリーソフトです．
- 改造や検証用途で自由に利用できます．
- 本ツールの使用により発生した損害について，作者は責任を負いません．

## Disclaimer / 免責

This is an unofficial tool. Always back up your target data before editing it.

本ツールは非公式ツールです．対象データを編集する前に必ずバックアップを作成してください．
