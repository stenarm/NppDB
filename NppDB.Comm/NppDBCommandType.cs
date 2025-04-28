namespace NppDB.Comm
{
    public enum NppDbCommandType
    {
        NewFile,
        CreateResultView,
        DestroyResultView,
        GetActivatedBufferID,
        ExecuteSQL,
        AppendToCurrentView,
        ActivateBuffer,
        GetAttachedBufferID
    }
}
