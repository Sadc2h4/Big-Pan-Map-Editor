# 洞窟モード

## ユニット一覧
![MD_Description_11](images/MD_Description_11.png)
- 洞窟モードでは arc 配下のユニットフォルダを一覧表示します．
- 検索欄に文字を入力すると，ユニット名を絞り込めます．
- ユニットを選択すると，pretty画像，3Dモデル，layout，route，waterbox を読み込みます．
- pretty画像が埋め込みリソースにある場合は，exe内の画像を優先して表示します．
- 3Dモデルが必要な場合は，Hocotate_Toolkit.exe を使って arc.szs からキャッシュ生成します．
***

## 保存
- 保存ボタンは現在の編集対象を保存します．
- すべて保存は layout，route，waterbox をまとめて保存します．
- キャッシュ経由で編集している場合，保存時に arc.szs と texts.szs への反映を試行します．
- 保存前には対象フォルダのバックアップを推奨します．
