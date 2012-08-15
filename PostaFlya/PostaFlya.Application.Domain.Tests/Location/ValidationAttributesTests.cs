using System.Collections.Generic;
using MbUnit.Framework;
using PostaFlya.Application.Domain.Location;

namespace PostaFlya.Application.Domain.Tests.Location
{
    [TestFixture]
    public class ValidationAttributesTests
    {
        [Test]
        public void ValidLocationsShouldSucceedForListOfValidLocation()
        {
            var validate = new ValidLocationsAttribute();
            var locs = new List<PostaFlya.Domain.Location.Location>() { new PostaFlya.Domain.Location.Location(20, 20), new PostaFlya.Domain.Location.Location(30, 30) };
            Assert.IsTrue(validate.IsValid(locs));
        }

        [Test]
        public void ValidLocationsShouldFailForListWithInvalidLocation()
        {
            var validate = new ValidLocationsAttribute();
            var locs = new List<PostaFlya.Domain.Location.Location>() { new PostaFlya.Domain.Location.Location(20, 200), new PostaFlya.Domain.Location.Location(30, 30) };
            Assert.IsFalse(validate.IsValid(locs));
        }

        [Test]
        public void ValidLocationsShouldSucceedForEmptyLocationList()
        {
            var validate = new ValidLocationsAttribute();
            var locs = new List<PostaFlya.Domain.Location.Location>() { };
            Assert.IsTrue(validate.IsValid(locs));
        }
    }
}
