namespace m68kback
{
    public interface IPrintf
    {
        uint printf(string str, IStackAccess stack);
    }
}
