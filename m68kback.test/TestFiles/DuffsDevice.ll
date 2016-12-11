; ModuleID = 'DuffsDevice.c'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

$"\01??_C@_0L@EBJKDCGI@Sum?5is?5?$CFd?6?$AA@" = comdat any

@"\01??_C@_0L@EBJKDCGI@Sum?5is?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [11 x i8] c"Sum is %d\0A\00", comdat, align 1

; Function Attrs: nounwind
define i32 @sum(i16* nocapture %to, i16* nocapture readonly %from, i32 %count) #0 {
entry:
  %add = add nsw i32 %count, 7
  %div = sdiv i32 %add, 8
  %rem = srem i32 %count, 8
  switch i32 %rem, label %sw.epilog [
    i32 0, label %entry.do.body_crit_edge
    i32 7, label %entry.sw.bb.4_crit_edge
    i32 6, label %entry.sw.bb.10_crit_edge
    i32 5, label %entry.sw.bb.16_crit_edge
    i32 4, label %entry.sw.bb.22_crit_edge
    i32 3, label %entry.sw.bb.28_crit_edge
    i32 2, label %entry.sw.bb.34_crit_edge
    i32 1, label %entry.sw.bb.40_crit_edge
  ]

entry.sw.bb.40_crit_edge:                         ; preds = %entry
  %.pre85 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.40

entry.sw.bb.34_crit_edge:                         ; preds = %entry
  %.pre84 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.34

entry.sw.bb.28_crit_edge:                         ; preds = %entry
  %.pre83 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.28

entry.sw.bb.22_crit_edge:                         ; preds = %entry
  %.pre82 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.22

entry.sw.bb.16_crit_edge:                         ; preds = %entry
  %.pre81 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.16

entry.sw.bb.10_crit_edge:                         ; preds = %entry
  %.pre80 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.10

entry.sw.bb.4_crit_edge:                          ; preds = %entry
  %.pre79 = load i16, i16* %to, align 2, !tbaa !1
  br label %sw.bb.4

entry.do.body_crit_edge:                          ; preds = %entry
  %.pre = load i16, i16* %to, align 2, !tbaa !1
  br label %do.body

do.body:                                          ; preds = %entry.do.body_crit_edge, %sw.bb.40
  %0 = phi i16 [ %add44, %sw.bb.40 ], [ %.pre, %entry.do.body_crit_edge ]
  %from.addr.0 = phi i16* [ %incdec.ptr41, %sw.bb.40 ], [ %from, %entry.do.body_crit_edge ]
  %n.0 = phi i32 [ %dec, %sw.bb.40 ], [ %div, %entry.do.body_crit_edge ]
  %incdec.ptr = getelementptr inbounds i16, i16* %from.addr.0, i32 1
  %1 = load i16, i16* %from.addr.0, align 2, !tbaa !1
  %add2 = add i16 %0, %1
  store i16 %add2, i16* %to, align 2, !tbaa !1
  br label %sw.bb.4

sw.bb.4:                                          ; preds = %entry.sw.bb.4_crit_edge, %do.body
  %2 = phi i16 [ %add2, %do.body ], [ %.pre79, %entry.sw.bb.4_crit_edge ]
  %from.addr.1 = phi i16* [ %incdec.ptr, %do.body ], [ %from, %entry.sw.bb.4_crit_edge ]
  %n.1 = phi i32 [ %n.0, %do.body ], [ %div, %entry.sw.bb.4_crit_edge ]
  %incdec.ptr5 = getelementptr inbounds i16, i16* %from.addr.1, i32 1
  %3 = load i16, i16* %from.addr.1, align 2, !tbaa !1
  %add8 = add i16 %2, %3
  store i16 %add8, i16* %to, align 2, !tbaa !1
  br label %sw.bb.10

sw.bb.10:                                         ; preds = %entry.sw.bb.10_crit_edge, %sw.bb.4
  %4 = phi i16 [ %add8, %sw.bb.4 ], [ %.pre80, %entry.sw.bb.10_crit_edge ]
  %from.addr.2 = phi i16* [ %incdec.ptr5, %sw.bb.4 ], [ %from, %entry.sw.bb.10_crit_edge ]
  %n.2 = phi i32 [ %n.1, %sw.bb.4 ], [ %div, %entry.sw.bb.10_crit_edge ]
  %incdec.ptr11 = getelementptr inbounds i16, i16* %from.addr.2, i32 1
  %5 = load i16, i16* %from.addr.2, align 2, !tbaa !1
  %add14 = add i16 %4, %5
  store i16 %add14, i16* %to, align 2, !tbaa !1
  br label %sw.bb.16

