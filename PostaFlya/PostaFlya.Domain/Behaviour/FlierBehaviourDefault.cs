using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Behaviour
{
    public class FlierBehaviourDefault : FlierBehaviourBase<FlierBehaviourInterface>
    {
        public override FlierInterface Flier { get; set; }
        public override PropertyGroup FlierProperties { get; set; }
    }
}
