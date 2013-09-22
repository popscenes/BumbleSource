using System;
using PostaFlya.Domain.Flier.Analytic;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Domain.Flier.Command
{
    public class FlierAnalyticCommandHandler : MessageHandlerInterface<FlierAnalyticCommand>
    {
        private readonly GenericRepositoryInterface _repository;

        public FlierAnalyticCommandHandler(GenericRepositoryInterface repository)
        {
            _repository = repository;
        }

        public void Handle(FlierAnalyticCommand command)
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

            _repository.Store(newRecord);

        }
    }
}