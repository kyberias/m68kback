namespace m68kback
{
    public enum Token
    {
        LocalIdentifier, // %foo
        GlobalIdentifier, // @foo
        Global,
        Common,
        To,
        Null,
        Target,
        Datalayout,
        Triple,
        StringLiteral,
        Dollar,
        //At,
        Comdat,
        Comment,
        Align,
        IntegerLiteral,
        //Percentage,
        Define,
        Void,
        Symbol,
        Hash,
        Asterisk,
        CurlyBraceOpen,
        CurlyBraceClose,
        Exclamation,
        Assign,
        Any,
        Sub,
        Constant,
        BracketOpen,
        BracketClose,
        Declare,
        Dot,
        ParenOpen,
        ParenClose,
        Comma,
        Ellipsis,
        Type,
        X,
        Call,
        Tail,
        Ret,
        GetElementPtr,
        Inbounds,
        Colon,
        Alloca,
        Bitcast,
        Trunc,
        Load,
        Store,
        Ashr,
        Add,
        Xor,
        Zext,
        Sext,
        Nsw,
        Nuw,
        And,
        Or,
        I64,
        I32,
        I16,
        I8,
        I1,
        Label,
        Phi,
        Icmp,
        Sgt,
        Slt,
        Mul,
        Sdiv,
        Br,
        Minus,
        Srem,
        Eq,
        Ne,
        Attributes,
        NoCapture,
        ReadOnly,
        True,
        False,
        Switch,
        Undef,
        ZeroInitializer,
        Unknown
    }
}