using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Util;

namespace PostaFlya.DataRepository.Internal
{
    internal static class CustomEdmSerializers
    {
        public static readonly Dictionary<string, EdmSerializer> CustomSerializers
            = new Dictionary<string, EdmSerializer>()
            {
                {"Location", GetLocationSerializer()},
                {"Tags", GetTagsSerializer()},
                {"Locations", GetLocationsSerializer()}
            };

        internal static EdmSerializer GetLocationSerializer()
        {
            return new EdmSerializer()
            {
                InternalTyp = typeof (Location),
                EdmTyp = Edm.String,
                Deserializer = pair => new Location(pair.Value as string),
                Serializer =
                    o =>
                    new KeyValuePair<EdmProp, object>(new EdmProp() {EdmTyp = Edm.String},
                                                        (o as Location).ToString())
            };
        }

        internal static EdmSerializer GetTagsSerializer()
        {
            return new EdmSerializer()
            {
                InternalTyp = typeof(Tags),
                EdmTyp = Edm.String,
                Deserializer = pair => new Tags(pair.Value as string),
                Serializer =
                    o =>
                    new KeyValuePair<EdmProp, object>(new EdmProp() { EdmTyp = Edm.String },
                                                        (o as Tags).ToString())
            };
        }

        internal static EdmSerializer GetLocationsSerializer()
        {
            return new EdmSerializer()
            {
                InternalTyp = typeof(Locations),
                EdmTyp = Edm.Binary,
                Deserializer = pair => SerializeUtil.FromByteArray(pair.Value as byte[]),
                Serializer =
                    o =>
                    new KeyValuePair<EdmProp, object>(new EdmProp() { EdmTyp = Edm.Binary },
                                                        SerializeUtil.ToByteArray(o))
            };
        }

    }
}
