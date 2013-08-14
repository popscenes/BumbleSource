using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Website.Application.Queue;
using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public class QueuedMessageProcessor
    {
        private readonly QueueInterface _messageQueue;
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        private int _messageCount;
        private int _waitLength = 200;
        private const int MaxBackOffLimit = 1000;
        private const int MaxWorkInProgress = 15;

        

        public QueuedMessageProcessor(QueueInterface messageQueue,  
            MessageSerializerInterface messageSerializer, 
            MessageHandlerRespositoryInterface handlerRespository)
        {
            _messageQueue = messageQueue;
            _messageSerializer = messageSerializer;
            _handlerRespository = handlerRespository;
        }

        //[UnitTestOnly]
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

                if (ret.Result == QueuedMessageProcessResult.RetryError)
                {
                    WorkComplete(ret);
                    throw new Exception("Shouldn't expect error");
                }
                    

            } while (ret.Result == QueuedMessageProcessResult.Retry);
            
            return ret;
        }
        
        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_wip >= MaxWorkInProgress) continue;

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
            public QueuedMessageProcessResult Result { get; set; }
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
            IncWip();
            var progress = workInProgress;
            var task = new Task<WorkInProgress>(() => TaskProc(progress));
            task.Start();
            var wipret = await task;

            if ((task.IsCanceled || task.IsFaulted) || (wipret.Result > QueuedMessageProcessResult.Retry))
            {
                IncWip(false);
                return wipret;
            }

            WorkComplete(wipret);
            return wipret;
        }

        private int _wip;
        private void IncWip(bool increment = true)
        {
            if(increment)
                Interlocked.Increment(ref _wip);
            else
                Interlocked.Decrement(ref _wip);
        }

        private void WorkComplete(WorkInProgress wipret)
        {
            IncWip(false);            
            lock (_messageQueue)
            {
                _messageSerializer.ReleaseCommand(wipret.Command);
                _messageQueue.DeleteMessage(wipret.Message);
                _messageCount++;
            }
        }

        private WorkInProgress TaskProc(WorkInProgress work)
        {
            //var work = wip as WorkInProgress;
            if (work == null)
                return null;

            dynamic command = _messageSerializer.FromByteArray<MessageInterface>(work.Message.Bytes);
            work.Command = command;

            if(command == null)
            {
                Trace.TraceInformation("QueuedCommandScheduler TaskProc couldn't de-serialize message");
                work.Result = QueuedMessageProcessResult.Error;
                return work;
            }

            try
            {
                var handler = _handlerRespository.FindHandler(command);
                if(handler != null)
                {
                    var ret = handler.Handle(command);
                    if(ret is QueuedMessageProcessResult)
                        work.Result = ret;
                    else
                        work.Result = QueuedMessageProcessResult.Successful;
                }
                else
                {
                    work.Result = QueuedMessageProcessResult.Retry;
                }
                    
            }
            catch (Exception e)
            {
                Trace.TraceError("QueuedCommandScheduler TaskProc Error: {0}, Stack {1}", e.Message, e.StackTrace);
                work.Result = QueuedMessageProcessResult.RetryError;

            }
    
            return work;
        }
    }
}
