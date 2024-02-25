using System;
using System.Collections.Generic;

namespace NppDB.Comm
{
    public interface ISQLExecutor
    {
        bool CanExecute();
        void Execute(IList<string> sqlQueries, Action<IList<CommandResult>> callback);
        bool CanStop();
        void Stop();
        ParserResult Parse(string sqlText, CaretPosition caretPosition);
    }
}
