    xref _printf
    xref _atoi
    xdef _main
        section text,code
_main:

    sub.l #152,SP
entry:

    moveq #0,D0
    move.l D0,0(SP)
    move.l 160(SP),D0
    move.l D0,4(SP)
    move.l 156(SP),D0
    move.l D0,8(SP)
    move.l 8(SP),D0
    move.l D0,28(SP)
    move.l 28(SP),D0
    cmp.l #2,D0
    blt if$then
    jmp if$end
if$then:

    move.l 8(SP),D0
    move.l D0,36(SP)
    move.l 36(SP),D0
    move.l D0,-(SP)
    lea.l __01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,40(SP)
    moveq #-1,D0
    move.l D0,0(SP)
    jmp return
if$end:

    move.l 4(SP),D0
    move.l D0,44(SP)
    move.l 44(SP),A0
    adda.l #4,A0
    move.l A0,D0
    move.l D0,48(SP)
    movea.l 48(SP),A0
    move.l (A0),D0
    move.l D0,52(SP)
    move.l 52(SP),D0
    move.l D0,-(SP)
    jsr _atoi
    adda.l #4,SP
    move.l D0,56(SP)
    move.l 56(SP),D0
    move.l D0,12(SP)
    move.l 12(SP),D0
    move.l D0,60(SP)
    move.l 60(SP),D0
    move.l D0,-(SP)
    lea.l __01$$_C__07PIJPKGHP_max$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,64(SP)
    moveq #2,D0
    move.l D0,16(SP)
    jmp for$cond
for$cond:

    move.l 16(SP),D0
    move.l D0,68(SP)
    move.l 12(SP),D0
    move.l D0,72(SP)
    move.l 68(SP),D0
    cmp.l 72(SP),D0
    blt for$body
    jmp for$end$15
for$body:

    move.l 16(SP),D0
    move.l D0,80(SP)
    move.l 80(SP),D0
    sub.l #1,D0
    move.l D0,84(SP)
    move.l 84(SP),D0
    move.l D0,20(SP)
    jmp for$cond$4
for$cond$4:

    move.l 20(SP),D0
    move.l D0,88(SP)
    move.l 88(SP),D0
    cmp.l #1,D0
    bgt for$body$6
    jmp for$end
for$body$6:

    move.l 16(SP),D0
    move.l D0,96(SP)
    move.l 20(SP),D0
    move.l D0,100(SP)
    move.l 96(SP),D0
    move.l 100(SP),D1
    divs.w D1,D0
    moveq #16,D1
    lsr.l D1,D0
    move.l D0,104(SP)
    move.l 104(SP),D0
    move.l D0,24(SP)
    move.l 24(SP),D0
    move.l D0,108(SP)
    move.l 108(SP),D0
    cmp.l #0,D0
    beq if$then$8
    jmp if$end$9
if$then$8:

    jmp for$end
if$end$9:

    jmp for$inc
for$inc:

    move.l 20(SP),D0
    move.l D0,116(SP)
    move.l 116(SP),D0
    add.l #-1,D0
    move.l D0,120(SP)
    move.l 120(SP),D0
    move.l D0,20(SP)
    jmp for$cond$4
for$end:

    move.l 20(SP),D0
    move.l D0,124(SP)
    move.l 124(SP),D0
    cmp.l #1,D0
    beq if$then$11
    jmp if$end$13
if$then$11:

    move.l 16(SP),D0
    move.l D0,132(SP)
    move.l 132(SP),D0
    move.l D0,-(SP)
    lea.l __01$$_C__05MAEKFANH_$$CL$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,136(SP)
    jmp if$end$13
if$end$13:

    jmp for$inc$14
for$inc$14:

    move.l 16(SP),D0
    move.l D0,140(SP)
    move.l 140(SP),D0
    add.l #1,D0
    move.l D0,144(SP)
    move.l 144(SP),D0
    move.l D0,16(SP)
    jmp for$cond
for$end$15:

    moveq #0,D0
    move.l D0,0(SP)
    jmp return
return:

    move.l 0(SP),D0
    move.l D0,148(SP)
    move.l 148(SP),D0
    add.l #152,SP
    rts 
         section __MERGED,DATA
__01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_    dc.b 97,114,103,99,58,32,37,100,10,0
__01$$_C__07PIJPKGHP_max$5$$CFd$6$$AA_    dc.b 109,97,120,32,37,100,10,0
__01$$_C__05MAEKFANH_$$CL$5$$CFd$6$$AA_    dc.b 43,32,37,100,10,0
         end
