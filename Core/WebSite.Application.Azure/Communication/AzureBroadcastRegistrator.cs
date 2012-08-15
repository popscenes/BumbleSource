using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using WebSite.Application.Command;
using WebSite.Application.Communication;
using WebSite.Azure.Common.TableStorage;

namespace WebSite.Application.Azure.Communication
{

    public class AzureBroadcastRegistrator : AzureRepositoryBase<SimpleExtendableEntity, SimpleExtendableEntity>, BroadcastRegistratorInterface
    {
        public static TableNameAndPartitionProvider<SimpleExtendableEntity> TableNameBinding
            = new TableNameAndPartitionProvider<SimpleExtendableEntity>()
            {
                {typeof (SimpleExtendableEntity), 0, "broadcastCommunicators", e => "", e => e.Get<string>("Endpoint")}
            };

        private readonly AzureTableContext _context;
        private readonly CommandQueueFactoryInterface _commandQueueFactory;


        public AzureBroadcastRegistrator([Named("broadcastCommunicators")]AzureTableContext context
                                         , CommandQueueFactoryInterface commandQueueFactory)
            : base(context)
        {
            _context = context;
            _commandQueueFactory = commandQueueFactory;
        }

        public void RegisterEndpoint(string myEndpoint)
        {
            UpdateEntity(myEndpoint
                , registrationEntry =>
                    {
                        registrationEntry["LastRegisterTime"] = DateTime.UtcNow;
                        registrationEntry["Endpoint"] = myEndpoint;
                        registrationEntry.RowKey = myEndpoint;
                        registrationEntry.PartitionKey = "";
                    });
            SaveChanges();

            PerformMutationActionOnContext(context =>
                    {
                        foreach (var extendableTableServiceEntity in context.PerformQuery<SimpleExtendableEntity>()
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
            var registrationEntries = _context.PerformQuery<SimpleExtendableEntity>();
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

        protected override SimpleExtendableEntity GetEntityForUpdate(string myEndpoint)
        {
            var registrationEntries = _context.PerformQuery<SimpleExtendableEntity>();
            return registrationEntries.SingleOrDefault(e => e.Get<string>("Endpoint") == myEndpoint) ??
                                    new SimpleExtendableEntity();
        }

        protected override SimpleExtendableEntity GetStorageForEntity(SimpleExtendableEntity entity)
        {
            return entity;
        }
    }
}