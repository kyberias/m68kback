; ModuleID = 'test2.c'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

$"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@" = comdat any

$"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@" = comdat any

$"\01??_C@_0BE@IGNBHOIO@reverse?0?5from?$DN?$CF08X?6?$AA@" = comdat any

$"\01??_C@_0BE@HAKBDHCG@reverse?0?5from?$DN?8?$CFs?8?6?$AA@" = comdat any

$"\01??_C@_07IJNMEDPD@l?5?$DN?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_0BG@BJGBGJBA@reverse?0?5to?5end?$DN?$CF08X?6?$AA@" = comdat any

$"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_0M@JBPHDKJA@len?5is?3?5?$CFd?6?$AA@" = comdat any

$"\01??_C@_09PHPMMDDP@buf?$DN?$CF08X?6?$AA@" = comdat any

$"\01??_C@_0BC@HAPMGLEP@reverse?5is?3?5?$CF08X?6?$AA@" = comdat any

$"\01??_C@_0BC@IGIMCCOH@reverse?5is?3?5?8?$CFs?8?6?$AA@" = comdat any

@"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [15 x i8] c"len, str=%08X\0A\00", comdat, align 1
@"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [18 x i8] c"reverse, to=%08X\0A\00", comdat, align 1
@"\01??_C@_0BE@IGNBHOIO@reverse?0?5from?$DN?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [20 x i8] c"reverse, from=%08X\0A\00", comdat, align 1
@"\01??_C@_0BE@HAKBDHCG@reverse?0?5from?$DN?8?$CFs?8?6?$AA@" = linkonce_odr unnamed_addr constant [20 x i8] c"reverse, from='%s'\0A\00", comdat, align 1
@"\01??_C@_07IJNMEDPD@l?5?$DN?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [8 x i8] c"l = %d\0A\00", comdat, align 1
@"\01??_C@_0BG@BJGBGJBA@reverse?0?5to?5end?$DN?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [22 x i8] c"reverse, to end=%08X\0A\00", comdat, align 1
@"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [10 x i8] c"argc: %d\0A\00", comdat, align 1
@"\01??_C@_0M@JBPHDKJA@len?5is?3?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [12 x i8] c"len is: %d\0A\00", comdat, align 1
@"\01??_C@_09PHPMMDDP@buf?$DN?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [10 x i8] c"buf=%08X\0A\00", comdat, align 1
@"\01??_C@_0BC@HAPMGLEP@reverse?5is?3?5?$CF08X?6?$AA@" = linkonce_odr unnamed_addr constant [18 x i8] c"reverse is: %08X\0A\00", comdat, align 1
@"\01??_C@_0BC@IGIMCCOH@reverse?5is?3?5?8?$CFs?8?6?$AA@" = linkonce_odr unnamed_addr constant [18 x i8] c"reverse is: '%s'\0A\00", comdat, align 1

; Function Attrs: nounwind
define i32 @len(i8* %str) #0 {
entry:
  %call = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %str) #1
  %0 = load i8, i8* %str, align 1, !tbaa !1
  %tobool.3 = icmp eq i8 %0, 0
  br i1 %tobool.3, label %while.end, label %while.body.preheader

while.body.preheader:                             ; preds = %entry
  br label %while.body

while.body:                                       ; preds = %while.body.preheader, %while.body
  %l.05 = phi i32 [ %inc, %while.body ], [ 0, %while.body.preheader ]
  %str.addr.04 = phi i8* [ %incdec.ptr, %while.body ], [ %str, %while.body.preheader ]
  %incdec.ptr = getelementptr inbounds i8, i8* %str.addr.04, i32 1
  %inc = add nuw nsw i32 %l.05, 1
  %1 = load i8, i8* %incdec.ptr, align 1, !tbaa !1
  %tobool = icmp eq i8 %1, 0
  br i1 %tobool, label %while.end.loopexit, label %while.body

while.end.loopexit:                               ; preds = %while.body
  %inc.lcssa = phi i32 [ %inc, %while.body ]
  br label %while.end


while.end:                                        ; preds = %while.end.loopexit, %entry
  %l.0.lcssa = phi i32 [ 0, %entry ], [ %inc.lcssa, %while.end.loopexit ]
  ret i32 %l.0.lcssa
}

; Function Attrs: nounwind
declare void @llvm.lifetime.start(i64, i8* nocapture) #1

; Function Attrs: nounwind
declare i32 @printf(i8* nocapture readonly, ...) #0

; Function Attrs: nounwind
declare void @llvm.lifetime.end(i64, i8* nocapture) #1

