using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        X,
        Call,
        Tail,
        Ret,
        GetElementPtr,
        Inbounds,
        Colon,
        Alloca,
        Load,
        Store,
        Add,
        Nsw,
        Nuw,
        I64,
        I32,
        I8,
        I1,
        Label,
        Phi,
        Icmp,
        Sgt,
        Slt,
        Mul,
        Br,
        Minus,
        Srem,
        Eq,
        Ne,
        Bitcast,
        Attributes,
        NoCapture,
        ReadOnly,
        Unknown
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
        private int line = 0;

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
            { "tail", Token.Tail },
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
            { "nuw", Token.Nuw },
            { "i64", Token.I64 },
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
            { "phi", Token.Phi },
            { "bitcast", Token.Bitcast },
            { "to", Token.To },
            { "attributes", Token.Attributes },
            { "nocapture", Token.NoCapture },
            { "readonly", Token.ReadOnly }
        };

        Regex CreateRegex()
        {
            var sb = new StringBuilder();

            var rg = new[]
            {
                $"(?<{Token.Comment}>;.*$)",
                $"(?<{Token.IntegerLiteral}>[0-9]+)",
                $@"(?<{Token.Dollar}>\$)",
                $@"(?<{Token.Asterisk}>\*)",
                $@"(?<{Token.CurlyBraceOpen}>\{{)",
                $@"(?<{Token.CurlyBraceClose}>\}})",
                $@"(?<{Token.BracketOpen}>\[)",
                $@"(?<{Token.BracketClose}>\])",
                $@"(?<{Token.ParenOpen}>\()",
                $@"(?<{Token.ParenClose}>\))",
                $@"(?<{Token.Exclamation}>\!)",
                $@"(?<{Token.Assign}>\=)",
                $@"(?<{Token.Minus}>-)",
                $@"(?<{Token.Comma}>,)",
                $@"(?<{Token.Hash}>#)",
                $@"(?<{Token.Colon}>:)",
                $@"(?<{Token.Ellipsis}>\.\.\.)",
                $@"(?<{Token.Dot}>\.)",

                //$@"(?<{Token.GlobalIdentifier}>@([a-zA-Z_$][a-zA-Z0-9_$]*|""(?:\\.|[^\\""])*""))",
                $"(?<{Token.GlobalIdentifier}>@([a-zA-Z_$][a-zA-Z0-9_$\\.]*|\"(?:\\.|[^\\\"])*\"))",
                $@"(?<{Token.LocalIdentifier}>%[a-zA-Z0-9_$\.]*)",
                $@"(?<{Token.StringLiteral}>c?""(?:\\.|[^\\""])*"")",

                $@"(?<{Token.Symbol}>[a-zA-Z_$][a-zA-Z0-9_$\.]*)",
            };

            sb.Append(string.Join("|", /*keywords.Select(kw => $"(?<{kw.Value}>{kw.Key})")*/ /*.Union(*/rg/*)*/ ));

            sb.Append($"|(?<{Token.Unknown}>[^\\s]+)");

            return new Regex(sb.ToString(), RegexOptions.Multiline);
        }

        public IEnumerable<TokenElement> Lex(Stream stream)
        {
            var regex = CreateRegex();

            using (var r = new StreamReader(stream))
            {
                var str = r.ReadToEnd();
                var matches = regex.Matches(str);

                foreach (Match match in matches)
                {
                    int ix = 0;
                    foreach (Group g in match.Groups)
                    {
                        var mVal = g.Value;
                        if (g.Success && ix > 1)
                        {
                            var groupName = regex.GroupNameFromNumber(ix);
                            var tokenType = (Token)Enum.Parse(typeof(Token), groupName);
                            if (tokenType != Token.Comment)
                            {
                                if (tokenType == Token.Symbol)
                                {
                                    if (keywords.ContainsKey(mVal))
                                    {
                                        yield return new TokenElement(keywords[mVal]);
                                        break;
                                    }
                                }

                                if (tokenType == Token.Unknown)
                                {
                                    Console.WriteLine(mVal);
                                }
                                yield return new TokenElement(tokenType, mVal);
                            }
                            break;
                        }
                        ix++;
                    }
                }
            }

            yield break;

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
                    if (cSeen)
                    {
                        name.Append('c');
                        cSeen = false;
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

                    if (c == '\n')
                    {
                        line++;
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
                        case '$':
                            yield return new TokenElement(Token.Dollar);
                            break;
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