#include <stdio.h>

#pragma libcall SysBase FreeMem d2 0902

//__attribute__((amiga("foobar")))
int main(int argc, char** argv)
{
	int max, i, j, mod;

	if (argc < 2)
	{
		printf("argc: %d\n", argc);
		return -1;
	}

	max = atoi(argv[1]);
	printf("max %d\n", max);

	// Print primes
	for (i = 2; i < max; i++)
	{
		for(j=i-1;j>1;j--)
		{
			mod = (i % j);
			if(mod == 0)
			{
				break;
			}
		}

		if (j == 1)
		{
			printf("+ %d\n", i);
		}
	}

	return 0;
}
