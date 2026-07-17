using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBinaryFormat
{
    public class Container : ISerializable
    {
        private event Func<Writer, Task>? Write;
        private event Action<Region>? Read;
        public Container() { }
        public Container OnWrite(Func<Writer, Task> write) {
            Write += write;
            return this;
        }

        public Container OnRead(Action<Region> read)
        {
            Read += read;
            return this;
        }

        async Task ISerializable.WriteToWriter(Writer writer)
        {
            if(Write == null)
            {
                throw new InvalidOperationException("Tried to write container with no defined write");
            }
            await Write(writer);
        }

        void ISerializable.FromReader(Region reader)
        {
            if (Read == null)
            {
                throw new InvalidOperationException("Tried to read container with no defined read");
            }
            Read(reader);
        }
    }
}
