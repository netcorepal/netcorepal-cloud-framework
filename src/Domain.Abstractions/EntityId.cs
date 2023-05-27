using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    public record EntityId : IComparable, IComparable<Int64>, IConvertible, IEquatable<Int64>, ISpanFormattable, IFormattable
    {
        public EntityId()
        {
            _id = 0;
        }
        public EntityId(long id)
        {
            _id = id;
        }

        readonly private long _id;

        public int CompareTo(object? obj)
        {
            return _id.CompareTo(obj);
        }

        public int CompareTo(long other)
        {
            return _id.CompareTo(other);
        }

        public bool Equals(long other)
        {
            return _id.Equals(other);
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            return Convert.ToBoolean(_id, provider);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(_id, provider);
        }

        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(_id, provider);
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(_id, provider);
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(_id, provider);
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return Convert.ToDouble(_id, provider);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(_id, provider);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(_id, provider);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(_id, provider);
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(_id, provider);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(_id, provider);
        }

        public string ToString(IFormatProvider? provider)
        {
            return _id.ToString(provider);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return _id.ToString(format, formatProvider);
        }

        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(_id, conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            return Convert.ToUInt16(_id, provider);
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            return Convert.ToUInt32(_id, provider);
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            return Convert.ToUInt64(_id, provider);
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            return _id.TryFormat(destination, out charsWritten, format, provider);
        }


        public static implicit operator long(EntityId d)
        {
            return d._id;
        }

        public static implicit operator EntityId(long d)
        {
            return new EntityId(d);
        }

        public override bool Equals(object obj)
        {
            return _id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !(left == right);
        }

        public static bool operator <(EntityId left, EntityId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(EntityId left, EntityId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(EntityId left, EntityId right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(EntityId left, EntityId right)
        {
            return left.CompareTo(right) >= 0;
        }
    }


    public record EntityId<TEntity> : EntityId
    { 

    }
}
