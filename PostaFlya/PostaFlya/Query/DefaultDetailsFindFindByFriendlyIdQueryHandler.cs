using PostaFlya.Areas.Default.Models;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Query
{
    public class DefaultDetailsFindFindByFriendlyIdQueryHandler : QueryHandlerInterface<FindByFriendlyIdQuery, DefaultDetailsViewModel>
    {
        private readonly QueryChannelInterface _queryChannel;

        public DefaultDetailsFindFindByFriendlyIdQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public DefaultDetailsViewModel Query(FindByFriendlyIdQuery argument)
        {
            var flyer = _queryChannel.Query(argument, (Flier) null);
            if (flyer == null) return null;
            var ret = new DefaultDetailsViewModel();

            ret.Flier = _queryChannel.ToViewModel<BulletinFlierDetailModel, Flier>(flyer);

            return ret;
        }
    }
}