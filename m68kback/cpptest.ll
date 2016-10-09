; ModuleID = 'cpptest.cpp'
target datalayout = "e-m:x-p:32:32-i64:64-f80:32-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc18.0.0"

%class.Test = type { i8* }

$"\01??0Test@@QAE@XZ" = comdat any

$"\01?Doit@Test@@QAEXXZ" = comdat any

$"\01??1Test@@QAE@XZ" = comdat any

define i32 @main() #0 personality i8* bitcast (i32 (...)* @__CxxFrameHandler3 to i8*) {
entry:
  %retval = alloca i32, align 4
  %test = alloca %class.Test, align 4
  %exn.slot = alloca i8*
  %ehselector.slot = alloca i32
  %t = alloca %class.Test*, align 4
  store i32 0, i32* %retval
  %call = call x86_thiscallcc %class.Test* @"\01??0Test@@QAE@XZ"(%class.Test* %test)
  invoke x86_thiscallcc void @"\01?Doit@Test@@QAEXXZ"(%class.Test* %test)
          to label %invoke.cont unwind label %lpad

invoke.cont:                                      ; preds = %entry
  %call2 = invoke noalias i8* @"\01??2@YAPAXI@Z"(i32 4) #4
          to label %invoke.cont.1 unwind label %lpad

invoke.cont.1:                                    ; preds = %invoke.cont
  %0 = bitcast i8* %call2 to %class.Test*
  %call5 = invoke x86_thiscallcc %class.Test* @"\01??0Test@@QAE@XZ"(%class.Test* %0)
          to label %invoke.cont.4 unwind label %lpad.3

invoke.cont.4:                                    ; preds = %invoke.cont.1
  store %class.Test* %0, %class.Test** %t, align 4
  %1 = load %class.Test*, %class.Test** %t, align 4
  invoke x86_thiscallcc void @"\01?Doit@Test@@QAEXXZ"(%class.Test* %1)
          to label %invoke.cont.6 unwind label %lpad

invoke.cont.6:                                    ; preds = %invoke.cont.4
  %2 = load %class.Test*, %class.Test** %t, align 4
  %isnull = icmp eq %class.Test* %2, null
  br i1 %isnull, label %delete.end, label %delete.notnull

delete.notnull:                                   ; preds = %invoke.cont.6
  call x86_thiscallcc void @"\01??1Test@@QAE@XZ"(%class.Test* %2) #5
  %3 = bitcast %class.Test* %2 to i8*
  call void @"\01??3@YAXPAX@Z"(i8* %3) #6
  br label %delete.end

delete.end:                                       ; preds = %delete.notnull, %invoke.cont.6
  store i32 0, i32* %retval
  call x86_thiscallcc void @"\01??1Test@@QAE@XZ"(%class.Test* %test) #5
  %4 = load i32, i32* %retval
  ret i32 %4

lpad:                                             ; preds = %invoke.cont.4, %invoke.cont, %entry
  %5 = landingpad { i8*, i32 }
          cleanup
  %6 = extractvalue { i8*, i32 } %5, 0
  store i8* %6, i8** %exn.slot
  %7 = extractvalue { i8*, i32 } %5, 1
  store i32 %7, i32* %ehselector.slot
  br label %ehcleanup

lpad.3:                                           ; preds = %invoke.cont.1
  %8 = landingpad { i8*, i32 }
          cleanup
  %9 = extractvalue { i8*, i32 } %8, 0
  store i8* %9, i8** %exn.slot
  %10 = extractvalue { i8*, i32 } %8, 1
  store i32 %10, i32* %ehselector.slot
  call void @"\01??3@YAXPAX@Z"(i8* %call2) #6
  br label %ehcleanup

ehcleanup:                                        ; preds = %lpad.3, %lpad
  call x86_thiscallcc void @"\01??1Test@@QAE@XZ"(%class.Test* %test) #5
  br label %eh.resume

eh.resume:                                        ; preds = %ehcleanup
  %exn = load i8*, i8** %exn.slot
  %sel = load i32, i32* %ehselector.slot
  %lpad.val = insertvalue { i8*, i32 } undef, i8* %exn, 0
  %lpad.val.7 = insertvalue { i8*, i32 } %lpad.val, i32 %sel, 1
  resume { i8*, i32 } %lpad.val.7
}

define linkonce_odr x86_thiscallcc %class.Test* @"\01??0Test@@QAE@XZ"(%class.Test* returned %this) unnamed_addr #0 comdat align 2 {
entry:
  %this.addr = alloca %class.Test*, align 4
  store %class.Test* %this, %class.Test** %this.addr, align 4
  %this1 = load %class.Test*, %class.Test** %this.addr
  %call = call noalias i8* @"\01??_U@YAPAXI@Z"(i32 500) #4
  %buf = getelementptr inbounds %class.Test, %class.Test* %this1, i32 0, i32 0
  store i8* %call, i8** %buf, align 4
  ret %class.Test* %this1
}

; Function Attrs: nounwind
define linkonce_odr x86_thiscallcc void @"\01?Doit@Test@@QAEXXZ"(%class.Test* %this) #1 comdat align 2 {
entry:
  %this.addr = alloca %class.Test*, align 4
  store %class.Test* %this, %class.Test** %this.addr, align 4
  %this1 = load %class.Test*, %class.Test** %this.addr
  ret void
}

declare i32 @__CxxFrameHandler3(...)

; Function Attrs: nobuiltin
declare noalias i8* @"\01??2@YAPAXI@Z"(i32) #2

; Function Attrs: nobuiltin nounwind
declare void @"\01??3@YAXPAX@Z"(i8*) #3

; Function Attrs: nounwind
define linkonce_odr x86_thiscallcc void @"\01??1Test@@QAE@XZ"(%class.Test* %this) unnamed_addr #1 comdat align 2 {
entry:
  %this.addr = alloca %class.Test*, align 4
  store %class.Test* %this, %class.Test** %this.addr, align 4
  %this1 = load %class.Test*, %class.Test** %this.addr
  %buf = getelementptr inbounds %class.Test, %class.Test* %this1, i32 0, i32 0
  %0 = load i8*, i8** %buf, align 4
  %isnull = icmp eq i8* %0, null
  br i1 %isnull, label %delete.end, label %delete.notnull

delete.notnull:                                   ; preds = %entry
  call void @"\01??_V@YAXPAX@Z"(i8* %0) #6
  br label %delete.end

delete.end:                                       ; preds = %delete.notnull, %entry
  ret void
}

; Function Attrs: nobuiltin
declare noalias i8* @"\01??_U@YAPAXI@Z"(i32) #2

; Function Attrs: nobuiltin nounwind
declare void @"\01??_V@YAXPAX@Z"(i8*) #3

attributes #0 = { "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { nobuiltin "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #3 = { nobuiltin nounwind "disable-tail-calls"="false" "less-precise-fpmad"="false" "no-frame-pointer-elim"="true" "no-frame-pointer-elim-non-leaf" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+sse,+sse2" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #4 = { builtin }
attributes #5 = { nounwind }
attributes #6 = { builtin nounwind }

!llvm.ident = !{!0}

!0 = !{!"clang version 3.7.0 (trunk 240302) (llvm/trunk 240300)"}
