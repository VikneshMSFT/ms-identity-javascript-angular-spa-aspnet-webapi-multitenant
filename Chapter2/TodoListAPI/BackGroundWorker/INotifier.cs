using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BackGroundWorker.MessageHandler;

namespace TodoListAPI.BackGroundWorker
{
    public interface INotifier
    {
        public void Notify(AbstractMessage message);

        public void AddMessageHandler(string messageType, IMessageHandler handler);

        public void NotifyCompletion();

        public void StartPolingAsync();
    
    }
}
