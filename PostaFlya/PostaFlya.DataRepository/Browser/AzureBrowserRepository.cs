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

    internal class AzureBrowserRepository : AzureRepositoryBase<BrowserInterface, BrowserStorageDomain> 
        , BrowserRepositoryInterface
        , BrowserQueryServiceInterface
    {
        private readonly AzureTableContext _tableContext;

        public AzureBrowserRepository([Named("browser")]AzureTableContext tableContext)
            : base(tableContext)
        {
            _tableContext = tableContext;
        }

        public BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.IdentityProvider) 
                || string.IsNullOrWhiteSpace(credential.UserIdentifier))
                return null;

            return BrowserStorageDomain.FindByIdentityProvider(credential, _tableContext);
        }

        public BrowserInterface FindByHandle(string handle)
        {
            return BrowserStorageDomain.FindByHandle(handle, _tableContext);
        }

        public BrowserInterface FindById(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? null : BrowserStorageDomain.FindById(id, _tableContext);
        }

        protected override BrowserStorageDomain GetEntityForUpdate(string id)
        {
            return BrowserStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override BrowserStorageDomain GetStorageForEntity(BrowserInterface entity)
        {
            return new BrowserStorageDomain(entity, _tableContext);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }
    }
}