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
            var tryName = name.ToLowerUnderScore();
            var tryNameBase = tryName;

            Board boardFind = null;

            if (queryService != null && (boardFind = queryService.FindByFriendlyId<Board>(tryName)) != null
                && boardFind.Id != targetBoard.Id)
            {
                if (targetBoard.Location.HasAddressParts())
                {
                    var locInfo = targetBoard.Location.Locality;
                    if (string.IsNullOrWhiteSpace(locInfo))
                        locInfo = targetBoard.Location.PostCode;
                    if (!string.IsNullOrWhiteSpace(locInfo))
                    {
                        tryNameBase = locInfo + "_" + tryNameBase;
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
                tryName = (counter++) + "_" + tryNameBase;
            }
            return tryName;            
        }
    }
}