using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{
    public class TeamsChannelList
    {
        [JsonProperty("value")]
        public List<TeamsChannel> channels { get; set; }
    }

    public class TeamsChannel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
