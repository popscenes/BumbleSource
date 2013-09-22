using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Website.Infrastructure.Command;

namespace Website.Infrastructure.Task
{
    public class QueueWorker<WorkType> where WorkType : class
    {
        private readonly Action<WorkType> _workProcess;
        private readonly CancellationToken _cancellationToken;
        private readonly BlockingCollection<WorkType> _workQueue;
        private readonly ConcurrentQueue<WorkType> _completeQueue;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private System.Threading.Tasks.Task _workerTask;

        public QueueWorker(Action<WorkType> workProcess, CancellationToken cancellationToken
            , int maxWork, ConcurrentQueue<WorkType> completeQueue
            , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _workProcess = workProcess;
            _cancellationToken = cancellationToken;
            _completeQueue = completeQueue;
            _unitOfWorkFactory = unitOfWorkFactory;
            _workQueue = new BlockingCollection<WorkType>(new ConcurrentQueue<WorkType>(), maxWork);
            _workerTask = new System.Threading.Tasks.Task(Run, _cancellationToken, TaskCreationOptions.LongRunning);
        }

        public BlockingCollection<WorkType> WorkQueue
        {
            get { return _workQueue; }
        }

        public TaskStatus Status
        {
            get { return _workerTask.Status; }
        }

        public void Start()
        {
            if (_workerTask.Status == TaskStatus.Running) return;

            if (_workerTask.Status == TaskStatus.Faulted)
                _workerTask = new System.Threading.Tasks.Task(Run, _cancellationToken, TaskCreationOptions.LongRunning);

            _workerTask.Start();
        }

        protected void Run()
        {
            while (!_cancellationToken.IsCancellationRequested || _workQueue.Count > 0)
            {
                try
                {
                    var wip = _cancellationToken.IsCancellationRequested && _workQueue.Count > 0 
                                  ? _workQueue.Take()
                                  : _workQueue.Take(_cancellationToken);
                    if (wip == null) continue;

                    using (_unitOfWorkFactory.GetUowInContext().Begin())
                    {
                        _workProcess(wip);
                    }

                    if(_completeQueue != null)
                        _completeQueue.Enqueue(wip);
                }
                catch (Exception e)
                {
                    if(!(e is OperationCanceledException))
                        Trace.WriteLine(e);
                }
            }
        }
    }
}