using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Command
{
    public class QueuedCommandProcessor
    {
        private readonly QueueInterface _messageQueue;
        private readonly CommandSerializerInterface _commandSerializer;
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        private int _messageCount;
        private int _waitLength = 200;
        private const int MaxBackOffLimit = 1000;
        private const int MaxWorkInProgress = 15;

        

        public QueuedCommandProcessor(QueueInterface messageQueue,  
            CommandSerializerInterface commandSerializer, 
            CommandHandlerRespositoryInterface handlerRespository)
        {
            _messageQueue = messageQueue;
            _commandSerializer = commandSerializer;
            _handlerRespository = handlerRespository;
        }

        public WorkInProgress ProcessOneSynch()
        {
            WorkInProgress ret = null;
            var message = GetMessage();

            do
            {                
                if (message == null)
                    return null;


                var workInProgress = new WorkInProgress()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Message = message,
                    };

                ret = RunTask(workInProgress).Result;

            } while (ret.Result == QueuedCommandResult.Retry);
            
            return ret;
        }
        
        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Wip >= MaxWorkInProgress) continue;

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

            while (_wip > 0)
            {
                Thread.Sleep(_waitLength);
            }
        }

        public int GetMessageCount()
        {
            return _messageCount;
        }

        public class WorkInProgress
        {
            public string Id { get; set; }
            public QueueMessageInterface Message { get; set; }
            public QueuedCommandResult Result { get; set; }
            public dynamic Command { get; set; }
        }

        private bool CheckForMessage()
        {
            var message = GetMessage();
            if (message == null) 
               return false;
            

            var workInProgress = new WorkInProgress()
                                     {
                                         Id = Guid.NewGuid().ToString(),
                                         Message = message,
                                     };

            RunTask(workInProgress);
            return true;
        }

        private QueueMessageInterface GetMessage()
        {
            lock (_messageQueue)
            {
                return _messageQueue.GetMessage();
            }          
        }

        private async Task<WorkInProgress> RunTask(WorkInProgress workInProgress)
        {
            Wip++;
            var progress = workInProgress;
            var task = new Task<WorkInProgress>(() => TaskProc(progress));
            task.Start();
            var wipret = await task;

            if ((task.IsCanceled || task.IsFaulted) || (wipret.Result == QueuedCommandResult.Retry))
            {
                Wip--;
                return wipret;
            }

            WorkComplete(wipret);
            return wipret;
        }

        private int _wip;
        private int Wip
        {
            get
            {
                lock (_messageQueue)
                {
                    return _wip;
                }
            }
            set
            {
                lock (_messageQueue)
                {
                    _wip = value;
                }
            }
        }

        private void WorkComplete(WorkInProgress wipret)
        {
            Wip--;
            lock (_messageQueue)
            {
                _commandSerializer.ReleaseCommand(wipret.Command);
                _messageQueue.DeleteMessage(wipret.Message);
                _messageCount++;
            }
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
                else
                {
                    work.Result = QueuedCommandResult.Retry;
                }
                    
            }
            catch (Exception e)
            {
                Trace.TraceError("QueuedCommandScheduler TaskProc Error: {0}, Stack {1}", e.Message, e.StackTrace);
                work.Result = QueuedCommandResult.Retry;

            }
    
            return work;
        }
    }
}
