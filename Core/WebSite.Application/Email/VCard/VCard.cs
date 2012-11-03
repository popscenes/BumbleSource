using System;
using System.Net.Mail;
using System.Text;
using Website.Infrastructure.Util.Extension;

namespace Website.Application.Email.VCard 
{
    public class VCard
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string JobTitle { get; set; }
        public string StreetAddress { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string CountryName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string HomePage { get; set; }
        public byte[] Image { get; set; }
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCARD");
            builder.AppendLine("VERSION:2.1");
            // Name
            builder.AppendLine("N:" + LastName.EmptyIfNull() + ";" + FirstName.EmptyIfNull());
            // Full name
            builder.AppendLine("FN:" + FirstName.EmptyIfNull() + " " + LastName.EmptyIfNull());
            // Address
            builder.Append("ADR;HOME;PREF:;;");
            builder.Append(StreetAddress.EmptyIfNull() + ";");
            builder.Append(Locality.EmptyIfNull() + ";");
            builder.Append(Region.EmptyIfNull() + ";");
            builder.Append(PostalCode.EmptyIfNull() + ";");
            builder.AppendLine(CountryName.EmptyIfNull());
            // Other data
            builder.AppendLine("ORG:" + Organization.EmptyIfNull());
            builder.AppendLine("TITLE:" + JobTitle.EmptyIfNull());
            builder.AppendLine("TEL;HOME;VOICE:" + Phone.EmptyIfNull());
            builder.AppendLine("TEL;CELL;VOICE:" + Mobile.EmptyIfNull());
            builder.AppendLine("URL;" + HomePage.EmptyIfNull());
            builder.AppendLine("EMAIL;PREF;INTERNET:" + Email.EmptyIfNull());
            
            if(Image != null && Image.Length > 0)
            {
                builder.AppendLine("PHOTO;ENCODING=BASE64;TYPE=JPEG:");
                builder.AppendLine(Convert.ToBase64String(Image));
                builder.AppendLine(string.Empty);                
            }

            builder.AppendLine("END:VCARD");
            return builder.ToString();
        }

        public byte[] GetUtf8Bytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(ToString());
        }

        public String GetFileName(String filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
                return filename;

            var ret = "";
            if (!String.IsNullOrWhiteSpace(ret))
                ret += " ";
            ret += FirstName.EmptyIfNull();
            if (!String.IsNullOrWhiteSpace(ret))
                ret += " ";
            ret += LastName.EmptyIfNull();

            return ret + ".vcf";
        }

    }
}
