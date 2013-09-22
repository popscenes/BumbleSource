using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Website.Application.Queue;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Task;

namespace Website.Application.Messaging
{
    public class QueuedMessageProcessor
    {
        private readonly QueueInterface _messageQueue;
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        private int _completeCount;
        private int _waitLength = 200;
        private const int MaxBackOffLimit = 1000;

        private readonly ConcurrentQueue<WorkInProgress> _completedWork;
        private readonly QueueWorker<WorkInProgress>[] _workers;
        private const int WorkerQueueBounds = 15;
        

        public QueuedMessageProcessor(QueueInterface messageQueue,  
            MessageSerializerInterface messageSerializer, 
            MessageHandlerRespositoryInterface handlerRespository, int numWorkers = 4)
        {
            _messageQueue = messageQueue;
            _messageSerializer = messageSerializer;
            _handlerRespository = handlerRespository;
            _workers = new QueueWorker<WorkInProgress>[numWorkers];
            _completedWork = new ConcurrentQueue<WorkInProgress>();

        }

        //[UnitTestOnly]
        public WorkInProgress ProcessOneSynch()
        {
            WorkInProgress ret = null;
            var workInProgress = TryGetMessage();

            do
            {
                if (workInProgress == null)
                    return null;

                ret = TaskProc(workInProgress);

                    

            } while (ret.Result == QueuedMessageProcessResult.Retry);

            _messageSerializer.ReleaseCommand(ret.Command);
            _messageQueue.DeleteMessage(ret.Message);
            if (ret.Result == QueuedMessageProcessResult.RetryError)
            {
                throw new Exception("Shouldn't expect error");
            }
            
            return ret;
        }
        
//        public void Run(CancellationToken cancellationToken)
//        {
//            
//            while (!cancellationToken.IsCancellationRequested)
//            {
//                if (_wip >= _numWorkers) continue;
//
//                var gotMsg = this.CheckForMessage();
//                if (!gotMsg)
//                {
//                    Thread.Sleep(_waitLength);
//                    _waitLength = Math.Min(MaxBackOffLimit, _waitLength + 200);
//                }
//                else
//                {
//                    _waitLength = 200;
//                }
//            }
//
//            while (_wip > 0)
//            {
//                Thread.Sleep(_waitLength);
//            }
//        }

        private void InitWorkers(CancellationToken cancellationToken)
        {
            for (var i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new QueueWorker<WorkInProgress>(
                    (w) => TaskProc(w)
                    , cancellationToken
                    , WorkerQueueBounds
                    , _completedWork);
                _workers[i].Start();
            }
        }

        private void RunInternal(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var gotMsg = this.TryGetMessage();

                if (gotMsg != null && !Dispatch(gotMsg))
                {
                    this.AbandonMessage(gotMsg);
                    gotMsg = null;
                }

                CheckCompleted();

                BackOffWait(gotMsg == null && _completedWork.Count == 0);
            }

            while (_workers.Any(
                worker =>
                    worker.WorkQueue.Count > 0 ||
                    worker.Status == TaskStatus.Running))
            {
                CheckCompleted();
                Thread.Sleep(200);
            }
        }

        public void Run(CancellationToken cancellationToken)
        {
            InitWorkers(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RunInternal(cancellationToken);
                }
                catch (Exception e)
                {
                    Trace.TraceError("error occured queue command processor \n" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        private void AbandonMessage(WorkInProgress gotMsg)
        {
            _messageQueue.ReturnMessage(gotMsg.Message);
        }

        private bool BackOffWait(bool backOff = true)
        {
            if (backOff)
            {
                Thread.Sleep(_waitLength);
                _waitLength = Math.Min(MaxBackOffLimit, _waitLength + 200);
            }
            else
            {
                _waitLength = 200;
            }
            return backOff;
        }

        private void CheckCompleted()
        {
            while (_completedWork.Count > 0)
            {
                WorkInProgress wip;
                if (!_completedWork.TryDequeue(out wip))
                    continue;
                if (wip.Result >= QueuedMessageProcessResult.Retry) continue;

                _messageSerializer.ReleaseCommand(wip.Command);
                _messageQueue.DeleteMessage(wip.Message);
                _completeCount++;
            }
        }

        private bool Dispatch(WorkInProgress gotMsg)
        {

            if(string.IsNullOrWhiteSpace(gotMsg.Message.CorrelationId))
                return _workers.OrderBy(worker => worker.WorkQueue.Count).First().WorkQueue.TryAdd(gotMsg);
            else
            {
                var worker = Math.Abs(gotMsg.Message.CorrelationId.GetHashCode()%_workers.Length);
                return _workers[worker].WorkQueue.TryAdd(gotMsg);
            }
        }

        public int GetMessageCount()
        {
            return _completeCount;
        }

        public class WorkInProgress
        {
            public string Id { get; set; }
            public QueueMessageInterface Message { get; set; }
            public QueuedMessageProcessResult Result { get; set; }
            public dynamic Command { get; set; }
        }

//        private bool CheckForMessage()
//        {
//            var message = GetMessage();
//            if (message == null) 
//               return false;
//            
//
//            var workInProgress = new WorkInProgress()
//                                     {
//                                         Id = Guid.NewGuid().ToString(),
//                                         Message = message,
//                                     };
//
//            RunTask(workInProgress);
//            return true;
//        }

        private WorkInProgress TryGetMessage()
        {
            var message = GetMessage();
            if (message == null)
                return null;


            var workInProgress = new WorkInProgress()
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
            };

            return workInProgress;
        }

        private QueueMessageInterface GetMessage()
        {
            return _messageQueue.GetMessage();
        }

//        private async Task<WorkInProgress> RunTask(WorkInProgress workInProgress)
//        {
//            IncWip();
//            var progress = workInProgress;
//            var task = new Task<WorkInProgress>(() => TaskProc(progress));
//            task.Start();
//            var wipret = await task;
//
//            if ((task.IsCanceled || task.IsFaulted) || (wipret.Result > QueuedMessageProcessResult.Retry))
//            {
//                IncWip(false);
//                return wipret;
//            }
//
//            WorkComplete(wipret);
//            return wipret;
//        }

//        private int _wip;
//        private void IncWip(bool increment = true)
//        {
//            if(increment)
//                Interlocked.Increment(ref _wip);
//            else
//                Interlocked.Decrement(ref _wip);
//        }
//
//        private void WorkComplete(WorkInProgress wipret)
//        {
//            IncWip(false);            
//            lock (_messageQueue)
//            {
//                _messageSerializer.ReleaseCommand(wipret.Command);
//                _messageQueue.DeleteMessage(wipret.Message);
//                _completeCount++;
//            }
//        }

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
                    handler.Handle(command);
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
