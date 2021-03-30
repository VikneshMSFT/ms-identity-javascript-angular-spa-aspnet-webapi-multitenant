using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels.TeamsModels
{
    public class ItemBody
    {
        [JsonProperty(PropertyName = "contentType")]
        public string contentType { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string content { get; set; }
    }
}
