using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.DataRepository.Search.Services
{
    interface FlierSearchServiceInterface
        : EntityUpdateNotificationInterface<EntityInterface>
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);  
    }
}
