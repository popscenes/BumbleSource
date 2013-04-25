using System;
using System.Text;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Domain.Boards.Query
{
    public static class BoardQueryServiceUtil
    {
        public static string FindFreeFriendlyId(this GenericQueryServiceInterface queryService, BoardInterface targetBoard)
        {
            var name = targetBoard.FriendlyId;
            var tryName = name.ToLowerHiphen();
            var tryNameBase = tryName;

            Board boardFind = null;

            if (queryService != null && (boardFind = queryService.FindByFriendlyId<Board>(tryName)) != null
                && boardFind.Id != targetBoard.Id)
            {
                var venueBoard = targetBoard as VenueBoardInterface;
                if (venueBoard != null && venueBoard.Location.HasAddressParts())
                {
                    var locInfo = venueBoard.Location.Locality;
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
            while ((boardFind = queryService.FindByFriendlyId<Board>(tryName)) != null
                && boardFind.Id != targetBoard.Id)
            {
                tryName = (counter++) + "-" + tryNameBase;
            }
            return tryName;            
        }
    }
}