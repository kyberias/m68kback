SC = amirun sc:c/sc
ASM = amirun sc:c/asm
SLINK = amirun sc:c/slink
CLANG = clang -Dalign2byte= -IC:\projects\AmigaInclude\include_h -S -O3 -emit-llvm
M68KBACK = ../../bin/debug/m68kback
TARGETS = lines.run
#lines.run

all : $(TARGETS)

%.run : %.o
	$(SLINK) from lib:c.o $< to $@ lib lib:sc.lib lib:amiga.lib

.PRECIOUS: %.o
%.o : %.s
	$(ASM) $<

%.s : %.ll
	$(M68KBACK) $< > $@

%.ll : %.c
	$(CLANG) $<
