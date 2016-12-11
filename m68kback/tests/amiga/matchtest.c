#include <stdio.h>
#include <exec/exec.h>
#include <exec/tasks.h>
#include <dos/dos.h>

#include <proto/exec.h>
#include <proto/dos.h>

int main(int argc, char**argv)
{
	int error;
	struct AnchorPath* ap;

	if (argc < 2)
	{
		printf("usage: matchtest <pattern>\n");
		return 1;
	}

	ap = AllocMem(sizeof(struct AnchorPath), 0);

	error = MatchFirst(argv[1], ap);

	while (error == 0)
	{
		printf("file %s\n", ap->ap_Info.fib_FileName);

		error = MatchNext(ap);
	}

	MatchEnd(ap);

	FreeMem(ap);

	return 0;
}
