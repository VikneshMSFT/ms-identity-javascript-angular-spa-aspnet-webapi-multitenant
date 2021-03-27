using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BackGroundWorker.MessageHandler;

namespace TodoListAPI.BackGroundWorker
{
    public class Notifier : INotifier
    {
        private Queue<AbstractMessage> MessageQueue = new Queue<AbstractMessage>();
        private Dictionary<string, List<IMessageHandler>> messageHandlers = new Dictionary<string, List<IMessageHandler>>();
        private Object QueueSyncMonitor = new object();
        private int ConcurrentProcessingLimit = 20;
        private int CurrentMessagesHandled = 0;

        public void Notify(AbstractMessage message)
        {
            lock (QueueSyncMonitor) {
                MessageQueue.Enqueue(message);
                Monitor.Pulse(QueueSyncMonitor);
            }            
        }

        public void AddMessageHandler(string messageType, IMessageHandler handler)
        {
            if(!messageHandlers.ContainsKey(messageType))
            {
                messageHandlers[messageType] = new List<IMessageHandler>();
            }
            messageHandlers[messageType].Add(handler);
        }

        private void StartPollerThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    lock (QueueSyncMonitor)
                    {
                        if (MessageQueue.Count == 0 || CurrentMessagesHandled >= ConcurrentProcessingLimit)
                        {
                            Monitor.Wait(QueueSyncMonitor);
                            continue;
                        }                        
                    }
                
                    var message = MessageQueue.Dequeue();
                    var handlers = messageHandlers[message.MessageType];
                    foreach (IMessageHandler handler in handlers)
                    {
                        Thread.Sleep(2000);
                        CurrentMessagesHandled++;
                        ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => handler.HandleMessageAsync(message)));                        
                    }
                }
            });
        }

        public void NotifyCompletion()
        {
            CurrentMessagesHandled--;
            Monitor.Pulse(QueueSyncMonitor);

        }

        public void StartPolingAsync()
        {
            this.StartPollerThread();
        }
    }
}
