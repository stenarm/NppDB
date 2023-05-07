using System;
using System.Collections.Generic;

namespace NppDB.Comm
{
    public interface ISQLExecutor
    {
        bool CanExecute();
        void Execute(IList<string> sql, Action<IList<CommandResult>> callback);
        bool CanStop();
        void Stop();
        ParserResult Parse(string sql, CaretPosition caretPosition, bool includeSuggestions);
    }
}
