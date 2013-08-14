using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards.Event
{
    [Serializable]
    public class BoardModifiedEvent : EntityModifiedEvent<Board>
    {
    }
}
