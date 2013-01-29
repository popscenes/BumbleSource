using System;
using PostaFlya.Domain.Flier.Analytic;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Command
{
    public class FlierAnalyticCommandHandler : CommandHandlerInterface<FlierAnalyticCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;

        public FlierAnalyticCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory, GenericRepositoryInterface repository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
        }

        public object Handle(FlierAnalyticCommand command)
        {
            var newRecord = new FlierAnalytic()
                {
                    Id = Guid.NewGuid().ToString(),
                    AggregateId = command.FlierId,
                    AggregateTypeTag = typeof(Flier).FullName,
                    BrowserId = command.Browser.Id,
                    TemporaryBrowser = command.Browser.IsTemporary(),
                    IpAddress = command.IpAddress,
                    Location = command.Location,
                    UserAgent = command.UserAgent,
                    TrackingId = command.TrackingId,
                    Time = command.Time,
                    SourceAction = command.SourceAction,
                    LocationFromSearch = command.LocationFromSearch
                };

            var uow = _unitOfWorkFactory.GetUnitOfWork(_repository);           
            using (uow)
            {
                _repository.Store(newRecord);
            }

            if (uow.Successful)
                return new MsgResponse("Analytic Command Recorded", false)
                    .AddEntityId(newRecord.Id)
                    .AddCommandId(command);

            
            return new MsgResponse("Failed to Record Analytic Command", true)
                    .AddEntityId(newRecord.Id)
                    .AddCommandId(command);
        }
    }
}