using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{
    public class CreateTeam
    {
        [JsonProperty(PropertyName = "@microsoft.graph.teamCreationMode")]
        public string teamCreationMode { get; set; }

        [JsonProperty(PropertyName = "bind")]
        public string bind { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string displayName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "createdDateTime")]
        public string createdDateTime { get; set; }
    }
    
}
