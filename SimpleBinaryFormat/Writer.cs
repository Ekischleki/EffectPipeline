using System.Buffers.Binary;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace SimpleBinaryFormat
{


    public class Writer
    {
        public static async Task EncodeTo(Stream stream, ISerializable obj)
        {
            var writer = new Writer(stream, obj);
            await writer.Encode();
        }
        /// <summary>
        /// How long a field identifyer is in bytes
        /// </summary>
        public const int FIELD_IDENT_LEN = 4;
        private Stream stream;
        private Dictionary<ISerializable, int> ObjectPointers = [];
        /// <summary>
        /// Stores all objects that have yet to be serialized.
        /// </summary>
        private Queue<ISerializable> obj = [];
        private int currentWorkingObj = 0;

        internal Writer(Stream stream, ISerializable initial)
        {
            this.stream = stream;
            obj.Enqueue(initial);
            ObjectPointers.Add(initial, 0);
        }
        internal static byte[] GetFieldIdentifier(string fieldName)
        {
            var hash = SHA3_256.HashData(Encoding.UTF8.GetBytes(fieldName));
            return hash[..FIELD_IDENT_LEN];
        }
        internal async Task WriteFieldName(string fieldName)
        {
            await stream.WriteAsync(GetFieldIdentifier(fieldName));
        }

        internal int GetObjectPtr(ISerializable v)
        {
            if (!ObjectPointers.TryGetValue(v, out int ptr))
            {
                obj.Enqueue(v);
                ptr = currentWorkingObj + obj.Count;
                ObjectPointers.Add(v, ptr);
            }
            return ptr;
        }
        public async Task WriteObject(string fieldName, ISerializable v)
        {
            int ptr = GetObjectPtr(v);
            byte[] buf = [(byte)DataType.SubRegion, 0, 0, 0, 0];
            BinaryPrimitives.WriteInt32BigEndian(buf.AsSpan()[1..], ptr);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
            await WriteFieldName(fieldName);
        }
        public async Task WriteArray<T>(string fieldName, T[] v)
            where T : ISerializable
        {
            byte[] buf = [(byte)DataType.RegionArray, 0, 0, 0, 0, 0, 0, 0, 0];
            BinaryPrimitives.WriteInt64BigEndian(buf.AsSpan()[1..], v.LongLength);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
            await WriteFieldName(fieldName);
            foreach(T t in v)
            {
                int ptr = GetObjectPtr(t);
                buf = [0, 0, 0, 0];
                BinaryPrimitives.WriteInt32BigEndian(buf, ptr);
                await stream.WriteAsync(buf);
            }
        }
        internal async Task WriteSimpleFieldDescriptor(byte bytes_len, string fieldName)
        {
            await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)DataType.Simple, bytes_len]));
            await WriteFieldName(fieldName);
        }
        internal async Task WriteSimpleArrayFieldDescriptor(long arr_len, string fieldName)
        {
            byte[] buf = [(byte)DataType.SimpleArray, 0, 0, 0, 0, 0, 0, 0, 0];
            BinaryPrimitives.WriteInt64BigEndian(buf.AsSpan()[1..], arr_len);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
            await WriteFieldName(fieldName);
        }
        public async Task WriteInt(string fieldName, int v)
        {
            await WriteSimpleFieldDescriptor(4, fieldName);
            var buf = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buf, v);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
        }
        public async Task WriteUInt(string fieldName, uint v)
        {
            await WriteSimpleFieldDescriptor(4, fieldName);
            var buf = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buf, v);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
        }
        public async Task WriteFloat(string fieldName, float v)
        {
            await WriteSimpleFieldDescriptor(4, fieldName);
            var buf = new byte[4];
            BinaryPrimitives.WriteSingleBigEndian(buf, v);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
        }
        public async Task WriteDouble(string fieldName, double v)
        {
            await WriteSimpleFieldDescriptor(8, fieldName);
            var buf = new byte[8];
            BinaryPrimitives.WriteDoubleBigEndian(buf, v);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
        }
        public async Task WriteLong(string fieldName, long v)
        {
            await WriteSimpleFieldDescriptor(8, fieldName);
            var buf = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(buf, v);
            await stream.WriteAsync(new ReadOnlyMemory<byte>(buf));
        }
        public async Task WriteBytes(string fieldName, ReadOnlyMemory<byte> v)
        {
            await WriteSimpleArrayFieldDescriptor(v.Length, fieldName);
            await stream.WriteAsync(v);
        }
        public async Task WriteString(string fieldName, string v)
        {
            var bytes = Encoding.UTF8.GetBytes(v);
            await WriteBytes(fieldName, bytes);
        }
        internal async Task Encode()
        {
            while(obj.TryDequeue(out ISerializable? serializable))
            {
                await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)TransitionType.RegionStart]));
                await serializable.WriteToWriter(this);
                currentWorkingObj++;
                await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)DataType.RegionBoundry]));
            }
            await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)TransitionType.StreamEnd]));
        }
    }
}
