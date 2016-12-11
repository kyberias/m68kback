#include <stdio.h>

int main(int argc, char**argv)
{
	printf("Hello world!\n");
	printf("argc = %d\n", argc);
	printf("argv = %08X\n", argv);
	printf("argv[0] = %08X\n", argv[0]);
	printf("argv[0] = %s\n", argv[0]);
	printf("argv[1] = %08X\n", argv[1]);
	if (argv[1])
	{
		printf("argv[1] = %s\n", argv[1]);
	}
	printf("Exiting with %d...\n", 42);
	return 42;
}
