using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace m68kback
{
    public class Tokenizer
    {
        Dictionary<string,Token> keywords = new Dictionary<string, Token>
        {
            { "global", Token.Global },
            { "common", Token.Common },
            { "private", Token.Private },
            { "external", Token.External },
            { "target", Token.Target },
            { "datalayout", Token.Datalayout },
            { "triple", Token.Triple },
            { "comdat", Token.Comdat },
            { "constant", Token.Constant },
            { "any", Token.Any },
            { "define", Token.Define },
            { "void", Token.Void },
            { "null", Token.Null },
            { "zeroinitializer", Token.ZeroInitializer },
            { "undef", Token.Undef },
            { "declare", Token.Declare },
            { "call", Token.Call },
            { "tail", Token.Tail },
            { "nonnull", Token.Nonnull },
            { "ret", Token.Ret },
            { "getelementptr", Token.GetElementPtr },
            { "inbounds", Token.Inbounds },
            { "alloca", Token.Alloca },
            { "load", Token.Load },
            { "store", Token.Store },
            { "align", Token.Align },
            { "ashr", Token.Ashr },
            { "add", Token.Add },
            { "sub", Token.Sub },
            { "nsw", Token.Nsw },
            { "nuw", Token.Nuw },
            { "xor", Token.Xor },
            { "and", Token.And },
            { "or", Token.Or },
            { "zext", Token.Zext },
            { "sext", Token.Sext },
            { "i64", Token.I64 },
            { "i32", Token.I32 },
            { "i16", Token.I16 },
            { "i8", Token.I8 },
            { "i1", Token.I1 },
            { "label", Token.Label },
            { "br", Token.Br },
            { "icmp", Token.Icmp },
            { "lshr", Token.Lshr },
            { "eq", Token.Eq },
            { "ne", Token.Ne },
            { "sgt", Token.Sgt },
            { "sge", Token.Sge },
            { "slt", Token.Slt },
            { "mul", Token.Mul },
            { "sdiv", Token.Sdiv },
            { "srem", Token.Srem },
            { "phi", Token.Phi },
            { "bitcast", Token.Bitcast },
            { "trunc", Token.Trunc },
            { "inttoptr", Token.Inttoptr },
            { "to", Token.To },
            { "attributes", Token.Attributes },
            { "nocapture", Token.NoCapture },
            { "readonly", Token.ReadOnly },
            { "type", Token.Type },
            { "true", Token.True },
            { "false", Token.False },
            { "switch", Token.Switch},
            { "ult", Token.Ult},
            { "select", Token.Select }
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
                $"(?<{Token.GlobalIdentifier}>@([a-zA-Z_$\\.][a-zA-Z0-9_$\\.]*|\"(?:\\.|[^\\\"])*\"))",
                $@"(?<{Token.LocalIdentifier}>%[a-zA-Z0-9_$\.]*)",
                $@"(?<{Token.StringLiteral}>c?""(?:\\.|[^\\""])*"")",

                $@"(?<{Token.Symbol}>[a-zA-Z_$][a-zA-Z0-9_$\.]*)",
            };

            sb.Append(string.Join("|", rg));

            sb.Append($"|(?<{Token.Unknown}>[^\\s]+)");

            return new Regex(sb.ToString(), RegexOptions.Multiline);
        }

        public static string Unescape(string s)
        {
            StringBuilder sb = new StringBuilder();

            bool backSlash = false;
            string digits = "";

            foreach (char c in s)
            {
                if (c == '\\')
                {
                    if (backSlash)
                    {
                        backSlash = false;
                        sb.Append('\\');
                        continue;
                    }

                    backSlash = true;
                    digits = "";
                    continue;
                }

                if (backSlash)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        digits += c;
                    }

                    if (digits.Length == 2)
                    {
                        var val = Convert.ToByte(digits, 16);
                        sb.Append(Convert.ToChar(val));
                        backSlash = false;
                    }
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
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

                                if (tokenType == Token.StringLiteral)
                                {
                                    if (mVal.StartsWith("c\""))
                                    {
                                        mVal = mVal.Substring(2, mVal.Length - 3);
                                    }
                                    mVal = Unescape(mVal);
                                }

                                if (tokenType == Token.GlobalIdentifier || tokenType == Token.LocalIdentifier)
                                {
                                    if (mVal[1] == '"')
                                    {
                                        mVal = mVal[0] +  mVal.Substring(2, mVal.Length - 3);
                                    }
                                }

                                yield return new TokenElement(tokenType, mVal);
                            }
                            break;
                        }
                        ix++;
                    }
                }
            }
        }
    }
}