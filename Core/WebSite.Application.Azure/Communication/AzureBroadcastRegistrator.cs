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
    public class AzureBroadcastRegistration : SimpleExtendableEntity
    {
        
    }

    public class AzureBroadcastRegistrator : RepositoryBase<AzureBroadcastRegistration>, BroadcastRegistratorInterface
    {
//        public static TableNameAndPartitionProvider<SimpleExtendableEntity> TableNameBinding
//            = new TableNameAndPartitionProvider<SimpleExtendableEntity>()
//            {
//                {typeof (SimpleExtendableEntity), 0, "broadcastCommunicators", e => "", e => e.Get<string>("Endpoint")}
//            };


        private readonly CommandQueueFactoryInterface _commandQueueFactory;


//        public AzureBroadcastRegistrator([Named("broadcastCommunicators")]AzureTableContext context
//                                         , CommandQueueFactoryInterface commandQueueFactory)
//            : base(context)
//        {
//            _context = context;
//            _commandQueueFactory = commandQueueFactory;
//        }

        private readonly string _tableName;
        public AzureBroadcastRegistrator(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService
            , CommandQueueFactoryInterface commandQueueFactory) 
            : base(tableContext, nameAndPartitionProviderService)
        {
            _commandQueueFactory = commandQueueFactory;
            _tableName = nameAndPartitionProviderService.GetTableName<AzureBroadcastRegistration>(IdPartition);
        }

        public void RegisterEndpoint(string myEndpoint)
        {
            Action<AzureBroadcastRegistration> update 
                = registrationEntry =>
                    {
                        registrationEntry["LastRegisterTime"] = DateTime.UtcNow;
                        registrationEntry["Endpoint"] = myEndpoint;
                        registrationEntry.RowKey = myEndpoint;
                        registrationEntry.PartitionKey = "";
                    };

            var existing = FindById<AzureBroadcastRegistration>(myEndpoint);
            if (existing != null)
                UpdateEntity(myEndpoint, update);
            else
            {
                var newEntry = new AzureBroadcastRegistration();
                update(newEntry);
                Store(newEntry);
            }
            SaveChanges();

            PerformMutationActionOnContext(context =>
                    {
                        foreach (var extendableTableServiceEntity in context.PerformQuery<AzureBroadcastRegistration>(_tableName)
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
            var registrationEntries = TableContext.PerformQuery<AzureBroadcastRegistration>(_tableName);
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

        protected override StorageAggregate GetEntityForUpdate(Type entity, string id)
        {
            var root = FindById(entity, id);
            if (root == null)
                return null;
            var ret = new StorageAggregate(root, NameAndPartitionProviderService);
            ret.LoadAllTableEntriesForUpdate<AzureBroadcastRegistration>(TableContext);
            return ret;
        }

    }
}