using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    class ProtoSC
    {
        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class protoDic
        {
            [ProtoMember(1, DataFormat = DataFormat.Group)]
            public Dictionary<int, protoRow> items { get; set; }
        }

        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class protoRow
        {
            [ProtoMember(1)]
            public string Name { get; set; } // имя кто говорит
            [ProtoMember(2)]
            public string Message { get; set; } // сообщение которое говорят
            [ProtoMember(3)]
            public int Background { get; set; } // номер картинки фона
            [ProtoMember(4)]
            public int Layer { get; set; } // номер картинки слоя
            [ProtoMember(5)]
            public int Sound { get; set; } // номер музыки
            [ProtoMember(6)]
            public bool AutoJump { get; set; } // флаг автоперехода (не ждём тапа по экрану, переходим сами через AutoJumpTimer секунд) - действует только если в списке ButtonChoice только 1 итем 
            [ProtoMember(7)]
            public double AutoJumpTimer { get; set; } // таймер автоперехода (не ждём тапа по экрану, переходим через AutoJumpTimer секунд) - действует только если в списке ButtonChoice только 1 итем 
            [ProtoMember(8, DataFormat = DataFormat.Group)]
            public List<protoСhoice> ButtonChoice { get; set; } // список выборов перехода
        }
        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class protoСhoice
        {
            [ProtoMember(10)]
            public string ButtonText { get; set; } // текст кнопки (действует, если в ButtonChoice больше 1 пункта)
            [ProtoMember(11)]
            public int NextScene { get; set; } // номер(адрес) следующей сцены
            [ProtoMember(12)]
            public int Price { get; set; } // стоимость  перехода на следующую сцену - при стоимость 0, не показывается на кнопке.
        }



        public byte[] protoSerializeDic(protoDic items)
        {
            byte[] result = null;
            try
            {
                using (var stream = new MemoryStream()) { Serializer.SerializeWithLengthPrefix<protoDic>(stream, items, PrefixStyle.Base128, Serializer.ListItemTag); result = stream.ToArray(); }
                return result;
            }
            catch { return null; }

        }

        public protoDic protoDeserializeDic(byte[] message)
        {
            protoDic item = new protoDic();
            using (var stream = new MemoryStream(message))
            {
                try { item = Serializer.DeserializeWithLengthPrefix<protoDic>(stream, PrefixStyle.Base128, Serializer.ListItemTag); }
                catch { item = null; }
            }
            return item;
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
