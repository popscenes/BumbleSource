using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Query;

namespace PostaFlya.DataRepository.Content
{
    internal class AzureImageRepository : AzureRepositoryBase<ImageInterface, ImageStorageDomain>
        , ImageRepositoryInterface
        , ImageQueryServiceInterface
    {
        private readonly AzureTableContext _tableContext;

        public AzureImageRepository([Named("image")]AzureTableContext tableContext)
            : base(tableContext)
        {
            _tableContext = tableContext;
        }

        #region Implementation of ImageQueryServiceInterface

        public IQueryable<ImageInterface> GetByBrowserId(string browserId)
        {
            return ImageStorageDomain.GetByBrowserId(browserId, _tableContext);
        }

        public ImageInterface FindById(string id)
        {
            return ImageStorageDomain.FindById(id, _tableContext);
        }


        #endregion


        protected override ImageStorageDomain GetEntityForUpdate(string id)
        {
            return ImageStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override ImageStorageDomain GetStorageForEntity(ImageInterface entity)
        {
            return new ImageStorageDomain(entity, _tableContext);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }
    }
}