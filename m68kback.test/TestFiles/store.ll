; ModuleID = 'store.c'
source_filename = "store.c"
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc19.0.23918"

; Function Attrs: norecurse nounwind
define void @store16(i16* nocapture readonly %source, i16* nocapture %dest, i32 %len) local_unnamed_addr #0 {
entry:
  %tobool2 = icmp eq i32 %len, 0
  br i1 %tobool2, label %while.end, label %while.body.preheader

while.body.preheader:                             ; preds = %entry
  br label %while.body

while.body:                                       ; preds = %while.body.preheader, %while.body
  %source.addr.05 = phi i16* [ %incdec.ptr, %while.body ], [ %source, %while.body.preheader ]
  %dest.addr.04 = phi i16* [ %incdec.ptr1, %while.body ], [ %dest, %while.body.preheader ]
  %len.addr.03 = phi i32 [ %dec, %while.body ], [ %len, %while.body.preheader ]
  %dec = add nsw i32 %len.addr.03, -1
  %incdec.ptr = getelementptr inbounds i16, i16* %source.addr.05, i32 1
  %0 = load i16, i16* %source.addr.05, align 2, !tbaa !1
  %incdec.ptr1 = getelementptr inbounds i16, i16* %dest.addr.04, i32 1
  store i16 %0, i16* %dest.addr.04, align 2, !tbaa !1
  %tobool = icmp eq i32 %dec, 0
  br i1 %tobool, label %while.end.loopexit, label %while.body

while.end.loopexit:                               ; preds = %while.body
  br label %while.end

while.end:                                        ; preds = %while.end.loopexit, %entry
  ret void
}

attributes #0 = { norecurse nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="false" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }

!llvm.ident = !{!0}

!0 = !{!"clang version 4.0.0 (tags/RELEASE_400/final)"}
!1 = !{!2, !2, i64 0}
!2 = !{!"short", !3, i64 0}
!3 = !{!"omnipotent char", !4, i64 0}
!4 = !{!"Simple C/C++ TBAA"}
