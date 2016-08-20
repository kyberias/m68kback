    xref _printf
    xdef _main
        section text,code
_main:

    sub.l #20,SP
entry:

    moveq #0,D0
    move.l D0,0(SP)
    move.l 28(SP),D0
    move.l D0,4(SP)
    move.l 24(SP),D0
    move.l D0,8(SP)
    move.l 8(SP),D0
    move.l D0,12(SP)
    move.l 12(SP),D0
    move.l D0,-(SP)
    lea.l __01$$_C__03PMGGPEJJ_$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,16(SP)
    moveq #0,D0
    add.l #20,SP
    rts 
         section __MERGED,DATA
__01$$_C__03PMGGPEJJ_$$CFd$6$$AA_    dc.b 37,100,92,48,65,92,48,48,0
         end
