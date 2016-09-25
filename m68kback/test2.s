    xref _printf
    xdef _len
    xdef _reverse
    xdef _main
        section text,code
_len:

    sub.l #48,SP
    move.l 52(SP),A0
entry0:

    move.l A0,0(SP) ; Store to stack
    moveq #0,D0
    move.l D0,4(SP) ; Store to stack
    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0P_IEEEKLOJ_len$0$5str$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    jmp while$cond0
while$cond0:

    move.l 44(SP),A0 ; Spilled reg A15 load
    move.l 0(SP),A0 ; Load from stack
    move.l A0,44(SP) ; Spilled reg A15 store
    move.l 44(SP),A0 ; Spilled reg A15 load
    adda.l #1,A0
    move.l A0,0(SP) ; Store to stack
    move.l 44(SP),A0 ; Spilled reg A15 load
    move.b (A0),D0 ; Load by register address
    cmp.b #0,D0
    bne while$body0
    jmp while$end0
while$body0:

    move.l 4(SP),D0 ; Load from stack
    add.l #1,D0
    move.l D0,4(SP) ; Store to stack
    jmp while$cond0
while$end0:

    move.l 4(SP),D0 ; Load from stack
    add.l #48,SP
    rts 
_reverse:

    sub.l #180,SP
    move.l 184(SP),A1
    move.l 188(SP),A0
entry1:

    move.l A0,0(SP) ; Store to stack
    move.l A1,4(SP) ; Store to stack
    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BE_IGNBHOIO_reverse$0$5from$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BE_HAKBDHCG_reverse$0$5from$$DN$8$$CFs$8$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),A0 ; Load from stack
    move.l A0,-(SP)
    jsr _len
    adda.l #4,SP
    move.l D0,4(SP) ; Store to stack
    move.l 4(SP),D0 ; Load from stack
    move.l D0,-(SP)
    lea.l __01$$_C__07IJNMEDPD_l$5$$DN$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    moveq #0,D0
    move.l D0,4(SP) ; Store to stack
    jmp for$cond1
for$cond1:

    move.l 4(SP),D0 ; Load from stack
    move.l 4(SP),D0 ; Load from stack
    cmp.l D0,D0
    blt for$body1
    jmp for$end1
for$body1:

    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),D1 ; Load from stack
    move.l 4(SP),D0 ; Load from stack
    sub.l D0,D1
    move.l D1,D0
    sub.l #1,D0
    move.l 4(SP),A0 ; Load from stack
    adda.l D0,A0
    move.b (A0),D1 ; Load by register address
    move.l 4(SP),D0 ; Load from stack
    move.l 0(SP),A0 ; Load from stack
    adda.l D0,A0
    move.l D1,(A0) ; Store to reg
    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    jmp for$inc1
for$inc1:

    move.l 4(SP),D0 ; Load from stack
    add.l #1,D0
    move.l D0,4(SP) ; Store to stack
    jmp for$cond1
for$end1:

    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),D0 ; Load from stack
    move.l 0(SP),A0 ; Load from stack
    adda.l D0,A0
    moveq #0,D0
    move.l D0,(A0) ; Store to reg
    move.l 0(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),D0 ; Load from stack
    move.l 0(SP),A0 ; Load from stack
    adda.l D0,A0
    move.l A0,-(SP)
    lea.l __01$$_C__0BG_BJGBGJBA_reverse$0$5to$5end$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 0(SP),A0 ; Load from stack
    move.l A0,D0
    add.l #180,SP
    rts 
_main:

    sub.l #500,SP
    move.l 504(SP),D0
    move.l 508(SP),A0
entry2:

    moveq #0,D1
    move.l D1,0(SP) ; Store to stack
    move.l A0,4(SP) ; Store to stack
    move.l D0,4(SP) ; Store to stack
    move.l 4(SP),D0 ; Load from stack
    cmp.l #2,D0
    blt if$then2
    jmp if$end2
if$then2:

    move.l 4(SP),D0 ; Load from stack
    move.l D0,-(SP)
    lea.l __01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    moveq #-1,D0
    move.l D0,0(SP) ; Store to stack
    jmp return2
if$end2:

    move.l 4(SP),A0 ; Load from stack
    adda.l #4,A0
    move.l (A0),A0 ; Load by register address
    move.l A0,-(SP)
    jsr _len
    adda.l #4,SP
    move.l D0,-(SP)
    lea.l __01$$_C__0M_JBPHDKJA_len$5is$3$5$$CFd$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),A0
    move.l A0,-(SP)
    lea.l __01$$_C__09PHPMMDDP_buf$$DN$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 4(SP),A0
    move.l 4(SP),A1 ; Load from stack
    adda.l #4,A1
    move.l (A1),A1 ; Load by register address
    move.l A0,-(SP)
    move.l A1,-(SP)
    jsr _reverse
    adda.l #8,SP
    move.l D0,100(SP) ; Store to stack
    move.l 100(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_HAPMGLEP_reverse$5is$3$5$$CF08X$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l 100(SP),A0 ; Load from stack
    move.l A0,-(SP)
    lea.l __01$$_C__0BC_IGIMCCOH_reverse$5is$3$5$8$$CFs$8$6$$AA_,A0
    move.l A0,-(SP)
    jsr _printf
    adda.l #8,SP
    moveq #0,D0
    move.l D0,0(SP) ; Store to stack
    jmp return2
return2:

    move.l 0(SP),D0 ; Load from stack
    add.l #500,SP
    rts 
         section __MERGED,DATA
__01$$_C__0P_IEEEKLOJ_len$0$5str$$DN$$CF08X$6$$AA_    dc.b 108,101,110,44,32,115,116,114,61,37,48,56,88,10,0
__01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_    dc.b 114,101,118,101,114,115,101,44,32,116,111,61,37,48,56,88,10,0
__01$$_C__0BE_IGNBHOIO_reverse$0$5from$$DN$$CF08X$6$$AA_    dc.b 114,101,118,101,114,115,101,44,32,102,114,111,109,61,37,48,56,88,10,0
__01$$_C__0BE_HAKBDHCG_reverse$0$5from$$DN$8$$CFs$8$6$$AA_    dc.b 114,101,118,101,114,115,101,44,32,102,114,111,109,61,39,37,115,39,10,0
__01$$_C__07IJNMEDPD_l$5$$DN$5$$CFd$6$$AA_    dc.b 108,32,61,32,37,100,10,0
__01$$_C__0BG_BJGBGJBA_reverse$0$5to$5end$$DN$$CF08X$6$$AA_    dc.b 114,101,118,101,114,115,101,44,32,116,111,32,101,110,100,61,37,48,56,88,10,0
__01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_    dc.b 97,114,103,99,58,32,37,100,10,0
__01$$_C__0M_JBPHDKJA_len$5is$3$5$$CFd$6$$AA_    dc.b 108,101,110,32,105,115,58,32,37,100,10,0
__01$$_C__09PHPMMDDP_buf$$DN$$CF08X$6$$AA_    dc.b 98,117,102,61,37,48,56,88,10,0
__01$$_C__0BC_HAPMGLEP_reverse$5is$3$5$$CF08X$6$$AA_    dc.b 114,101,118,101,114,115,101,32,105,115,58,32,37,48,56,88,10,0
__01$$_C__0BC_IGIMCCOH_reverse$5is$3$5$8$$CFs$8$6$$AA_    dc.b 114,101,118,101,114,115,101,32,105,115,58,32,39,37,115,39,10,0
         end
