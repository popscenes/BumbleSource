using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Domain
{
    //CONSIDERING REMOVING
    //Only really meant for simple aggregates, like say you expect a list of 4 or 5 entities under 
    //an aggregate root and prefer the whole aggregate returned as one, however still occasionally
    //need to find the root from an aggregate lookup, if there are large number of entites under an
    //aggregate just store the aggregate members individually and use FindAggregateEntityIds to retrieve 
    //them rather than storing them with the aggregate root as well. 
    //considering removing this attribute all together as next step of simplifying.
    //This way you are forced to just use FindAggregateEntityIds and store the aggregate members as needed....mmm
    public class AggregateMemberEntityAttribute : Attribute
    {
        public static void GetAggregateEnities(HashSet<object> list, object root, bool recursive = true)
        {
            if(list.Contains(root))
            {
                Trace.TraceWarning("Aggregate list already contains object, possible circular reference in aggregate: " + root);
                return;
            }
            
            list.Add(root);

            var props = SerializeUtil.GetPropertiesWithAttribute(root.GetType(), typeof (AggregateMemberEntityAttribute));
            var topLevelMembers = new List<object>();
            foreach (var propertyInfo in props)
            {
                if(propertyInfo.PropertyType.FindInterfaces((m, criteria) => 
                                                            m.Equals(criteria), typeof(IEnumerable)).Any())
                {
                    var coll = propertyInfo.GetValue(root, null) as IEnumerable;
                    if(coll != null)
                        topLevelMembers.AddRange(coll.Cast<object>());
                }
                else if(!propertyInfo.GetIndexParameters().Any())
                {
                    var val = propertyInfo.GetValue(root, null);
                    if(val != null)
                        topLevelMembers.Add(val);
                }
                else
                {
                    throw new ArgumentException("Can't apply AggregateMemberEntityAttribute to index parameters atm: " + propertyInfo.Name);
                }
            }

            if (!recursive)
            {
                foreach (var topLevelMember in topLevelMembers)
                {
                    list.Add(topLevelMember);
                }
                return;
            }


            foreach (var topLevelMember in topLevelMembers)
            {
                GetAggregateEnities(list, topLevelMember);
            }

        }
    }
}