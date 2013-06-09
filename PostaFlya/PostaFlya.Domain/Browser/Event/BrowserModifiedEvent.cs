using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser.Event
{
    [Serializable]
    public class BrowserModifiedEvent
        : EntityModifiedDomainEvent<Browser> 
    {
    }
}
