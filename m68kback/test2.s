    xref _printf
    xdef _len
    xdef _reverse
    xdef _main
        section text,code
_len:

    sub.l #44,SP
entry0:

    move.l 48(SP),D0 ; Variable reference
    move.l D0,0(SP)
    moveq #0,D0
    move.l D0,4(SP)
    move.l 0(SP),D0
    move.l D0,8(SP)
    move.l 8(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0P_IEEEKLOJ_len$0$5str$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,12(SP)
    jmp while$cond0
while$cond0:

    move.l 0(SP),D0
    move.l D0,16(SP)
    move.l 16(SP),A0
    adda.l #1,A0
    move.l A0,D0
    move.l D0,20(SP)
    move.l 20(SP),D0 ; Variable reference
    move.l D0,0(SP)
    movea.l 16(SP),A0
    move.b (A0),D0
    move.b D0,24(SP)
    move.b 24(SP),D0
    cmp.b #0,D0
    bne while$body0
    jmp while$end0
while$body0:

    move.l 4(SP),D0
    move.l D0,32(SP)
    move.l 32(SP),D0
    add.l #1,D0
    move.l D0,36(SP)
    move.l 36(SP),D0 ; Variable reference
    move.l D0,4(SP)
    jmp while$cond0
while$end0:

    move.l 4(SP),D0
    move.l D0,40(SP)
    move.l 40(SP),D0 ; Variable reference
    add.l #44,SP
    rts 
_reverse:

    sub.l #180,SP
entry1:

    move.l 188(SP),D0 ; Variable reference
    move.l D0,0(SP)
    move.l 184(SP),D0 ; Variable reference
    move.l D0,4(SP)
    move.l 0(SP),D0
    move.l D0,16(SP)
    move.l 16(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,20(SP)
    move.l 4(SP),D0
    move.l D0,24(SP)
    move.l 24(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BE_IGNBHOIO_reverse$0$5from$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,28(SP)
    move.l 4(SP),D0
    move.l D0,32(SP)
    move.l 32(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BE_HAKBDHCG_reverse$0$5from$$DN$8$$CFs$8$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,36(SP)
    move.l 4(SP),D0
    move.l D0,40(SP)
    move.l 40(SP),D0 ; Variable reference
    move.l D0,-(SP)
    jsr _len
    adda.l #4,SP
    move.l D0,44(SP)
    move.l 44(SP),D0 ; Variable reference
    move.l D0,8(SP)
    move.l 8(SP),D0
    move.l D0,48(SP)
    move.l 48(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__07IJNMEDPD_l$5$$DN$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,52(SP)
    moveq #0,D0
    move.l D0,12(SP)
    jmp for$cond1
for$cond1:

    move.l 12(SP),D0
    move.l D0,56(SP)
    move.l 8(SP),D0
    move.l D0,60(SP)
    move.l 56(SP),D0
    cmp.l 60(SP),D0
    blt for$body1
    jmp for$end1
for$body1:

    move.l 0(SP),D0
    move.l D0,68(SP)
    move.l 68(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,72(SP)
    move.l 8(SP),D0
    move.l D0,76(SP)
    move.l 12(SP),D0
    move.l D0,80(SP)
    move.l 76(SP),D0
    sub.l 80(SP),D0
    move.l D0,84(SP)
    move.l 84(SP),D0
    sub.l #1,D0
    move.l D0,88(SP)
    move.l 4(SP),D0
    move.l D0,92(SP)
    move.l 92(SP),A0
    adda.l 88(SP),A0
    move.l A0,D0
    move.l D0,96(SP)
    movea.l 96(SP),A0
    move.b (A0),D0
    move.b D0,100(SP)
    move.l 12(SP),D0
    move.l D0,104(SP)
    move.l 0(SP),D0
    move.l D0,108(SP)
    move.l 108(SP),A0
    adda.l 104(SP),A0
    move.l A0,D0
    move.l D0,112(SP)
    move.b 100(SP),D0 ; Variable reference
    move.l 112(SP),A0 ; Store
    move.b D0,(A0) ; Store
    move.l 0(SP),D0
    move.l D0,116(SP)
    move.l 116(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,120(SP)
    jmp for$inc1
for$inc1:

    move.l 12(SP),D0
    move.l D0,124(SP)
    move.l 124(SP),D0
    add.l #1,D0
    move.l D0,128(SP)
    move.l 128(SP),D0 ; Variable reference
    move.l D0,12(SP)
    jmp for$cond1
for$end1:

    move.l 0(SP),D0
    move.l D0,132(SP)
    move.l 132(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,136(SP)
    move.l 12(SP),D0
    move.l D0,140(SP)
    move.l 0(SP),D0
    move.l D0,144(SP)
    move.l 144(SP),A0
    adda.l 140(SP),A0
    move.l A0,D0
    move.l D0,148(SP)
    moveq #0,D0
    move.l 148(SP),A0 ; Store
    move.b D0,(A0) ; Store
    move.l 0(SP),D0
    move.l D0,152(SP)
    move.l 152(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_GHOHLIIH_reverse$0$5to$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,156(SP)
    move.l 12(SP),D0
    move.l D0,160(SP)
    move.l 0(SP),D0
    move.l D0,164(SP)
    move.l 164(SP),A0
    adda.l 160(SP),A0
    move.l A0,D0
    move.l D0,168(SP)
    move.l 168(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BG_BJGBGJBA_reverse$0$5to$5end$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,172(SP)
    move.l 0(SP),D0
    move.l D0,176(SP)
    move.l 176(SP),D0 ; Variable reference
    add.l #180,SP
    rts 
_main:

    sub.l #500,SP
entry2:

    moveq #0,D0
    move.l D0,0(SP)
    move.l 508(SP),D0 ; Variable reference
    move.l D0,4(SP)
    move.l 504(SP),D0 ; Variable reference
    move.l D0,8(SP)
    move.l 8(SP),D0
    move.l D0,416(SP)
    move.l 416(SP),D0
    cmp.l #2,D0
    blt if$then2
    jmp if$end2
if$then2:

    move.l 8(SP),D0
    move.l D0,424(SP)
    move.l 424(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__09NKIIDDPL_argc$3$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,428(SP)
    moveq #-1,D0
    move.l D0,0(SP)
    jmp return2
if$end2:

    move.l 4(SP),D0
    move.l D0,432(SP)
    move.l 432(SP),A0
    adda.l #4,A0
    move.l A0,D0
    move.l D0,436(SP)
    movea.l 436(SP),A0
    move.l (A0),D0
    move.l D0,440(SP)
    move.l 440(SP),D0 ; Variable reference
    move.l D0,-(SP)
    jsr _len
    adda.l #4,SP
    move.l D0,444(SP)
    move.l 444(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0M_JBPHDKJA_len$5is$3$5$$CFd$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,448(SP)
    move.l SP,A0 ; GetElementPtr (array)
    adda.l #12,A0
    adda.l #0,A0
    move.l A0,D0
    move.l D0,452(SP)
    move.l 452(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__09PHPMMDDP_buf$$DN$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,456(SP)
    move.l SP,A0 ; GetElementPtr (array)
    adda.l #12,A0
    adda.l #0,A0
    move.l A0,D0
    move.l D0,460(SP)
    move.l 4(SP),D0
    move.l D0,464(SP)
    move.l 464(SP),A0
    adda.l #4,A0
    move.l A0,D0
    move.l D0,468(SP)
    movea.l 468(SP),A0
    move.l (A0),D0
    move.l D0,472(SP)
    move.l 460(SP),D0 ; Variable reference
    move.l D0,-(SP)
    move.l 476(SP),D0 ; Variable reference
    move.l D0,-(SP)
    jsr _reverse
    adda.l #8,SP
    move.l D0,476(SP)
    move.l 476(SP),D0 ; Variable reference
    move.l D0,412(SP)
    move.l 412(SP),D0
    move.l D0,480(SP)
    move.l 480(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_HAPMGLEP_reverse$5is$3$5$$CF08X$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,484(SP)
    move.l 412(SP),D0
    move.l D0,488(SP)
    move.l 488(SP),D0 ; Variable reference
    move.l D0,-(SP)
    lea.l __01$$_C__0BC_IGIMCCOH_reverse$5is$3$5$8$$CFs$8$6$$AA_,A0
    move.l A0,D0
    move.l D0,-(SP)
    jsr _printf
    adda.l #8,SP
    move.l D0,492(SP)
    moveq #0,D0
    move.l D0,0(SP)
    jmp return2
return2:

    move.l 0(SP),D0
    move.l D0,496(SP)
    move.l 496(SP),D0 ; Variable reference
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
