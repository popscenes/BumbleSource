using System;
using PostaFlya.Domain.Flier.Query;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Command;
using Website.Application.Domain.Content;

namespace PostaFlya.Application.Domain.ExternalSource
{
    class FlierImportService : FlierImportServiceInterface
    {
        private FlierQueryServiceInterface _queryService;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;

        public FlierImportService(FlierQueryServiceInterface queryService, UrlContentRetrieverFactoryInterface contentRetrieverFactory, CommandBusInterface commandBus)
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
