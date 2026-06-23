# Big Pan Map Editor
<!-- .NET 8 / Windows x64 -->
![.NET](https://img.shields.io/badge/language-.NET%208-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D4?style=flat-square&logo=windows&logoColor=white)
![Architecture](https://img.shields.io/badge/arch-x64-gray?style=flat-square)

<img width="60%" alt="アプリロゴ" src="https://github.com/user-attachments/assets/b1bcce82-0bc4-4ac9-9ac7-793f4c85fa33" />

## Download

<a href="https://github.com/Sadc2h4/Big-Pan-Map-Editor/releases/tag/v2.2a">
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
- Field object templates for gates, bridges, paper bags, water drains, treasures, plants, rocks, eggs, and other generator objects
- Texture footprint display and hit testing for bridges, gates, and block-like field objects
- Field `route.txt` waypoint editing: add, delete, move, radius, and link editing
- Persistent mini controller visibility settings for spawn, route, radius, waterbox, and connection overlays
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
- ゲート，橋，紙袋，排水溝，お宝，草，岩，タマゴなどの地上 object テンプレート
- 橋，ゲート，ブロック系 object のテクスチャフットプリント表示とクリック選択
- 地上 route.txt の Waypoint 追加，削除，移動，Radius，接続編集
- Spawn，Route，Radius，Waterbox，接続点の表示状態の引き継ぎ
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

## Version 2.2a Notes / v2.2a 変更点

### Highlights

- Added the Create Test Cave feature for quickly checking the cave unit being edited in the in-game Challenge Mode.
- Improved cave UnitConnection and doorway handling so newly added doors keep their connection index，route waypoint，and exported test-cave unit definition consistent.
- Fixed field generator parsing around inline closing braces，including `{pelt}` treasure blocks，so objects after a treasure block are not dropped during load or save.
- Field generator save now refreshes the declared object count after add，delete，and raw text edits for `initgen` / `defaultgen` style files.
- Embedded `pikmin-tools-master` style field object templates into the exe and loads the selected template into the Raw editor before adding.
- Added icons and display handling for water drains，treasures，plants，rocks，eggs，Pellet Posies，Honeywisps，bridges，gates，paper bags，and block-like objects.
- Bridges，gates，and block-like objects now use texture footprints with object angle，lower drawing priority，texture click selection，and rotate-mode-compatible right-drag behavior.
- Mini controller overlay visibility for spawn，route，radius，waterbox，and connection points is now saved and restored.

### Cave Mode

- A Create Test Cave button is shown at the top-right of the preview while a cave unit is loaded.
- The test cave registers the current unit together with helper units and writes the unit spec，caveinfo，`user/Matoba/challenge/stages.txt`，and `user/Totaka/ChallengeBgmList.txt` together.
- Floor counts，time lines，and BGM counts are kept consistent between the generated files.
- Existing caveinfo / stages.txt / ChallengeBgmList.txt are backed up to `.bak` on the first run.
- Challenge templates are embedded into the exe. The app still updates the target game's existing `stages.txt` and `ChallengeBgmList.txt`.

### Field Mode

- Built-in templates from `object_templates` are embedded into the exe and can be edited in the Raw box before insertion.
- Raw template text keeps line breaks instead of collapsing into one line.
- Treasure / pelt objects are detected from the generator text and displayed with the item icon.
- Water drains use `WaterDrains_icon.png` instead of the generic treasure icon.
- Grass-like teki objects use `Teki_plant_icon.png`，stone / boulder objects use `Teki_Stone_icon.png`，and eggs use `Teki_egg_icon.png`.
- Bridge，gate，paper bag，and block footprints are drawn below routes and points so points remain readable.
- Clicking a footprint selects its generator point. Rotation by right-drag is limited to rotate mode and follows the same behavior as normal spawn points.

### Notes

- Doorway changes have been adjusted to avoid inconsistent exported test-cave definitions，but real hardware validation is still recommended before distributing edited cave data.
- Back up target game data before saving edited files.

## Version 2.1a Notes / v2.1a 変更点

### Highlights

- Renamed the application to Big Pan Map Editor.
- Added a home screen for registering ISO / GCR / extracted root folders and Hocotate Tool Kit.
- Added Cave Gen Editor and Field Gen Editor entry points.
- Added Japanese / English language switching.
- Added an in-app manual and md-based manual source files.
- Added a single-file Windows publish flow.

Cave mode
- Added cave unit list loading from mapunits arc folders.
- Added pretty image preview and 3D model preview support.
- Added Spawn editing: add，delete，move，angle，radius，type，and count.
- Added Route editing: waypoint add，delete，move，radius，link add，and link delete.
- Added Waterbox editing: add，delete，move，XZ range edit，and height inspection.
- Save now writes layout / route / waterbox together and repacks arc.szs / texts.szs when possible.

Field mode
- Added field map list loading from user/Abe/map and user/Kando/map.
- Added route.txt and generator file loading.
- Added elapsed-day filtering for field generator objects.
- Added the Field Generator Console for day condition and add-target control.
- Added field generator object editing: add，delete，move，angle，radius，and raw text editing.
- Added field route.txt editing: waypoint add，delete，move，radius，and link editing.
- Added icons for Onion，Rocket，Cave Entrance，Item，Pikmin，bridges，gates，paper bags，and seesaw blocks.
- Added rectangular footprints for bridges，gates，and other range-based objects.
- Field save writes route.txt and generator txt files back to the map folder.

### UI and assets

- Added a floating mini controller for Spawn / Route / Waterbox editing.
- Added minimize buttons for floating controller windows.
- Added a centered loading dialog with animated Loading.gif.
- Added embedded resources for button icons，spawn icons，home screen assets，manual images，loading image，and pretty images.
- Changed the application icon to BigPan_icon_trim_20260522_161519.ico.
- Moved image assets under the PikminUnitEditor project folder for repository deployment.

### Manual and documentation

- Added manual md files under PikminUnitEditor/manual.
- Added manual_editor.html for editing manual md files before build.
- Manual images under PikminUnitEditor/manual/images are embedded during build.
- The in-app manual supports headings，lists，images，and horizontal rules.

### Build / publish

- Target framework: .NET 8 Windows.
- Platform: Windows.
- Release publish can output a single BigPanMapEditor.exe.

### Notes

- Hocotate_Toolkit.exe is required for SZS extraction，cache generation，and archive repacking.
- Back up target game data before saving edited files.
- Generated cache folders and settings json are runtime files and are not included in the release exe.

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

Image assets，field object templates，entity name data，and challenge template text are collected under `PikminUnitEditor` and embedded into the exe at build time.

画像素材，地上 object テンプレート，entity 名データ，チャレンジ用テンプレートテキストは `PikminUnitEditor` 配下へ集約し，ビルド時に埋め込みリソースとして exe に含めます．

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
