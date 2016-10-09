    xref _printf
    xref _atoi
    xdef _main
        section text,code
_main:

    sub.l #12,SP
    move.l D7,0(SP) ; Spilled reg D14 store
    move.l D6,4(SP) ; Spilled reg D13 store
    move.l D5,8(SP) ; Spilled reg D12 store
    move.l 16(SP),D0
    move.l 20(SP),A0
entry0:

    cmp.l #2,D0
    blt if$then0
    jmp if$end0
if$then0:

    move.l D0,-(SP)
    lea.l __01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    jmp cleanup0$5
if$end0:

    adda.l #4,A0
    move.l (A0),A0 ; Load by register address
    move.l A0,-(SP)
    jsr _atoi
    move.l D0,D6
    adda.l #4,SP
    move.l D6,-(SP)
    lea.l __01$$_C__07PIJPKGHP_max$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    cmp.l #2,D6
    bgt for$cond$4$preheader$preheader0
    jmp cleanup0$6
for$cond$4$preheader$preheader0:

    jmp for$cond$4$preheader0$1
for$cond$4$preheader0:

for$cond$4$preheader0$0:

    jmp for$cond$4$preheader0$end ; from Phi
for$cond$4$preheader0$1:

    moveq #2,D5 ; from Phi
    jmp for$cond$4$preheader0$end ; from Phi
for$cond$4$preheader0$end:

    move.l D5,D0
    jmp for$cond$40$3
for$cond$40:

for$cond$40$2:

    jmp for$cond$40$end ; from Phi
for$cond$40$3:

    jmp for$cond$40$end ; from Phi
for$cond$40$end:

    add.l #-1,D0
    cmp.l #1,D0
    bgt for$body$60
    jmp for$end0$4
for$body$60:

    move.l D5,D1
    divs.w D0,D1
    moveq #16,D7
    lsr.l D7,D1
    cmp.l #0,D1
    beq for$inc$14$loopexit0
    jmp for$cond$40$2
for$end0:

for$end0$4:

    jmp for$end0$end ; from Phi
for$end0$end:

    cmp.l #1,D0
    beq if$then$110
    jmp for$inc$140
if$then$110:

    move.l D5,-(SP)
    lea.l __01$$_C__05MAEKFANH_$$CL$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    jmp for$inc$140
for$inc$14$loopexit0:

    jmp for$inc$140
for$inc$140:

    add.l #1,D5
    cmp.l D6,D5
    beq cleanup$loopexit0
    jmp for$cond$4$preheader0$0
cleanup$loopexit0:

    jmp cleanup0$7
cleanup0:

cleanup0$5:

    moveq #-1,D0 ; from Phi
    jmp cleanup0$end ; from Phi
cleanup0$6:

    moveq #0,D0 ; from Phi
    jmp cleanup0$end ; from Phi
cleanup0$7:

    moveq #0,D0 ; from Phi
    jmp cleanup0$end ; from Phi
cleanup0$end:

    move.l 8(SP),D5 ; Spilled reg D12 load
    move.l 4(SP),D6 ; Spilled reg D13 load
    move.l 0(SP),D7 ; Spilled reg D14 load
    add.l #12,SP
    rts 
         section __MERGED,DATA
__01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_    dc.b 99,34,97,114,103,99,58,32,37,100,10,0,34
__01$$_C__07PIJPKGHP_max$5$$CFd$6$$AA_    dc.b 99,34,109,97,120,32,37,100,10,0,34
__01$$_C__05MAEKFANH_$$CL$5$$CFd$6$$AA_    dc.b 99,34,43,32,37,100,10,0,34
         end
