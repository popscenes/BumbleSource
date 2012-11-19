using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public static class BoardInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BoardInterface target, BoardInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            target.AllowOthersToPostFliers = source.AllowOthersToPostFliers;
            target.RequireApprovalOfPostedFliers = source.RequireApprovalOfPostedFliers;
            target.Location = source.Location != null ? new Location(source.Location) : null;
            target.PercentageOfPublicFliersToShow = source.PercentageOfPublicFliersToShow;
            target.Status = source.Status;
        }
    }

    public interface BoardInterface : EntityInterface, BrowserIdInterface
    {
        bool AllowOthersToPostFliers { get; set; }
        bool RequireApprovalOfPostedFliers { get; set; }
        Location Location { get; set; }
        string Description { get; set; }
        BoardStatus Status { get; set; }
        int PercentageOfPublicFliersToShow { get; set; }
    }
}