#include <stdio.h>

int id(int a)
{
	return a;
}

int x[2], y[2];

int main()
{
/*	printf("%d\n", id(1000) % 10);
	printf("%d\n", id(1001) % 10);
	printf("%d\n", id(1002) % 10);
	printf("%d\n", id(1003) % 10);
	printf("%d\n", id(1004) % 10);
	printf("\n");*/

	x[0] = id(1000) % 10;
	x[1] = id(1001) % 10;
	y[0] = id(1002) % 10;
	y[1] = id(1003) % 10;

	printf("%d\n", x[0]);
	printf("%d\n", x[1]);
	printf("%d\n", y[0]);
	printf("%d\n", y[1]);

	return 0;
}
