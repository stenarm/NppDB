using System;
using System.Runtime.InteropServices;

namespace NppDB.Comm
{
    [Guid("56332a51-8621-41f7-a174-ac7c1a045f31")]
    public class ConnectAttr : Attribute
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
