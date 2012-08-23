using System;
using System.Collections.Generic;
using System.IO;

namespace TravelSite.Applications.Intergrations.Expedia
{
    public class ExpediaFileImporter
    {

        public IEnumerable<T> ImportExpediaFile<T>(Func<String[], T> copyFunc, String filePath) where T : new()
        {
            using (var reader = File.OpenText(filePath))
            {
                //header
                var line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    yield return ParseTextLine(copyFunc, line);
                }
            }
        }

        private T ParseTextLine<T>(Func<String[], T> copyFunc, String text) where T : new()
        {
            var propertyValues = text.Split(new char[] { '|' }, StringSplitOptions.None);
            var expediaEntity = copyFunc(propertyValues);
            return expediaEntity;
        }

        public static ExpediaAttributePropertyLink CopyAttributePropertyLink(string[] values)
        {
            Int32 tempInt = 0;
            var attributePropertyLink = new ExpediaAttributePropertyLink();

            attributePropertyLink.AppendTxt = values[(Int32) ExpediaAttributePropertyLinkFields.AppendTxt];

            Int32.TryParse(values[(Int32)ExpediaAttributePropertyLinkFields.AttributeId], out tempInt);
            attributePropertyLink.AttributeId = tempInt;

            Int32.TryParse(values[(Int32)ExpediaAttributePropertyLinkFields.EANHotelId], out tempInt);
            attributePropertyLink.EANHotelId = tempInt;

            attributePropertyLink.LanguageCode = values[(Int32)ExpediaAttributePropertyLinkFields.LanguageCode];
            
            return attributePropertyLink;
        }

        public static ExpediaAttributes CopyAttributes(string[] attributeValues)
        {
            var attributes = new ExpediaAttributes();
            Int32 tempInt = 0;
            Int32.TryParse(attributeValues[(Int32)ExpediaAttributeFields.AttributeId], out tempInt);
            attributes.AttributeId = tempInt;

            attributes.AttributeDesc = attributeValues[(Int32)ExpediaAttributeFields.AttributeDesc];
            attributes.LanguageCode = attributeValues[(Int32)ExpediaAttributeFields.LanguageCode];
            attributes.SubType = attributeValues[(Int32)ExpediaAttributeFields.SubType];
            attributes.Type = attributeValues[(Int32)ExpediaAttributeFields.Type];

            return attributes;

        }

        public static ExpediaRoomTypes CopyRoomType(String[] propertyValues)
        {
            
            var roomType = new ExpediaRoomTypes();
            Int32 tempInt = 0;
            Int32.TryParse(propertyValues[(Int32)ExpediaPropertyFields.EANHotelId], out tempInt);
            roomType.EANHotelId = tempInt;

            roomType.LanguageCode = propertyValues[(Int32)ExpediaRoomTypeFields.LanguageCode];
            roomType.RoomTypeDescription = propertyValues[(Int32)ExpediaRoomTypeFields.RoomTypeDescription];
            
            Int32.TryParse(propertyValues[(Int32)ExpediaRoomTypeFields.RoomTypeId], out tempInt);
            roomType.RoomTypeId = tempInt;

            roomType.RoomTypeImage = propertyValues[(Int32)ExpediaRoomTypeFields.RoomTypeImage];
            roomType.RoomTypeName = propertyValues[(Int32)ExpediaRoomTypeFields.RoomTypeName];

            return roomType;
        }


