using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Content;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;

namespace PostaFlya.DataRepository.Browser
{

    internal class AzureBrowserRepository : JsonRepository 
        , BrowserRepositoryInterface
        , BrowserQueryServiceInterface
    {
        public const int HandlePartitionId = 1;

        public AzureBrowserRepository(TableContextInterface tableContext, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
        }

        public BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.IdentityProvider) 
                || string.IsNullOrWhiteSpace(credential.UserIdentifier))
                return null;

            var prov = this.FindById<BrowserIdentityProviderCredential>(credential.GetHash());
            return (prov != null) ?
                FindById<Website.Domain.Browser.Browser>(prov.BrowserId) :
                null;
        }

        public BrowserInterface FindByHandle(string handle)
        {
            return this.FindById<Website.Domain.Browser.Browser>(handle, HandlePartitionId);
        }
    }
}