using System;
using System.Collections.Generic;
using System.IO;
using SceneCreator.Proto;

namespace SceneCreator.Utils
{
    public static class FilesWorks
    {
        // For load 
        public enum Type
        {
            Binary,
            Scenes,
            Chapters
        }

        //answer 
        public class result
        {
            public object Value { get; set; }
            public Exception Ex { get; set; }
            public result(object value, Exception exception)
            { Value = value; Ex = exception; }
        }

        #region Save metods
        public static result Save(Dictionary<int, byte[]> items, string path)
        {
            try
            {
                Proto.ProtoBinaryData protoBinary = new Proto.ProtoBinaryData();
                byte[] fileBytes = protoBinary.protoSerialize(items);
                if (File.Exists(path)) { File.Delete(path); }
                using (Stream file = File.OpenWrite(path))
                {
                    file.Write(fileBytes, 0, fileBytes.Length);
                }
                return null;
            }
            catch (Exception ex) { return new result(null, ex); }
        }

        public static result Save(Dictionary<int, ProtoChapter.protoRow> items, string path)
        {
            try
            {
                Proto.ProtoChapter prt = new Proto.ProtoChapter();
                byte[] fileBytes = prt.protoSerialize(items);
                if (File.Exists(path)) { File.Delete(path); }
                using (Stream file = File.OpenWrite(path))
                {
                    file.Write(fileBytes, 0, fileBytes.Length);
                }
                return null;
            }
            catch (Exception ex) { return new result(null, ex); }
        }
        public static result Save(Dictionary<int, Proto.ProtoScene.protoRow> items, string path)
        {
            try
            {
                Proto.ProtoScene prt = new Proto.ProtoScene();
                byte[] fileBytes = prt.protoSerialize(items);
                if (File.Exists(path)) { File.Delete(path); }
                using (Stream file = File.OpenWrite(path))
                {
                    file.Write(fileBytes, 0, fileBytes.Length);
                }
                return null;
            }
            catch (Exception ex) { return new result(null, ex); }
        }
        #endregion

        #region Load metods
        public static result Load(string path, Type type)
        {
            switch (type)
            {
                case Type.Binary: return LoadBinaryData(path);
                case Type.Scenes: return LoadScenes(path);
                case Type.Chapters: return LoadChapters(path);
            }
            return new result(null, new Exception("Неопознанный тип для десериализации"));
        }

        private static result LoadBinaryData(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Proto.ProtoBinaryData binaryData = new Proto.ProtoBinaryData();
                    Dictionary<int, byte[]> items;
                    byte[] fileBytes = File.ReadAllBytes(path);
                    items = binaryData.protoDeserialize(fileBytes);
                    if (items == null) { return new result(null, new Exception("Ошибка сериализации!")); }
                    return new result(items, null);
                }
                else { return new result(null, new Exception($"Файл {path} не найден!")); }
            }
            catch (Exception ex) { return new result(null, ex); }
        }

        private static result LoadScenes(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Proto.ProtoScene prt = new Proto.ProtoScene();
                    byte[] fileBytes = File.ReadAllBytes(path);
                    Dictionary<int, Proto.ProtoScene.protoRow> items = prt.protoDeserialize(fileBytes);
                    if (items == null) { return new result(null, new Exception("Ошибка сериализации!")); }
                    return new result(items, null);
                }
                else { return new result(null, new Exception($"Файл {path} не найден!")); }
            }
            catch (Exception ex) { return new result(null, ex); }
        }

        private static result LoadChapters(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Proto.ProtoChapter prt = new Proto.ProtoChapter();
                    byte[] fileBytes = File.ReadAllBytes(path);
                    Dictionary<int, Proto.ProtoChapter.protoRow> items = prt.protoDeserialize(fileBytes);
                    if (items == null) { return new result(null, new Exception("Ошибка сериализации!")); }
                    return new result(items, null);
                }
                else { return new result(null, new Exception($"Файл {path} не найден!")); }
            }
            catch (Exception ex) { return new result(null, ex); }
        }
        #endregion
    }
}
