using System;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;

namespace Website.Application.Publish
{
    internal class PublishCommandHandler : CommandHandlerInterface<PublishCommand>
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _publishService = typeof(PublishServiceInterface<>);

        public PublishCommandHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        readonly ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        public object Handle(PublishCommand command)
        {
            var publishService = _publishService.MakeGenericType(command.PublishObject.GetType());
            var pubservices = _resolutionRoot.GetAll(publishService)
                .Cast<dynamic>()
                .Where(ps => ps.IsEnabled);
            dynamic ob = command.PublishObject;

            var res = Parallel.ForEach(pubservices, _parallelOptions, 
                                       o =>
                                       o.Publish(ob));

            return res.IsCompleted;
        }
    }
}