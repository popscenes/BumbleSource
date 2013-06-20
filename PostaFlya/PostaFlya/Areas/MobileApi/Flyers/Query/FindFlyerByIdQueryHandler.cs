using System;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Query
{
    public class FindFlyerByIdQueryHandler : QueryHandlerInterface<FindByIdQuery, FlyerDetailModel>
    {
        private readonly QueryChannelInterface _queryChannel;

        public FindFlyerByIdQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyerDetailModel Query(FindByIdQuery argument)
        {
            argument.Id = new Guid(argument.Id).ToString();
            var flyer = _queryChannel.Query(argument, (Flier) null);
            if (flyer == null || flyer.Status != FlierStatus.Active)
                return null;
            return _queryChannel.ToViewModel<FlyerDetailModel>(flyer);
        }
    }
}