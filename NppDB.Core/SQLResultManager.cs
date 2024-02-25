using System;
using System.Collections.Generic;
using System.Linq;
using NppDB.Comm;

namespace NppDB.Core
{
    public class SQLResultManager
    {
        private Dictionary<IntPtr, SQLResult> _bind = new Dictionary<IntPtr, SQLResult>();
        public SQLResult CreateSQLResult(IntPtr id, IDBConnect connect, ISQLExecutor sqlExecutor)
        {
            if (_bind.ContainsKey(id)) 
                throw new ApplicationException("A database connection is already attached to the current document.");
            var ret = _bind[id] = new SQLResult(connect, sqlExecutor) { Visible = false };//Visible = false to prevent Flicker
            return ret;
        }

        public int Count => _bind.Count;
        
        public void Remove(IntPtr id)
        {
            _bind.Remove(id);
        }
        public SQLResult GetSQLResult(IntPtr id)
        {
            return _bind.ContainsKey(id) ? _bind[id] : null;
        }
        public void RemoveSQLResults(IDBConnect connect)
        {
            foreach (var result in _bind.Where(x => x.Value.LinkedDBConnect == connect).Select(x => x.Key).ToList())
            {
                _bind.Remove(result);
            }
        }

        private static SQLResultManager _inst = null;
        public static SQLResultManager Instance
        {
            get
            {
                if (_inst == null) _inst = new SQLResultManager();
                return _inst;
            }
        }
    }
}
