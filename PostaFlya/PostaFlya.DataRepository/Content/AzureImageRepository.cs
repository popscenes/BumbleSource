using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using PostaFlya.DataRepository.Internal;
using Website.Azure.Common.TableStorage;
using PostaFlya.Domain.Flier;
using Website.Infrastructure.Query;
using Website.Domain.Content.Command;
using Website.Domain.Content.Query;

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