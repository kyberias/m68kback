using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [TestCase("%ret1", ExpectedResult = 1)]
        [TestCase("%ret2", ExpectedResult = 2)]
        [TestCase("%ret3", ExpectedResult = 3)]
        public int MultipleReturns(string labelname)
        {
            var source = $@"define i32 @main() #0 {{
    br label {labelname} 
ret1: 
    ret i32 1
ret2: 
    ret i32 2
ret3: 
    ret i32 3
}}";
            var emul = RunFunction(source, "@main");

            Assert.IsTrue(emul.Instructions.Count < 20);

            return (int)emul.Regs[0];
        }

        [Test]
        public void GetElementPtr()
        {
            var source = @"
%struct.MsgPort = type { %struct.Node, i8, i8, i8*, %struct.List }
%struct.Node = type { %struct.Node*, %struct.Node*, i8, i8, i8* }
%struct.List = type { %struct.Node*, %struct.Node*, %struct.Node*, i8, i8 }
%struct.Message = type { %struct.Node, %struct.MsgPort*, i16 }

define i32 @main(%struct.Message* %par1) #0 {
  %Code = getelementptr inbounds %struct.Message, %struct.Message* %par1, i32 1, i32 0, i32 1
  %40 = bitcast %struct.Node** %Code to i32
    ret i32 %40
}";
            var emul = RunFunction(source, "@main", 0);

