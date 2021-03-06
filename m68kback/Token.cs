namespace m68kback
{
    public enum Token
    {
        LocalIdentifier, // %foo
        GlobalIdentifier, // @foo
        Global,
        Common,
        Private,
        External,
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
        Nonnull,
        Ret,
        GetElementPtr,
        Inbounds,
        Colon,
        Alloca,
        Bitcast,
        Inttoptr,
        Trunc,
        Load,
        Store,
        Ashr,
        Lshr,
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
        Select,
        Icmp,
        Sgt,
        Sge,
        Slt,
        Ult,
        Mul,
        Sdiv,
        Br,
        Minus,
        Srem,
        Eq,
        Ne,
        Attributes,
        NoCapture,
        NoAlias,
        ReadOnly,
        True,
        False,
        Switch,
        Undef,
        ZeroInitializer,
        LocalUnnamedAddr,
        Internal,
        Unreachable,
        PtrToInt,
        WriteOnly,
        Opaque,
        ZeroExt,
        Byval,
        Unknown
    }
}