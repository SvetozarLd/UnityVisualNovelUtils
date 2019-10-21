using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    public class ProtoImages
    {
        [ProtoContract]
        public class protoRow
        {
            [ProtoMember(1)]
            public Dictionary<int, byte[]> images { get; set; }
        }

        public byte[] protoSerialize(Dictionary<int, byte[]> items)
        {
            byte[] result = null;
            try
            {
                using (var stream = new MemoryStream()) { Serializer.SerializeWithLengthPrefix<Dictionary<int, byte[]>>(stream, items, PrefixStyle.Base128, Serializer.ListItemTag); result = stream.ToArray(); }
                return result;
            }
            catch { return null; }

        }

        public Dictionary<int, byte[]> protoDeserialize(byte[] message)
        {
            Dictionary<int, byte[]> item = new Dictionary<int, byte[]>();
            using (var stream = new MemoryStream(message))
            {
                try { item = Serializer.DeserializeWithLengthPrefix<Dictionary<int, byte[]>>(stream, PrefixStyle.Base128, Serializer.ListItemTag); }
                catch { item = null; }
            }
            return item;
        }
    }
}

