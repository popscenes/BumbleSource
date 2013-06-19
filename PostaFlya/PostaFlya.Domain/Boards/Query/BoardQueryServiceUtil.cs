using System;
using System.Text;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;
using System.Linq;

namespace PostaFlya.Domain.Boards.Query
{
    public static class BoardQueryServiceUtil
    {
        public static string FindFreeFriendlyId(this QueryChannelInterface queryChannel, BoardInterface targetBoard)
        {
            var name = targetBoard.FriendlyId;
            var tryName = name.ToLowerHiphen();
            var tryNameBase = tryName;

            Board boardFind = null;

            if (queryChannel != null && 
                ( boardFind = queryChannel.Query(new FindByFriendlyIdQuery() { FriendlyId = tryName }, (Board)null)) != null
                && boardFind.Id != targetBoard.Id)
            {
                if (targetBoard.BoardTypeEnum != BoardTypeEnum.InterestBoard 
                    && targetBoard.InformationSources.First().Address.HasAddressParts())
                {
                    var locInfo = targetBoard.InformationSources.First().Address.Locality;
                    if (!string.IsNullOrWhiteSpace(locInfo))
                    {
                        tryNameBase = locInfo + "-" + tryNameBase;
                        tryName = tryNameBase;
                    }
                }
            }
            else
                return tryName;


            var counter = 0;
            while ((boardFind = queryChannel.Query(new FindByFriendlyIdQuery() { FriendlyId = tryName }, (Board)null)) != null
                && boardFind.Id != targetBoard.Id)
            {
                tryName = (counter++) + "-" + tryNameBase;
            }
            return tryName;            
        }
    }
}