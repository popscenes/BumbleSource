using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Website.Application.ApplicationCommunication;
using Website.Infrastructure.Domain;
using Website.Application.Command;
using Website.Azure.Common.TableStorage;

namespace Website.Application.Azure.Communication
{
    public class AzureBroadcastRegistrationEntry : SimpleExtendableEntity, AggregateRootInterface
    {
        
    }

    public class AzureApplicationBroadcastCommunicatorRegistration : RepositoryBase<AzureBroadcastRegistrationEntry>, ApplicationBroadcastCommunicatorRegistrationInterface
    {
        private readonly CommandQueueFactoryInterface _commandQueueFactory;

        private readonly string _tableName;
        public AzureApplicationBroadcastCommunicatorRegistration(TableContextInterface tableContext
            , TableNameAndIndexProviderServiceInterface nameAndIndexProviderService
            , CommandQueueFactoryInterface commandQueueFactory) 
            : base(tableContext, nameAndIndexProviderService)
        {
            _commandQueueFactory = commandQueueFactory;
            _tableName = nameAndIndexProviderService.GetTableName<AzureBroadcastRegistrationEntry>();
        }

        public void RegisterEndpoint(string myEndpoint)
        {
            Action<AzureBroadcastRegistrationEntry> update 
                = registrationEntry =>
                    {
                        registrationEntry["LastRegisterTime"] = DateTime.UtcNow;
                        registrationEntry["Endpoint"] = myEndpoint;
                        registrationEntry.RowKey = myEndpoint;
                        registrationEntry.PartitionKey = "";
                    };

            var existing = FindById<AzureBroadcastRegistrationEntry>(myEndpoint);
            if (existing != null)
                UpdateEntity(myEndpoint, update);
            else
            {
                var newEntry = new AzureBroadcastRegistrationEntry();
                update(newEntry);
                Store(newEntry);
            }
            SaveChanges();

            PerformMutationActionOnContext(context =>
                    {
                        foreach (var extendableTableServiceEntity in context.PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                            .Where(e => IsAboveRegThreshHold(e) && (string)e["Endpoint"] != myEndpoint))
                        {
                            context.Delete(extendableTableServiceEntity);
                            _commandQueueFactory.Delete(extendableTableServiceEntity.Get<string>("Endpoint"));
                        }
                    });
            SaveChanges();
        }

        public IList<string> GetCurrentEndpoints()
        {
            var registrationEntries = TableContext.PerformQuery<AzureBroadcastRegistrationEntry>(_tableName);
            return registrationEntries
                .Where(e => (DateTime.UtcNow - e.Get<DateTime>("LastRegisterTime")).Minutes < 10 )
                .Select(e => e.Get<string>("Endpoint"))
                .ToList();
        }

        private static bool IsBelowReRegThreshHold(ExtendableTableEntry registrationEntry)
        {
            return (DateTime.UtcNow - registrationEntry.Get<DateTime>("LastRegisterTime")).Minutes < 5;
        }

        private static bool IsAboveRegThreshHold(ExtendableTableEntry registrationEntry)
        {
            return (DateTime.UtcNow - registrationEntry.Get<DateTime>("LastRegisterTime")).Minutes > 10;
        }


    }
}