//            Assert.IsTrue(emul.Instructions.Count < 20);

  //          return (int)emul.Regs[0];
        }

        [TestCase(1,2,ExpectedResult = 3)]
        public int GetElementPtrArray(int a, int b)
        {
            var source = @"
@x = common global [2 x i32] zeroinitializer, align 4

define i32 @main(i32 %par1, i32 %par2) #0 {
  store i32 %par1, i32* getelementptr inbounds ([2 x i32], [2 x i32]* @x, i32 0, i32 0), align 4, !tbaa !1
  store i32 %par2, i32* getelementptr inbounds ([2 x i32], [2 x i32]* @x, i32 0, i32 1), align 4, !tbaa !1

  %0 = load i32, i32* getelementptr inbounds ([2 x i32], [2 x i32]* @x, i32 0, i32 0), align 4, !tbaa !1
  %1 = load i32, i32* getelementptr inbounds ([2 x i32], [2 x i32]* @x, i32 0, i32 1), align 4, !tbaa !1

  %res = add i32 %0, %1

    ret i32 %res
}";
            var emul = RunFunction(source, "@main", a, b);
            return (int)emul.Regs[0];
        }

        [TestCase(5, 2, ExpectedResult = 1)]
        [TestCase(10, 5, ExpectedResult = 0)]
        [TestCase(int.MaxValue / 2, 300, ExpectedResult = (int.MaxValue / 2) % 300)]
        [TestCase(int.MaxValue / 9, 100, ExpectedResult = (int.MaxValue / 9) % 100)]
        public int ModTest(int rand, int a)
        {
            var source = @"
define i32 @main(i32 %par1) #0 {

  %r = tail call i32 @rand() #2

  %res = srem i32 %r, %par1

    ret i32 %res
}
declare i32 @rand() #1
";
            var emul = BuildEmulator(source);
            emul.Functions["@rand"] = f => (uint)rand;

            emul.RunFunction("@main", a);

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

        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 2)]
        [TestCase(2, ExpectedResult = 2)]
        public int Icmp(int par1)
        {
            var source = @"define i32 @main(i32 %par1) #0 {   
%tobool = icmp eq i8 %par1, 0
  br i1 %tobool, label %do.cond, label %if.then

do.cond:
  ret i32 1

if.then:
  ret i32 2
}
";
            var emul = RunFunction(source, "@main", par1);
            return (int)emul.Regs[0];
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

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(1, ExpectedResult = 10)]
        [TestCase(2, ExpectedResult = 20)]
        [TestCase(3, ExpectedResult = 30)]
        public int SwitchTest(int arg)
        {
            var source = @"define i32 @main(i32 %arg, i32 %par0, i32 %par1, i32 %par2, i32 %par3, i32 %def) #0 { 
entry: 
  switch i32 %arg, label %deflab [
    i32 0, label %zero
    i32 1, label %one
    i32 2, label %two
    i32 3, label %three
  ]

zero:
    br label %retlab
one:
    br label %retlab
two:
    br label %retlab
three:
    br label %retlab
deflab:
    br label %retlab

retlab:
    %retval = phi i32 [%par0, %zero], [%par1, %one], [%par2, %two], [%par3, %three], [%def, %deflab]
    ret i32 %retval
}";
            var emul = RunFunction(source, "@main", arg, 0, 10, 20, 30, 42);
            return (int)emul.Regs[0];
        }

        [TestCase(1,5,15,42, ExpectedResult = 15)]
        [TestCase(0, 5, 15, 42, ExpectedResult = 5)]
        [TestCase(2, 5, 15, 42, ExpectedResult = 42)]
        public int CmpPhiTest(int arg, int par0, int par1, int def)
        {
            var source = @"define i32 @main(i32 %arg, i32 %par0, i32 %par1, i32 %def) #0 { 
lc0: 
    %c0 = icmp eq i32 %arg, 0
    br i1 %c0, label %r0, label %lc1
r0:
    ret i32 %par0
lc1: 
    %c1 = icmp eq i32 %arg, 1
    br i1 %c1, label %r1, label %deflab
r1:
    ret i32 %par1
deflab:
    ret i32 %def
}";
            var emul = RunFunction(source, "@main", arg, par0, par1, def);
            return (int)emul.Regs[0];
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
        public void Store16()
        {
            var prg = GetFileFromResource("store.ll");

            var emul = BuildEmulator(prg);
            var s = emul.AllocGlobalWord(1);
            emul.AllocGlobalWord(0xFFFF);
            emul.AllocGlobalWord(0x890);
            var d = emul.AllocMemory(2*3);

            emul.RunFunction("@store16", s, d, 3);

            Assert.AreEqual(1, emul.Memory[d]);
            Assert.AreEqual(0, emul.Memory[d+1]);
            Assert.AreEqual(0xff, emul.Memory[d+2]);
            Assert.AreEqual(0xff, emul.Memory[d + 3]);
            Assert.AreEqual(0x90, emul.Memory[d+4]);
            Assert.AreEqual(0x08, emul.Memory[d + 5]);
        }

        [TestCase(100, 4, ExpectedResult = 100 / 4)]
        [TestCase(100, 2, ExpectedResult = 100 / 2)]
        [TestCase(100, 3, ExpectedResult = 100 / 3)]
        [TestCase(6000, 1, ExpectedResult = 6000)]
        public uint SDiv(int a, int b)
        {
            var source = @"define i32 @divtest(i32 %a, i32 %b) #0 {
entry:
  %ret = sdiv i32 %a, %b 
  ret i32 %ret
}";
            var emul = RunFunction(source, "@divtest", a, b);
            return emul.Regs[0];
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
        [Ignore("This doesn't yet work.")]
        public void PrimesPrg()
        {
            var prg = GetFileFromResource("primes.ll");

            var emul = BuildEmulator(prg);
            var par0 = emul.AllocGlobal("program");
            var par1 = emul.AllocGlobal("42");

            var arrStart = emul.AllocGlobal(par0);
            emul.AllocGlobal(par1);

            Console.WriteLine($"argv: {arrStart}");
            Console.WriteLine($"par0: {par0}");
            Console.WriteLine($"par1: {par1}");

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

        [Test]
        public void DuffsDeviceTest()
        {
            var prg = GetFileFromResource("DuffsDevice.ll");

            var emul = BuildEmulator(prg);
            emul.MaximumInstructionsToExecute = 10000;

            emul.RunFunction("@main");

            CollectionAssert.AreEquivalent(new[] { "Sum is 4950\n" }, printf.PrintedStrings);
        }

        [Test]
        public void TestLoop()
        {
            var prg = GetFileFromResource("TestLoop.ll");

            var emul = BuildEmulator(prg);
            emul.MaximumInstructionsToExecute = 10000;

            emul.RunFunction("@main", 1);

            AssertOutputWithResource("TestLoop.reference_output", printf.PrintedStrings);
        }

        void AssertOutputWithResource(string resource, IEnumerable<string> output)
        {
            var res = GetFileFromResource(resource);

            CollectionAssert.AreEquivalent(output.Select(s => s.Replace("\n", "")), res.Split('\n'));
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

        uint atoi(IStackAccess sa)
        {
            var str = sa.GetString(-1);
            return uint.Parse(str);
        }

        Emulator BuildEmulator(string source)
        {
            var prg = Parse(source);
            var codeGenerator = new CodeGenerator(false);
            codeGenerator.Visit(prg);

            printf = new PrintfImpl();
            var emulator = new Emulator(codeGenerator.AllInstructions, codeGenerator.Globals, printf);
            emulator.Functions["@atoi"] = atoi;

            return emulator;
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
