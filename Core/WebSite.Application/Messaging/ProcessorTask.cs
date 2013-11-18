using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Syntax;
using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public interface ProcessorTaskInterface
    {
        QueuedMessageProcessor.WorkInProgress TaskProc(QueuedMessageProcessor.WorkInProgress work);
    }

    
    public class SubscriptionProcessorTask : ProcessorTaskInterface
    {
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly Type _eventHandlerType;
        private readonly IResolutionRoot _resolutionRoot;

        public SubscriptionProcessorTask(MessageSerializerInterface messageSerializer, IResolutionRoot resolutionRoot)
        {
            _messageSerializer = messageSerializer;
            _resolutionRoot = resolutionRoot;
        }

        public QueuedMessageProcessor.WorkInProgress TaskProc(QueuedMessageProcessor.WorkInProgress work)
        {
            //var work = wip as WorkInProgress;
            if (work == null)
                return null;

            dynamic command = _messageSerializer.FromByteArray<MessageInterface>(work.Message.Bytes);
            work.Command = command;

            return null;
        }
    }
    public class QueueProcessorTask : ProcessorTaskInterface
    {
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        private readonly MessageSerializerInterface _messageSerializer;

        public QueueProcessorTask(MessageHandlerRespositoryInterface handlerRespository, MessageSerializerInterface messageSerializer)
        {
            _handlerRespository = handlerRespository;
            _messageSerializer = messageSerializer;
        }

        public QueuedMessageProcessor.WorkInProgress TaskProc(QueuedMessageProcessor.WorkInProgress work)
        {
            //var work = wip as WorkInProgress;
            if (work == null)
                return null;

            dynamic command = _messageSerializer.FromByteArray<MessageInterface>(work.Message.Bytes);
            work.Command = command;

            if (command == null)
            {
                Trace.TraceInformation("QueuedCommandScheduler TaskProc couldn't de-serialize message");
                work.Result = QueuedMessageProcessResult.Error;
                return work;
            }

            try
            {
                var handler = _handlerRespository.FindHandler(command);
                if (handler != null)
                {
                    var ret = handler.Handle(command);
                    if (ret is QueuedMessageProcessResult)
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
