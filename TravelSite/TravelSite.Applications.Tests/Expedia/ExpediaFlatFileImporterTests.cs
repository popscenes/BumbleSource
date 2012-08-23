using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using TravelSite.Applications.Intergrations.Expedia;

namespace TravelSite.Applications.Tests.Expedia
{
    [TestFixture]
    public class ExpediaFlatFileImporterTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
        }



        [Test]
        public void ExpediaFileImporterImportHotelImagesTest()
        {
            var expediaFlatFileImporter = new ExpediaFileImporter();
            String path = "..\\..\\Resources\\HotelImageListExtract.txt";
            IEnumerable<ExpediaHotelImage> hotelImages = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyHotelImage, path).Take(10);
            var hotelImagesList = hotelImages.ToList();
            Assert.Count(10, hotelImagesList);
            Assert.AreEqual(hotelImagesList[0].EANHotelId, 4110);
            Assert.AreEqual(hotelImagesList[0].Caption, "Property Amenity");
            Assert.AreEqual(hotelImagesList[0].Url, "http://media.expedia.com/hotels/1000000/50000/40200/40186/40186_6_b.jpg");
            Assert.AreEqual(hotelImagesList[0].Width, 350);
            Assert.AreEqual(hotelImagesList[0].Height, 350);
            Assert.AreEqual(hotelImagesList[0].ByteSize, 0);
            Assert.AreEqual(hotelImagesList[0].ThumbnailUrl, "http://media.expedia.com/hotels/1000000/50000/40200/40186/40186_6_t.jpg");
            Assert.AreEqual(hotelImagesList[0].DefaultImage, "0");
        }

        [Test]
        public void ExpediaFileImporterImportAttributePropertyLinkTest()
        {
            var expediaFlatFileImporter = new ExpediaFileImporter();
            String path = "..\\..\\Resources\\PropertyAttributeLinkExtract.txt";
            IEnumerable<ExpediaAttributePropertyLink> attributesPropertyLinks = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyAttributePropertyLink, path).Take(10);

            var attributesPropertyLinkList = attributesPropertyLinks.ToList();
            Assert.Count(10, attributesPropertyLinkList);
            Assert.AreEqual(attributesPropertyLinkList[0].EANHotelId, 106425);
            Assert.AreEqual(attributesPropertyLinkList[0].AttributeId, 51);
            Assert.AreEqual(attributesPropertyLinkList[0].LanguageCode, "en_US");
            Assert.AreEqual(attributesPropertyLinkList[0].AppendTxt, "");
        }

       

        [Test]
        public void ExpediaFileImporterImportAttributesTest()
        {
            var expediaFlatFileImporter = new ExpediaFileImporter();
            String path = "..\\..\\Resources\\AttributeList.txt";
            IEnumerable<ExpediaAttributes> attributes = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyAttributes, path).Take(10);

            var attributesList = attributes.ToList();
            Assert.Count(10, attributesList);
            Assert.AreEqual(attributesList[0].AttributeId, 1);
            Assert.AreEqual(attributesList[0].LanguageCode, "en_US");
            Assert.AreEqual(attributesList[0].AttributeDesc, "Air conditioning");
            Assert.AreEqual(attributesList[0].Type, "RoomAmenity");
            Assert.AreEqual(attributesList[0].SubType, "RoomAmenity");    
        }

        [Test]
        public void ExpediaFileImporterImportRoomTypesTest()
        {
            var expediaFlatFileImporter = new ExpediaFileImporter();
            String path = "..\\..\\Resources\\RoomTypesListExtract.txt";
            IEnumerable<ExpediaRoomTypes> roomTypes = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyRoomType, path).Take(10);

            var roomTypesList = roomTypes.ToList();
            Assert.Count(10, roomTypesList);
            Assert.AreEqual(roomTypesList[0].EANHotelId, 4110);
            Assert.AreEqual(roomTypesList[0].RoomTypeId, 43316);
            Assert.AreEqual(roomTypesList[0].LanguageCode, "en_US");
            Assert.AreEqual(roomTypesList[0].RoomTypeImage, "http://media.expedia.com/hotels/1000000/50000/40200/40186/40186_33_s.jpg");
            Assert.AreEqual(roomTypesList[0].RoomTypeName, "Ocean Tower Deluxe");
            Assert.AreEqual(roomTypesList[0].RoomTypeDescription, "<strong><ul><li>One King Bed</li><li>Two Double Bed" +
                                                                  "s</li></ul></strong>This room measures 365 square feet (34 square meters). Get a good night's sleep with premium bedding and blackout drapes/curtains. Bathroom amenities include complimentary toiletries. Wireless Internet access (surcharge) keeps you connected, and satellite channels with pay movies are offered for your entertainment. You can request an in-room massage. Climate control, daily housekeeping, and an iron and ironing board are among the included amenities.");
        }

        [Test]
        public void ExpediaFileImporterActivePropertyImportTest()
        {
            var expediaFlatFileImporter = new ExpediaFileImporter();
            String path = "..\\..\\Resources\\ActivePropertyListExtract.txt";
            IEnumerable<ExpediaProperty> hotels = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyProperty, path).Take(10);
            
            var hotelsList = hotels.ToList();
            Assert.Count(10, hotelsList);
            Assert.AreEqual(hotelsList[0].EANHotelId, 109411);
            Assert.AreEqual(hotelsList[0].SequenceNumber, 1);
            Assert.AreEqual(hotelsList[0].Name, "Lux 11 Berlin Mitte");
            Assert.AreEqual(hotelsList[0].Address1, "Rosa-Luxemburg-Str. 9-13");
            Assert.AreEqual(hotelsList[0].Address2, "");
            Assert.AreEqual(hotelsList[0].City, "Berlin");
            Assert.AreEqual(hotelsList[0].StateProvince, "");
            Assert.AreEqual(hotelsList[0].PostalCode, "10178");
            Assert.AreEqual(hotelsList[0].Country, "DE");
            Assert.AreEqual(hotelsList[0].Latitude, 52.52453);
            Assert.AreEqual(hotelsList[0].Longitude, 13.41001);
            Assert.AreEqual(hotelsList[0].AirportCode, "TXL");
            Assert.AreEqual(hotelsList[0].PropertyCategory, "1");
            Assert.AreEqual(hotelsList[0].PropertyCurrency, "EUR");
            Assert.AreEqual(hotelsList[0].StarRating, 4.0);
            Assert.AreEqual(hotelsList[0].Confidence, 52);
            Assert.AreEqual(hotelsList[0].SupplierType, "ESR");
            Assert.AreEqual(hotelsList[0].Location, "Near Alexanderplatz");
            Assert.AreEqual(hotelsList[0].ChainCodeId, "");
            Assert.AreEqual(hotelsList[0].RegionId, 536);
            Assert.AreEqual(hotelsList[0].HighRate, 282.4372);
            Assert.AreEqual(hotelsList[0].LowRate, 145.7203);
            Assert.AreEqual(hotelsList[0].CheckInTime, "4 PM");
            Assert.AreEqual(hotelsList[0].CheckOutTime, "noon");

            hotels = expediaFlatFileImporter.ImportExpediaFile(ExpediaFileImporter.CopyProperty, path).Skip(50).Take(50);
            hotelsList = hotels.ToList();

            Assert.Count(49, hotelsList);
        }

    }
}
