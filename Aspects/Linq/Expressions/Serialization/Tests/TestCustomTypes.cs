﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    public enum EnumTest
    {
        One,
        Two,
        Three,
    };

    [Flags]
    public enum EnumFlagsTest
    {
        One = 1,
        Two = 2,
        Three = 4,
    }

    [DataContract(Namespace = "urn:vm.AspectsTest.Diagnostics", IsReference = true)]
    public class Object1
    {
        [DataMember]
        public object ObjectProperty
        { get; set; }
        [DataMember]
        public int? NullIntProperty
        { get; set; }
        [DataMember]
        public long? NullLongProperty
        { get; set; }
        [DataMember]
        public bool BoolProperty
        { get; set; }
        [DataMember]
        public char CharProperty
        { get; set; }
        [DataMember]
        public byte ByteProperty
        { get; set; }
        [DataMember]
        public sbyte SByteProperty
        { get; set; }
        [DataMember]
        public short ShortProperty
        { get; set; }
        [DataMember]
        public int IntProperty
        { get; set; }
        [DataMember]
        public long LongProperty
        { get; set; }
        [DataMember]
        public ushort UShortProperty
        { get; set; }
        [DataMember]
        public uint UIntProperty
        { get; set; }
        [DataMember]
        public ulong ULongProperty
        { get; set; }
        [DataMember]
        public double DoubleProperty
        { get; set; }
        [DataMember]
        public float FloatProperty
        { get; set; }
        [DataMember]
        public decimal DecimalProperty
        { get; set; }
        [DataMember]
        public Guid GuidProperty
        { get; set; }
        [DataMember]
        public Uri UriProperty
        { get; set; }
        [DataMember]
        public DateTime DateTimeProperty
        { get; set; }
        [DataMember]
        public TimeSpan TimeSpanProperty
        { get; set; }
        [DataMember]
        public DateTimeOffset DateTimeOffsetProperty
        { get; set; }

        public string StringField;

        public static Object1 GetObject1() => new Object1
        {
            ObjectProperty         = null,
            NullIntProperty        = null,
            NullLongProperty       = 1L,
            BoolProperty           = true,
            CharProperty           = 'A',
            ByteProperty           = 1,
            SByteProperty          = 1,
            ShortProperty          = 1,
            IntProperty            = 1,
            LongProperty           = 1,
            UShortProperty         = 1,
            UIntProperty           = 1,
            ULongProperty          = 1,
            DoubleProperty         = 1.0,
            FloatProperty          = (float)1.0,
            DecimalProperty        = 1M,
            GuidProperty           = Guid.Empty,
            UriProperty            = new Uri("http://localhost"),
            DateTimeProperty       = new DateTime(2013, 1, 13),
            TimeSpanProperty       = new TimeSpan(123L),
            DateTimeOffsetProperty = new DateTimeOffset(new DateTime(2013, 1, 13)),
        };
    }

    [DataContract]
    public class ClassDataContract1
    {
        [DataMember]
        public int IntProperty
        { get; set; }

        [DataMember]
        public string StringProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    [Serializable]
    public class ClassSerializable1
    {
        public int IntProperty
        { get; set; }
        public string StringProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    public class ClassNonSerializable
    {
        public int IntProperty
        { get; set; }
        public string StringProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    [Serializable]
    public struct StructSerializable1
    {
        public int IntProperty
        { get; set; }
        public string StringProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    [DataContract]
    public struct StructDataContract1
    {
        [DataMember]
        public int IntProperty
        { get; set; }

        [DataMember]
        public string StringProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    [DataContract]
    public class A
    {
        [DataMember]
        public int a;

        public static A operator -(A x) => new A { a = -x.a };

        public static A operator +(A x) => new A { a = x.a };
    }

    [DataContract]
    class B
    {
        [DataMember]
        public bool b;

        public static B operator !(B x) => new B { b = !x.b };
    }

    class TestMethods
    {
        readonly int _a = 3;

        public static int Method1() => 1;

        public static int Method2(int i, string s) => i;

        public int Method3(int i, double d) => i+_a;
    }

    class Inner
    {
        public int IntProperty
        { get; set; }
        public string StringProperty
        { get; set; }
    }

    class TestMembersInitialized
    {
        public int TheOuterIntProperty
        { get; set; }
        public DateTime Time;
        public Inner InnerProperty
        { get; set; }
        public IEnumerable<string> MyProperty
        { get; set; }
    }
}
