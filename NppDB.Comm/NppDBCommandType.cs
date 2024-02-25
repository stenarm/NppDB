namespace NppDB.Comm
{
    public enum NppDBCommandType
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
