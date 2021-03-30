using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels
{
    /* 
     *  "id": "F3CbafdljsfjkdfgBA7",
      "message": "And you?",
      "sender": "myemail@someemailaddr.com",
      "date_time": "2019-09-17T20:25:21Z",
      "timestamp":  1568751921626
                    1616991997975
     */
    public class ChannelMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("timestamp")]
        public long EpochTime { get; set; }

        [JsonProperty("date_time")]
        public string DateTime { get; set; }
    }
}
