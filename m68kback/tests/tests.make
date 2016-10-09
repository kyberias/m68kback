AMIRUN = amirun
SC = amirun sc:c/sc
SLINK = amirun sc:c/slink
CLANG = clang -S -emit-llvm
M68KBACK = ../bin/debug/m68kback
TARGETS = test2.run test.run

all : $(TARGETS)

%.run : %.o
	$(SLINK) from lib:c.o $< to $@ lib lib:sc.lib

%.o : %.s
	$(AMIRUN) sc:c/asm $<

%.s : %.ll
	$(M68KBACK) $< > $@

%.ll : %.c
	$(CLANG) $<
