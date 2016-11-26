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
  br label %vector.body

vector.body:                                      ; preds = %entry
  %1 = bitcast [100 x i16]* %Array to <8 x i16>*
  store <8 x i16> <i16 0, i16 1, i16 2, i16 3, i16 4, i16 5, i16 6, i16 7>, <8 x i16>* %1, align 2, !tbaa !1
  %2 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 8
  %3 = bitcast i16* %2 to <8 x i16>*
  store <8 x i16> <i16 8, i16 9, i16 10, i16 11, i16 12, i16 13, i16 14, i16 15>, <8 x i16>* %3, align 2, !tbaa !1
  %4 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 16
  %5 = bitcast i16* %4 to <8 x i16>*
  store <8 x i16> <i16 16, i16 17, i16 18, i16 19, i16 20, i16 21, i16 22, i16 23>, <8 x i16>* %5, align 2, !tbaa !1
  %6 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 24
  %7 = bitcast i16* %6 to <8 x i16>*
  store <8 x i16> <i16 24, i16 25, i16 26, i16 27, i16 28, i16 29, i16 30, i16 31>, <8 x i16>* %7, align 2, !tbaa !1
  %8 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 32
  %9 = bitcast i16* %8 to <8 x i16>*
  store <8 x i16> <i16 32, i16 33, i16 34, i16 35, i16 36, i16 37, i16 38, i16 39>, <8 x i16>* %9, align 2, !tbaa !1
  %10 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 40
  %11 = bitcast i16* %10 to <8 x i16>*
  store <8 x i16> <i16 40, i16 41, i16 42, i16 43, i16 44, i16 45, i16 46, i16 47>, <8 x i16>* %11, align 2, !tbaa !1
  %12 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 48
  %13 = bitcast i16* %12 to <8 x i16>*
  store <8 x i16> <i16 48, i16 49, i16 50, i16 51, i16 52, i16 53, i16 54, i16 55>, <8 x i16>* %13, align 2, !tbaa !1
  %14 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 56
  %15 = bitcast i16* %14 to <8 x i16>*
  store <8 x i16> <i16 56, i16 57, i16 58, i16 59, i16 60, i16 61, i16 62, i16 63>, <8 x i16>* %15, align 2, !tbaa !1
  %16 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 64
  %17 = bitcast i16* %16 to <8 x i16>*
  store <8 x i16> <i16 64, i16 65, i16 66, i16 67, i16 68, i16 69, i16 70, i16 71>, <8 x i16>* %17, align 2, !tbaa !1
  %18 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 72
  %19 = bitcast i16* %18 to <8 x i16>*
  store <8 x i16> <i16 72, i16 73, i16 74, i16 75, i16 76, i16 77, i16 78, i16 79>, <8 x i16>* %19, align 2, !tbaa !1
  %20 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 80
  %21 = bitcast i16* %20 to <8 x i16>*
  store <8 x i16> <i16 80, i16 81, i16 82, i16 83, i16 84, i16 85, i16 86, i16 87>, <8 x i16>* %21, align 2, !tbaa !1
  %22 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 88
  %23 = bitcast i16* %22 to <8 x i16>*
  store <8 x i16> <i16 88, i16 89, i16 90, i16 91, i16 92, i16 93, i16 94, i16 95>, <8 x i16>* %23, align 2, !tbaa !1
  br label %for.body

for.body:                                         ; preds = %vector.body
  %arrayidx = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 96
  store i16 96, i16* %arrayidx, align 2, !tbaa !1
  %arrayidx.1 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 97
  store i16 97, i16* %arrayidx.1, align 2, !tbaa !1
  %arrayidx.2 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 98
  store i16 98, i16* %arrayidx.2, align 2, !tbaa !1
  %arrayidx.3 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 99
  store i16 99, i16* %arrayidx.3, align 2, !tbaa !1
  %arraydecay = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 0
  %incdec.ptr23.i.9 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 1
  %24 = load i16, i16* %arraydecay, align 2, !tbaa !1
  %incdec.ptr29.i.10 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 2
  %25 = load i16, i16* %incdec.ptr23.i.9, align 2, !tbaa !1
  %add32.i.11 = add i16 %24, %25
  %incdec.ptr35.i.12 = getelementptr inbounds [100 x i16], [100 x i16]* %Array, i32 0, i32 3
  %26 = load i16, i16* %incdec.ptr29.i.10, align 2, !tbaa !1
  %add38.i.13 = add i16 %add32.i.11, %26
  %27 = load i16, i16* %incdec.ptr35.i.12, align 2, !tbaa !1
  %add44.i.14 = add i16 %add38.i.13, %27
  br label %do.body.i

do.body.i:                                        ; preds = %for.body, %do.body.i
  %add44.i17 = phi i16 [ %add44.i.14, %for.body ], [ %add44.i, %do.body.i ]
  %n.4.i16 = phi i32 [ 13, %for.body ], [ %dec.i, %do.body.i ]
  %from.addr.4.i15 = phi i16* [ %arraydecay, %for.body ], [ %incdec.ptr17.i, %do.body.i ]
  %incdec.ptr41.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 4
  %dec.i = add nsw i32 %n.4.i16, -1
  %incdec.ptr.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 5
  %28 = load i16, i16* %incdec.ptr41.i, align 2, !tbaa !1
  %add2.i = add i16 %28, %add44.i17
  %incdec.ptr5.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 6
  %29 = load i16, i16* %incdec.ptr.i, align 2, !tbaa !1
  %add8.i = add i16 %add2.i, %29
  %incdec.ptr11.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 7
  %30 = load i16, i16* %incdec.ptr5.i, align 2, !tbaa !1
  %add14.i = add i16 %add8.i, %30
  %incdec.ptr17.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 8
  %31 = load i16, i16* %incdec.ptr11.i, align 2, !tbaa !1
  %add20.i = add i16 %add14.i, %31
  %incdec.ptr23.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 9
  %32 = load i16, i16* %incdec.ptr17.i, align 2, !tbaa !1
  %add26.i = add i16 %32, %add20.i
  %incdec.ptr29.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 10
  %33 = load i16, i16* %incdec.ptr23.i, align 2, !tbaa !1
  %add32.i = add i16 %add26.i, %33
  %incdec.ptr35.i = getelementptr inbounds i16, i16* %from.addr.4.i15, i32 11
  %34 = load i16, i16* %incdec.ptr29.i, align 2, !tbaa !1
  %add38.i = add i16 %add32.i, %34
  %35 = load i16, i16* %incdec.ptr35.i, align 2, !tbaa !1
  %add44.i = add i16 %add38.i, %35
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
