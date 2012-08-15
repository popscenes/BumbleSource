using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Behaviour.Command
{
    public class FlierBehaviourDefaultRespository : GenericRepositoryInterface<FlierBehaviourInterface>
        , GenericQueryServiceInterface<FlierBehaviourInterface>
    {

        public void UpdateEntity(string id, Action<FlierBehaviourInterface> updateAction)
        {
        }

        public void Store(FlierBehaviourInterface entity)
        {
        }

        public FlierBehaviourInterface FindById(string id)
        {
            return null;
        }

        public void Store(object entity)
        {
            Store(entity as FlierBehaviourInterface);
        }

        public bool SaveChanges()
        {
            return true;
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }


    }
}
