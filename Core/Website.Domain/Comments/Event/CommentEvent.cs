using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Domain;

namespace Website.Domain.Comments.Event
{
    public class CommentEvent : EntityModifiedDomainEvent<Comment>
    {
    }
}
