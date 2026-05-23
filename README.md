# Big Pan Map Editor
<!-- .NET 8 / Windows x64 -->
![.NET](https://img.shields.io/badge/language-.NET%208-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D4?style=flat-square&logo=windows&logoColor=white)
![Architecture](https://img.shields.io/badge/arch-x64-gray?style=flat-square)

<img width="60%" alt="アプリロゴ" src="https://github.com/user-attachments/assets/b1bcce82-0bc4-4ac9-9ac7-793f4c85fa33" />

## Download

<a href="https://github.com/Sadc2h4/Big-Pan-Map-Editor/releases/tag/v2.0a">
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

## Features / 主な機能
Big Pan Map Editor is a Windows application for inspecting and editing the cave units and field maps used in the GameCube title *Pikmin 2*.

It was built by combining the ideas behind `pikmin-tools-master`'s `pikmingen_editor` and `route_editor`, with the goal of handling 3D models, pretty images, `layout`, `route`, `waterbox`, and field generator data in one place.

----------------------------------------------------------------------------------------------------

Big Pan Map Editor は，ゲームキューブ用ソフト『ピクミン2』の洞窟ユニットと地上マップを確認しながら編集するための Windows アプリです．

`pikmin-tools-master` の `pikmingen_editor` と `route_editor` を参考に，3Dモデル，pretty画像，layout，route，waterbox，地上 generator を同じ画面で扱えるようにすることを目的にしています．

## Supported Inputs / 対応する読み込み対象
<img width="400" height="137" alt="MD_Description_3" src="https://github.com/user-attachments/assets/147b7375-5eb4-4daf-b424-9823cc8fe0b4" />

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
<img width="600" height="371" alt="洞窟モード" src="https://github.com/user-attachments/assets/57944715-c4fb-436b-89c8-cf7bf5a5767d" />

Cave mode edits `layout.txt`, `route.txt`, and `waterbox` data. When saving, the editor stores the cached edits and repacks them back into `arc.szs` and `texts.szs` when possible.

Main targets:

- Spawn
- Route Waypoints
- Route Links
- Waterboxes

----------------------------------------------------------------------------------------------------

洞窟モードでは `layout.txt`，`route.txt`，`waterbox` を編集できます．保存時はキャッシュ上の編集内容を保存し，可能な場合は `arc.szs` と `texts.szs` へ再圧縮して元ユニットへ反映します．

主な編集対象:

- Spawn
- Route Waypoints
- Route Links
- Waterboxes

## Field Mode / 地上モード
<img width="600" height="308" alt="地上モード" src="https://github.com/user-attachments/assets/1e3183e9-15ff-4bec-b028-61548b4945f8" />

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
<img width="400" height="231" alt="MD_Description_4" src="https://github.com/user-attachments/assets/617d91c9-a0ba-4e65-a690-bf45cc4c18ae" />

3D models and preview images use cache data. Cache files are created in the runtime environment and are not bundled into the release exe.
Long cache jobs display a centered loading dialog. Building cache for many units can take a while.

----------------------------------------------------------------------------------------------------

3Dモデルやプレビュー画像の表示にはキャッシュを使用します．キャッシュは実行環境に作成され，配布exeには含まれません．
長いキャッシュ生成中は画面中央にロード表示が出ます．大量のユニットキャッシュを作成する場合は時間がかかります．

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
