using System.Collections.Generic;

namespace TweetNicoCount
{
    // JSONと同じ構造のクラス定義.
    class JsonDefinition
    {
        public string Dqnid { get; set; }
        public string Type { get; set; }
        public List<JsonDefinitionValue> Values { get; set; }
    }
}
