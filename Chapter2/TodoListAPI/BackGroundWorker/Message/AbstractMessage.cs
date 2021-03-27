using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BackGroundWorker.Message
{
    public abstract class AbstractMessage
    {
        public string MessageType { get; set; }       

        public int RetryCount { get; set; }

        public string ErrorMessageInPreviousTry { get; set; }
    }
}