; Function Attrs: nounwind
define i8* @reverse(i8* %from, i8* %to) #0 {
entry:
  %call = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %to) #1
  %call1 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @"\01??_C@_0BE@IGNBHOIO@reverse?0?5from?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %from) #1
  %call2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @"\01??_C@_0BE@HAKBDHCG@reverse?0?5from?$DN?8?$CFs?8?6?$AA@", i32 0, i32 0), i8* %from) #1
  %call.i = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %from) #1
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
  %call4 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @"\01??_C@_07IJNMEDPD@l?5?$DN?5?$CFd?6?$AA@", i32 0, i32 0), i32 %l.0.lcssa.i) #1
  %cmp.33 = icmp sgt i32 %l.0.lcssa.i, 0
  %call5.34 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %to) #1
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
  %call8 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %to) #1
  %inc = add nuw nsw i32 %i.035, 1
  %call5 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %to) #1
  %exitcond = icmp eq i32 %inc, %l.0.lcssa.i
  br i1 %exitcond, label %for.end.loopexit, label %for.body

for.end.loopexit:                                 ; preds = %for.body
  br label %for.end

for.end:                                          ; preds = %for.end.loopexit, %len.exit
  %i.0.lcssa = phi i32 [ 0, %len.exit ], [ %l.0.lcssa.i, %for.end.loopexit ]
  %arrayidx10 = getelementptr inbounds i8, i8* %to, i32 %i.0.lcssa
  store i8 0, i8* %arrayidx10, align 1, !tbaa !1
  %call11 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %to) #1
  %call13 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([22 x i8], [22 x i8]* @"\01??_C@_0BG@BJGBGJBA@reverse?0?5to?5end?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %arrayidx10) #1
  ret i8* %to
}

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** nocapture readonly %argv) #0 {
entry:
  %buf = alloca [100 x i8], align 1
  %0 = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  call void @llvm.lifetime.start(i64 100, i8* %0) #1
  %cmp = icmp slt i32 %argc, 2
  br i1 %cmp, label %if.then, label %if.end

if.then:                                          ; preds = %entry
  %call = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@", i32 0, i32 0), i32 %argc) #1
  br label %cleanup

if.end:                                           ; preds = %entry
  %arrayidx = getelementptr inbounds i8*, i8** %argv, i32 1
  %1 = load i8*, i8** %arrayidx, align 4, !tbaa !4
  %call.i = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %1) #1
  %2 = load i8, i8* %1, align 1, !tbaa !1
  %tobool.3.i = icmp eq i8 %2, 0
  br i1 %tobool.3.i, label %len.exit, label %while.body.i.preheader

while.body.i.preheader:                           ; preds = %if.end
  br label %while.body.i

while.body.i:                                     ; preds = %while.body.i.preheader, %while.body.i
  %l.05.i = phi i32 [ %inc.i, %while.body.i ], [ 0, %while.body.i.preheader ]
  %str.addr.04.i = phi i8* [ %incdec.ptr.i, %while.body.i ], [ %1, %while.body.i.preheader ]
  %incdec.ptr.i = getelementptr inbounds i8, i8* %str.addr.04.i, i32 1
  %inc.i = add nuw nsw i32 %l.05.i, 1
  %3 = load i8, i8* %incdec.ptr.i, align 1, !tbaa !1
  %tobool.i = icmp eq i8 %3, 0
  br i1 %tobool.i, label %len.exit.loopexit, label %while.body.i

len.exit.loopexit:                                ; preds = %while.body.i
  %inc.i.lcssa = phi i32 [ %inc.i, %while.body.i ]
  br label %len.exit

len.exit:                                         ; preds = %len.exit.loopexit, %if.end
  %l.0.lcssa.i = phi i32 [ 0, %if.end ], [ %inc.i.lcssa, %len.exit.loopexit ]
  %call2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([12 x i8], [12 x i8]* @"\01??_C@_0M@JBPHDKJA@len?5is?3?5?$CFd?6?$AA@", i32 0, i32 0), i32 %l.0.lcssa.i) #1
  %call3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @"\01??_C@_09PHPMMDDP@buf?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %0) #1
  %4 = load i8*, i8** %arrayidx, align 4, !tbaa !4
  %call6 = call i8* @reverse(i8* %4, i8* %0)
  %call7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@HAPMGLEP@reverse?5is?3?5?$CF08X?6?$AA@", i32 0, i32 0), i8* %call6) #1
  %call8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@IGIMCCOH@reverse?5is?3?5?8?$CFs?8?6?$AA@", i32 0, i32 0), i8* %call6) #1
  br label %cleanup

cleanup:                                          ; preds = %len.exit, %if.then
  %retval.0 = phi i32 [ -1, %if.then ], [ 0, %len.exit ]
  call void @llvm.lifetime.end(i64 100, i8* %0) #1
  ret i32 %retval.0
}

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nounwind }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
!1 = !{!2, !2, i64 0}
!2 = !{!"omnipotent char", !3, i64 0}
!3 = !{!"Simple C/C++ TBAA"}
!4 = !{!5, !5, i64 0}
!5 = !{!"any pointer", !2, i64 0}
