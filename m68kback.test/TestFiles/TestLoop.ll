; ModuleID = 'TestLoop.c'
source_filename = "TestLoop.c"
target datalayout = "e-m:e-p:32:32-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i386-pc-none"

@.str = private unnamed_addr constant [4 x i8] c"%d\0A\00", align 1

; Function Attrs: noinline nounwind
define i32 @foo(i32 %i, i32 %j) #0 {
entry:
  %i.addr = alloca i32, align 4
  %j.addr = alloca i32, align 4
  store i32 %i, i32* %i.addr, align 4
  store i32 %j, i32* %j.addr, align 4
  %0 = load i32, i32* %j.addr, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.str, i32 0, i32 0), i32 %0)
  %1 = load i32, i32* %j.addr, align 4
  ret i32 %1
}

declare i32 @printf(i8*, ...) #1

; Function Attrs: noinline nounwind
define i32 @main(i32 %level) #0 {
entry:
  %retval = alloca i32, align 4
  %level.addr = alloca i32, align 4
  %i = alloca i32, align 4
  %fval = alloca [4 x i32], align 4
  store i32 0, i32* %retval, align 4
  store i32 %level, i32* %level.addr, align 4
  store i32 3, i32* %i, align 4
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %0 = load i32, i32* %i, align 4
  %cmp = icmp sge i32 %0, 0
  br i1 %cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %1 = load i32, i32* %level.addr, align 4
  %sub = sub nsw i32 %1, 1
  %2 = load i32, i32* %level.addr, align 4
  %mul = mul nsw i32 %2, 4
  %3 = load i32, i32* %i, align 4
  %add = add nsw i32 %mul, %3
  %add1 = add nsw i32 %add, 1
  %call = call i32 @foo(i32 %sub, i32 %add1)
  %4 = load i32, i32* %i, align 4
  %arrayidx = getelementptr inbounds [4 x i32], [4 x i32]* %fval, i32 0, i32 %4
  store i32 %call, i32* %arrayidx, align 4
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %5 = load i32, i32* %i, align 4
  %dec = add nsw i32 %5, -1
  store i32 %dec, i32* %i, align 4
  br label %for.cond

for.end:                                          ; preds = %for.cond
  store i32 0, i32* %i, align 4
  br label %for.cond2

for.cond2:                                        ; preds = %for.inc7, %for.end
  %6 = load i32, i32* %i, align 4
  %cmp3 = icmp slt i32 %6, 4
  br i1 %cmp3, label %for.body4, label %for.end8

for.body4:                                        ; preds = %for.cond2
  %7 = load i32, i32* %i, align 4
  %arrayidx5 = getelementptr inbounds [4 x i32], [4 x i32]* %fval, i32 0, i32 %7
  %8 = load i32, i32* %arrayidx5, align 4
  %call6 = call i32 @foo(i32 0, i32 %8)
  br label %for.inc7

for.inc7:                                         ; preds = %for.body4
  %9 = load i32, i32* %i, align 4
  %inc = add nsw i32 %9, 1
  store i32 %inc, i32* %i, align 4
  br label %for.cond2

for.end8:                                         ; preds = %for.cond2
  ret i32 0
}

attributes #0 = { noinline nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0}

!0 = !{!"clang version 4.0.0 (tags/RELEASE_400/final)"}
