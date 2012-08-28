using System.Linq;

namespace Website.Infrastructure.Command
{
    public static class MsgResponseCommandExtension
    {
        public static string GetCommandId(this MsgResponse msg)
        {
            if (msg.Details == null)
                return null;
            var ret = msg.Details.SingleOrDefault(d => d.Property == "CommandId");
            return ret != null ? ret.Message : null;
        }

        public static MsgResponse AddCommandId(this MsgResponse msg, CommandInterface cmd)
        {
            return msg.AddMessageProperty("CommandId", cmd.CommandId);
        }
    }
}
