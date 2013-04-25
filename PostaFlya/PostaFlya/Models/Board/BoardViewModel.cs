using System.Runtime.Serialization;
using Website.Common.Model;
using Website.Common.Model.Query;

namespace PostaFlya.Models.Board
{
    public class ToBoardViewModel : ViewModelMapperInterface<BoardViewModel, PostaFlya.Domain.Boards.Board>
    {
        public BoardViewModel ToViewModel(BoardViewModel target, Domain.Boards.Board source)
        {
            if(target == null)
                target = new BoardViewModel();
            target.Description = source.Description;
            return target;
        }
    }

    [DataContract]
    public class BoardViewModel
    {
        [DataMember]
        public string Description { get; set; }
    }
}