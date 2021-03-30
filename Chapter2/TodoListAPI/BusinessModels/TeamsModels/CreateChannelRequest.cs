using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{
    public class CreateChannelRequest
    {
        [JsonProperty(PropertyName = "@microsoft.graph.channelCreationMode")]
        public string channelCreationMode { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string displayName
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "membershipType")]
        public string membershipType { get; set; }

        [JsonProperty(PropertyName = "createdDateTime")]
        public string createdDateTime { get; set; }
    }
}
