﻿using System.Collections.Generic;
using ProtoBuf;
using System.IO;

namespace SceneCreator.Proto
{
    public class ProtoScene
    {
        [ProtoContract]
        public class protoRow
        {
            [ProtoMember(11)]
            public string Name { get; set; } // имя кто говорит
            [ProtoMember(12)]
            public string Message { get; set; } // сообщение которое говорят
            [ProtoMember(13)]
            public int Background { get; set; } // номер картинки фона
            [ProtoMember(14)]
            public int Layer { get; set; } // номер картинки слоя
            [ProtoMember(15)]
            public int Sound { get; set; } // номер музыки
            [ProtoMember(16)]
            public bool AutoJump { get; set; } // флаг автоперехода (не ждём тапа по экрану, переходим сами через AutoJumpTimer секунд) - действует только если в списке ButtonChoice только 1 итем 
            [ProtoMember(17)]
            public double AutoJumpTimer { get; set; } // таймер автоперехода (не ждём тапа по экрану, переходим через AutoJumpTimer секунд) - действует только если в списке ButtonChoice только 1 итем 
            [ProtoMember(18, DataFormat = DataFormat.Group)]
            public List<protoСhoice> ButtonChoice { get; set; } // список выборов перехода
        }
        [ProtoContract]
        public class protoСhoice
        {
            [ProtoMember(19)]
            public string ButtonText { get; set; } // текст кнопки (действует, если в ButtonChoice больше 1 пункта)
            [ProtoMember(20)]
            public int NextScene { get; set; } // номер(адрес) следующей сцены
            [ProtoMember(21)]
            public int Price { get; set; } // стоимость  перехода на следующую сцену - при стоимость 0, не показывается на кнопке.
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
