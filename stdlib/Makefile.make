ASM = ../../amirun/debug/amirun sc:c/asm
SLINK = ../../amirun/debug/amirun sc:c/slink
CLANG = clang -target i386-pc-none -m32 -O2 -fno-vectorize -S -emit-llvm -I internals -I ..\..\PDCLib\internals -I ..\..\PDCLib\includes -I includes
M68KBACK = ../m68kback/bin/debug/m68kback
STDIOTARGETS = fopen.o printf.o vfprintf.o _vcbprintf.o funlockfile.o 
STDLIBTARGETS = abort.o 
SIGNALTARGETS = raise.o 

#-fno-tree-vectorize

.SECONDARY:

all : stdlib.lib

stdlib.lib : $(STDIOTARGETS) $(STDLIBTARGETS) $(SIGNALTARGETS)
	$(SLINK) from $(STDIOTARGETS) $(STDLIBTARGETS) $(SIGNALTARGETS) to $@

%.o : %.s
	$(ASM) $<

%.s : %.ll
	$(M68KBACK) $< $@

%.ll : ..\..\PDCLib\functions\stdio\%.c
	$(CLANG) $<

%.ll : ..\..\PDCLib\functions\stdlib\%.c
	$(CLANG) $<

%.ll : ..\..\PDCLib\functions\signal\%.c
	$(CLANG) $<
