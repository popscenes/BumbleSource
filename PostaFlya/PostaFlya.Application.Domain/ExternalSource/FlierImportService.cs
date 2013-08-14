using System;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.ExternalSource
{
    class FlierImportService : FlierImportServiceInterface
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly MessageBusInterface _messageBus;

        public FlierImportService(GenericQueryServiceInterface queryService, 
            UrlContentRetrieverFactoryInterface contentRetrieverFactory, 
            MessageBusInterface messageBus, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _messageBus = messageBus;
            _queryChannel = queryChannel;
        }

        public FlierImporterInterface GetImporter(string providerName)
        {
            if (providerName == IdentityProviders.FACEBOOK)
            {
                return new FacebookFlierImporter(_queryChannel, _queryService, _contentRetrieverFactory, _messageBus);
            }
            throw new ArgumentException();
        }
    }
}
