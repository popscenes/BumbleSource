using System;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Binding;
using Website.Domain.Location;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Application.Domain.Flier
{
    public class DefaultFlierWebAnalyticService : FlierWebAnalyticServiceInterface
    {
        private readonly MessageBusInterface _workerMessageBus;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;

        public DefaultFlierWebAnalyticService([WorkerCommandBus]MessageBusInterface workerMessageBus
            , PostaFlyaBrowserInformationInterface browserInformation)
        {
            _workerMessageBus = workerMessageBus;
            _browserInformation = browserInformation;
        }

        public void RecordVisit(string flierId, FlierAnalyticSourceAction context, Location location = null)
        {
            if (string.IsNullOrWhiteSpace(_browserInformation.TrackingId))
                _browserInformation.TrackingId = Guid.NewGuid().ToString();

//            _workerCommandBus.Send(new FlierAnalyticCommand()
//            {
//                FlierId = flierId,
//                Browser = _browserInformation.Browser as Website.Domain.Browser.Browser,
//                TrackingId = _browserInformation.TrackingId,
//                IpAddress = _browserInformation.IpAddress,
//                UserAgent = _browserInformation.UserAgent,
//                Location = location ?? _browserInformation.LastSearchLocation,
//                LocationFromSearch = location == null,
//                SourceAction = context
//            });
        }

        public void SetLastSearchLocation(Location loc)
        {
            if (loc.IsValid)
                _browserInformation.LastSearchLocation = loc;
        }
    }
}