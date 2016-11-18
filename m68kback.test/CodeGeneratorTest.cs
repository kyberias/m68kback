using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    class CodeGeneratorTest
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

        [TestCase(1,2,3, "%par1", ExpectedResult = 1)]
        [TestCase(1, 2, 3, "%par2", ExpectedResult = 2)]
        [TestCase(1, 2, 3, "%par3", ExpectedResult = 3)]
        public int ReturnParameter(int par1, int par2, int par3, string parname)
        {
            var source = $@"define i32 @main(i32 %par1, i32 %par2, i32 %par3) #0 {{ 
entry: 
    ret i32 {parname} 
}}";
            var emul = RunFunction(source, "@main", par1, par2, par3);
            return (int)emul.Regs[0];
        }

        [TestCase(6, 4, ExpectedResult = 6)]
        [TestCase(4, 6, ExpectedResult = 6)]
        [TestCase(1, 2, ExpectedResult = 2)]
        [TestCase(0, 4, ExpectedResult = 4)]
        public int BooleanCmpWithBranch(int par1, int par2)
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %cmp2 = icmp sgt i32 %par1, %par2
    br i1 %cmp2, label %greaterthan, label %notgreaterthan

greaterthan:
    br label %doret

notgreaterthan:
    br label %doret

doret:
    %retval = phi i32 [%par1, %greaterthan], [%par2, %notgreaterthan]
    ret i32 %retval
}";
            var emul = RunFunction(source, "@main", par1, par2);
            return (int)emul.Regs[0];
        }

        [TestCase(1, 2, ExpectedResult = 2)]
        [TestCase(2, 1, ExpectedResult = 2)]
        public int BooleanCmpWithBranchPhi(int par1, int par2)
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %cmp2 = icmp sgt i32 %par1, %par2
    br i1 %cmp2, label %doret, label %other

other:
    br label %doret

doret:
    %retval = phi i32 [%par1, %entry], [%par2, %other]
    ret i32 %retval
}";
            var emul = RunFunction(source, "@main", par1, par2);
            return (int)emul.Regs[0];
        }

        [TestCase(1, 2, ExpectedResult = 1)]
        [TestCase(0, 2, ExpectedResult = 0)]
        public int BooleanCmpWithBranchPhiAndConversionToInt(int par1, int par2)
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %cmp2 = icmp sgt i32 %par1, 0
    br i1 %cmp2, label %doret, label %other

other:
    br label %doret

doret:
    %bval = phi i32 [%cmp2, %entry], [false, %other]
    %retval = zext i1 %bval to i32
    ret i32 %retval
}";
            var emul = RunFunction(source, "@main", par1, par2);
            return (int)emul.Regs[0];
        }


        [Test]
        public void Cmp()
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %b = add i32 %par1, %par2
    %cmp1 = icmp eq i32 %par1, %par2
    %cmp2 = icmp eq i32 %par1, %b
    br label %step2
step2:
    %lnot6 = icmp eq i1 %cmp1, false
    br i1 %lnot6, label %endit, label %step2
endit:

