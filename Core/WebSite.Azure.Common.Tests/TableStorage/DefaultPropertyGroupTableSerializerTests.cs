using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MbUnit.Framework;
using WebSite.Azure.Common.TableStorage;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Util;
using WebSite.Test.Common;

namespace WebSite.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class DefaultPropertyGroupTableSerializerTests
    {

        [Test]
        public ExtendableTableEntry DefaultPropertyGroupTableSerializerTestEdmTypesSerialize()
        {
            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection
                                    {   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };

            FillPropertyGroupWithEdmTypes(propertyGroup[""]);
            FillPropertyGroupWithEdmTypes(propertyGroup["another"]);

            var tableEntity = new ExtendableTableEntry();
            testOb.MergeProperties(tableEntity, propertyGroup);
            Assert.Count(26, tableEntity.GetAllProperties());
            Assert.Count(13, tableEntity.GetAllProperties().Where(kv => kv.Key.Name.StartsWith("_")));
            Assert.Count(13, tableEntity.GetAllProperties().Where(kv => kv.Key.Name.StartsWith("another_")));
            AssertEdmTypes(tableEntity, "");
            AssertEdmTypes(tableEntity, "another");
            return tableEntity;
        }

        [Test]
        public void DefaultPropertyGroupTableSerializerTestEdmTypesDeserialize()
        {
            var tableEnt = DefaultPropertyGroupTableSerializerTestEdmTypesSerialize();

            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection();
            testOb.LoadProperties(propertyGroup, tableEnt);

            var propertyGroupOrig = new PropertyGroupCollection(){   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };
            FillPropertyGroupWithEdmTypes(propertyGroupOrig[""]);
            FillPropertyGroupWithEdmTypes(propertyGroupOrig["another"]);

            AssertPropertyGroupsAreEqual(propertyGroupOrig, propertyGroup);
        }


        [Test]
        public ExtendableTableEntry DefaultPropertyGroupTableSerializerTestNonEdmTypesSerialize()
        {
            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection
                                    {   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };

            FillPropertyGroupWithNonEdmTypes(propertyGroup[""]);
            FillPropertyGroupWithNonEdmTypes(propertyGroup["another"]);

            var tableEntity = new ExtendableTableEntry();
            testOb.MergeProperties(tableEntity, propertyGroup);
            Assert.Count(4, tableEntity.GetAllProperties());
            Assert.Count(2, tableEntity.GetAllProperties().Where(kv => kv.Key.Name.StartsWith("_")));
            Assert.Count(2, tableEntity.GetAllProperties().Where(kv => kv.Key.Name.StartsWith("another_")));
            AssertNonEdmTypes(tableEntity, "");
            AssertNonEdmTypes(tableEntity, "another");
            return tableEntity;
        }

        [Test]
        public void DefaultPropertyGroupTableSerializerTestNonEdmTypesDeserialize()
        {
            var tableEnt = DefaultPropertyGroupTableSerializerTestNonEdmTypesSerialize();

            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection();
            testOb.LoadProperties(propertyGroup, tableEnt);

            var propertyGroupOrig = new PropertyGroupCollection(){   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };
            FillPropertyGroupWithNonEdmTypes(propertyGroupOrig[""]);
            FillPropertyGroupWithNonEdmTypes(propertyGroupOrig["another"]);

            AssertPropertyGroupsAreEqual(propertyGroupOrig, propertyGroup);
        }

        [Test]
        public ExtendableTableEntry DefaultPropertyGroupTableSerializerTestAllTypesSerialize()
        {
            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection
                                    {   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };

            FillPropertyGroupWithNonEdmTypes(propertyGroup[""]);
            FillPropertyGroupWithNonEdmTypes(propertyGroup["another"]);
            FillPropertyGroupWithEdmTypes(propertyGroup[""]);
            FillPropertyGroupWithEdmTypes(propertyGroup["another"]);
            
            var tableEntity = new ExtendableTableEntry();
            testOb.MergeProperties(tableEntity, propertyGroup);

            AssertNonEdmTypes(tableEntity, "");
            AssertNonEdmTypes(tableEntity, "another");
            AssertEdmTypes(tableEntity, "");
            AssertEdmTypes(tableEntity, "another");
            return tableEntity;
        }

        [Test]
        public void DefaultPropertyGroupTableSerializerTestAllTypesDeserialize()
        {
            var tableEnt = DefaultPropertyGroupTableSerializerTestAllTypesSerialize();

            var testOb = new DefaultPropertyGroupTableSerializer(CustomEdmSerializers.CustomSerializers);
            var propertyGroup = new PropertyGroupCollection();
            testOb.LoadProperties(propertyGroup, tableEnt);

            var propertyGroupOrig = new PropertyGroupCollection(){   
                                        new PropertyGroup() {Name = ""}, 
                                        new PropertyGroup() {Name = "another"}
                                    };
            FillPropertyGroupWithNonEdmTypes(propertyGroupOrig[""]);
            FillPropertyGroupWithNonEdmTypes(propertyGroupOrig["another"]);
            FillPropertyGroupWithEdmTypes(propertyGroupOrig[""]);
            FillPropertyGroupWithEdmTypes(propertyGroupOrig["another"]);
            

            AssertPropertyGroupsAreEqual(propertyGroupOrig, propertyGroup);
        }
        
        public static void AssertPropertyGroupsAreEqual(PropertyGroupCollection one, PropertyGroupCollection two)
        {
            foreach (var pgroup in one)
            {
                AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(pgroup.Properties, two[pgroup.Name].Properties);
            }
        }

        private void FillPropertyGroupWithEdmTypes(PropertyGroup entity)
        {
            //un supported types converting to supported types in serialization
            //"Edm.String"
            entity["stringval"] = "1234";
            //"Edm.Byte"
            entity["byteval"] = (byte)1;
            //"Edm.SByte"
            entity["sbyteval"] = (sbyte)-1;
            //"Edm.Int16"
            entity["int16val"] = (short)4500;
            //"Edm.Single"
            entity["singleval"] = (float)4500.45;
            //"Edm.Decimal" 
            entity["decimalval"] = (decimal)4500.45;

            //"Edm.Int32"
            entity["int32val"] = (int)4500;
            //"Edm.Int64"
            entity["int64val"] = (long)4500;
            //"Edm.Double"
            entity["doubleval"] = (double) 4500.45;   
            //"Edm.Boolean"
            entity["boolval"] = true;
            //"Edm.DateTime"
            entity["datetimeval"] = new DateTime(2001, 1, 1);
            //"Edm.Binary"
            entity["binval"] = new byte[] { 1, 2, 3 };
            //"Edm.Guid"
            entity["guidval"] = new Guid("871531C8-52DD-4A97-8508-37A84E63DD35");
        }

        private void AssertEdmTypes(ExtendableTableEntry entry, string groupname)
        {

            //un supported types converting to supported types in serialization
            //"Edm.String"
            Assert.AreEqual("1234", entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "stringval")]);
            //"Edm.Byte"
            Assert.AreEqual((byte)1, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "byteval")]);
            //"Edm.SByte"
            Assert.AreEqual((sbyte)-1, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "sbyteval")]);
            //"Edm.Int16"
            Assert.AreEqual((short)4500, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "int16val")]);
            //"Edm.Single"
            Assert.AreEqual((float)4500.45, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "singleval")]);
            //"Edm.Decimal" 
            Assert.AreEqual((decimal)4500.45, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "decimalval")]);

            //"Edm.Int32"
            Assert.AreEqual((int)4500, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "int32val")]);
            //"Edm.Int64"
            Assert.AreEqual((long)4500, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "int64val")]);
            //"Edm.Double"
            Assert.AreEqual((double)4500.45, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "doubleval")]);
            //"Edm.Boolean"
            Assert.AreEqual(true, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "boolval")]);
            //"Edm.DateTime"
            Assert.AreEqual(new DateTime(2001, 1, 1), entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "datetimeval")]);
            //"Edm.Binary"
            Assert.AreEqual(new byte[] { 1, 2, 3 }, entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "binval")]);
            //"Edm.Guid"
            Assert.AreEqual(new Guid("871531C8-52DD-4A97-8508-37A84E63DD35"), entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "guidval")]);
        }

        private void FillPropertyGroupWithNonEdmTypes(PropertyGroup entity)
        {
            entity["TestCustomEdmClassVal"] = new TestCustomEdmClass(22.234, 22.234);
            entity["TestCustomEdmClassCollectionVal"] = new TestCustomEdmClassCollection() { new TestCustomEdmClass(22.234, 22.234), new TestCustomEdmClass(22.2345, 22.2345) };
        }

        private void AssertNonEdmTypes(ExtendableTableEntry entry, string groupname)
        {
            Assert.AreEqual<string>(new TestCustomEdmClass(22.234, 22.234).ToString()
                , (string)entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "TestCustomEdmClassVal", "TestCustomEdmClass")]);

            var locs = new TestCustomEdmClassCollection() {new TestCustomEdmClass(22.234, 22.234), new TestCustomEdmClass(22.2345, 22.2345)};
            var serial = SerializeUtil.ToByteArray(locs);
            Assert.AreEqual<byte[]>(serial
                , (byte[])entry[DefaultPropertyGroupTableSerializer.GetPropertyNameString(groupname, "TestCustomEdmClassCollectionVal", "TestCustomEdmClassCollection")]);

        }


    }

    internal static class CustomEdmSerializers
    {
        public static readonly Dictionary<string, EdmSerializer> CustomSerializers
            = new Dictionary<string, EdmSerializer>()
            {
                {"TestCustomEdmClass", GetLocationSerializer()},
                {"TestCustomEdmClassCollection", GetLocationsSerializer()}
            };

        internal static EdmSerializer GetLocationSerializer()
        {
            return new EdmSerializer()
            {
                InternalTyp = typeof(TestCustomEdmClass),
                EdmTyp = Edm.String,
                Deserializer = pair => new TestCustomEdmClass(pair.Value as string),
                Serializer =
                    o =>
                    new KeyValuePair<EdmProp, object>(new EdmProp() { EdmTyp = Edm.String },
                                                        (o as TestCustomEdmClass).ToString())
            };
        }


        internal static EdmSerializer GetLocationsSerializer()
        {
            return new EdmSerializer()
            {
                InternalTyp = typeof(TestCustomEdmClassCollection),
                EdmTyp = Edm.Binary,
                Deserializer = pair => SerializeUtil.FromByteArray(pair.Value as byte[]),
                Serializer =
                    o =>
                    new KeyValuePair<EdmProp, object>(new EdmProp() { EdmTyp = Edm.Binary },
                                                        SerializeUtil.ToByteArray(o))
            };
        }

    }

    [Serializable]
    public class TestCustomEdmClass 
    {
        private const double Invalid = -200;

        public TestCustomEdmClass()
        {
            Longitude = Invalid;
            Latitude = Invalid;
            Description = "";
        }

        public TestCustomEdmClass(TestCustomEdmClass source)
        {
            Longitude = source.Longitude;
            Latitude = source.Latitude;
            Description = source.Description;
        }

        public TestCustomEdmClass(double longitude, double latitude)
            : this()
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public TestCustomEdmClass(string coords)
            : this()
        {
            var arr = coords.Split(':');
            if (arr.Length != 2)
                return;

            Latitude = Double.Parse(arr[0]);
            Longitude = Double.Parse(arr[1]);

        }

        public override string ToString()
        {
            return Latitude + ":" + Longitude;
        }

        public string ToString(int fieldWidth)
        {
            var format = "F" + fieldWidth;
            return Latitude.ToString(format) + ":" + Longitude.ToString(format);
        }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Description { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var equals = false;
            var location = obj as TestCustomEdmClass;
            if (location == null)
                return false;

            if (Math.Abs(this.Latitude - location.Latitude) < 0.00001 && Math.Abs(this.Longitude - location.Longitude) < 0.00001)
            {
                equals = true;
            }

            return equals;

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    [Serializable]
    public class TestCustomEdmClassCollection : HashSet<TestCustomEdmClass>
    {
        public TestCustomEdmClassCollection()
        {
        }

        public TestCustomEdmClassCollection(IEnumerable<TestCustomEdmClass> collection)
            : base(collection)
        {
        }

        public TestCustomEdmClassCollection(string coordinateStrings)
            : base(coordinateStrings.Split().AsQueryable().Select(s => new TestCustomEdmClass(s)))
        {
        }

        //because HashSet serialization constructor is private 
        public TestCustomEdmClassCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var location in this)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(' ');
                stringBuilder.Append(location);

            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;

            var other = obj as TestCustomEdmClassCollection;
            return other != null && SetEquals(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, element) => current ^ element.GetHashCode());
        }
    }
}
