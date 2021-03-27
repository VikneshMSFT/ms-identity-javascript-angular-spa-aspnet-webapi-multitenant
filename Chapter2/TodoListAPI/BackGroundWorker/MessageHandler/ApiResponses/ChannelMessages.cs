using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels;

namespace TodoListAPI.BackGroundWorker.MessageHandler.ApiResponses
{
    public class ChannelMessages
    {
        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }

        [JsonProperty("messages")]
        public List<ChannelMessage> messages { get; set; }
    }
}
