using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using CoreTweet;
using System.Configuration;

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
            var postContent
                    = string.Format("{{\"query\":\"{0}\",\"service\":[\"video\"],\"search\":[\"title\",\"description\",\"tags\"],\"join\":[\"cmsid\",\"title\",\"description\",\"thumbnail_url\",\"start_time\",\"view_counter\",\"comment_counter\",\"mylist_counter\",\"channel_id\",\"main_community_id\",\"length_seconds\",\"last_res_body\"],\"filters\":[],\"sort_by\":\"start_time\",\"order\":\"desc\",\"from\":0,\"size\":25,\"timeout\":10000,\"issuer\":\"pc\",\"reason\":\"user\"}}", QUERY);

            // nicovideo.jpをクエリー検索して結果のJSONを取得する.
            var rawJson = await new Func<Task<string>>(async () =>
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
                    var content = new StringContent(postContent, System.Text.Encoding.UTF8, MEDIA_TYPE);
                    var response = await client.PostAsync(URL, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return await Task.FromResult<string>(null);
                    }
                }
            })();

            // ツイッターAPIのトークンを取得する.
            var tokens = Tokens.Create(ConfigurationManager.AppSettings["TwitterApiKey"], 
                ConfigurationManager.AppSettings["TwitterSecretKey"], 
                ConfigurationManager.AppSettings["twitterAccessToken"], 
                ConfigurationManager.AppSettings["TwitterAccessSecret"]);

            if (rawJson != null)
            {
                // 1行目のJSONを抜き出す.
                var jsonFistLine = rawJson.Substring(0, rawJson.IndexOf("\n"));

                // 検索結果のJSONをパースする.
                try
                {
                    var json = JsonConvert.DeserializeObject<JsonDefinition>(jsonFistLine);
                    var total = json.Values[0].Total;
                    var msg = string.Format("nicovide.jpに投稿された {0} の動画は ＼{1}件／ です\n"
                        + "http://search.nicovideo.jp/video/search/{2}?sort=upload_time\n"
                        + "{3}",
                        QUERY, total, Uri.EscapeUriString(QUERY), TWEET_TAG);

                    // 検索した結果をツイッターに投稿する.
#if DEBUG
                    Console.WriteLine(msg);
#else
                    await tokens.Statuses.UpdateAsync(status => msg);
#endif
                }
                catch (JsonException e)
                {
                    var msg = string.Format("{0} JsonException\n{1} {2}", TWITTER_SCREEN_NAME, e.Message, TWEET_TAG);
                    Console.WriteLine(msg);
                    await tokens.Statuses.UpdateAsync(status => msg);
                }
            }
            else
            {
                var msg = string.Format("{0} failed to get Web API responses {1}", TWITTER_SCREEN_NAME, TWEET_TAG);
                Console.WriteLine(msg);
                await tokens.Statuses.UpdateAsync(status => msg);
            }
        }
    }
}