sw.bb.16:                                         ; preds = %entry.sw.bb.16_crit_edge, %sw.bb.10
  %6 = phi i16 [ %add14, %sw.bb.10 ], [ %.pre81, %entry.sw.bb.16_crit_edge ]
  %from.addr.3 = phi i16* [ %incdec.ptr11, %sw.bb.10 ], [ %from, %entry.sw.bb.16_crit_edge ]
  %n.3 = phi i32 [ %n.2, %sw.bb.10 ], [ %div, %entry.sw.bb.16_crit_edge ]
  %incdec.ptr17 = getelementptr inbounds i16, i16* %from.addr.3, i32 1
  %7 = load i16, i16* %from.addr.3, align 2, !tbaa !1
  %add20 = add i16 %6, %7
  store i16 %add20, i16* %to, align 2, !tbaa !1
  br label %sw.bb.22

sw.bb.22:                                         ; preds = %entry.sw.bb.22_crit_edge, %sw.bb.16
  %8 = phi i16 [ %add20, %sw.bb.16 ], [ %.pre82, %entry.sw.bb.22_crit_edge ]
  %from.addr.4 = phi i16* [ %incdec.ptr17, %sw.bb.16 ], [ %from, %entry.sw.bb.22_crit_edge ]
  %n.4 = phi i32 [ %n.3, %sw.bb.16 ], [ %div, %entry.sw.bb.22_crit_edge ]
  %incdec.ptr23 = getelementptr inbounds i16, i16* %from.addr.4, i32 1
  %9 = load i16, i16* %from.addr.4, align 2, !tbaa !1
  %add26 = add i16 %8, %9
  store i16 %add26, i16* %to, align 2, !tbaa !1
  br label %sw.bb.28

sw.bb.28:                                         ; preds = %entry.sw.bb.28_crit_edge, %sw.bb.22
  %10 = phi i16 [ %add26, %sw.bb.22 ], [ %.pre83, %entry.sw.bb.28_crit_edge ]
  %from.addr.5 = phi i16* [ %incdec.ptr23, %sw.bb.22 ], [ %from, %entry.sw.bb.28_crit_edge ]
  %n.5 = phi i32 [ %n.4, %sw.bb.22 ], [ %div, %entry.sw.bb.28_crit_edge ]
  %incdec.ptr29 = getelementptr inbounds i16, i16* %from.addr.5, i32 1
  %11 = load i16, i16* %from.addr.5, align 2, !tbaa !1
  %add32 = add i16 %10, %11
  store i16 %add32, i16* %to, align 2, !tbaa !1
  br label %sw.bb.34

sw.bb.34:                                         ; preds = %entry.sw.bb.34_crit_edge, %sw.bb.28
  %12 = phi i16 [ %add32, %sw.bb.28 ], [ %.pre84, %entry.sw.bb.34_crit_edge ]
  %from.addr.6 = phi i16* [ %incdec.ptr29, %sw.bb.28 ], [ %from, %entry.sw.bb.34_crit_edge ]
  %n.6 = phi i32 [ %n.5, %sw.bb.28 ], [ %div, %entry.sw.bb.34_crit_edge ]
  %incdec.ptr35 = getelementptr inbounds i16, i16* %from.addr.6, i32 1
  %13 = load i16, i16* %from.addr.6, align 2, !tbaa !1
  %add38 = add i16 %12, %13
  store i16 %add38, i16* %to, align 2, !tbaa !1
  br label %sw.bb.40

sw.bb.40:                                         ; preds = %entry.sw.bb.40_crit_edge, %sw.bb.34
  %14 = phi i16 [ %.pre85, %entry.sw.bb.40_crit_edge ], [ %add38, %sw.bb.34 ]
  %from.addr.7 = phi i16* [ %from, %entry.sw.bb.40_crit_edge ], [ %incdec.ptr35, %sw.bb.34 ]
  %n.7 = phi i32 [ %div, %entry.sw.bb.40_crit_edge ], [ %n.6, %sw.bb.34 ]
  %incdec.ptr41 = getelementptr inbounds i16, i16* %from.addr.7, i32 1
  %15 = load i16, i16* %from.addr.7, align 2, !tbaa !1
  %add44 = add i16 %14, %15
  store i16 %add44, i16* %to, align 2, !tbaa !1
  %dec = add nsw i32 %n.7, -1
  %cmp = icmp sgt i32 %n.7, 1
  br i1 %cmp, label %do.body, label %sw.epilog

sw.epilog:                                        ; preds = %sw.bb.40, %entry
  ret i32 undef
}

