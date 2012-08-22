using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Content;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Query;

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
                FindById<Domain.Browser.Browser>(prov.BrowserId) :
                null;
        }

        public BrowserInterface FindByHandle(string handle)
        {
            return this.FindById<Domain.Browser.Browser>(handle, HandlePartitionId);
        }
    }
}