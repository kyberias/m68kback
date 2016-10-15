; ModuleID = 'structs.c'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

%struct.Test = type { i8*, [100 x i8], i32, i32, i32 }

$"\01??_C@_07DNAACEEL@Testing?$AA@" = comdat any

$"\01??_C@_06GCLLLIDK@Foobar?$AA@" = comdat any

$"\01??_C@_0M@NLKJDCPF@?$CFs?5?$CFd?5?$CFd?5?$CFd?$AA@" = comdat any

@"\01??_C@_07DNAACEEL@Testing?$AA@" = linkonce_odr unnamed_addr constant [8 x i8] c"Testing\00", comdat, align 1
@"\01??_C@_06GCLLLIDK@Foobar?$AA@" = linkonce_odr unnamed_addr constant [7 x i8] c"Foobar\00", comdat, align 1
@"\01??_C@_0M@NLKJDCPF@?$CFs?5?$CFd?5?$CFd?5?$CFd?$AA@" = linkonce_odr unnamed_addr constant [12 x i8] c"%s %d %d %d\00", comdat, align 1

; Function Attrs: nounwind
define void @str_copy(i8* %dest, i8* %source) #0 {
entry:
  %source.addr = alloca i8*, align 4
  %dest.addr = alloca i8*, align 4
  store i8* %source, i8** %source.addr, align 4
  store i8* %dest, i8** %dest.addr, align 4
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %0 = load i8*, i8** %source.addr, align 4
  %1 = load i8, i8* %0, align 1
  %tobool = icmp ne i8 %1, 0
  br i1 %tobool, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %2 = load i8*, i8** %source.addr, align 4
  %incdec.ptr = getelementptr inbounds i8, i8* %2, i32 1
  store i8* %incdec.ptr, i8** %source.addr, align 4
  %3 = load i8, i8* %2, align 1
  %4 = load i8*, i8** %dest.addr, align 4
  %incdec.ptr1 = getelementptr inbounds i8, i8* %4, i32 1
  store i8* %incdec.ptr1, i8** %dest.addr, align 4
  store i8 %3, i8* %4, align 1
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %5 = load i8*, i8** %dest.addr, align 4
  store i8 0, i8* %5, align 1
  ret void
}

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** %argv) #0 {
entry:
  %retval = alloca i32, align 4
  %argv.addr = alloca i8**, align 4
  %argc.addr = alloca i32, align 4
  %test = alloca %struct.Test, align 4
  store i32 0, i32* %retval
  store i8** %argv, i8*** %argv.addr, align 4
  store i32 %argc, i32* %argc.addr, align 4
  %ptr = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 0
  store i8* getelementptr inbounds ([8 x i8], [8 x i8]* @"\01??_C@_07DNAACEEL@Testing?$AA@", i32 0, i32 0), i8** %ptr, align 4
  %a = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 2
  store i32 100, i32* %a, align 4
  %b = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 3
  store i32 200, i32* %b, align 4
  %c = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 4
  store i32 300, i32* %c, align 4
  %buf = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 1
  %arraydecay = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  call void @str_copy(i8* %arraydecay, i8* getelementptr inbounds ([7 x i8], [7 x i8]* @"\01??_C@_06GCLLLIDK@Foobar?$AA@", i32 0, i32 0))
  %c1 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 4
  %0 = load i32, i32* %c1, align 4
  %b2 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 3
  %1 = load i32, i32* %b2, align 4
  %a3 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 2
  %2 = load i32, i32* %a3, align 4
  %ptr4 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 0
  %3 = load i8*, i8** %ptr4, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([12 x i8], [12 x i8]* @"\01??_C@_0M@NLKJDCPF@?$CFs?5?$CFd?5?$CFd?5?$CFd?$AA@", i32 0, i32 0), i8* %3, i32 %2, i32 %1, i32 %0)
  %c5 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 4
  %4 = load i32, i32* %c5, align 4
  %b6 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 3
  %5 = load i32, i32* %b6, align 4
  %a7 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 2
  %6 = load i32, i32* %a7, align 4
  %buf8 = getelementptr inbounds %struct.Test, %struct.Test* %test, i32 0, i32 1
  %arraydecay9 = getelementptr inbounds [100 x i8], [100 x i8]* %buf8, i32 0, i32 0
  %call10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([12 x i8], [12 x i8]* @"\01??_C@_0M@NLKJDCPF@?$CFs?5?$CFd?5?$CFd?5?$CFd?$AA@", i32 0, i32 0), i8* %arraydecay9, i32 %6, i32 %5, i32 %4)
  ret i32 0
}

declare i32 @printf(i8*, ...) #1

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