; Function Attrs: nounwind
declare void @llvm.lifetime.start(i64, i8* nocapture) #1

; Function Attrs: nounwind
declare void @llvm.lifetime.end(i64, i8* nocapture) #1

; Function Attrs: nounwind
define i32 @main() #0 {
entry:
  %Array = alloca [100 x i16], align 2
  %0 = bitcast [100 x i16]* %Array to i8*
  call void @llvm.lifetime.start(i64 200, i8* %0) #1
  br label %for.body

for.body:                                         ; preds = %entry, %for.body
  %i.018 = phi i32 [ 0, %entry ], [ %inc, %for.body ]
  %conv = trunc i32 %i.018 to i16
  %arrayidx = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 %i.018
  store i16 %conv, i16* %arrayidx, align 2, !tbaa !1
  %inc = add nuw nsw i32 %i.018, 1
  %cmp = icmp eq i32 %inc, 100
  br i1 %cmp, label %for.end, label %for.body

for.end:                                          ; preds = %for.body
  %arraydecay = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 0
  %incdec.ptr23.i.9 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 1
  %1 = load i16, i16* %arraydecay, align 2, !tbaa !1
  %incdec.ptr29.i.10 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 2
  %2 = load i16, i16* %incdec.ptr23.i.9, align 2, !tbaa !1
  %add32.i.11 = add i16 %1, %2
  %incdec.ptr35.i.12 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 3
  %3 = load i16, i16* %incdec.ptr29.i.10, align 2, !tbaa !1
  %add38.i.13 = add i16 %add32.i.11, %3
  %4 = load i16, i16* %incdec.ptr35.i.12, align 2, !tbaa !1
  %add44.i.14 = add i16 %add38.i.13, %4
  br label %do.body.i

do.body.i:                                        ; preds = %for.end, %do.body.i
  %add44.i17 = phi i16 [ %add44.i.14, %for.end ], [ %add44.i, %do.body.i ]
  %n.4.i16 = phi i32 [ 13, %for.end ], [ %dec.i, %do.body.i ]
  %from.addr.4.i15 = phi i16* [ %arraydecay, %for.end ], [ %incdec.ptr17.i, %do.body.i ]
  %incdec.ptr41.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 4
  %dec.i = add nsw i32 %n.4.i16, -1
  %incdec.ptr.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 5
  %5 = load i16, i16* %incdec.ptr41.i, align 2, !tbaa !1
  %add2.i = add i16 %5, %add44.i17
  %incdec.ptr5.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 6
  %6 = load i16, i16* %incdec.ptr.i, align 2, !tbaa !1
  %add8.i = add i16 %add2.i, %6
  %incdec.ptr11.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 7
  %7 = load i16, i16* %incdec.ptr5.i, align 2, !tbaa !1
  %add14.i = add i16 %add8.i, %7
  %incdec.ptr17.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 8
  %8 = load i16, i16* %incdec.ptr11.i, align 2, !tbaa !1
  %add20.i = add i16 %add14.i, %8
  %incdec.ptr23.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 9
  %9 = load i16, i16* %incdec.ptr17.i, align 2, !tbaa !1
  %add26.i = add i16 %9, %add20.i
  %incdec.ptr29.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 10
  %10 = load i16, i16* %incdec.ptr23.i, align 2, !tbaa !1
  %add32.i = add i16 %add26.i, %10
  %incdec.ptr35.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 11
  %11 = load i16, i16* %incdec.ptr29.i, align 2, !tbaa !1
  %add38.i = add i16 %add32.i, %11
  %12 = load i16, i16* %incdec.ptr35.i, align 2, !tbaa !1
  %add44.i = add i16 %add38.i, %12
  %cmp.i = icmp sgt i32 %dec.i, 1
  br i1 %cmp.i, label %do.body.i, label %sum.exit

sum.exit:                                         ; preds = %do.body.i
  %add44.i.lcssa = phi i16 [ %add44.i, %do.body.i ]
  %conv1 = sext i16 %add44.i.lcssa to i32
  %call2 = tail call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([11 x i8], [11 x i8]* @"\01??_C@_0L@EBJKDCGI@Sum?5is?5?$CFd?6?$AA@", i32 0, i32 0), i32 %conv1) #1
  call void @llvm.lifetime.end(i64 200, i8* %0) #1
  ret i32 0
}

; Function Attrs: nounwind
declare i32 @printf(i8* nocapture readonly, ...) #0

attributes #0 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nounwind }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
!1 = !{!2, !2, i64 0}
!2 = !{!"short", !3, i64 0}
!3 = !{!"omnipotent char", !4, i64 0}
!4 = !{!"Simple C/C++ TBAA"}
