using System.IO;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    public class ParserTest
    {
        [TestCase("@\"\\01??_C@_0BC@FFDLKEBL@intuition?4library?$AA@\" = linkonce_odr unnamed_addr constant [18 x i8] c\"intuition.library\\00\", comdat, align 1")]
        public void GlobalVarDeclrs(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define void @main() #0 { }")]
        [TestCase("define i32 @main() #0 { }")]
        [TestCase("define i8 @main() #0 { }")]
        [TestCase("define i8* @reverse(i8* %from, i8* %to) #0 { }")]
        [TestCase("define i32 @main(i32 %argc, i8** %argv) #0 {}")]
        public void FunctionDeclr(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { ret i32 42 }")]
        [TestCase("define i32 @main() #0 { ret i32 %local }")]
        [TestCase("define i32 @main() #0 { ret void }")]
        public void RetInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { %cmp = icmp slt i32 %0, 10 }")]
        [TestCase("define i32 @main() #0 { %cmp3 = icmp sgt i32 %3, 0 }")]
        [TestCase("define i32 @main() #0 { %cmp1 = icmp slt i32 %3, %4 }")]
        public void IcmpInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        // %4 = load i32, i32* %u, align 4
        [TestCase("define i32 @main() #0 { %4 = load i32, i32* %u, align 4 }")]
        [TestCase("define i32 @main() #0 { %4 = load i32, i32* %u }")]
        public void LoadInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { %4 = mul nsw i32 %4, %5 }")]
        public void MulInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { %4 = add nsw i32 %2, 1 }")]
        public void AddInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { store i32 0, i32* %retval }")]
        [TestCase("define i32 @main() #0 { store i32 0, i32* %i, align 4 }")]
        public void StoreInstruction(string prg)
        {
            Assert.IsTrue(Parse(prg).Functions[0].Statements[0] is StoreStatement);
        }

        [TestCase("define i32 @main() #0 { call void @PrintNumber(i32 %mul) }")]
        [TestCase("define i32 @main() #0 { %retval = call i32 @FoobarFunc() }")]
        [TestCase("define i32 @main() #0 { %call = call i8* @OpenLibrary(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @\"\\01??_C@_0BC@FFDLKEBL@intuition?4library?$AA@\", i32 0, i32 0), i32 0) }")]
        [TestCase("define i32 @main() #0 { %call5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([13 x i8], [13 x i8]* @\"\\01??_C@_0N@GIINEEDM@hello?5world?6?$AA@\", i32 0, i32 0))\n }")]
        [TestCase("define i32 @main() #0 { %call1 = call i32 bitcast (i32 (...)* @atoi to i32 (i8*)*)(i8* %3) }")]
        public void CallInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg).Functions[0].Statements[0]);
        }

        [TestCase("define i32 @main() #0 { br label %for.cond }")]
        [TestCase("define i32 @main() #0 { br i1 %cmp, label %for.body, label %for.end }")]
        public void BrInstruction(string prg)
        {
            Assert.IsNotNull(Parse(prg).Functions[0].Statements[0]);
        }

        [TestCase("define i32 @main() #0 { %retval = alloca i32, align 4 }")]
        public void VariableDeclaration(string prg)
        {
            Assert.IsTrue(Parse(prg).Functions[0].Statements[0] is VariableAssignmentStatement);
        }

        [TestCase("define i32 @main() #0 { %retval = alloca i32, align 4 } ; comment end of line")]
        [TestCase("; whole line comment define i32 @main() #0 { %retval = alloca i32, align 4 } ; comment end of line")]
        public void Comments(string prg)
        {
            Assert.IsNotNull(Parse(prg));
        }

        [TestCase("define i32 @main() #0 { %arrayidx = getelementptr inbounds [2 x i8], [2 x i8]* %tmp, i32 0, i32 0 }")]
        [TestCase("define i32 @main() #0 { %arrayidx = getelementptr inbounds i8, i8* %5, i32 %sub1 }")]
        public void GetElementPtr(string prg)
        {
            Assert.IsTrue(Parse(prg).Functions[0].Statements[0] is VariableAssignmentStatement);
        }

        [TestCase("define i32 @main() #0 { eka: %retval = alloca i32, align 4 }", 0, "eka")]
        [TestCase("define i32 @main() #0 { eka: %retval = alloca i32, align 4 toka: ret i32 42 }", 0, "eka")]
        [TestCase("define i32 @main() #0 { eka: %retval = alloca i32, align 4 for.cond: ret i32 42 }", 1, "for.cond")]
        public void StatementCanHaveLabels(string prg, int labelindex, string label)
        {
            Assert.AreEqual(label, Parse(prg).Functions[0].Statements[labelindex].Label);
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

        Program Parse(string prg)
        {
            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(prg));
            var parser = new Parser(elements);
            return parser.ParseProgram();
        }
    }
}
