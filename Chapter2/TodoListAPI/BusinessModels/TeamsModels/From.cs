using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{    

    public class From
    {
        [JsonProperty(PropertyName = "user")]
        public User user { get; set; }
    }

    public class ChatMessageRequest
    {
        [JsonProperty(PropertyName = "createdDateTime")]
        public string createdDateTime { get; set; }

        [JsonProperty(PropertyName = "body")]
        public ItemBody body { get; set; }

        [JsonProperty(PropertyName = "from")]
        public From from { get; set; }

    }

    public class CreateChannelResponse
    {
        [JsonProperty(PropertyName = "@odata.context")]
        string context;

        [JsonProperty(PropertyName = "id")]
        string id;

        [JsonProperty(PropertyName = "createdDateTime")]
        string createdDateTime;

        [JsonProperty(PropertyName = "displayName")]
        string displayName;

        [JsonProperty(PropertyName = "description")]
        string description;

    }

    public class AddMemberToTeam
    {
        [JsonProperty(PropertyName = "@odata.type")]
        public string type { get; set; }

        [JsonProperty(PropertyName = "roles")]
        public string[] roles { get; set; }

        [JsonProperty(PropertyName = "user@odata.bind")]
        public string bind { get; set; }

    }
}
