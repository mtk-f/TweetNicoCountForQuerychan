# TweetNicoCountForQuerychan
クエリちゃんの動画投稿数をツイートするサービス

## スクリーンショット
![スクリーンショット](https://github.com/mtk-f/TweetNicoCountForQuerychan/blob/master/screenshot.png)

## 開発環境
Visual Studio 2015 Community

## ビルド設定
* Twitter の Application Management でキーとトークンを発行します。
* トークンは Access Level が  Read and write の権限が必要です。
* App.config にキーとトークンを設定します。  
App.config の設定例
```xml
    <add key="TwitterApiKey"       value="Consumer Key (API Key)" />
    <add key="TwitterSecretKey"    value="Consumer Secret (API Secret)" />
    <add key="TwitterAccessToken"  value="Access Token" />
    <add key="TwitterAccessSecret" value="Access Token Secret" />
```
