using System;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Content.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content;

namespace PostaFlya.Application.Domain.ExternalSource
{
    class FlierImportService : FlierImportServiceInterface
    {
        private FlierQueryServiceInterface _queryService;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;
        private readonly ImageQueryServiceInterface _imageQueryServiceInterface;

        public FlierImportService(FlierQueryServiceInterface queryService, 
            UrlContentRetrieverFactoryInterface contentRetrieverFactory, 
            CommandBusInterface commandBus,
            ImageQueryServiceInterface imageQueryServiceInterface)
        {
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _commandBus = commandBus;
            _imageQueryServiceInterface = imageQueryServiceInterface;
        }

        public FlierImporterInterface GetImporter(string providerName)
        {
            if (providerName == IdentityProviders.FACEBOOK)
            {
                return new FacebookFlierImporter(_queryService, _contentRetrieverFactory, _commandBus, _imageQueryServiceInterface);
            }
            throw new ArgumentException();
        }
    }
}
