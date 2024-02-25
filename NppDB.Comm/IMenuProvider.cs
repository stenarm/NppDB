using System.Windows.Forms;

namespace NppDB.Comm
{
    public interface IMenuProvider
    {
        ContextMenuStrip GetMenu();
    }
}
