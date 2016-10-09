#include <stdio.h>

int len(char *str)
{
	int l = 0;

	printf("len, str=%08X\n", str);
	while (*str++)
	{
		l++;
	}
	return l;
}

char* reverse(char* from, char* to)
{
	int l, i;

	printf("reverse, to=%08X\n", to);
	printf("reverse, from=%08X\n", from);
	printf("reverse, from='%s'\n", from);

	l = len(from);
	printf("l = %d\n", l);

	for (i = 0; i < l; i++)
	{
		printf("reverse, to=%08X\n", to);
		to[i] = from[l-i-1];//from[i];//'a';
		printf("reverse, to=%08X\n", to);
	}
	printf("reverse, to=%08X\n", to);
	to[i] = 0;
	printf("reverse, to=%08X\n", to);
	printf("reverse, to end=%08X\n", &to[i]);
//	printf("reverse, to *end=%d\n", to[i]);

	return to;
}

int main(int argc, char** argv)
{
	char buf[100];
	char *r;

	if (argc < 2)
	{
		printf("argc: %d\n", argc);
		return -1;
	}

	printf("len is: %d\n", len(argv[1]));

	printf("buf=%08X\n", buf);

	r = reverse(argv[1], buf);
	printf("reverse is: %08X\n", r);
	printf("reverse is: '%s'\n", r);

	return 0;
}
