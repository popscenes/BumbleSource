using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Likes.Command
{
    internal interface AddLikeInterface
    {
        LikeableInterface Like(LikeInterface like);
    }

    internal interface AddLikeInterface<EntityType>
        : AddLikeInterface
        where EntityType : LikeableInterface
    {
    }
}
