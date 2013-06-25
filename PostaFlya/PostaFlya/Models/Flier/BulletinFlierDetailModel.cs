using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Flier
{
    public class ToBulletinFlierDetailModel : ViewModelMapperInterface<BulletinFlierDetailModel, PostaFlya.Domain.Flier.Flier>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToBulletinFlierDetailModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BulletinFlierDetailModel ToViewModel(BulletinFlierDetailModel target, Domain.Flier.Flier flier)
        {
            if (target == null)
                target = new BulletinFlierDetailModel();

            _queryChannel.ToViewModel<BulletinFlierSummaryModel, Domain.Flier.Flier>(flier, target);

            target.Description = flier.Description;
            target.UserLinks = _queryChannel.ToViewModel<UserLinkViewModel, UserLink>(flier.UserLinks);

            return target;
        }
    }

    public class BulletinFlierDetailModel : BulletinFlierSummaryModel
    {
        [Display(Name = "FlierDescription", ResourceType = typeof(Properties.Resources))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Properties.Resources), Name = "BulletinFlierModel_UserLinks_UserLinks")]
        public List<UserLinkViewModel> UserLinks { get; set; }
    }
}