        public static ExpediaProperty CopyProperty(String[] propertyValues)
        {
            var expediaProperty = new ExpediaProperty();
            Int32 tempInt = 0;
            double tempDbl = 0;

            expediaProperty.Address1 = propertyValues[(Int32)ExpediaPropertyFields.Address1];
            expediaProperty.Address2 = propertyValues[(Int32)ExpediaPropertyFields.Address2];
            expediaProperty.AirportCode = propertyValues[(Int32)ExpediaPropertyFields.AirportCode];
            expediaProperty.ChainCodeId = propertyValues[(Int32)ExpediaPropertyFields.ChainCodeId];
            expediaProperty.CheckInTime = propertyValues[(Int32)ExpediaPropertyFields.CheckInTime];
            expediaProperty.CheckOutTime = propertyValues[(Int32)ExpediaPropertyFields.CheckOutTime];
            expediaProperty.City = propertyValues[(Int32)ExpediaPropertyFields.City];
            
            
            Int32.TryParse(propertyValues[(Int32)ExpediaPropertyFields.Confidence], out tempInt);
            expediaProperty.Confidence = tempInt;

            expediaProperty.Country = propertyValues[(Int32)ExpediaPropertyFields.Country];

            Int32.TryParse(propertyValues[(Int32)ExpediaPropertyFields.EANHotelId], out tempInt);
            expediaProperty.EANHotelId = tempInt;

            Double.TryParse(propertyValues[(Int32)ExpediaPropertyFields.HighRate], out tempDbl);
            expediaProperty.HighRate = tempDbl;

            Double.TryParse(propertyValues[(Int32)ExpediaPropertyFields.Latitude], out tempDbl);
            expediaProperty.Latitude = tempDbl;

            expediaProperty.Location = propertyValues[(Int32)ExpediaPropertyFields.Location];

            Double.TryParse(propertyValues[(Int32)ExpediaPropertyFields.Longitude], out tempDbl);
            expediaProperty.Longitude = tempDbl;

            Double.TryParse(propertyValues[(Int32)ExpediaPropertyFields.LowRate], out tempDbl);
            expediaProperty.LowRate = tempDbl;

            expediaProperty.Name = propertyValues[(Int32)ExpediaPropertyFields.Name];
            expediaProperty.PostalCode = propertyValues[(Int32)ExpediaPropertyFields.PostalCode];
            expediaProperty.PropertyCategory = propertyValues[(Int32)ExpediaPropertyFields.PropertyCategory];
            expediaProperty.PropertyCurrency = propertyValues[(Int32)ExpediaPropertyFields.PropertyCurrency];

            Int32.TryParse(propertyValues[(Int32)ExpediaPropertyFields.RegionId], out tempInt);
            expediaProperty.RegionId = tempInt;

            Int32.TryParse(propertyValues[(Int32)ExpediaPropertyFields.SequenceNumber], out tempInt);
            expediaProperty.SequenceNumber = tempInt;

            Double.TryParse(propertyValues[(Int32)ExpediaPropertyFields.StarRating], out tempDbl);
            expediaProperty.StarRating = tempDbl;

            expediaProperty.StateProvince = propertyValues[(Int32)ExpediaPropertyFields.StateProvince];
            expediaProperty.SupplierType = propertyValues[(Int32)ExpediaPropertyFields.SupplierType];

            return expediaProperty;

        }

        public static T CopyHotelImage<T, TE>(string[] values, TE fields) 
            where T : new()
            where TE : struct, IComparable, IFormattable, IConvertible
        {
            var expediaEntity = new T();

            if (!typeof(TE).IsEnum)
            {
                throw new ArgumentException("TE must be an enumerated type");
            }

            foreach(var field in Enum.GetValues(typeof(TE)))
            {
                var stringVal = values[fields];
                var property = expediaEntity.GetType().GetProperty(field.ToString());

                if (property.PropertyType == typeof (string))
                {
                    property.SetValue(expediaEntity, stringVal, null);
                }
                else if(property.PropertyType == typeof (double))
                {
                    double tempDbl = 0.0;
                    Double.TryParse(stringVal, out tempDbl);
                    property.SetValue(expediaEntity, tempDbl, null);
                }
                else if (property.PropertyType == typeof(int))
                {
                    int tempInt = 0;
                    Int32.TryParse(stringVal, out tempInt);
                    property.SetValue(expediaEntity, tempInt, null);
                }
            }

            return expediaEntity;
        }
    }
}