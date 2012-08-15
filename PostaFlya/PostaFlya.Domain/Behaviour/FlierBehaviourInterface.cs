using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Behaviour
{
//    public static class FlierBehaviourInterfaceExtension
//    {
//        static Type GetTopLevelFlierBehaviourInterface
//    }
    public interface FlierBehaviourInterface : EntityInterface
    {
        FlierInterface Flier { get; set; }
        PropertyGroup FlierProperties { get; set; }
    }
}
