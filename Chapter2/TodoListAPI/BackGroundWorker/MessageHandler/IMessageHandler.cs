using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;

namespace TodoListAPI.BackGroundWorker.MessageHandler
{
    public interface IMessageHandler
    {
        public Task<bool> HandleMessageAsync(AbstractMessage message);
    }
}
