using System;
using Xunit;
using Meetup.Scheduling.Domain;

namespace Meetup.Scheduling.Test
{
    public class ValueObjectsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_Not_Create_Address(string address)
        {
            Address CreateAddress() => Address.From(address);
            Assert.ThrowsAny<Exception>(CreateAddress);
        }

        [Theory]
        [InlineData("Address 1")]
        public void Should_Create_Address(string address)
            => Assert.Equal(address, Address.From(address));

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void Should_Create_PositiveNumber(int number)
            => Assert.Equal<PositiveNumber>(number, PositiveNumber.From(number));

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(0)]
        [InlineData(null)]
        public void Should_Create_PositiveNumber_With_Zero_Value(int number)
            => Assert.Equal<PositiveNumber>(0, PositiveNumber.From(number));

        [Fact]
        public void Should_Create_Online_Location()
        {
            var expected = new Uri("http://zoom.us/netcorebcn");
            var sut      = Location.Online(expected);
            Assert.True(sut.IsOnline);
            Assert.Equal(expected, sut.Url);
        }

        [Fact]
        public void Should_Not_Create_Online_Location()
        {
            void CreateLocation() => Location.Online(null);
            Assert.Throws<ArgumentNullException>(CreateLocation);
        }

        [Fact]
        public void Should_Create_OnSite_Location()
        {
            var expected = Address.From("Address 1");
            var sut      = Location.OnSite(expected);
            Assert.False(sut.IsOnline);
            Assert.Equal(expected, sut.Address);
        }

        [Fact]
        public void Should_Not_Create_OnSite_Location()
        {
            void CreateLocation() => Location.OnSite(null);
            Assert.Throws<ArgumentNullException>(CreateLocation);
        }

        [Theory]
        [InlineData("netcorebcn")]
        public void Should_Create_GroupSlug(string group)
            => Assert.Equal(group, GroupSlug.From(group));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("netco%rebcn")]
        public void Should_Not_Create_GroupSlug(string group)
        {
            void CreateGroupSlug() => GroupSlug.From(group);
            Assert.ThrowsAny<ArgumentException>(CreateGroupSlug);
        }

        [Fact]
        public void Should_Create_DateTimeRange()
        {
            var start = DateTimeOffset.Now;
            var end   = start.AddHours(1);
            var sut   = DateTimeRange.From(start, end);

            Assert.Equal(start, sut.Start);
            Assert.Equal(end, sut.End);
        }

        [Fact]
        public void Should_Not_Create_DateTimeRange()
        {
            var start = DateTimeOffset.Now;

            void CreateDateTimeRange() => DateTimeRange.From(start, start.AddHours(-1));
            Assert.ThrowsAny<ArgumentException>(CreateDateTimeRange);
        }
    }
}