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
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;

        public FlierImportService(GenericQueryServiceInterface queryService, 
            UrlContentRetrieverFactoryInterface contentRetrieverFactory, 
            CommandBusInterface commandBus, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _commandBus = commandBus;
            _queryChannel = queryChannel;
        }

        public FlierImporterInterface GetImporter(string providerName)
        {
            if (providerName == IdentityProviders.FACEBOOK)
            {
                return new FacebookFlierImporter(_queryChannel, _queryService, _contentRetrieverFactory, _commandBus);
            }
            throw new ArgumentException();
        }
    }
}
