using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetOpenAuth.OAuth2;

namespace WebSite.Application.Authentication
{
    //-----------------------------------------------------------------------
    // <copyright file="FacebookGraph.cs" company="Andrew Arnott">
    //     Copyright (c) Andrew Arnott. All rights reserved.
    // </copyright>
    //-----------------------------------------------------------------------

    /*[DataContract]
    public class FacebookGraph
    {
        private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(FacebookGraph));

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "birthday")]
        public string Birthday { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        public static FacebookGraph Deserialize(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json");
            }

            return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public static FacebookGraph Deserialize(Stream jsonStream)
        {
            if (jsonStream == null)
            {
                throw new ArgumentNullException("jsonStream");
            }

            return (FacebookGraph)jsonSerializer.ReadObject(jsonStream);
        }
    }*/
}
