AMIRUN = amirun
SC = amirun sc:c/sc
SLINK = amirun sc:c/slink

all: test2

test2: test2.o
	$(SLINK) from lib:c.o test2.o to test2 lib lib:sc.lib

test2.o: test2.s
	$(AMIRUN) sc:c/asm test2.s

test2.s: test2.ll
	bin\debug\m68kback test2.ll >test2.s

test2.ll: test2.c
	clang -S -emit-llvm test2.c


test: test.o
	$(SLINK) from lib:c.o test.o to test lib lib:sc.lib

test.o: test.s
	$(AMIRUN) sc:c/asm test.s

test.s: test.ll
	bin\debug\m68kback test.ll >test.s

test.ll: test.c
	clang -S -emit-llvm test.c

cpptest: cpptest.o
	$(SLINK) from lib:c.o cpptest.o to cpptest lib lib:sc.lib

cpptest.o: cpptest.s
	$(AMIRUN) sc:c/asm cpptest.s

cpptest.s: cpptest.ll
	bin\debug\m68kback cpptest.ll >cpptest.s

cpptest.ll: cpptest.cpp
	clang++ -S -emit-llvm cpptest.cpp
