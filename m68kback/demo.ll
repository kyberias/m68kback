@"\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [10 x i8] c"argc: %d\0A\00", comdat, align 1
@"\01??_C@_07PIJPKGHP@max?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [8 x i8] c"max %d\0A\00", comdat, align 1
@"\01??_C@_05MAEKFANH@?$CL?5?$CFd?6?$AA@" = linkonce_odr unnamed_addr constant [6 x i8] c"+ %d\0A\00", comdat, align 1

; Function Attrs: nounwind
define i32 @main(i32 %argc, i8** %argv) #0 {
entry:
  %retval = alloca i32, align 4
  %argv.addr = alloca i8**, align 4
  %argc.addr = alloca i32, align 4
  %max = alloca i32, align 4
  %i = alloca i32, align 4
  %j = alloca i32, align 4
  %mod = alloca i32, align 4
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
  %call1 = call i32 bitcast (i32 (...)* @atoi to i32 (i8*)*)(i8* %3)
  store i32 %call1, i32* %max, align 4
  %4 = load i32, i32* %max, align 4
  %call2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([8 x i8], [8 x i8]* @"\01??_C@_07PIJPKGHP@max?5?$CFd?6?$AA@", i32 0, i32 0), i32 %4)
  store i32 2, i32* %i, align 4
  br label %for.cond

for.cond:                                         ; preds = %for.inc.14, %if.end
  %5 = load i32, i32* %i, align 4
  %6 = load i32, i32* %max, align 4
  %cmp3 = icmp slt i32 %5, %6
  br i1 %cmp3, label %for.body, label %for.end.15

for.body:                                         ; preds = %for.cond
  %7 = load i32, i32* %i, align 4
  %sub = sub nsw i32 %7, 1
  store i32 %sub, i32* %j, align 4
  br label %for.cond.4

for.cond.4:                                       ; preds = %for.inc, %for.body
  %8 = load i32, i32* %j, align 4
  %cmp5 = icmp sgt i32 %8, 1
  br i1 %cmp5, label %for.body.6, label %for.end

for.body.6:                                       ; preds = %for.cond.4
  %9 = load i32, i32* %i, align 4
  %10 = load i32, i32* %j, align 4
  %rem = srem i32 %9, %10
  store i32 %rem, i32* %mod, align 4
  %11 = load i32, i32* %mod, align 4
  %cmp7 = icmp eq i32 %11, 0
  br i1 %cmp7, label %if.then.8, label %if.end.9

if.then.8:                                        ; preds = %for.body.6
  br label %for.end

if.end.9:                                         ; preds = %for.body.6
  br label %for.inc

for.inc:                                          ; preds = %if.end.9
  %12 = load i32, i32* %j, align 4
  %dec = add nsw i32 %12, -1
  store i32 %dec, i32* %j, align 4
  br label %for.cond.4

for.end:                                          ; preds = %if.then.8, %for.cond.4
  %13 = load i32, i32* %j, align 4
  %cmp10 = icmp eq i32 %13, 1
  br i1 %cmp10, label %if.then.11, label %if.end.13

if.then.11:                                       ; preds = %for.end
  %14 = load i32, i32* %i, align 4
  %call12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @"\01??_C@_05MAEKFANH@?$CL?5?$CFd?6?$AA@", i32 0, i32 0), i32 %14)
  br label %if.end.13

if.end.13:                                        ; preds = %if.then.11, %for.end
  br label %for.inc.14

for.inc.14:                                       ; preds = %if.end.13
  %15 = load i32, i32* %i, align 4
  %inc = add nsw i32 %15, 1
  store i32 %inc, i32* %i, align 4
  br label %for.cond

for.end.15:                                       ; preds = %for.cond
  store i32 0, i32* %retval
  br label %return

return:                                           ; preds = %for.end.15, %if.then
  %16 = load i32, i32* %retval
  ret i32 %16
}

declare i32 @printf(i8*, ...) #1

declare i32 @atoi(...) #1
