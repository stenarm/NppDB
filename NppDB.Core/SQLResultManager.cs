using System;
using System.Collections.Generic;
using System.Linq;
using NppDB.Comm;

namespace NppDB.Core
{
    public class SQLResultManager
    {
        private Dictionary<IntPtr, SqlResult> _bind = new Dictionary<IntPtr, SqlResult>();
        public SqlResult CreateSQLResult(IntPtr id, IDbConnect connect, ISqlExecutor sqlExecutor)
        {
            if (_bind.ContainsKey(id)) 
                throw new ApplicationException("A database connection is already attached to the current document.");
            var ret = _bind[id] = new SqlResult(connect, sqlExecutor) { Visible = false };//Visible = false to prevent Flicker
            return ret;
        }

        public int Count => _bind.Count;
        
        public void Remove(IntPtr id)
        {
            _bind.Remove(id);
        }
        public SqlResult GetSQLResult(IntPtr id)
        {
            return _bind.ContainsKey(id) ? _bind[id] : null;
        }
        public void RemoveSQLResults(IDbConnect connect)
        {
            foreach (var result in _bind.Where(x => x.Value.LinkedDbConnect == connect).Select(x => x.Key).ToList())
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
