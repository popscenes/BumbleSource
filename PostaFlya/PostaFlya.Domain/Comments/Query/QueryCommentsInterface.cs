using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Domain.Comments.Query
{
    public interface QueryCommentsInterface
    {
        IQueryable<CommentInterface> GetComments(string id, int take = -1);
    }

    public interface QueryCommentsInterface<EntityType>
        : QueryCommentsInterface where EntityType : CommentableInterface
    {

    }
}
