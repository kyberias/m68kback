using System.IO;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    class AllStepsTest
    {
        [Test]
        public void VoidTest()
        {
            var source = @"define void @main() #0 { entry: ret void }";
            RunFunction(source, "@main");
        }

        [Test]
        public void OneParameterAndReturnValue()
        {
            var source = @"define i32 @main(i8 %par1) #0 { entry: ret i32 %par1 }";
            var emul = RunFunction(source, "@main", 42);
            Assert.AreEqual(42, emul.Regs[0]);
        }

        [Test]
        public void AddsParametersAndReturnsSum()
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %s = add i32 %par1, %par2
    ret i32 %s 
}";
            var emul = RunFunction(source, "@main", 42, 11);
            Assert.AreEqual(42+11, emul.Regs[0]);
        }

        Emulator RunFunction(string source, string func, params int[] pars)
        {
            var prg = Parse(source);
            var codeGenerator = new CodeGenerator();
            codeGenerator.Visit(prg);

            var emul = new Emulator(codeGenerator.Instructions);
            emul.RunFunction(func, pars);
            return emul;
        }

        Program Parse(string prg)
        {
            var tokenizer = new Tokenizer();
            var elements = tokenizer.Lex(StreamFromString(prg));
            var parser = new Parser(elements);
            return parser.ParseProgram();
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
