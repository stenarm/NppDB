namespace NppDB.Comm
{
    public enum NppDbCommandType
    {
        NEW_FILE,
        CREATE_RESULT_VIEW,
        DESTROY_RESULT_VIEW,
        GET_ACTIVATED_BUFFER_ID,
        EXECUTE_SQL,
        APPEND_TO_CURRENT_VIEW,
        ACTIVATE_BUFFER,
        GET_ATTACHED_BUFFER_ID,
        OPEN_FILE_IN_NPP,
        GET_PLUGIN_DIRECTORY
    }
}
