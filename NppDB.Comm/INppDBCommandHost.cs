namespace NppDB.Comm
{
    public interface INppDBCommandHost
    {
        object Execute(NppDBCommandType type, object[] parameters);
    }
}
