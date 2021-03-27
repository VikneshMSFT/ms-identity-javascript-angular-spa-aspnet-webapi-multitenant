using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels;

namespace TodoListAPI.BackGroundWorker.MessageHandler.ApiResponses
{
    public class ChannelList
    {
        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }

        [JsonProperty("channels")]
        public List<ZoomChannel> channelList { get; set;  }
    }
}
