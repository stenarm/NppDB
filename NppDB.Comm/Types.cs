using System;
using System.Collections.Generic;
using System.Data;

namespace NppDB.Comm
{
    public class ParserMessage
    {
        public ParserMessageType Type { get; set; } = ParserMessageType.NONE;
        public string Text { get; set; } = "";
        public int StartLine { get; set; } = -1;
        public int StartColumn { get; set; } = -1;
        public int StartOffset { get; set; } = -1;
        public int StopLine { get; set; } = -1;
        public int StopColumn { get; set; } = -1;
        public int StopOffset { get; set; } = -1;
        
        public override string ToString()
        {
            return $"{GetType()}(\"{Text}\", Line: {StartLine}, Col: {StartColumn}, {StartOffset}-{StopOffset})";
        }
    }

    public class ParserError : ParserMessage
    {
    }

    public class ParserWarning : ParserMessage
    {
        public int Importance { get; set; } = 0;
    }

    public class ParsedCommand : ParserMessage
    {
        public IList<ParserWarning> Warnings { get; set; } = new List<ParserWarning>();
    }

    public class ParserResult
    {
        public IList<ParsedCommand> Commands { get; set; }
        public int EnclosingCommandIndex { get; set; }
        public IList<ParserError> Errors { get; set; }
        public IList<string> Suggestions { get; set; }
    }

    public class CommandResult
    {
        public string CommandText { get; set; }
        public int RecordsAffected { get; set; }
        public DataTable QueryResult { get; set; }
        public Exception Error { get; set; }
    }

    public class CaretPosition
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int Offset { get; set; }

        public override string ToString()
        {
            return $"{GetType()}(Line={Line}, Column={Column}, Offset={Offset})";
        }
    }
}
