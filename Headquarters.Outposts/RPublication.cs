﻿using Headquarters.Communications;
using StackExchange.Redis;
using System;

namespace Headquarters.Outposts
{
    /// <summary>
    /// Implements an <see cref="IPublication"/> for Redis by wrapping <see cref="RedisValue"/>
    /// </summary>
    public struct RPublication : IPublication, IEquatable<RPublication>, IComparable<RPublication>, IComparable, IConvertible
    {
        private RedisValue _value;

        public static implicit operator RPublication(int? value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(long? value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(double value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(string value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(byte[] value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(bool? value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RPublication(RedisValue value)
        {
            return new RPublication { _value = value };
        }

        public static implicit operator RedisValue(RPublication value)
        {
            return value._value;
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)_value).CompareTo(obj);
        }

        public int CompareTo(RPublication other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(RPublication other)
        {
            return other._value == _value;
        }

        public TypeCode GetTypeCode()
        {
            return ((IConvertible)_value).GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)_value).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt64(provider);
        }

        public override bool Equals(object obj)
        {
            return _value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(RPublication left, RPublication right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(RPublication left, RPublication right)
        {
            return !(left == right);
        }

        public static bool operator <(RPublication left, RPublication right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(RPublication left, RPublication right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(RPublication left, RPublication right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(RPublication left, RPublication right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}