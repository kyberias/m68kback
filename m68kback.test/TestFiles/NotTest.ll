; ModuleID = 'NotTest.c'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

$"\01??_C@_0BK@OEFEKPJI@Bitwise?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_0CA@OEJBLFEO@Boolean?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@" = comdat any

@"\01??_C@_0BK@OEFEKPJI@Bitwise?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [26 x i8] c"Bitwise Not: %d %d %d %d\0A\00", comdat, align 1
@"\01??_C@_0CA@OEJBLFEO@Boolean?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [32 x i8] c"Boolean Not: %d %d %d %d %d %d\0A\00", comdat, align 1

; Function Attrs: nounwind
define void @testBitWiseNot(i32 %A, i32 %B, i32 %C, i32 %D) #0 {
entry:
  %D.addr = alloca i32, align 4
  %C.addr = alloca i32, align 4
  %B.addr = alloca i32, align 4
  %A.addr = alloca i32, align 4
  store i32 %D, i32* %D.addr, align 4
  store i32 %C, i32* %C.addr, align 4
  store i32 %B, i32* %B.addr, align 4
  store i32 %A, i32* %A.addr, align 4
  %0 = load i32, i32* %D.addr, align 4
  %neg = xor i32 %0, -1
  %1 = load i32, i32* %C.addr, align 4
  %neg1 = xor i32 %1, -1
  %2 = load i32, i32* %B.addr, align 4
  %neg2 = xor i32 %2, -1
  %3 = load i32, i32* %A.addr, align 4
  %neg3 = xor i32 %3, -1
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([26 x i8], [26 x i8]* @"\01??_C@_0BK@OEFEKPJI@Bitwise?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@", i32 0, i32 0), i32 %neg3, i32 %neg2, i32 %neg1, i32 %neg)
  ret void
}

declare i32 @printf(i8*, ...) #1

; Function Attrs: nounwind
define void @testBooleanNot(i32 %A, i32 %B, i32 %C, i32 %D) #0 {
entry:
  %D.addr = alloca i32, align 4
  %C.addr = alloca i32, align 4
  %B.addr = alloca i32, align 4
  %A.addr = alloca i32, align 4
  store i32 %D, i32* %D.addr, align 4
  store i32 %C, i32* %C.addr, align 4
  store i32 %B, i32* %B.addr, align 4
  store i32 %A, i32* %A.addr, align 4
  %0 = load i32, i32* %C.addr, align 4
  %cmp = icmp sgt i32 %0, 0
  br i1 %cmp, label %land.rhs, label %land.end

land.rhs:                                         ; preds = %entry
  %1 = load i32, i32* %D.addr, align 4
  %cmp1 = icmp sgt i32 %1, 0
  br label %land.end

land.end:                                         ; preds = %land.rhs, %entry
  %2 = phi i1 [ false, %entry ], [ %cmp1, %land.rhs ]
  %lnot = xor i1 %2, true
  %lnot.ext = zext i1 %lnot to i32
  %3 = load i32, i32* %B.addr, align 4
  %cmp2 = icmp sgt i32 %3, 0
  br i1 %cmp2, label %land.rhs.3, label %land.end.5

land.rhs.3:                                       ; preds = %land.end
  %4 = load i32, i32* %D.addr, align 4
  %cmp4 = icmp sgt i32 %4, 0
  br label %land.end.5

land.end.5:                                       ; preds = %land.rhs.3, %land.end
  %5 = phi i1 [ false, %land.end ], [ %cmp4, %land.rhs.3 ]
  %lnot6 = xor i1 %5, true
  %lnot.ext7 = zext i1 %lnot6 to i32
  %6 = load i32, i32* %B.addr, align 4
  %cmp8 = icmp sgt i32 %6, 0
  br i1 %cmp8, label %land.rhs.9, label %land.end.11

land.rhs.9:                                       ; preds = %land.end.5
  %7 = load i32, i32* %C.addr, align 4
  %cmp10 = icmp sgt i32 %7, 0
  br label %land.end.11

land.end.11:                                      ; preds = %land.rhs.9, %land.end.5
  %8 = phi i1 [ false, %land.end.5 ], [ %cmp10, %land.rhs.9 ]
  %lnot12 = xor i1 %8, true
  %lnot.ext13 = zext i1 %lnot12 to i32
  %9 = load i32, i32* %A.addr, align 4
  %cmp14 = icmp sgt i32 %9, 0
  br i1 %cmp14, label %land.rhs.15, label %land.end.17

land.rhs.15:                                      ; preds = %land.end.11
  %10 = load i32, i32* %D.addr, align 4
  %cmp16 = icmp sgt i32 %10, 0
  br label %land.end.17

land.end.17:                                      ; preds = %land.rhs.15, %land.end.11
  %11 = phi i1 [ false, %land.end.11 ], [ %cmp16, %land.rhs.15 ]
  %lnot18 = xor i1 %11, true
  %lnot.ext19 = zext i1 %lnot18 to i32
  %12 = load i32, i32* %A.addr, align 4
  %cmp20 = icmp sgt i32 %12, 0
  br i1 %cmp20, label %land.rhs.21, label %land.end.23

land.rhs.21:                                      ; preds = %land.end.17
  %13 = load i32, i32* %C.addr, align 4
  %cmp22 = icmp sgt i32 %13, 0
  br label %land.end.23

land.end.23:                                      ; preds = %land.rhs.21, %land.end.17
  %14 = phi i1 [ false, %land.end.17 ], [ %cmp22, %land.rhs.21 ]
  %lnot24 = xor i1 %14, true
  %lnot.ext25 = zext i1 %lnot24 to i32
  %15 = load i32, i32* %A.addr, align 4
  %cmp26 = icmp sgt i32 %15, 0
  br i1 %cmp26, label %land.rhs.27, label %land.end.29

land.rhs.27:                                      ; preds = %land.end.23
  %16 = load i32, i32* %B.addr, align 4
  %cmp28 = icmp sgt i32 %16, 0
  br label %land.end.29

land.end.29:                                      ; preds = %land.rhs.27, %land.end.23
  %17 = phi i1 [ false, %land.end.23 ], [ %cmp28, %land.rhs.27 ]
  %lnot30 = xor i1 %17, true
  %lnot.ext31 = zext i1 %lnot30 to i32
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([32 x i8], [32 x i8]* @"\01??_C@_0CA@OEJBLFEO@Boolean?5Not?3?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?5?$CFd?6?$AA@", i32 0, i32 0), i32 %lnot.ext31, i32 %lnot.ext25, i32 %lnot.ext19, i32 %lnot.ext13, i32 %lnot.ext7, i32 %lnot.ext)
  ret void
}

; Function Attrs: nounwind
define i32 @main() #0 {
entry:
  %retval = alloca i32, align 4
  store i32 0, i32* %retval
  call void @testBitWiseNot(i32 1, i32 2, i32 -3, i32 5)
  call void @testBooleanNot(i32 1, i32 2, i32 -3, i32 5)
  ret i32 0
}

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
