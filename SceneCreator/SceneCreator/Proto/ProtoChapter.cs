using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    public class ProtoChapter
    {
        [ProtoContract]
        public class protoRow
        {
            [ProtoMember(1)]
            public byte[] Preview { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string SceneFileName { get; set; }
            [ProtoMember(4)]
            public int SceneUid { get; set; }
            [ProtoMember(5)]
            public int OrderPosition { get; set; }
            [ProtoMember(6)]
            public Dictionary<int, Proto.ProtoScene.protoRow> Scenes { get; set; }
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