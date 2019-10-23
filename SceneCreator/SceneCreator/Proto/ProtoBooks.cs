using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    class ProtoBooks
    {
        [ProtoContract]
        public class protoRow
        {
            [ProtoMember(1)]
            public byte[] Preview { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public int CurrentChapter{ get; set; }
            [ProtoMember(3)]
            public int CurrentPosition { get; set; }
            [ProtoMember(4)]
            public Dictionary<int, byte[]> Chapters { get; set; }
            [ProtoMember(5)]
            public Dictionary<int, byte[]> Backs { get; set; }
            [ProtoMember(6)]
            public Dictionary<int, byte[]> Layers { get; set; }
            [ProtoMember(7)]
            public Dictionary<int, byte[]> Sounds { get; set; }
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
