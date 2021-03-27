using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BackGroundWorker.Message
{
    public class UserMessage : AbstractMessage
    {
        public string O365UserUPN { get; set; }
    }
}
