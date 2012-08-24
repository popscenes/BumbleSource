using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.DataRepository.Search.Services
{
    interface FlierSearchServiceInterface
        : EntityUpdateNotificationInterface<EntityInterface>
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);  
    }
}
