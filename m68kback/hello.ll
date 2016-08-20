; ModuleID = 'hello.c'
;target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
;target triple = "i686-pc-windows-msvc18.0.0"

;$"\01??_C@_03PMGGPEJJ@?$CFd?6?$AA@" = comdat any

@"\01??_C@_03PMGGPEJJ@?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [4 x i8] c"%d\0A\00", comdat, align 1

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** %argv) #0 {
entry:
  %retval = alloca i32, align 4
  %argv.addr = alloca i8**, align 4
  %argc.addr = alloca i32, align 4
  store i32 0, i32* %retval
  store i8** %argv, i8*** %argv.addr, align 4
  store i32 %argc, i32* %argc.addr, align 4
  %0 = load i32, i32* %argc.addr, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @"\01??_C@_03PMGGPEJJ@?$CFd?6?$AA@", i32 0, i32 0), i32 %0)
  ret i32 0
}

declare i32 @printf(i8*, ...) #1

;attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
;attributes #1 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }

;!llvm.ident = !{!0}

;!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
