namespace m68kback
{
    public interface IStackAccess
    {
        string GetString(int ix);
        uint GetUint(int ix);
        int GetInt(int ix);
    }
}