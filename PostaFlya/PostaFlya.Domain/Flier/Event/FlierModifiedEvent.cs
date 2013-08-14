using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Event
{
    [Serializable]
    public class FlierModifiedEvent : EntityModifiedEvent<Flier>
    {
    }
}
