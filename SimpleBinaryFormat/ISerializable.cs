using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBinaryFormat
{
    public interface ISerializable
    {
        public Task WriteToWriter(Writer writer);
        public void FromReader(Region reader);

        public virtual void Apply() { }
    }
}
