SC = amirun sc:c/sc
ASM = ../../../amirun/debug/amirun sc:c/asm
SLINK = ../../../amirun/debug/amirun sc:c/slink
CLANG = clang -target i386-pc-none -m32 -O2 -fno-vectorize -S -emit-llvm -I.
M68KBACK = ../bin/debug/m68kback
TARGETS = test2.run test.run printfparam.run structs.run NotTest.run TestLoop.run DuffsDevice.run 

#-fno-tree-vectorize

.SECONDARY:

all : $(TARGETS)

%.run : %.o
	$(SLINK) from lib:c.o $< to $@ lib lib:sc.lib

%.o : %.s
	$(ASM) $<

%.s : %.ll
	$(M68KBACK) $< $@ > $@

%.ll : %.c
	$(CLANG) $<
