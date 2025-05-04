﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace NppDB.Core
{
    [XmlRoot("Options")]
    public class Options : ICollection<Option>
    {
        private Dictionary<string, Option> _opts = new Dictionary<string, Option>();

        private Options()
        {
            _opts["forcetrans"] = new Option { Name = "forcetrans", Value = false };
        }

        public Option this[string name]
        {
            get { return _opts[name]; }
            set { _opts[name] = value; }
        }

        public void Add(Option option)
        {
            _opts[option.Name] = option;
        }

        public void LoadFromXml(string path)
        {
            if (!File.Exists(path)) throw new ApplicationException("file not find : " + path); ;
            using (var fs = File.OpenRead(path))
            {
                var xr = XmlReader.Create(fs);
                XmlSerializer xs = new XmlSerializer(typeof(Options));
                var opts = xs.Deserialize(xr) as Options;
                if (opts == null) return;
                _options = this;
            }
        }

        public void SaveToXml(string path)
        {
            using (var fw = File.Open(path, FileMode.Create))
            {
                XmlSerializer xs = null;
                try
                {
                    xs = new XmlSerializer(typeof(Options));
                    xs.Serialize(fw, this);
                }
                finally
                {
                    fw.Flush();
                    fw.Close();
                }
            }
        }


        public void Clear()
        {
            _opts.Clear();
        }

        public bool Contains(Option item)
        {
            return _opts.ContainsKey(item.Name);
        }

        public void CopyTo(Option[] array, int arrayIndex)
        {
            _opts.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _opts.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Option item)
        {
            return _opts.Remove(item.Name);
        }

        public IEnumerator<Option> GetEnumerator()
        {
            return _opts.Values.GetEnumerator(); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static Options _options;
        public static Options Instance
        {
            get
            {
                if (_options == null) _options = new Options();
                return _options;
            }
        }

    }

    public class Option
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
