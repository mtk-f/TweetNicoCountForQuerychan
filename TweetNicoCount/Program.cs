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
        static void Main()
        {
            TweetQuerychanCountAsync().Wait();
#if DEBUG
            Console.WriteLine("fin");
            Console.ReadLine();
#endif
        }

        private static async Task TweetQuerychanCountAsync()
        {
            const string TWITTER_SCREEN_NAME = "@mtk_f";
            const string TWEET_TAG = "#クエリちゃんの動画投稿数をツイートするサービス #自動";
            const string URL = "http://api.search.nicovideo.jp/api/";
            const string QUERY = "mmd クエリちゃん";
            const string MEDIA_TYPE = "application/json";
            var postConstnt
                    = string.Format("{{\"query\":\"{0}\",\"service\":[\"video\"],\"search\":[\"title\",\"description\",\"tags\"],\"join\":[\"cmsid\",\"title\",\"description\",\"thumbnail_url\",\"start_time\",\"view_counter\",\"comment_counter\",\"mylist_counter\",\"channel_id\",\"main_community_id\",\"length_seconds\",\"last_res_body\"],\"filters\":[],\"sort_by\":\"start_time\",\"order\":\"desc\",\"from\":0,\"size\":25,\"timeout\":10000,\"issuer\":\"pc\",\"reason\":\"user\"}}", QUERY);

            // nicovideo.jpをクエリー検索して結果のJSONを取得する.
            string rawJson = null;
            using (var client = new HttpClient())
            {
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
                        QUERY, total, TWEET_TAG);
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
                var msg = string.Format("{0} failed to get Web API responses {1}", TWITTER_SCREEN_NAME, TWEET_TAG);
                Console.WriteLine(msg);
                TweetMessage(msg);
            }
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
