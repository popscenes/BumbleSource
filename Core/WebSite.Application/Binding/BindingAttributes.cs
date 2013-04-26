﻿using System;

//These are used to identify different storage containers for injection
namespace Website.Application.Binding
{
    //blob storage contexts
    public class ImageStorageAttribute : Attribute { }
    public class ApplicationStorageAttribute : Attribute { }

    //public class FlierImageStorageAttribute : Attribute { }



    //for broadcast communication
    public class BroadcastCommunicatorAttribute : Attribute { }

}
