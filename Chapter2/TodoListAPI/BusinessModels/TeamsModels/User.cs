using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string displayName { get; set; }

        [JsonProperty(PropertyName = "userIdentityType")]
        public string userIdentityType { get; set; }

    }
}
