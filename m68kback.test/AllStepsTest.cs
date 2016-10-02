﻿using System.IO;
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

        [Test]
        public void PhiTest()
        {
            var source = @"define i32 @main(i32 %par1, i32 %par2) #0 { 
entry: 
    %ten = add i32 %par1, %par2
    br label %foo
foo:
    %u = phi i32 [%b, %foo], [%ten, %entry]
    %x = phi i32 [%r, %foo], [0, %entry]
    %r = add i32 %x, 1
    %b = add i32 %u, 1
    %i = icmp eq i32 %b, 20
    br i1 %i, label %fooexit, label %foo
fooexit:
    %rr = add i32 %u, %r
    ret i32 %rr 
}";
            var emul = RunFunction(source, "@main", 5, 5);
            Assert.AreEqual(30, emul.Regs[0]);
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
  %0 = load i8*, i8** %str.addr, align 4
;  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @""\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@"", i32 0, i32 0), i8* %0)
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
            var emul = RunFunction(source, "@len", "foobarfoobarfoobar");
            Assert.AreEqual(18, emul.Regs[0]);
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
            var emul = RunFunction(source, "@reverse", "12345");
            Assert.AreEqual(18, emul.Regs[0]);
        }

        Emulator RunFunction(string source, string func, params object[] pars)
        {
            var prg = Parse(source);
            var codeGenerator = new CodeGenerator();
            codeGenerator.Visit(prg);

            var emul = new Emulator(codeGenerator.Instructions, codeGenerator.Globals);
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
