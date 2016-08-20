using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace m68kback
{
    public enum Token
    {
        LocalIdentifier, // %foo
        GlobalIdentifier, // @foo
        To,
        Target,
        Datalayout,
        Triple,
        StringLiteral,
        Dollar,
        //At,
        Comdat,
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
        X,
        Call,
        Ret,
        GetElementPtr,
        Inbounds,
        Colon,
        Alloca,
        Load,
        Store,
        Add,
        Nsw,
        I32,
        I8,
        I1,
        Label,
        Icmp,
        Sgt,
        Slt,
        Mul,
        Br,
        Minus,
        Srem,
        Eq,
        Ne,
        Bitcast
    }

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

    public class Tokenizer
    {
        Dictionary<string,Token> keywords = new Dictionary<string, Token>
        {
            { "target", Token.Target },
            { "datalayout", Token.Datalayout },
            { "triple", Token.Triple },
            { "comdat", Token.Comdat },
            { "any", Token.Any },
            { "define", Token.Define },
            { "void", Token.Void },
            { "declare", Token.Declare },
            { "call", Token.Call },
            { "ret", Token.Ret },
            { "getelementptr", Token.GetElementPtr },
            { "inbounds", Token.Inbounds },
            { "alloca", Token.Alloca },
            { "load", Token.Load },
            { "store", Token.Store },
            { "align", Token.Align },
            { "add", Token.Add },
            { "sub", Token.Sub },
            { "nsw", Token.Nsw },
            { "i32", Token.I32 },
            { "i8", Token.I8 },
            { "i1", Token.I1 },
            { "label", Token.Label },
            { "br", Token.Br },
            { "icmp", Token.Icmp },
            { "eq", Token.Eq },
            { "ne", Token.Ne },
            { "sgt", Token.Sgt },
            { "slt", Token.Slt },
            { "mul", Token.Mul },
            { "srem", Token.Srem },
            { "bitcast", Token.Bitcast },
            { "to", Token.To },
        };

        public IEnumerable<TokenElement> Lex(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                bool cSeen = false;
                bool inComment = false;
                bool inStringLiteral = false;
                int dotsSeen = 0;

                char? identifierPrefix = null;

                StringBuilder name = new StringBuilder();
                StringBuilder number = new StringBuilder();

                foreach (var c in GetChars(reader))
                {
                    if (inStringLiteral && c != '"')
                    {
                        name.Append(c);
                        continue;
                    }

                    if (c == ';' && !inStringLiteral && !inComment && !identifierPrefix.HasValue)
                    {
                        inComment = true;
                        continue;
                    }

                    if (inComment)
                    {
                        if (c == '\n' || c == '\r')
                        {
                            inComment = false;
                        }
                        continue;
                    }

                    if ((char.IsLetter(c) && (c != 'c' || cSeen)) || c == '_' 
                        || (name.Length > 0 && (char.IsNumber(c) || c == '.' || c == 'c'))
                        || (char.IsNumber(c) && identifierPrefix.HasValue))
                    {
                        if (cSeen)
                        {
                            name.Append('c');
                            cSeen = false;
                        }
                        name.Append(c);
                        continue;
                    }

                    if (char.IsNumber(c) && !identifierPrefix.HasValue)
                    {
                        number.Append(c);
                        continue;
                    }

                    if (number.Length > 0)
                    {
                        yield return new TokenElement(Token.IntegerLiteral, number.ToString());
                        number.Clear();
                    }

                    if (name.Length > 0 && (!inStringLiteral || (inStringLiteral && identifierPrefix.HasValue && c == '"')))
                    {
                        if (identifierPrefix.HasValue)
                        {
                            if (identifierPrefix.Value == '@')
                            {
                                yield return new TokenElement(Token.GlobalIdentifier, "@" + name);
                            }
                            else
                            {
                                yield return new TokenElement(Token.LocalIdentifier, "%" + name);
                            }
                            identifierPrefix = null;
                        }
                        else
                        {
                            var value = name.ToString();
                            if (keywords.ContainsKey(value))
                            {
                                yield return new TokenElement(keywords[value]);
                            }
                            else
                            {
                                yield return new TokenElement(Token.Symbol, name.ToString());
                            }
                        }
                        name.Clear();

                        if (c == '"')
                        {
                            inStringLiteral = false;
                            continue;
                        }
                        //continue;
                    }

                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    switch (c)
                    {
                        case '.':
                            if (dotsSeen < 2)
                            {
                                dotsSeen++;
                                continue;
                            }
                            if (dotsSeen == 2)
                            {
                                yield return new TokenElement(Token.Ellipsis);
                                dotsSeen = 0;
                            }
                            continue;
                        case '"':
                            if (cSeen)
                            {
                                cSeen = false;
                            }
                            if (inStringLiteral)
                            {
                                yield return new TokenElement(Token.StringLiteral, name.ToString());
                                inStringLiteral = false;
                                name.Clear();
                            }
                            else
                            {
                                inStringLiteral = true;
                                name.Clear();
                            }
                            continue;
                        case '@':
                        case '%':
                            identifierPrefix = c;
                            continue;
                        case 'c':
                            cSeen = true;
                            continue;
                        case '*':
                            yield return new TokenElement(Token.Asterisk);
                            break;
                        case '{':
                            yield return new TokenElement(Token.CurlyBraceOpen);
                            break;
                        case '}':
                            yield return new TokenElement(Token.CurlyBraceClose);
                            break;
                        case '[':
                            yield return new TokenElement(Token.BracketOpen);
                            break;
                        case ']':
                            yield return new TokenElement(Token.BracketClose);
                            break;
                        case '(':
                            yield return new TokenElement(Token.ParenOpen);
                            break;
                        case ')':
                            yield return new TokenElement(Token.ParenClose);
                            break;
                        case '!':
                            yield return new TokenElement(Token.Exclamation);
                            continue;
                        case '=':
                            yield return new TokenElement(Token.Assign);
                            break;
                        case '-':
                            yield return new TokenElement(Token.Minus);
                            break;
                        case ',':
                            yield return new TokenElement(Token.Comma);
                            break;
                        case '#':
                            yield return new TokenElement(Token.Hash);
                            break;
                        case ':':
                            yield return new TokenElement(Token.Colon);
                            break;
                        default:
                            throw new Exception("Unknown token " + c);
                    }

                    if (dotsSeen > 0)
                    {
                        throw new Exception("Unexpected token '.'");
                    }
                }
            }
        }

        IEnumerable<char> GetChars(StreamReader reader)
        {
            int lineCount = 1;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                line += "\n";

                foreach (var c in line)
                {
                    yield return c;
                }
                lineCount++;
            }
        }
    }
}