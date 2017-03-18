#include <stdio.h>

char lotofdata[1000000];

int main(int argc, char**argv)
{
	lotofdata[999999] = 0;
	return 1;
}
