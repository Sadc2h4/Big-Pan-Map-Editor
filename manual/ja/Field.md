# 地上モード

## マップ一覧

- 地上モードでは user/Abe/map と user/Kando/map を探索し，地上マップを一覧表示します．
- マップを選択すると，route.txt と generator 関連ファイルを読み込みます．
- 表示用3Dモデルが生成済みの場合，地上 3D表示で確認できます．
- 地上モードの object は通常の洞窟 Spawn とは異なり，generator ファイルと経過日数条件に従って表示されます．
***

## 経過日数条件
![MD_Description_15](images/MD_Description_15.png)
- 地上 generator コンソールの経過日数を変更すると，その日数で有効な generator object だけを表示します．
- generator ファイルごとの条件により，同じマップでも表示される object が変化します．
- 表示対象外の object は編集対象から外れます．
- 日数条件を確認しながら編集することで，ゲーム内で実際に出現する状態に近い配置を確認できます．
***

## 地上 object の追加

- 地上 generator コンソールで追加先 generator を選択します．
- 追加テンプレートで Teki，Item，Pikmin，Cave Entrance を選択できます．
- ペンボタンを有効にしてマップ上をクリックすると，選択した generator に object を追加します．
- 追加された object は現在の経過日数条件に従って表示されます．
- オニオン，ロケット，橋，ゲート，シーソーブロックなどは専用アイコンや矩形表示で見分けやすくしています．
***

## 地上 object の編集

- 既存 object は左クリックで選択できます．
- 移動ボタンで object の座標を編集できます．
- Angle モードで向きを編集できます．
- Radius モードで半径を編集できます．
- 橋やゲートなど，配置範囲を持つ object は矩形で表示されます．
- Raw 欄には選択 object の generator テキストが表示されます．
- Raw を直接編集した場合は，選択 object raw 反映を押すことで内容を反映します．
- Raw 側に対応項目がない場合，一部のGUI編集は保存に反映できないことがあります．
***

## Route と保存

- 地上モードでも route.txt の Waypoint を表示，移動，編集できます．
- 保存時は route.txt と generator txt をマップフォルダへ上書きします．
- 地上 generator は洞窟 layout より条件が複雑なため，大きな編集前には対象マップフォルダのバックアップを推奨します．
