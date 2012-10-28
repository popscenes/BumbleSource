using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using Ninject;
using Website.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Content;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;

namespace PostaFlya.DataRepository.Browser
{

    internal class AzureBrowserRepository : JsonRepository 
        , BrowserRepositoryInterface
        , BrowserQueryServiceInterface
    {

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

    }
}