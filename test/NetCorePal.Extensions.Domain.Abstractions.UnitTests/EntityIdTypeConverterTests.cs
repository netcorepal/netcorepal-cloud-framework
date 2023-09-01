using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NetCorePal.Extensions.Domain.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdTypeConverterTests
    {
        public record OrderId1(long Id) : IInt64StronglyTypedId
        {
            public override string ToString()
            {
                return Id.ToString();
            }
        }

        public record OrderId2(int Id) : IInt32StronglyTypedId
        {
            public override string ToString()
            {
                return Id.ToString();
            }
        }

        public record OrderId3(string Id) : IStringStronglyTypedId
        {
            public override string ToString()
            {
                return Id.ToString();
            }
        }

        public record OrderId4(Guid Id) : IGuidStronglyTypedId
        {
            public override string ToString()
            {
                return Id.ToString();
            }
        }

        [Fact]
        public void CanConvertFromTest()
        {

            var longConvert = new EntityIdTypeConverter<OrderId1, long>();

            Assert.True(longConvert.CanConvertTo(typeof(string)));
            Assert.False(longConvert.CanConvertTo(typeof(Guid)));
            Assert.True(longConvert.CanConvertTo(typeof(long)));
            Assert.False(longConvert.CanConvertTo(typeof(int)));

            var intConvert = new EntityIdTypeConverter<OrderId2, int>();
            Assert.True(intConvert.CanConvertTo(typeof(string)));
            Assert.False(intConvert.CanConvertTo(typeof(Guid)));
            Assert.False(intConvert.CanConvertTo(typeof(long)));
            Assert.True(intConvert.CanConvertTo(typeof(int)));


            var stringConvert = new EntityIdTypeConverter<OrderId3, string>();
            Assert.True(stringConvert.CanConvertTo(typeof(string)));
            Assert.False(stringConvert.CanConvertTo(typeof(Guid)));
            Assert.False(stringConvert.CanConvertTo(typeof(long)));
            Assert.False(stringConvert.CanConvertTo(typeof(int)));

            var guidConvert = new EntityIdTypeConverter<OrderId4, Guid>();
            Assert.True(guidConvert.CanConvertTo(typeof(string)));
            Assert.True(guidConvert.CanConvertTo(typeof(Guid)));
            Assert.False(guidConvert.CanConvertTo(typeof(long)));
            Assert.False(guidConvert.CanConvertTo(typeof(int)));


        }


        [Fact]
        public void CanConvertToTest()
        {
            var longConvert = new EntityIdTypeConverter<OrderId1, long>();
            Assert.True(longConvert.CanConvertFrom(typeof(string)));
            Assert.False(longConvert.CanConvertFrom(typeof(Guid)));
            Assert.True(longConvert.CanConvertFrom(typeof(long)));
            Assert.False(longConvert.CanConvertFrom(typeof(int)));

            var intConvert = new EntityIdTypeConverter<OrderId2, int>();
            Assert.True(intConvert.CanConvertFrom(typeof(string)));
            Assert.False(intConvert.CanConvertFrom(typeof(Guid)));
            Assert.False(intConvert.CanConvertFrom(typeof(long)));
            Assert.True(intConvert.CanConvertFrom(typeof(int)));

            var stringConvert = new EntityIdTypeConverter<OrderId3, string>();
            Assert.True(stringConvert.CanConvertFrom(typeof(string)));
            Assert.False(stringConvert.CanConvertFrom(typeof(Guid)));
            Assert.False(stringConvert.CanConvertFrom(typeof(long)));
            Assert.False(stringConvert.CanConvertFrom(typeof(int)));

            var guidConvert = new EntityIdTypeConverter<OrderId4, Guid>();
            Assert.True(guidConvert.CanConvertFrom(typeof(string)));
            Assert.True(guidConvert.CanConvertFrom(typeof(Guid)));
            Assert.False(guidConvert.CanConvertFrom(typeof(long)));
            Assert.False(guidConvert.CanConvertFrom(typeof(int)));



        }


        [Fact]
        public void ConvertToTest()
        {
            var longConvert = new EntityIdTypeConverter<OrderId1, long>();
            Assert.Equal("1", longConvert.ConvertTo(new OrderId1(1), typeof(string)));

            var intConvert = new EntityIdTypeConverter<OrderId2, int>();
            Assert.Equal("1", intConvert.ConvertTo(new OrderId2(1), typeof(string)));

            var stringConvert = new EntityIdTypeConverter<OrderId3, string>();
            Assert.Equal("1", stringConvert.ConvertTo(new OrderId3("1"), typeof(string)));

            var guidConvert = new EntityIdTypeConverter<OrderId4, Guid>();
            Assert.Equal("00000000-0000-0000-0000-000000000001", guidConvert.ConvertTo(new OrderId4(Guid.Parse("00000000-0000-0000-0000-000000000001")), typeof(string)));


        }

        [Fact]
        public void ConvertFromTest()
        {

            var propertyDescriptor = new Mock<MockPropertyDescriptor>();
            propertyDescriptor.SetupGet(x => x.PropertyType).Returns(typeof(OrderId1));


            var context = new Mock<ITypeDescriptorContext>();
            context.SetupGet(x => x.PropertyDescriptor).Returns(propertyDescriptor.Object);
            var longConvert = new EntityIdTypeConverter<OrderId1, long>();
            Assert.Equal(new OrderId1(12), longConvert.ConvertFrom(context.Object, System.Globalization.CultureInfo.CurrentCulture, "12"));


            propertyDescriptor.SetupGet(x => x.PropertyType).Returns(typeof(OrderId2));
            var intConvert = new EntityIdTypeConverter<OrderId2, int>();
            Assert.Equal(new OrderId2(12), intConvert.ConvertFrom(context.Object, System.Globalization.CultureInfo.CurrentCulture, "12"));

            propertyDescriptor.SetupGet(x => x.PropertyType).Returns(typeof(OrderId3));
            var stringConvert = new EntityIdTypeConverter<OrderId3, string>();
            Assert.Equal(new OrderId3("12"), stringConvert.ConvertFrom(context.Object, System.Globalization.CultureInfo.CurrentCulture, "12"));

            propertyDescriptor.SetupGet(x => x.PropertyType).Returns(typeof(OrderId4));
            var guidConvert = new EntityIdTypeConverter<OrderId4, Guid>();
            Assert.Equal(new OrderId4(Guid.Parse("00000000-0000-0000-0000-000000000012")), guidConvert.ConvertFrom(context.Object, System.Globalization.CultureInfo.CurrentCulture, "00000000-0000-0000-0000-000000000012"));

        }

        public class MockMemberDescriptor : MemberDescriptor
        {
            public MockMemberDescriptor() : base("abc")
            {
            }
        }


        public class MockPropertyDescriptor : PropertyDescriptor
        {
            public MockPropertyDescriptor() : base(new MockMemberDescriptor()) { }


            public override Type ComponentType => throw new NotImplementedException();

            public override bool IsReadOnly => throw new NotImplementedException();

            public override Type PropertyType => throw new NotImplementedException();

            public override bool CanResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override object? GetValue(object? component)
            {
                throw new NotImplementedException();
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object? component, object? value)
            {
                throw new NotImplementedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                throw new NotImplementedException();
            }
        }
    }
}
