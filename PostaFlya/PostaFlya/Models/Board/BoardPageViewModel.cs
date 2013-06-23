using System.Linq;
using System.Runtime.Serialization;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Board
{

    public class ToBoardViewModel 
        : ViewModelMapperInterface<BoardPageViewModel, PostaFlya.Domain.Boards.Board>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToBoardViewModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BoardPageViewModel ToViewModel(BoardPageViewModel target, Domain.Boards.Board source)
        {
            if(target == null)
                target = new BoardPageViewModel();

            return _queryChannel.ToViewModel<BoardModel>(source, target) as BoardPageViewModel;

        }
    }

    [DataContract]
    public class BoardPageViewModel : BoardModel, PageModelInterface
    {
        public BoardPageViewModel()
        {
            PageId = WebConstants.BoardPage;
        }

        public string PageId { get; set; }
        public string ActiveNav { get; set; }
    }
}