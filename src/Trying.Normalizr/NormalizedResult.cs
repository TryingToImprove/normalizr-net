using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trying.Normalizr
{
    public class NormalizedResult
    {
        [JsonProperty("entities")]
        public Dictionary<string, Dictionary<object, object>> Entities { get; set; }

        [JsonProperty("result")]
        public object Result { get; set; }
    }
}