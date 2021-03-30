using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BackGroundWorker.Message
{
    public class ChannelAddedMessage : UserMessage
    {
        public string ZoomChannelId { get; set; }

        public string ZoomUserId { get; set; }

        public int DateToFetch { get; set; }
    }
}