ret i32 %b
}";
            var emul = RunFunction(source, "@main", 11, 42);
            Assert.AreEqual(42 + 11, emul.Regs[0]);
        }


        [Test]
        public void PhiTest()
        {
            var source = @"define i32 @main(i32 %par1) #0 { 
entry: 
    br label %foo
foo:
    %u = phi i32 [2, %foo], [1, %entry]
    %x = phi i32 [%b, %foo], [%par1, %entry]
    %b = add i32 %x, 0
    %i = icmp eq i32 %u, 2
    br i1 %i, label %fooexit, label %foo
fooexit:
    %rr = add i32 %x, %u
    ret i32 %rr
}";
            var emul = RunFunction(source, "@main", 42);
            Assert.AreEqual(42 + 2, emul.Regs[0]);
        }


        [Test]
        public void LenTest()
        {
            var source = @"
@""\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@"" = linkonce_odr unnamed_addr constant [15 x i8] c""len, str=%08X\0A\00"", comdat, align 1
define i32 @len(i8* %str) #0 {
entry:
  %str.addr = alloca i8*, align 4
  %l = alloca i32, align 4
  store i8* %str, i8** %str.addr, align 4
  store i32 0, i32* %l, align 4
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %1 = load i8*, i8** %str.addr, align 4
  %incdec.ptr = getelementptr inbounds i8, i8* %1, i32 1
  store i8* %incdec.ptr, i8** %str.addr, align 4
  %2 = load i8, i8* %1, align 1
  %tobool = icmp ne i8 %2, 0
  br i1 %tobool, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %3 = load i32, i32* %l, align 4
  %inc = add nsw i32 %3, 1
  store i32 %inc, i32* %l, align 4
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %4 = load i32, i32* %l, align 4
  ret i32 %4
}
";
            var emul = RunFunction(source, "@len", "foobarfoobarfoobar1");
            Assert.AreEqual(19, emul.Regs[0]);
        }

        [Test]
        public void Store8()
        {
            var source = @"define i32 @store(i8* %ptr, i32 %par) #0 {
entry:
  store i8 42, i8* %ptr, align 1
  ret i32 %par
}";
            var emul = RunFunction(source, "@store", 100, 66);
            Assert.AreEqual(66, emul.Regs[0]);
            Assert.AreEqual(42, emul.Memory[100]);
        }

        [Test]
        public void Arithmetic()
        {
            int a = 10;
            int b = -20;

            uint c = (uint)(a + b);

            Assert.AreEqual(-10, (int)c);
        }

        [Test]
        public void Reverse()
        {
            var source = @"define i8* @reverse(i8* %from, i8* %to) #0 {
entry:
  %0 = load i8, i8* %from, align 1, !tbaa !1
  %tobool.3.i = icmp eq i8 %0, 0
  br i1 %tobool.3.i, label %len.exit, label %while.body.i.preheader

while.body.i.preheader:                           ; preds = %entry
  br label %while.body.i

while.body.i:                                     ; preds = %while.body.i.preheader, %while.body.i
  %l.05.i = phi i32 [ %inc.i, %while.body.i ], [ 0, %while.body.i.preheader ]
  %str.addr.04.i = phi i8* [ %incdec.ptr.i, %while.body.i ], [ %from, %while.body.i.preheader ]
  %incdec.ptr.i = getelementptr inbounds i8, i8* %str.addr.04.i, i32 1
  %inc.i = add nuw nsw i32 %l.05.i, 1
  %1 = load i8, i8* %incdec.ptr.i, align 1, !tbaa !1
  %tobool.i = icmp eq i8 %1, 0
  br i1 %tobool.i, label %len.exit.loopexit, label %while.body.i

len.exit.loopexit:                                ; preds = %while.body.i
  %inc.i.lcssa = phi i32 [ %inc.i, %while.body.i ]
  br label %len.exit

len.exit:                                         ; preds = %len.exit.loopexit, %entry
  %l.0.lcssa.i = phi i32 [ 0, %entry ], [ %inc.i.lcssa, %len.exit.loopexit ]
  %cmp.33 = icmp sgt i32 %l.0.lcssa.i, 0
  br i1 %cmp.33, label %for.body.lr.ph, label %for.end

for.body.lr.ph:                                   ; preds = %len.exit
  %sub = add i32 %l.0.lcssa.i, -1
  br label %for.body

for.body:                                         ; preds = %for.body, %for.body.lr.ph
  %i.035 = phi i32 [ 0, %for.body.lr.ph ], [ %inc, %for.body ]
  %sub6 = sub i32 %sub, %i.035
  %arrayidx = getelementptr inbounds i8, i8* %from, i32 %sub6
  %2 = load i8, i8* %arrayidx, align 1, !tbaa !1
  %arrayidx7 = getelementptr inbounds i8, i8* %to, i32 %i.035
  store i8 %2, i8* %arrayidx7, align 1, !tbaa !1
  %inc = add nuw nsw i32 %i.035, 1
  %exitcond = icmp eq i32 %inc, %l.0.lcssa.i
  br i1 %exitcond, label %for.end.loopexit, label %for.body

for.end.loopexit:                                 ; preds = %for.body
  br label %for.end

for.end:                                          ; preds = %for.end.loopexit, %len.exit
  %i.0.lcssa = phi i32 [ 0, %len.exit ], [ %l.0.lcssa.i, %for.end.loopexit ]
  %arrayidx10 = getelementptr inbounds i8, i8* %to, i32 %i.0.lcssa
  store i8 0, i8* %arrayidx10, align 1, !tbaa !1
  ret i8* %to
}
";
            var emul = RunFunction(source, "@reverse", "12345", 100);
            Assert.AreEqual(100, emul.Regs[0]);

            var result = Encoding.ASCII.GetString(emul.Memory, 100, 5);

            Assert.AreEqual("54321", result);
        }

        [Test]
        public void PrintfParam()
        {
            var prg = GetFileFromResource("printfparam.ll");
            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("param");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            emul.RunFunction("@main", 2, arrStart);

            Assert.Contains("param\n", printf.PrintedStrings.ToList());
        }

        [Test]
        public void ReversePrg()
        {
            var prg = GetFileFromResource("test2.ll");

            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("reverse");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            emul.RunFunction("@main", 2, arrStart);

            Assert.Contains("reverse is: 'esrever'\n", printf.PrintedStrings.ToList());
        }

        [Test]
        public void PrimesPrg()
        {
            var prg = GetFileFromResource("primes.ll");

            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("42");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            emul.RunFunction("@main", 2, arrStart);
        }

        [Test]
        public void StructsPrg()
        {
            var prg = GetFileFromResource("structs.ll");

            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("42");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            emul.RunFunction("@main", arrStart, 2);

            CollectionAssert.Contains(printf.PrintedStrings, "Testing 100 200 300");
            CollectionAssert.Contains(printf.PrintedStrings, "Foobar 100 200 300");
        }

        [Test]
        public void NotTestPrg()
        {
            var prg = GetFileFromResource("NotTest.ll");

            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("42");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            emul.RunFunction("@main", 2, arrStart);

            CollectionAssert.AreEquivalent(printf.PrintedStrings,
                new [] { "Bitwise Not: -2 -3 2 -6\n", "Boolean Not: 0 1 0 1 0 1\n" });
        }

        string GetFileFromResource(string filename)
        {
            using (
                var stream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("m68kback.test.TestFiles." + filename))
            {
                using (var reader = new StreamReader(stream, Encoding.ASCII))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private PrintfImpl printf;

        Emulator BuildEmulator(string source)
        {
            var prg = Parse(source);
            var codeGenerator = new CodeGenerator();
            codeGenerator.Visit(prg);

            printf = new PrintfImpl();
            return new Emulator(codeGenerator.AllInstructions, codeGenerator.Globals, printf);
        }

        Emulator RunFunction(string source, string func, params object[] pars)
        {
            var emul = BuildEmulator(source);
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
