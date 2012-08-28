using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Website.Application.Content
{
   public class ExifImage
   {
      private const int PropertyTagExifDTOrig = 0x9003;
      private const int PropertyTagImageDescription = 0x010E;
      private const int PropertyTagImageTitle = 0x0320;
      private const int PropertyTagGpsLatitudeRef = 0x0001;
      private const int PropertyTagGpsLatitude = 0x0002;
      private const int PropertyTagGpsLongitudeRef = 0x0003;
      private const int PropertyTagGpsLongitude = 0x0004;
      private const short PropertyTagTypeASCII = 2;
      private const short PropertyTagTypeRational = 5;

      private readonly Image _image;

      public ExifImage(Image img)
      {
          _image = img;
      }

      protected bool Matches(PropertyItem item, int id, short type, int? length = null)
      {
         return (item.Id == id &&
            item.Type == type &&
            (length == null || item.Len == length));
      }

      protected DateTime? GetDateTime(PropertyItem item)
      {
         if (item != null)
         {
            string stringDate = item.Value.ToString();
            int year = int.Parse(stringDate.Substring(0, 4));
            int month = int.Parse(stringDate.Substring(5, 2));
            int day = int.Parse(stringDate.Substring(8, 2));
            int hour = int.Parse(stringDate.Substring(11, 2));
            int minute = int.Parse(stringDate.Substring(14, 2));
            int second = int.Parse(stringDate.Substring(17, 2));

            return new DateTime(year, month, day, hour, minute, second);
         }
         return null;
      }

      protected string GetString(PropertyItem item)
      {
         if (item != null)
         {
            return Encoding.ASCII.GetString(item.Value).Trim('\0');
         }
         return null;
      }

      protected double? GetDegreesAsDouble(PropertyItem item)
      {
         if (item != null)
         {
            var b0 = new byte[4];
            var b1 = new byte[4];
            var b2 = new byte[4];
            var b3 = new byte[4];
            var b4 = new byte[4];
            var b5 = new byte[4];

            Array.Copy(item.Value, 0, b0, 0, 4);
            Array.Copy(item.Value, 4, b1, 0, 4);
            Array.Copy(item.Value, 8, b2, 0, 4);
            Array.Copy(item.Value, 12, b3, 0, 4);
            Array.Copy(item.Value, 16, b4, 0, 4);
            Array.Copy(item.Value, 20, b5, 0, 4);

            var i0 = BitConverter.ToInt32(b0, 0);
            var i1 = BitConverter.ToInt32(b1, 0);
            var i2 = BitConverter.ToInt32(b2, 0);
            var i3 = BitConverter.ToInt32(b3, 0);
            var i4 = BitConverter.ToInt32(b4, 0);
            var i5 = BitConverter.ToInt32(b5, 0);

            if (i1 != 0 && i3 != 0 && i5 != 0)
            {
               //return 1.0 * i0 / i1 + (1.0 * i2 / i3) / 60.0 + (1.0 * i4 / i5) / 3600.0;
               return (1.0 * i0 / i1) + ((1.0 * i2 / i3) / 60.0) + ((1.0 * i4 / i5) / 3600.0);
                
            }
            return null;
         }
         return null;
      }

      protected byte[] GetDoubleAsDegrees(double decDegrees)
      {
            decDegrees = Math.Abs(decDegrees);
            var i0 = (int)decDegrees;
            const int i1 = 1;
            var mins = (decDegrees - i0)*60;
            var i2 = (int)mins;
            const int i3 = 1;
            var i4 = (int)((mins - i2)*60*1000);
            const int i5 = 1000;

            var b0 = BitConverter.GetBytes(i0);
            var b1 = BitConverter.GetBytes(i1);
            var b2 = BitConverter.GetBytes(i2);
            var b3 = BitConverter.GetBytes(i3);
            var b4 = BitConverter.GetBytes(i4);
            var b5 = BitConverter.GetBytes(i5);

           
            var ret = new byte[24];
            Array.Copy(b0, 0, ret, 0, 4);
            Array.Copy(b1, 0, ret, 4, 4);
            Array.Copy(b2, 0, ret, 8, 4);
            Array.Copy(b3, 0, ret, 12, 4);
            Array.Copy(b4, 0, ret, 16, 4);
            Array.Copy(b5, 0, ret, 20, 4);
            return ret;
      }

      protected PropertyItem FirstMatching(int id, short type, int? length = null)
      {
         return _image.PropertyItems.FirstOrDefault(item => Matches(item,id,type,length));
      }

      protected void UpdatePropertyItem(int id, short type, byte[] data)
      {
          PropertyItem prop = null;
          if(_image.PropertyIdList.Contains(id))
            prop = _image.GetPropertyItem(id);
          if(prop == null)
              prop = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true);

          prop.Id = id;
          prop.Type = type;
          prop.Len = data.Length;
          prop.Value = data;
          _image.SetPropertyItem(prop);
      }

      public DateTime? GetDataTime()
      {
          return _image != null ? GetDateTime(FirstMatching(PropertyTagExifDTOrig, PropertyTagTypeASCII, 20)) : null;
      }

       public double? GetLatitude()
      {
         if (_image != null)
         {
            double? computedLatitude = GetDegreesAsDouble(FirstMatching(PropertyTagGpsLatitude, PropertyTagTypeRational, 24));
            string reference = GetString(FirstMatching(PropertyTagGpsLatitudeRef, PropertyTagTypeASCII, 2));

            if (reference != null && computedLatitude != null)
            {
               if (reference.ToLower() == "s")
                  computedLatitude *= -1;

               return computedLatitude;
            }
         }
         return null;
      }

      public double? GetLongitude()
      {
         if (_image != null)
         {
            double? computedLongitude = GetDegreesAsDouble(FirstMatching(PropertyTagGpsLongitude, PropertyTagTypeRational, 24));
            string reference = GetString(FirstMatching(PropertyTagGpsLongitudeRef,PropertyTagTypeASCII,2));

            if (reference != null && computedLongitude != null)
            {
               if (reference.ToLower() == "w")
                  computedLongitude *= -1;

               return computedLongitude;
            }
         }
         return null;
      }

      public string GetImageDescription()
      {
          return _image != null ? GetString(FirstMatching(PropertyTagImageDescription, PropertyTagTypeASCII)) : "";
      }

       public string GetImageTitle()
       {
           return _image != null ? GetString(FirstMatching(PropertyTagImageTitle, PropertyTagTypeASCII)) : "";
       }

       public void SetLatitude(double value)
       {
           var ns = value < 0 ? "s" : "n";
           UpdatePropertyItem(PropertyTagGpsLatitudeRef, PropertyTagTypeASCII, Encoding.ASCII.GetBytes(ns +'\0'));
           UpdatePropertyItem(PropertyTagGpsLatitude, PropertyTagTypeRational, GetDoubleAsDegrees(value));
       }

       public void SetLongitude(double value)
       {
           var ew = value < 0 ? "w" : "e";
           UpdatePropertyItem(PropertyTagGpsLongitudeRef, PropertyTagTypeASCII, Encoding.ASCII.GetBytes(ew + '\0'));
           UpdatePropertyItem(PropertyTagGpsLongitude, PropertyTagTypeRational, GetDoubleAsDegrees(value));
       }

       public void SetImageTitle(string title)
       {
           UpdatePropertyItem(PropertyTagImageTitle, PropertyTagTypeASCII, Encoding.ASCII.GetBytes(title + '\0'));
       }

       public void SetImageDescription(string description)
       {
           UpdatePropertyItem(PropertyTagImageDescription, PropertyTagTypeASCII, Encoding.ASCII.GetBytes(description + '\0'));
       }
   }
}
