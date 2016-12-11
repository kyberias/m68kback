#include <stdio.h>
#include <exec/exec.h>
#include <exec/tasks.h>
#include <utility/tagitem.h>

#include <proto/exec.h>
#include <proto/dos.h>

int main(int argc, char**argv)
{
	LONG result;

	struct TagItem te = { TAG_DONE, 0 };

	printf("Running task %s ...\n", argv[1]);

	result = SystemTagList(argv[1], &te);

	printf("Result was %d\n", result);

	return 0;
}
