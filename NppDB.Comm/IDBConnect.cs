namespace NppDB.Comm
{
    public interface IDbConnect
    {
        void Reset();
        string Account { get; set; }
        string GetDefaultTitle();
        bool CheckLogin();
        void Connect();
        void Attach();
        string ConnectAndAttach();
        void Disconnect();
        string Title { get; set; }
        string Password { get; set; }
        void Refresh();
        string ServerAddress { get; set; }
        bool IsOpened { get; }
        string DatabaseSystemName { get; }
    }
}
