# 初期設定

## 初期設定
- このアプリを正常に動作させるためにはHokotate Tool Kit.exeが必須のため，事前に用意しておいてください．
![MD_Description_2](images/MD_Description_2.png)
***

## ホーム画面
- Big Pan Map Editor を起動するとホーム画面が表示されます．
- 上段の ISO / GCR / root 指定欄には，ISO，GCR，抽出済みディスクフォルダ，
- または洞窟 mapunits の arc フォルダを指定できます． 下段の Hocotate_Toolkit 指定欄には Hocotate_Toolkit.exe を指定します．

- Cave Gen Editor は洞窟ユニット編集画面へ移動します．
- Field Gen Editor は地上マップ編集画面へ移動します．
- About Hocotate Tool Kit は Hocotate Tool Kit の配布ページを開きます．
- Editor Manual はこのマニュアルを開きます．
- 言語選択はホーム画面と編集画面の主要UIに反映されます．
***

## 読み込み対象
- ISO / GCR を指定した場合，Hocotate_Toolkit.exe を使ってディスク抽出を試行します．
![MD_Description_4](images/MD_Description_3.png)

- sys/files を含む抽出済みディスクフォルダを指定した場合，抽出処理なしで参照先を探索します．
- 洞窟モードでは user/Mukki/mapunits/arc を直接指定できます．この場合，arc 配下の各ユニットが一覧に表示されます．
- arc.szs と texts.szs を直下に持つ単体ユニットフォルダを指定した場合，そのユニットだけを直接表示できます．
- 地上モードでは user/Abe/map と user/Kando/map を探索し，地上マップ一覧を表示します．
![MD_Description_1](images/MD_Description_1.png)
***

## キャッシュ
- 3Dモデルやpretty画像の表示にはキャッシュを使用します．
![MD_Description_4](images/MD_Description_4.png)

- 洞窟モードではユニット単位のキャッシュ生成と，全ユニットキャッシュ生成を使用できます．
- 地上モードではマップ単位の表示用キャッシュを使用します．
- Hocotate_Toolkit.exe が未設定の場合，SZS 展開が必要なキャッシュ生成はスキップされます．
- キャッシュ確認ボタンから，生成済みのユニットキャッシュや画像キャッシュのフォルダを確認できます．
