using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

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

        EntityIdTypeConverter<OrderId1,long> _longconverter = new EntityIdTypeConverter<OrderId1,long>();

        [Fact]
        public void CanConvertFromTest()
        {
            Assert.True(_longconverter.CanConvertFrom(typeof(string)));
            Assert.False(_longconverter.CanConvertFrom(typeof(Guid)));
            Assert.False(_longconverter.CanConvertFrom(typeof(long)));
            Assert.False(_longconverter.CanConvertFrom(typeof(int)));
        }


        [Fact]
        public void CanConvertToTest()
        {
            Assert.True(_longconverter.CanConvertFrom(typeof(string)));
            Assert.False(_longconverter.CanConvertFrom(typeof(Guid)));
            Assert.False(_longconverter.CanConvertFrom(typeof(long)));
            Assert.False(_longconverter.CanConvertFrom(typeof(int)));
        }


        [Fact]
        public void ConvertToTest()
        {
            Assert.Equal("1", _longconverter.ConvertTo(new OrderId1(1), typeof(string)));

        }

        [Fact]
        public void ConvertFromTest()
        {

            var propertyDescriptor = new Mock<MockPropertyDescriptor>();
            propertyDescriptor.SetupGet(x => x.PropertyType).Returns(typeof(OrderId1));


            var context = new Mock<ITypeDescriptorContext>();
            context.SetupGet(x => x.PropertyDescriptor).Returns(propertyDescriptor.Object);
            Assert.Equal(new OrderId1(12), _longconverter.ConvertFrom(context.Object, System.Globalization.CultureInfo.CurrentCulture, "12"));
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
