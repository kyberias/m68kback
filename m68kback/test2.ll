; ModuleID = 'tests/test2.c'
target datalayout = "e-m:e-p:32:32-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i386-unknown-linux-gnu"

@.str = private unnamed_addr constant [15 x i8] c"len, str=%08X\0A\00", align 1
@.str.1 = private unnamed_addr constant [18 x i8] c"reverse, to=%08X\0A\00", align 1
@.str.2 = private unnamed_addr constant [20 x i8] c"reverse, from=%08X\0A\00", align 1
@.str.3 = private unnamed_addr constant [20 x i8] c"reverse, from='%s'\0A\00", align 1
@.str.4 = private unnamed_addr constant [8 x i8] c"l = %d\0A\00", align 1
@.str.5 = private unnamed_addr constant [22 x i8] c"reverse, to end=%08X\0A\00", align 1
@.str.6 = private unnamed_addr constant [10 x i8] c"argc: %d\0A\00", align 1
@.str.7 = private unnamed_addr constant [12 x i8] c"len is: %d\0A\00", align 1
@.str.8 = private unnamed_addr constant [10 x i8] c"buf=%08X\0A\00", align 1
@.str.9 = private unnamed_addr constant [18 x i8] c"reverse is: %08X\0A\00", align 1
@.str.10 = private unnamed_addr constant [18 x i8] c"reverse is: '%s'\0A\00", align 1

; Function Attrs: nounwind
define i32 @len(i8* %str) #0 {
  %1 = alloca i8*, align 4
  %l = alloca i32, align 4
  store i8* %str, i8** %1, align 4
  store i32 0, i32* %l, align 4
  %2 = load i8*, i8** %1, align 4
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @.str, i32 0, i32 0), i8* %2)
  br label %4

; <label>:4                                       ; preds = %9, %0
  %5 = load i8*, i8** %1, align 4
  %6 = getelementptr inbounds i8, i8* %5, i32 1
  store i8* %6, i8** %1, align 4
  %7 = load i8, i8* %5, align 1
  %8 = icmp ne i8 %7, 0
  br i1 %8, label %9, label %12

; <label>:9                                       ; preds = %4
  %10 = load i32, i32* %l, align 4
  %11 = add nsw i32 %10, 1
  store i32 %11, i32* %l, align 4
  br label %4

; <label>:12                                      ; preds = %4
  %13 = load i32, i32* %l, align 4
  ret i32 %13
}

declare i32 @printf(i8*, ...) #1

; Function Attrs: nounwind
define i8* @reverse(i8* %from, i8* %to) #0 {
  %1 = alloca i8*, align 4
  %2 = alloca i8*, align 4
  %l = alloca i32, align 4
  %i = alloca i32, align 4
  store i8* %from, i8** %1, align 4
  store i8* %to, i8** %2, align 4
  %3 = load i8*, i8** %2, align 4
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.1, i32 0, i32 0), i8* %3)
  %5 = load i8*, i8** %1, align 4
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @.str.2, i32 0, i32 0), i8* %5)
  %7 = load i8*, i8** %1, align 4
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @.str.3, i32 0, i32 0), i8* %7)
  %9 = load i8*, i8** %1, align 4
  %10 = call i32 @len(i8* %9)
  store i32 %10, i32* %l, align 4
  %11 = load i32, i32* %l, align 4
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.4, i32 0, i32 0), i32 %11)
  store i32 0, i32* %i, align 4
  br label %13

; <label>:13                                      ; preds = %32, %0
  %14 = load i32, i32* %i, align 4
  %15 = load i32, i32* %l, align 4
  %16 = icmp slt i32 %14, %15
  br i1 %16, label %17, label %35

; <label>:17                                      ; preds = %13
  %18 = load i8*, i8** %2, align 4
  %19 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.1, i32 0, i32 0), i8* %18)
  %20 = load i32, i32* %l, align 4
  %21 = load i32, i32* %i, align 4
  %22 = sub nsw i32 %20, %21
  %23 = sub nsw i32 %22, 1
  %24 = load i8*, i8** %1, align 4
  %25 = getelementptr inbounds i8, i8* %24, i32 %23
  %26 = load i8, i8* %25, align 1
  %27 = load i32, i32* %i, align 4
  %28 = load i8*, i8** %2, align 4
  %29 = getelementptr inbounds i8, i8* %28, i32 %27
  store i8 %26, i8* %29, align 1
  %30 = load i8*, i8** %2, align 4
  %31 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.1, i32 0, i32 0), i8* %30)
  br label %32

; <label>:32                                      ; preds = %17
  %33 = load i32, i32* %i, align 4
  %34 = add nsw i32 %33, 1
  store i32 %34, i32* %i, align 4
  br label %13

; <label>:35                                      ; preds = %13
  %36 = load i8*, i8** %2, align 4
  %37 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.1, i32 0, i32 0), i8* %36)
  %38 = load i32, i32* %i, align 4
  %39 = load i8*, i8** %2, align 4
  %40 = getelementptr inbounds i8, i8* %39, i32 %38
  store i8 0, i8* %40, align 1
  %41 = load i8*, i8** %2, align 4
  %42 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.1, i32 0, i32 0), i8* %41)
  %43 = load i32, i32* %i, align 4
  %44 = load i8*, i8** %2, align 4
  %45 = getelementptr inbounds i8, i8* %44, i32 %43
  %46 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([22 x i8], [22 x i8]* @.str.5, i32 0, i32 0), i8* %45)
  %47 = load i8*, i8** %2, align 4
  ret i8* %47
}

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** %argv) #0 {
  %1 = alloca i32, align 4
  %2 = alloca i32, align 4
  %3 = alloca i8**, align 4
  %buf = alloca [100 x i8], align 1
  %r = alloca i8*, align 4
  store i32 0, i32* %1, align 4
  store i32 %argc, i32* %2, align 4
  store i8** %argv, i8*** %3, align 4
  %4 = load i32, i32* %2, align 4
  %5 = icmp slt i32 %4, 2
  br i1 %5, label %6, label %9

; <label>:6                                       ; preds = %0
  %7 = load i32, i32* %2, align 4
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.6, i32 0, i32 0), i32 %7)
  store i32 -1, i32* %1, align 4
  br label %26

; <label>:9                                       ; preds = %0
  %10 = load i8**, i8*** %3, align 4
  %11 = getelementptr inbounds i8*, i8** %10, i32 1
  %12 = load i8*, i8** %11, align 4
  %13 = call i32 @len(i8* %12)
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.7, i32 0, i32 0), i32 %13)
  %15 = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.8, i32 0, i32 0), i8* %15)
  %17 = load i8**, i8*** %3, align 4
  %18 = getelementptr inbounds i8*, i8** %17, i32 1
  %19 = load i8*, i8** %18, align 4
  %20 = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  %21 = call i8* @reverse(i8* %19, i8* %20)
  store i8* %21, i8** %r, align 4
  %22 = load i8*, i8** %r, align 4
  %23 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.9, i32 0, i32 0), i8* %22)
  %24 = load i8*, i8** %r, align 4
  %25 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.10, i32 0, i32 0), i8* %24)
  store i32 0, i32* %1, align 4
  br label %26

; <label>:26                                      ; preds = %9, %6
  %27 = load i32, i32* %1, align 4
  ret i32 %27
}

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+fxsr,+mmx,+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+fxsr,+mmx,+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.8.0-2ubuntu3~trusty4 (tags/RELEASE_380/final)"}
