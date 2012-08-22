using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using PostaFlya.DataRepository.Internal;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Query;

namespace PostaFlya.DataRepository.Content
{
    internal class AzureImageRepository : JsonRepositoryWithBrowser
        , ImageRepositoryInterface
        , ImageQueryServiceInterface
    {
        public const int BrowsePartitionId = 0;

        #region Implementation of ImageQueryServiceInterface

        public AzureImageRepository(TableContextInterface tableContext, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
        }

        #endregion
    }
}