using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Domain.Flier
{
    [Serializable]    
    public class FlierImage
    {
        public String ImageID { get; set; }

        //public FlierImage(Guid imageID)
        //{

        //}

        public FlierImage(String imageID)
        {
            ImageID = imageID;
        }

    }
}
