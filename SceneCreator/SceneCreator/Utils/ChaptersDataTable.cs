﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SceneCreator.Utils
{

    public static class ChaptersDataTable
    {
        public class result
        {
            public DataTable dt { get; set; }
            public Exception ex { get; set; }
            public result(DataTable dataTable, Exception exception)
            {
                ex = exception;
                if (dataTable != null) { dt = dataTable; }

            }
        }
        private static DataTable dt = null;

        public static Exception Initialization()
        {
            try
            {
                dt = new DataTable();
                dt.Columns.Add("uid", typeof(int));
                dt.Columns.Add("text", typeof(string));
                dt.Columns.Add("trans", typeof(string));
                //dt.Columns.Add("preview", typeof(byte[]));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static result ConvertFromProto(Dictionary<int, Proto.ProtoScene.protoRow> protoChapters)
        {
            if (dt == null) { Exception ex = Initialization(); if (ex != null) { return new result(null, ex); } }
            if (protoChapters == null) { return new result(null, new Exception("Dictionary<int,Proto.ProtoChapters> protoChapters is NULL!")); }
            if (dt.Rows.Count > 0) { dt.Rows.Clear(); }
            try
            {
                foreach (KeyValuePair<int, Proto.ProtoScene.protoRow> item in protoChapters)
                {
                    Exception ex = AddNewRow(item).ex;
                    if (ex != null) { return new result(null, ex); }
                }
                return new result(dt, null);
            }
            catch (Exception ex)
            {
                return new result(null, ex);
            }
        }
        public static result AddNewRow(KeyValuePair<int, Proto.ProtoScene.protoRow> item)
        {
            if (dt == null) { Exception ex = Initialization(); if (ex != null) { return new result(null, ex); } }
            try
            {
                string tmp = string.Empty;
                DataRow row = dt.NewRow();
                row["uid"] = item.Key;
                if (item.Value.Message.Length > 20)
                {
                    tmp = string.Concat(item.Value.Message.Take(20)) + "...";
                }
                else { tmp = item.Value.Message; }

                if (!string.IsNullOrEmpty(item.Value.Name) || !string.IsNullOrWhiteSpace(item.Value.Name))
                {
                    tmp = item.Value.Name + ": " + tmp;
                }
                row["text"] = tmp;
                tmp = string.Empty;
                if (item.Value.ButtonChoice != null)
                {
                    foreach (Proto.ProtoScene.protoСhoice trans in item.Value.ButtonChoice)
                    {
                        tmp += trans.NextScene + ";";
                    }
                }
                row["trans"] = tmp;
                dt.Rows.Add(row);
                return new result(null, null);
            }
            catch (Exception ex)
            {
                return new result(null, ex);
            }
        }
        public static result UpdateRow(KeyValuePair<int, Proto.ProtoScene.protoRow> item)
        {
            if (dt == null) { Exception ex = Initialization(); if (ex != null) { return new result(null, ex); } }
            try
            {
                string tmp = string.Empty;
                DataRow row = dt.Select("uid =" + item.Key).Single();
                row["uid"] = item.Key;
                if (item.Value.Message.Length > 20)
                {
                    tmp = string.Concat(item.Value.Message.Take(20)) + "...";
                }
                else { tmp = item.Value.Message; }

                if (!string.IsNullOrEmpty(item.Value.Name) || !string.IsNullOrWhiteSpace(item.Value.Name))
                {
                    tmp = item.Value.Name + ": " + tmp;
                }
                row["text"] = tmp;
                tmp = string.Empty;
                if (item.Value.ButtonChoice != null)
                {
                    foreach (Proto.ProtoScene.protoСhoice trans in item.Value.ButtonChoice)
                    {
                        tmp += trans.NextScene + ";";
                    }
                }
                row["trans"] = tmp;
                return new result(null, null);
            }
            catch (Exception ex)
            {
                return new result(null, ex);
            }
        }
        public static result DeleteRow(int uid)
        {
            if (dt == null) { Exception ex = Initialization(); if (ex != null) { return new result(null, ex); } }
            try
            {

                DataRow row = dt.Select("uid =" + uid).Single();
                row.Delete();
                return new result(null, null);
            }
            catch (Exception ex)
            {
                return new result(null, ex);
            }

        }
    }
}
