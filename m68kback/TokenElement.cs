namespace m68kback
{
    public class TokenElement
    {
        public Token Type
        {
            get;
            set;
        }
        public string Data { get; set; }

        public TokenElement(Token type, string data = null)
        {
            Type = type;
            Data = data;
        }
    }
}