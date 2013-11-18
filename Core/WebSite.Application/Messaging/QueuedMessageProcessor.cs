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
        private readonly QueueReceiverInterface _messageQueue;
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly ProcessorTaskInterface _queueProcessorTask;
        
        private int _completeCount;
        private int _waitLength = 200;
        private const int MaxBackOffLimit = 1000;

        private readonly ConcurrentQueue<WorkInProgress> _completedWork;
        private readonly QueueWorker<WorkInProgress>[] _workers;
        private const int WorkerQueueBounds = 15;


        public QueuedMessageProcessor(QueueReceiverInterface messageQueue,
            MessageSerializerInterface messageSerializer, ProcessorTaskInterface queueProcessorTask, int numWorkers = 4)
        {
            _messageQueue = messageQueue;
            _messageSerializer = messageSerializer;
            _queueProcessorTask = queueProcessorTask;
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

                ret = _queueProcessorTask.TaskProc(workInProgress);

            } while (ret.Result == QueuedMessageProcessResult.Retry);

            _messageSerializer.ReleaseCommand(ret.Command);
            _messageQueue.DeleteMessage(ret.Message);
            if (ret.Result == QueuedMessageProcessResult.RetryError)
            {
                throw new Exception("Shouldn't expect error");
            }
            
            return ret;
        }
        

        private void InitWorkers(CancellationToken cancellationToken)
        {
            for (var i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new QueueWorker<WorkInProgress>(
                    (w) => _queueProcessorTask.TaskProc(w)
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
    }
}
