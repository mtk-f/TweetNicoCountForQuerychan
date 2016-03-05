using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using CoreTweet;
using System.Configuration;
using System.Threading;

namespace TweetNicoCount
{
    class Program
    {
        private const string TWITTER_SCREEN_NAME = "@mtk_f";
        private const string KEYWORD = "mmd クエリちゃん";
        private const string TWEET_TAG = "#クエリちゃんの動画投稿数をツイートするサービス #自動";

        // 非同期待ちフラグ.
        private static bool IsDoing { get; set; } = false;

        static void Main()
        {
            IsDoing = true;

#pragma warning disable 4014
            TweetQuerychanCountAsync();
#pragma warning restore 4014

            // エントリーポイントはasyncできないので
            // 非同期処理が終わるまで待つ苦肉の策.
            // wait async.
            var startTime = DateTime.Now;
            while (IsDoing)
            {
                Thread.Sleep(10);

                // 無限ループ防止のため1分経ったら強制終了する.
                var diff = DateTime.Now - startTime;
                if (diff.TotalMinutes > 1)
                {
                    Console.WriteLine("timeout");
                    break;
                }
            }
#if DEBUG
            Console.WriteLine("fin");
            Console.ReadLine();
#endif
        }

        private static async Task TweetQuerychanCountAsync()
        {
            const string URL = "http://api.search.nicovideo.jp/api/";
            const string MEDIA_TYPE = "application/json";
            var postConstnt
                    = string.Format("{{\"query\":\"{0}\",\"service\":[\"video\"],\"search\":[\"title\",\"description\",\"tags\"],\"join\":[\"cmsid\",\"title\",\"description\",\"thumbnail_url\",\"start_time\",\"view_counter\",\"comment_counter\",\"mylist_counter\",\"channel_id\",\"main_community_id\",\"length_seconds\",\"last_res_body\"],\"filters\":[],\"sort_by\":\"start_time\",\"order\":\"desc\",\"from\":0,\"size\":25,\"timeout\":10000,\"issuer\":\"pc\",\"reason\":\"user\"}}", KEYWORD);

            string rawJson = null;

            using (var client = new HttpClient())
            {
                // 検索して結果のJSONを取得する.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
                var content = new StringContent(postConstnt, System.Text.Encoding.UTF8, MEDIA_TYPE);
                var response = await client.PostAsync(URL, content);
                if (response.IsSuccessStatusCode)
                {
                    rawJson = await response.Content.ReadAsStringAsync();
                }
            }

            if (rawJson != null)
            {
                // 1行目のJSONを抜き出す.
                var jsonFistLine = rawJson.Substring(0, rawJson.IndexOf("\n"));

                // JsonConvertがパースできない [] を除去する.
                var sanitizedJson = jsonFistLine.Replace("[", "").Replace("]", "");

                // JSONと同じ構造の匿名型を定義する.
                var definition = new
                {
                    Dqnid = "",
                    Type = "",
                    Values = new
                    {
                        _rowid = 0,
                        Service = "",
                        Total = 0,
                    },
                };

                // 検索結果のJSONをパースする.
                try
                {
                    var json = JsonConvert.DeserializeAnonymousType(sanitizedJson, definition);
                    var total = json.Values.Total;

                    var msg = string.Format("nicovide.jpに投稿された {0} の動画は ＼ {1}件／ です\n"
                        + "http://search.nicovideo.jp/video/search/mmd%20%E3%82%AF%E3%82%A8%E3%83%AA%E3%81%A1%E3%82%83%E3%82%93?sort=upload_time\n"
                        + "{2}",
                        KEYWORD, total, TWEET_TAG);
                    TweetMessage(msg);
                }
                catch (JsonException e)
                {
                    var msg = string.Format("{0} JsonException\n{1} {2}", TWITTER_SCREEN_NAME, e.Message, TWEET_TAG);
                    Console.WriteLine(msg);
                    TweetMessage(msg);
                }
            }
            else
            {
                var msg = string.Format("{0} failed to get web contents {1}", TWITTER_SCREEN_NAME, TWEET_TAG);
                Console.WriteLine(msg);
                TweetMessage(msg);
            }

            IsDoing = false;
        }

        private static void TweetMessage(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#else
            var twitterApiKey = ConfigurationManager.AppSettings["TwitterApiKey"];
            var twitterSecretKey = ConfigurationManager.AppSettings["TwitterSecretKey"];
            var twitterAccessToken = ConfigurationManager.AppSettings["twitterAccessToken"];
            var twitterAccessSecret = ConfigurationManager.AppSettings["TwitterAccessSecret"];
            var tokens = Tokens.Create(twitterApiKey, twitterSecretKey, twitterAccessToken, twitterAccessSecret);
            tokens.Statuses.Update(status => msg);
#endif
        }
    }
}
