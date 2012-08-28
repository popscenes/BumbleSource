using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Command;

namespace Website.Infrastructure.Domain
{
    public static class MsgResponseEntityExtension
    {
        public static string GetEntityId(this MsgResponse msg)
        {
            if (msg.Details == null)
                return null;
            var ret = msg.Details.SingleOrDefault(d => d.Property == "EntityId");
            return ret != null ? ret.Message : null;
        }

        public static MsgResponse AddEntityId(this MsgResponse msg, string id)
        {
            return msg.AddMessageProperty("EntityId", id);
        }

        public static MsgResponse AddEntityIdError(this MsgResponse msg, string id)
        {
            return msg.AddMessageProperty("Detail", "Entity Doesn't Exist")
                .AddMessageProperty("EntityId", id);
        }
    }
}
