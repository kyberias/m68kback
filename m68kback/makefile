AMIRUN = amirun
SC = amirun sc:c/sc
SLINK = amirun sc:c/slink

test2: test2.o
	$(SLINK) from lib:c.o test2.o to test2 lib lib:sc.lib

test2.o: test2.s
	$(AMIRUN) sc:c/asm test2.s

test2.s: test2.ll
	bin\debug\m68kback test2.ll >test2.s

test2.ll: test2.c
	clang -S -emit-llvm test2.c
