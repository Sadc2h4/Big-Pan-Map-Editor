# 基本操作

## 画面構成
![MD_Description_5](images/MD_Description_5.png)
- 左側には参照先，読み込み形式，モード切替，ユニットまたはマップ一覧が表示されます．
- 中央には2Dマップ表示と，必要に応じて3D表示が表示されます．
- 右側には選択中の Spawn，Waypoint，Waterbox，地上 object の詳細が表示されます．
- 下部のコンソールには読み込み結果，保存結果，エラー情報が表示されます．
- ミニコントローラーでは Spawn，Route，Waterbox の編集対象と表示状態を切り替えます．
- 地上モードでは地上 generator コンソールが追加表示され，経過日数や追加先 generator を操作できます．
***

## オーバーレイ
- スポーン表示は Spawn や地上 object のポイントを表示します．
- ルート表示は Waypoint と接続線を表示します．
- Radius 表示は半径を持つ対象の範囲を表示します．
- 水を表示は Waterbox を表示します．
- 洞窟モードでは Spawn，Route，Waterbox のバナーが編集対象に合わせて切り替わります．
***

## 3D表示
![MD_Description_6](images/MD_Description_6.png)
- OBJ 3D表示は洞窟ユニットの3Dモデルを表示します．
- 地上 3D表示は地上マップの3Dモデルを表示します．
- 3D表示は配置確認用です．細かい編集は2D表示とインスペクタを併用すると確認しやすくなります．
- モデルが表示されない場合は，Hocotate_Toolkit.exe の設定とキャッシュ生成状況を確認してください．

- 3Dモードの操作は2Dモードと異なるため注意してください．右クリックで回転，マウスホイール押下で視点移動ができます．
![MD_Description_8](images/MD_Description_8.png)
***

## 選択と移動
![MD_Description_12](images/MD_Description_12.png)
- 左クリックで Spawn，Waypoint，Waterbox，地上 object を選択します．
- 移動ボタンを有効にすると，選択ポイントを左ドラッグで移動できます．
- 3D表示中も選択中の対象に対して移動や角度編集を行えますが，操作モードによって右ドラッグの役割が変わります．
- フローティングコンソールは上部グリップをドラッグして移動できます．
- フローティングコンソールの最小化ボタンで，表示領域を小さくできます．
- ウォーターボックスやルートはCtrlを押しながらで垂直方向，Shiftを押しながらで水平方向を固定して移動できます．
![MD_Description_16](images/MD_Description_16.png)

***


## 追加と削除
![MD_Description_7](images/MD_Description_7.png)
- ペンボタンはクリック追加モードです．有効にした後，マップ上をクリックすると現在対象のポイントを追加します．
- 消しゴムボタンはクリック削除モードです．有効にした後，対象ポイントをクリックすると削除します．
- 追加モード中でも対象の種類はミニコントローラーで切り替えます．
- 洞窟モードの Spawn 追加では Spawn Type を選択してから配置します．
- 地上モードの Spawn 追加では，地上 generator コンソールで追加先 generator と追加テンプレートを選択します．
***

## Spawn 編集
- ミニコントローラーで Spawn を選択すると，layout.txt の Spawn を編集できます．
- ペンボタンで Spawn を追加します．追加時の Type はミニコントローラーの Spawn Type で指定します．
- 消しゴムボタンで Spawn を削除します．
- 移動ボタンで Spawn をドラッグ移動します．
- Angle モードで Spawn の向きを編集します．
- Radius モードで Spawn の半径を編集します．
- 右インスペクタでは Type，Min Count，Max Count，位置，角度，半径などを編集できます．
***

## Route 編集
![MD_Description_13](images/MD_Description_13.png)
- ミニコントローラーで Route を選択すると，route.txt の Waypoint を編集できます．
- ペンボタンで Waypoint を追加します．
- 消しゴムボタンで Waypoint を削除します．
- 移動ボタンで Waypoint をドラッグ移動します．
- Radius モードで Waypoint の半径を編集します．
- 接続追加と接続削除で Waypoint 同士のリンクを編集できます．
- 接続座標へ移動ボタンは，選択 Waypoint を最寄りのユニット接続座標へ移動します．
- ルート矢印の色はルート間の高低差を色に指定表示したものです．
![MD_Description_10](images/MD_Description_10.png)
***

## Waterbox 編集
![MD_Description_14](images/MD_Description_14.png)
- ミニコントローラーで Waterbox を選択すると，waterbox を編集できます．
- ペンボタンで Waterbox を追加します．
- 消しゴムボタンで Waterbox を削除します．
- 移動ボタンで Waterbox の中心位置を移動します．
- Waterbox は矩形として表示され，XZ方向の範囲を確認できます．
- 3D表示では高さ方向の確認に使用できます．
***

## Angle と Radiusの編集方法
![MD_Description_9](images/MD_Description_9.png)
- Angle は Spawn や地上 object の向きを編集するためのモードです．
- 2D表示では三角ハンドルをドラッグするか，選択ポイントから右ドラッグして向きを変更します．
- 3D表示ではモードにより右ドラッグがカメラ回転または選択 Spawn の角度編集になります．
- Radius は Spawn，Route，地上 object の半径や範囲を編集するためのモードです．
- Radius を有効にした後，選択ポイントから目的の半径位置までドラッグします．
- Raw 側に半径項目を持たない地上 object は，Radius 編集をしても保存先に反映できない場合があります．
***

## 表示切替
- スポーン表示，ルート表示，水を表示で各オーバーレイを切り替えます．
- Radius 表示を有効にすると，半径を持つ対象の範囲を確認できます．
- OBJ 3D表示は洞窟ユニットの3Dモデル確認に使用します．
- 地上 3D表示は地上マップの3Dモデル確認に使用します．
