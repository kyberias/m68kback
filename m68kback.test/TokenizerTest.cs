using System.IO;
using System.Linq;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    class TokenizerTest
    {
        [Test]
        public void Tokens()
        {
            var source = @"add 123 sub $ ! = # : ""hello there"" ""string literal"" @goo.doo @""yeah joo"" ... . ...";
            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(source));

            CollectionAssert.AreEqual(new[]
            {
                Token.Add,
                Token.IntegerLiteral,
                Token.Sub,
                Token.Dollar,
                Token.Exclamation,
                Token.Assign,
                Token.Hash,
                Token.Colon,
                Token.StringLiteral,
                Token.StringLiteral,
                Token.GlobalIdentifier,
                Token.GlobalIdentifier,
                Token.Ellipsis,
                Token.Dot,
                Token.Ellipsis,
            }, elements.Select(t => t.Type));
        }

        [Test]
        public void Comments()
        {
            var source = @"; comments
1234 ; comments
foobar";
            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(source));

            CollectionAssert.AreEqual(new[]
            {
                Token.IntegerLiteral,
                Token.Symbol,
            }, elements.Select(t => t.Type));
        }

        [TestCase("len, str=%08X\\0A\\00", ExpectedResult = "len, str=%08X\x0A\x00")]
        public string Unescape(string source)
        {
            return Tokenizer.Unescape(source);
        }

        [Test]
        public void StringLiterals()
        {
            var source = @"""\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@"" = 1 $""\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@"" c""len, str=%08X\0A\00""";
            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(source)).ToList();

            CollectionAssert.AreEqual(new[]
            {
                Token.StringLiteral,
                Token.Assign,
                Token.IntegerLiteral,
                Token.Dollar,
                Token.StringLiteral,
                Token.StringLiteral,
            }, elements.Select(t => t.Type));

            Assert.AreEqual("\"\x01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@\"", elements.First().Data);
            Assert.AreEqual("len, str=%08X\x0A\x00", elements.Last().Data);
        }

        [Test]
        public void A()
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry.foobar: 
    %0 = add i32 0, 1
    %c.a = add i32 0, 2
}";

            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(source));

            CollectionAssert.AreEqual(new []
            {
                Token.Define,
                Token.I32,
                Token.GlobalIdentifier,
                Token.ParenOpen,
                Token.I32,
                Token.LocalIdentifier,
                Token.Comma,
                Token.I32,
                Token.LocalIdentifier,
                Token.ParenClose,
                Token.Hash,
                Token.IntegerLiteral,
                Token.CurlyBraceOpen,
                Token.Symbol,
                Token.Colon,
                Token.LocalIdentifier,
                Token.Assign,
                Token.Add,
                Token.I32,
                Token.IntegerLiteral,
                Token.Comma,
                Token.IntegerLiteral,
                Token.LocalIdentifier,
                Token.Assign,
                Token.Add,
                Token.I32,
                Token.IntegerLiteral,
                Token.Comma,
                Token.IntegerLiteral,
                Token.CurlyBraceClose

            }, elements.Select(t => t.Type));
        }

        public Stream StreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}
