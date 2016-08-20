	.text
	.def	 @feat.00;
	.scl	3;
	.type	0;
	.endef
	.globl	@feat.00
@feat.00 = 1
	.def	 _main;
	.scl	2;
	.type	32;
	.endef
	.globl	_main
	.align	16, 0x90
_main:                                  # @main
# BB#0:                                 # %entry
	pushl	%ebp
	movl	%esp, %ebp
	subl	$16, %esp
	movl	$0, -4(%ebp)
	movl	$2, -8(%ebp)
LBB0_1:                                 # %for.cond
                                        # =>This Loop Header: Depth=1
                                        #     Child Loop BB0_3 Depth 2
	cmpl	$1000, -8(%ebp)         # imm = 0x3E8
	jge	LBB0_12
# BB#2:                                 # %for.body
                                        #   in Loop: Header=BB0_1 Depth=1
	movl	-8(%ebp), %eax
	subl	$1, %eax
	movl	%eax, -12(%ebp)
LBB0_3:                                 # %for.cond.1
                                        #   Parent Loop BB0_1 Depth=1
                                        # =>  This Inner Loop Header: Depth=2
	cmpl	$1, -12(%ebp)
	jle	LBB0_8
# BB#4:                                 # %for.body.3
                                        #   in Loop: Header=BB0_3 Depth=2
	movl	-8(%ebp), %eax
	cltd
	idivl	-12(%ebp)
	cmpl	$0, %edx
	jne	LBB0_6
# BB#5:                                 # %if.then
                                        #   in Loop: Header=BB0_1 Depth=1
	jmp	LBB0_8
LBB0_6:                                 # %if.end
                                        #   in Loop: Header=BB0_3 Depth=2
	jmp	LBB0_7
LBB0_7:                                 # %for.inc
                                        #   in Loop: Header=BB0_3 Depth=2
	movl	-12(%ebp), %eax
	addl	$-1, %eax
	movl	%eax, -12(%ebp)
	jmp	LBB0_3
LBB0_8:                                 # %for.end
                                        #   in Loop: Header=BB0_1 Depth=1
	cmpl	$1, -12(%ebp)
	je	LBB0_10
# BB#9:                                 # %if.then.6
                                        #   in Loop: Header=BB0_1 Depth=1
	movl	-8(%ebp), %eax
	movl	%eax, (%esp)
	calll	_print
LBB0_10:                                # %if.end.7
                                        #   in Loop: Header=BB0_1 Depth=1
	jmp	LBB0_11
LBB0_11:                                # %for.inc.8
                                        #   in Loop: Header=BB0_1 Depth=1
	movl	-8(%ebp), %eax
	addl	$1, %eax
	movl	%eax, -8(%ebp)
	jmp	LBB0_1
LBB0_12:                                # %for.end.9
	xorl	%eax, %eax
	addl	$16, %esp
	popl	%ebp
	retl


