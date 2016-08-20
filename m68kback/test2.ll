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
  %str.addr = alloca i8*, align 4
  %l = alloca i32, align 4
  store i8* %str, i8** %str.addr, align 4
  store i32 0, i32* %l, align 4
  %0 = load i8*, i8** %str.addr, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([15 x i8], [15 x i8]* @"\01??_C@_0P@IEEEKLOJ@len?0?5str?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %0)
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %1 = load i8*, i8** %str.addr, align 4
  %incdec.ptr = getelementptr inbounds i8, i8* %1, i32 1
  store i8* %incdec.ptr, i8** %str.addr, align 4
  %2 = load i8, i8* %1, align 1
  %tobool = icmp ne i8 %2, 0
  br i1 %tobool, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %3 = load i32, i32* %l, align 4
  %inc = add nsw i32 %3, 1
  store i32 %inc, i32* %l, align 4
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %4 = load i32, i32* %l, align 4
  ret i32 %4
}

declare i32 @printf(i8*, ...) #1

; Function Attrs: nounwind
define i8* @reverse(i8* %from, i8* %to) #0 {
entry:
  %to.addr = alloca i8*, align 4
  %from.addr = alloca i8*, align 4
  %l = alloca i32, align 4
  %i = alloca i32, align 4
  store i8* %to, i8** %to.addr, align 4
  store i8* %from, i8** %from.addr, align 4
  %0 = load i8*, i8** %to.addr, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %0)
  %1 = load i8*, i8** %from.addr, align 4
  %call1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @"\01??_C@_0BE@IGNBHOIO@reverse?0?5from?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %1)
  %2 = load i8*, i8** %from.addr, align 4
  %call2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([20 x i8], [20 x i8]* @"\01??_C@_0BE@HAKBDHCG@reverse?0?5from?$DN?8?$CFs?8?6?$AA@", i32 0, i32 0), i8* %2)
  %3 = load i8*, i8** %from.addr, align 4
  %call3 = call i32 @len(i8* %3)
  store i32 %call3, i32* %l, align 4
  %4 = load i32, i32* %l, align 4
  %call4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @"\01??_C@_07IJNMEDPD@l?5?$DN?5?$CFd?6?$AA@", i32 0, i32 0), i32 %4)
  store i32 0, i32* %i, align 4
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %5 = load i32, i32* %i, align 4
  %6 = load i32, i32* %l, align 4
  %cmp = icmp slt i32 %5, %6
  br i1 %cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %7 = load i8*, i8** %to.addr, align 4
  %call5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %7)
  %8 = load i32, i32* %l, align 4
  %9 = load i32, i32* %i, align 4
  %sub = sub nsw i32 %8, %9
  %sub6 = sub nsw i32 %sub, 1
  %10 = load i8*, i8** %from.addr, align 4
  %arrayidx = getelementptr inbounds i8, i8* %10, i32 %sub6
  %11 = load i8, i8* %arrayidx, align 1
  %12 = load i32, i32* %i, align 4
  %13 = load i8*, i8** %to.addr, align 4
  %arrayidx7 = getelementptr inbounds i8, i8* %13, i32 %12
  store i8 %11, i8* %arrayidx7, align 1
  %14 = load i8*, i8** %to.addr, align 4
  %call8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %14)
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %15 = load i32, i32* %i, align 4
  %inc = add nsw i32 %15, 1
  store i32 %inc, i32* %i, align 4
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %16 = load i8*, i8** %to.addr, align 4
  %call9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %16)
  %17 = load i32, i32* %i, align 4
  %18 = load i8*, i8** %to.addr, align 4
  %arrayidx10 = getelementptr inbounds i8, i8* %18, i32 %17
  store i8 0, i8* %arrayidx10, align 1
  %19 = load i8*, i8** %to.addr, align 4
  %call11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@GHOHLIIH@reverse?0?5to?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %19)
  %20 = load i32, i32* %i, align 4
  %21 = load i8*, i8** %to.addr, align 4
  %arrayidx12 = getelementptr inbounds i8, i8* %21, i32 %20
  %call13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([22 x i8], [22 x i8]* @"\01??_C@_0BG@BJGBGJBA@reverse?0?5to?5end?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %arrayidx12)
  %22 = load i8*, i8** %to.addr, align 4
  ret i8* %22
}

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** %argv) #0 {
entry:
  %retval = alloca i32, align 4
  %argv.addr = alloca i8**, align 4
  %argc.addr = alloca i32, align 4
  %buf = alloca [100 x i8], align 1
  %r = alloca i8*, align 4
  store i32 0, i32* %retval
  store i8** %argv, i8*** %argv.addr, align 4
  store i32 %argc, i32* %argc.addr, align 4
  %0 = load i32, i32* %argc.addr, align 4
  %cmp = icmp slt i32 %0, 2
  br i1 %cmp, label %if.then, label %if.end

if.then:                                          ; preds = %entry
  %1 = load i32, i32* %argc.addr, align 4
  %call = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@", i32 0, i32 0), i32 %1)
  store i32 -1, i32* %retval
  br label %return

if.end:                                           ; preds = %entry
  %2 = load i8**, i8*** %argv.addr, align 4
  %arrayidx = getelementptr inbounds i8*, i8** %2, i32 1
  %3 = load i8*, i8** %arrayidx, align 4
  %call1 = call i32 @len(i8* %3)
  %call2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([12 x i8], [12 x i8]* @"\01??_C@_0M@JBPHDKJA@len?5is?3?5?$CFd?6?$AA@", i32 0, i32 0), i32 %call1)
  %arraydecay = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  %call3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([10 x i8], [10 x i8]* @"\01??_C@_09PHPMMDDP@buf?$DN?$CF08X?6?$AA@", i32 0, i32 0), i8* %arraydecay)
  %arraydecay4 = getelementptr inbounds [100 x i8], [100 x i8]* %buf, i32 0, i32 0
  %4 = load i8**, i8*** %argv.addr, align 4
  %arrayidx5 = getelementptr inbounds i8*, i8** %4, i32 1
  %5 = load i8*, i8** %arrayidx5, align 4
  %call6 = call i8* @reverse(i8* %5, i8* %arraydecay4)
  store i8* %call6, i8** %r, align 4
  %6 = load i8*, i8** %r, align 4
  %call7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@HAPMGLEP@reverse?5is?3?5?$CF08X?6?$AA@", i32 0, i32 0), i8* %6)
  %7 = load i8*, i8** %r, align 4
  %call8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([18 x i8], [18 x i8]* @"\01??_C@_0BC@IGIMCCOH@reverse?5is?3?5?8?$CFs?8?6?$AA@", i32 0, i32 0), i8* %7)
  store i32 0, i32* %retval
  br label %return

return:                                           ; preds = %if.end, %if.then
  %8 = load i32, i32* %retval
  ret i32 %8
}
