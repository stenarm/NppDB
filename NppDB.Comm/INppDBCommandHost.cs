namespace NppDB.Comm
{
    public interface INppDbCommandHost
    {
        object Execute(NppDBCommandType type, object[] parameters);
    }
}
