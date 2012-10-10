using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Website.Infrastructure.Command;

namespace Website.Application.Command
{
    public class QueuedCommandScheduler
    {
        private readonly ConcurrentDictionary<string, Task<WorkInProgress>> _tasks = new ConcurrentDictionary<string, Task<WorkInProgress>>();
        private readonly QueueInterface _messageQueue;
        private readonly CommandSerializerInterface _commandSerializer;
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        private int _messageCount;

        private int _waitLength = 200;
        private const int MaxBackOffLimit = 1000;
        private const int MaxWorkInProgress = 15;
        

        public QueuedCommandScheduler(QueueInterface messageQueue,  
            CommandSerializerInterface commandSerializer, 
            CommandHandlerRespositoryInterface handlerRespository)
        {
            _messageQueue = messageQueue;
            _commandSerializer = commandSerializer;
            _handlerRespository = handlerRespository;
        }

        
        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.CheckProcessedMessages() >= MaxWorkInProgress) continue;

                var gotMsg = this.CheckForMessage();
                if (!gotMsg)
                {
                    Thread.Sleep(_waitLength);
                    _waitLength = Math.Min(MaxBackOffLimit, _waitLength + 200);
                }
                else
                {
                    _waitLength = 200;
                }
            }

            while(this.CheckProcessedMessages() > 0)
            {
            }
        }

        public int GetMessageCount()
        {
            return _messageCount;
        }

        private class WorkInProgress
        {
            public string Id { get; set; }
            public QueueMessageInterface Message { get; set; }
            public QueuedCommandResult Result { get; set; }
            public dynamic Command { get; set; }
        }

        private bool CheckForMessage()
        {
            var message = _messageQueue.GetMessage();
            if (message == null) 
               return false;
            

            var workInProgress = new WorkInProgress()
                                     {
                                         Id = Guid.NewGuid().ToString(),
                                         Message = message,
                                     };
            
            var task = new Task<WorkInProgress>(() => TaskProc(workInProgress));
            _tasks[workInProgress.Id] = task;
            task.Start();
            return true;
        }

        private int CheckProcessedMessages()
        {
            foreach (var wip in _tasks.ToArray())
            {
                try
                {
                    if (!wip.Value.IsCompleted) continue;

                    Task<WorkInProgress> wipTask;
                    if (!_tasks.TryRemove(wip.Key, out wipTask)) continue;
                    if (wipTask.IsCanceled || wipTask.IsFaulted) continue;
                    if (wipTask.Result.Result == QueuedCommandResult.Retry) continue;

                    _commandSerializer.ReleaseCommand(wipTask.Result.Command);
                    _messageQueue.DeleteMessage(wipTask.Result.Message);
                    _messageCount++;
                }
                catch (Exception e)
                {
                    Trace.TraceError("QueuedCommandScheduler Error: %s, Stack %s", e.Message, e.StackTrace);
                }
            }

            return _tasks.Count;
        }

        private WorkInProgress TaskProc(WorkInProgress work)
        {
            //var work = wip as WorkInProgress;
            if (work == null)
                return null;

            dynamic command = _commandSerializer.FromByteArray<CommandInterface>(work.Message.Bytes);
            work.Command = command;

            if(command == null)
            {
                Trace.TraceInformation("QueuedCommandScheduler TaskProc couldn't de-serialize message");
                work.Result = QueuedCommandResult.Error;
                return work;
            }

            try
            {
                var handler = _handlerRespository.FindHandler(command);
                if(handler != null)
                {
                    var ret = handler.Handle(command);
                    if(ret is QueuedCommandResult)
                        work.Result = ret;
                    else
                        work.Result = QueuedCommandResult.Successful;
                }
                    
            }
            catch (Exception e)
            {
                Trace.TraceError("QueuedCommandScheduler TaskProc Error: %s, Stack %s", e.Message, e.StackTrace);
                work.Result = QueuedCommandResult.Error;

            }
    
            return work;
        }
    }
}
