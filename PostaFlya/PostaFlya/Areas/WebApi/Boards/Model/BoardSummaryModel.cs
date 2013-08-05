using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PostaFlya.Domain.Boards;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Common.Model;
using Res = PostaFlya.Properties.Resources;


namespace PostaFlya.Areas.WebApi.Boards.Model
{
    public class ToBoardSummaryModel : ViewModelMapperInterface<BoardSummaryModel, Board>
    {
        private readonly BlobStorageInterface _blobStorageInterface;

        public ToBoardSummaryModel([ImageStorage]BlobStorageInterface blobStorage)
        {
            _blobStorageInterface = blobStorage;
        }

        public BoardSummaryModel ToViewModel(BoardSummaryModel target, Board source)
        {
            if(target == null)
                target = new BoardSummaryModel();

            target.Id = source.Id;
            target.BoardImageUrl = string.IsNullOrWhiteSpace(source.ImageId) ? "" : _blobStorageInterface.GetBlobUri(source.ImageId).ToString();
            target.Description = source.Description;
            target.FriendlyId = source.FriendlyId;
            target.Name = source.Name;
            target.BoardAdminAddresses = source.AdminEmailAddresses;

            return target;
        }
    }
    public class BoardSummaryModel : IsModelInterface
    {
        [Display(Name = "Id", ResourceType = typeof(Res))]
        public string Id { get; set; }

        [Display(Name = "FriendlyId", ResourceType = typeof(Res))]
        public string FriendlyId { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Res))]
        public string Description { get; set; }

        [Display(Name = "BoardImage", ResourceType = typeof (Res))]
        public string BoardImageUrl { get; set; }

        [Display(Name = "BoardAdminEmailAddresses", ResourceType = typeof (Res))]
        public List<string> BoardAdminAddresses { get; set; }

        [Display(Name = "BoardCreateEditModel_BoardName" ,ResourceType = typeof(Res))]
        public string Name { get; set; }
    }
}