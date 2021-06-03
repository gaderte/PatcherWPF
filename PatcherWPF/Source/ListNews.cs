using System.Collections.Generic;
using Newtonsoft.Json;

namespace PatcherWPF.Source
{
    class ListNews
    {
        [JsonProperty("news")]
        public List<Source.News> news { get; set; }
    }
}
