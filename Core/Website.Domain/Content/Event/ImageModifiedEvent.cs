﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Domain;

namespace Website.Domain.Content.Event
{
    [Serializable]
    public class ImageModifiedEvent : EntityModifiedEvent<Image>
    {
    }
}
