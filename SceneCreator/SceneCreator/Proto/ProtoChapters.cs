using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    public class ProtoChapters
    {
        [ProtoContract]
        public class protoRow
        {
            [ProtoMember(1)]
            public string name { get; set; }
            [ProtoMember(2)]
            public string message { get; set; }
            [ProtoMember(3)]
            public int background { get; set; }
            [ProtoMember(4)]
            public int layer { get; set; }
            [ProtoMember(5)]
            public int sound { get; set; }
            [ProtoMember(6)]
            public bool autojump { get; set; }
            [ProtoMember(7)]
            public int nextscene { get; set; }
            [ProtoMember(8)]
            public Dictionary<int, string> buttonchoice { get; set; }
        }

        public byte[] protoSerialize(Dictionary<int, protoRow> items)
        {
            byte[] result = null;
            try
            {
                using (var stream = new MemoryStream()) { Serializer.SerializeWithLengthPrefix<Dictionary<int, protoRow>>(stream, items, PrefixStyle.Base128, Serializer.ListItemTag); result = stream.ToArray(); }
                return result;
            }
            catch { return null; }

        }

        public Dictionary<int, protoRow> protoDeserialize(byte[] message)
        {
            Dictionary<int, protoRow> item = new Dictionary<int, protoRow>();
            using (var stream = new MemoryStream(message))
            {
                try { item = Serializer.DeserializeWithLengthPrefix<Dictionary<int, protoRow>>(stream, PrefixStyle.Base128, Serializer.ListItemTag); }
                catch { item = null; }
            }
            return item;
        }

    }
}
