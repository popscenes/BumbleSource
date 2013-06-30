using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Boards;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Domain.Flier.Query
{
    public static class FlierQueryServiceUtil
    {

        public static string FindFreeFriendlyIdForFlier(this QueryChannelInterface queryChannel, FlierInterface targetFlier)
        {
            const string pattern = "@dd-MMM-yy";

            var title = targetFlier.Title;
            var tryTitleBase = title.ToLowerHiphen() + targetFlier
                .GetFirstEventDate().DateTime
                .ToString(pattern);
            var tryTitle = tryTitleBase;

            Flier flierFind = null;

            if (queryChannel != null && 
                (flierFind = queryChannel.Query(new FindByFriendlyIdQuery() { FriendlyId = tryTitle }, (Flier)null)) != null
                && flierFind.Id != targetFlier.Id)
            {
                var board = queryChannel.Query(new FindByIdQuery<Board>() {Id = targetFlier.Boards.FirstOrDefault().BoardId}, (Board) null);
                if (board != null && board.Venue() != null)
                {
                    var locInfo = board.Venue().Address.Locality.ToLowerHiphen();
                    if (string.IsNullOrWhiteSpace(locInfo))
                        locInfo = board.Venue().Address.PostCode;
                    if (!string.IsNullOrWhiteSpace(locInfo))
                    {
                        tryTitleBase = locInfo + "-" + tryTitleBase;
                        tryTitle = tryTitleBase;                        
                    }
                }
            }
            else
                return tryTitle;


            var counter = 0;
            while ((flierFind = queryChannel.Query(new FindByFriendlyIdQuery() { FriendlyId = tryTitle }, (Flier)null)) != null
                && flierFind.Id != targetFlier.Id)
            {
                tryTitle = (counter++) + "-" + tryTitleBase;
            }
            return tryTitle;
        }
    }
}
