#include <stdio.h>

struct Test
{
	char* ptr;
	char buf[100];
	int a, b, c;
};

void str_copy(char* dest, char *source)
{
	while (*source)
	{
		*dest++ = *source++;
	}
	*dest = 0;
}

int main(int argc, char** argv)
{
	struct Test test;

	test.ptr = "Testing";
	test.a = 100;
	test.b = 200;
	test.c = 300;

	str_copy(test.buf, "Foobar");

	printf("%s %d %d %d", test.ptr, test.a, test.b, test.c);
	printf("%s %d %d %d", test.buf, test.a, test.b, test.c);

	return 0;
}
