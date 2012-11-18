using System;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.ExternalSource
{
    class FlierImportService : FlierImportServiceInterface
    {
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;

        public FlierImportService(QueryServiceForBrowserAggregateInterface queryService, 
            UrlContentRetrieverFactoryInterface contentRetrieverFactory, 
            CommandBusInterface commandBus)
        {
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _commandBus = commandBus;
        }

        public FlierImporterInterface GetImporter(string providerName)
        {
            if (providerName == IdentityProviders.FACEBOOK)
            {
                return new FacebookFlierImporter(_queryService, _contentRetrieverFactory, _commandBus);
            }
            throw new ArgumentException();
        }
    }
}
