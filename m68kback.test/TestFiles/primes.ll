; ModuleID = 'test.c'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

$"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_07PIJPKGHP@max?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_05MAEKFANH@?$CL?5?$CFd?6?$AA@" = comdat any

@"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [10 x i8] c"argc: %d\0A\00", comdat, align 1
@"\01??_C@_07PIJPKGHP@max?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [8 x i8] c"max %d\0A\00", comdat, align 1
@"\01??_C@_05MAEKFANH@?$CL?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [6 x i8] c"+ %d\0A\00", comdat, align 1

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** nocapture readonly %argv) #0 {
entry:
  %cmp = icmp slt i32 %argc, 2
  br i1 %cmp, label %if.then, label %if.end

if.then:                                          ; preds = %entry
  %call = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@", i32 0, i32 0), i32 %argc) #2
  br label %cleanup

if.end:                                           ; preds = %entry
  %arrayidx = getelementptr inbounds i8*, i8** %argv, i32 1
  %0 = load i8*, i8** %arrayidx, align 4, !tbaa !1
  %call1 = tail call i32 bitcast (i32 (...)* @atoi to i32 (i8*)*)(i8* %0) #2
  %call2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @"\01??_C@_07PIJPKGHP@max?5?$CFd?6?$AA@", i32 0, i32 0), i32 %call1) #2
  %cmp3.34 = icmp sgt i32 %call1, 2
  br i1 %cmp3.34, label %for.cond.4.preheader.preheader, label %cleanup

for.cond.4.preheader.preheader:                   ; preds = %if.end
  br label %for.cond.4.preheader

for.cond.4.preheader:                             ; preds = %for.cond.4.preheader.preheader, %for.inc.14
  %i.035 = phi i32 [ %inc, %for.inc.14 ], [ 2, %for.cond.4.preheader.preheader ]
  br label %for.cond.4

for.cond.4:                                       ; preds = %for.cond.4.preheader, %for.body.6
  %j.0.in = phi i32 [ %j.0, %for.body.6 ], [ %i.035, %for.cond.4.preheader ]
  %j.0 = add nsw i32 %j.0.in, -1
  %cmp5 = icmp sgt i32 %j.0, 1
  br i1 %cmp5, label %for.body.6, label %for.end

for.body.6:                                       ; preds = %for.cond.4
  %rem = srem i32 %i.035, %j.0
  %cmp7 = icmp eq i32 %rem, 0
  br i1 %cmp7, label %for.inc.14.loopexit, label %for.cond.4

for.end:                                          ; preds = %for.cond.4
  %j.0.lcssa = phi i32 [ %j.0, %for.cond.4 ]
  %cmp10 = icmp eq i32 %j.0.lcssa, 1
  br i1 %cmp10, label %if.then.11, label %for.inc.14

if.then.11:                                       ; preds = %for.end
  %call12 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @"\01??_C@_05MAEKFANH@?$CL?5?$CFd?6?$AA@", i32 0, i32 0), i32 %i.035) #2
  br label %for.inc.14

for.inc.14.loopexit:                              ; preds = %for.body.6
  br label %for.inc.14

for.inc.14:                                       ; preds = %for.inc.14.loopexit, %for.end, %if.then.11
  %inc = add nuw nsw i32 %i.035, 1
  %exitcond = icmp eq i32 %inc, %call1
  br i1 %exitcond, label %cleanup.loopexit, label %for.cond.4.preheader

cleanup.loopexit:                                 ; preds = %for.inc.14
  br label %cleanup

cleanup:                                          ; preds = %cleanup.loopexit, %if.end, %if.then
  %retval.0 = phi i32 [ -1, %if.then ], [ 0, %if.end ], [ 0, %cleanup.loopexit ]
  ret i32 %retval.0
}

; Function Attrs: nounwind
declare i32 @printf(i8* nocapture readonly, ...) #0

declare i32 @atoi(...) #1

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { nounwind }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
!1 = !{!2, !2, i64 0}
!2 = !{!"any pointer", !3, i64 0}
!3 = !{!"omnipotent char", !4, i64 0}
!4 = !{!"Simple C/C++ TBAA"}
