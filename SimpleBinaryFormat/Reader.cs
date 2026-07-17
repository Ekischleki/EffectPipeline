using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBinaryFormat
{
    public class Reader
    {
        internal Stream stream;
        internal Dictionary<int, ISerializable> ptr = [];

        internal Reader(Stream stream)
        {
            this.stream = stream;
        }

        internal async Task<Region> ReadRegion()
        {
            var region = new Region(this);
            while (!await region.ReadValue()) { }
            return region;
        }

        public static async Task<T> Deserialize<T>(Stream stream)
            where T : ISerializable, new()
        {
            T start = new();
            var reader = new Reader(stream);
            reader.ptr.Add(0, start);
            await reader.DeserializeInternal();
            return start;
        }

        private async Task DeserializeInternal() 
        {
            byte[] transition_type_buf = [0];
            for(int working = 0; ; working++)
            {
                await stream.ReadExactlyAsync(transition_type_buf);
                var transitionType = transition_type_buf[0];
                if (transitionType == (byte)TransitionType.StreamEnd)
                {
                    break;
                }
                else if (transitionType != (byte)TransitionType.RegionStart) 
                {
                    throw new InvalidDataException("Expected Transition type");
                }
                var region = await ReadRegion();
                if(ptr.TryGetValue(working, out ISerializable? s))
                {
                    s.FromReader(region);
                }
            }
            foreach (var obj in ptr.Values)
            {
                obj.Apply();
            }
        }
    }

    public class Region
    {
        internal Reader parent;
        internal Dictionary<byte[], byte[]> namedSimple = new(new ByteArrayComparer());
        internal Dictionary<byte[], byte[]> namedSimpleArr = new(new ByteArrayComparer());
        internal Dictionary<byte[], int> namedObj = new(new ByteArrayComparer());
        internal Dictionary<byte[], int[]> namedObjArr = new(new ByteArrayComparer());

        internal Region(Reader parent)
        {
            this.parent = parent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Whether a region boundary has been reached</returns>
        internal async Task<bool> ReadValue()
        {
            byte[] field_name;
            byte[] tag_buf = [0];
            byte[] ptr_buf = new byte[4];
            byte[] len_buf = new byte[8];
            long len;
            int ptr;


            await parent.stream.ReadExactlyAsync(tag_buf.AsMemory());
            var tag = (DataType)tag_buf[0];
            switch(tag)
            {
                case DataType.RegionBoundry:
                    return true;
                case DataType.Simple:
                    byte[] bytes_len_buf = [0];
                    await parent.stream.ReadExactlyAsync(bytes_len_buf.AsMemory());
                    byte bytes_len = bytes_len_buf[0];
                    field_name = await ReadFieldName();
                    byte[] simple_buf = new byte[bytes_len];
                    await parent.stream.ReadExactlyAsync(simple_buf.AsMemory());
                    namedSimple.Add(field_name, simple_buf);
                    return false;
                case DataType.SimpleArray:
                    await parent.stream.ReadExactlyAsync(len_buf);
                    len = BinaryPrimitives.ReadInt64BigEndian(len_buf);
                    field_name = await ReadFieldName();
                    byte[] bytes = new byte[len];
                    await parent.stream.ReadExactlyAsync(bytes);
                    namedSimpleArr.Add(field_name, bytes);
                    return false;
                case DataType.SubRegion:
                    await parent.stream.ReadExactlyAsync(ptr_buf);
                    ptr = BinaryPrimitives.ReadInt32BigEndian(ptr_buf);
                    field_name = await ReadFieldName();
                    namedObj.Add(field_name, ptr);
                    return false;
                case DataType.RegionArray:
                    await parent.stream.ReadExactlyAsync(len_buf);
                    len = BinaryPrimitives.ReadInt64BigEndian(len_buf);
                    field_name = await ReadFieldName();
                    int[] ptrs = new int[len];
                    for(long i = 0; i < len; i++)
                    {
                        await parent.stream.ReadExactlyAsync(ptr_buf);
                        ptr = BinaryPrimitives.ReadInt32BigEndian(ptr_buf);
                        ptrs[i] = ptr;
                    }
                    namedObjArr.Add(field_name, ptrs);
                    return false;
                default:
                    throw new InvalidDataException();
            }
        }
        private async Task<byte[]> ReadFieldName()
        {
            byte[] name_buf = new byte[Writer.FIELD_IDENT_LEN];
            await parent.stream.ReadExactlyAsync(name_buf.AsMemory());
            return name_buf;
        }


        private T GetObject<T>(int ptr)
            where T : ISerializable, new()
        {
            if (parent.ptr.TryGetValue(ptr, out var value))
            {
                return (T)value;
            }
            T new_value = new();
            parent.ptr[ptr] = new_value;
            return new_value;
        }

        private ISerializable GetObject(int ptr, Type t)
        {
            if (parent.ptr.TryGetValue(ptr, out var value))
            {
                return value;
            }
            ISerializable new_value = (ISerializable)Activator.CreateInstance(t)!;
            parent.ptr[ptr] = new_value;
            return new_value;
        }


        /// <summary>
        /// Reads an <see cref="ISerializable[]"/>. Each element may not be initialized yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public T[] ReadObjectArr<T>(string fieldName)
            where T : ISerializable, new()
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var ptrs = namedObjArr[ident];
            return ptrs.Select(GetObject<T>).ToArray();
        }

        public ISerializable[] ReadObjectArr(string fieldName, Type[] types)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var ptrs = namedObjArr[ident];
            if(types.Length != ptrs.Length)
            {
                throw new ArgumentException();
            }
            return ptrs.Zip(types).Select(x => GetObject(x.First, x.Second)).ToArray();
        }

        public byte[] ReadBytes(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimpleArr[ident];
            return bytes;
        }

        public string ReadString(string fieldName)
        {
            var bytes = ReadBytes(fieldName);
            return Encoding.UTF8.GetString(bytes);
        }

        public ISerializable ReadObject(string fieldName, Type type)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var ptr = namedObj[ident];
            if (parent.ptr.TryGetValue(ptr, out var value))
            {
                return value;
            }
            ISerializable new_value = (ISerializable)Activator.CreateInstance(type)!;
            parent.ptr[ptr] = new_value;
            return new_value;
        }

        /// <summary>
        /// Reads an <see cref="ISerializable"/> object, which may not be initialized yet. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public T ReadObject<T>(string fieldName)
            where T : ISerializable, new()
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var ptr = namedObj[ident];
            return GetObject<T>(ptr);
        }

        public int ReadInt(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimple[ident];
            return BinaryPrimitives.ReadInt32BigEndian(bytes);
        }
        public uint ReadUInt(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimple[ident];
            return BinaryPrimitives.ReadUInt32BigEndian(bytes);
        }
        public float ReadFloat(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimple[ident];
            return BinaryPrimitives.ReadSingleBigEndian(bytes);
        }
        public double ReadDouble(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimple[ident];
            return BinaryPrimitives.ReadDoubleBigEndian(bytes);
        }
        public long ReadLong(string fieldName)
        {
            var ident = Writer.GetFieldIdentifier(fieldName);
            var bytes = namedSimple[ident];
            return BinaryPrimitives.ReadInt64BigEndian(bytes);
        }
    }
    internal sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.AsSpan().SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            HashCode hash = new();

            foreach (byte b in obj)
                hash.Add(b);

            return hash.ToHashCode();
        }
    }
}
