SC = amirun sc:c/sc
ASM = amirun sc:c/asm
SLINK = amirun sc:c/slink
CLANG = clang -S -emit-llvm
M68KBACK = ../bin/debug/m68kback
TARGETS = test2.run test.run printfparam.run structs.run NotTest.run

all : $(TARGETS)

%.run : %.o
	$(SLINK) from lib:c.o $< to $@ lib lib:sc.lib

%.o : %.s
	$(ASM) $<

%.s : %.ll
	$(M68KBACK) $< > $@

%.ll : %.c
	$(CLANG) $<
