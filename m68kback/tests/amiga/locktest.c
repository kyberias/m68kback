#include <stdio.h>
#include <exec/exec.h>
#include <exec/tasks.h>
#include <utility/tagitem.h>

#include <proto/exec.h>
#include <proto/dos.h>

int main(int argc, char**argv)
{
	struct Process* me = (struct Process*)FindTask(NULL);
	char buffer[500];
	BPTR lock = me->pr_CurrentDir;

	printf("Cur dir: %x\n", lock);

	if (!NameFromLock(lock, buffer, 500))
	{
		printf("NameFromLock failed\n");
	}

	printf("Lock name: %s\n", buffer);

	return 0;
}
