#include <stdio.h>
#include <exec/exec.h>
#include <exec/tasks.h>

#include <proto/exec.h>
#include <proto/dos.h>

int simpleexited = 0;

__saveds void simpletask(void)
{
	int n = 5000000;
	//printf("simpletask() entered.\n");
	// Might not be really safe to call dos.library from a task
	//Delay(10000);

	while (n--);

	//printf("simpletask() exiting...\n");
	simpleexited = 1;
}

#if 0
struct FakeMemEntry {
	ULONG fme_Reqs;
	ULONG fme_Length;
};

#define ME_TASK         0
#define ME_STACK        1
#define NUMENTRIES      2

struct FakeMemList {
	struct Node fml_Node;
	UWORD       fml_NumEntries;
	struct FakeMemEntry fml_ME[NUMENTRIES];
} TaskMemTemplate = {
		{ 0 },                                              /* Node */
		NUMENTRIES,                                         /* num entries */
		{                                                   /* actual entries: */
			{ MEMF_PUBLIC | MEMF_CLEAR, sizeof(struct Task) },    /* task */
			{ MEMF_CLEAR, 0 }                                     /* stack */
		}
};

struct Task * Create68kTask(char *name, ULONG pri, APTR initPC, ULONG stackSize)
{
	struct Task *newTask;
	struct FakeMemList fakememlist;
	struct MemList *ml;
	struct List *list;
	UWORD* pc;

	printf("Create68KTask\n");

	/* round the stack up to longwords... */
	stackSize = (stackSize + 3) & ~3;

	/*
	* This will allocate two chunks of memory: task of PUBLIC
	* and stack of PRIVATE
	*/
	fakememlist = TaskMemTemplate;
	fakememlist.fml_ME[ME_STACK].fme_Length = stackSize;

	ml = (struct MemList *)AllocEntry((struct MemList *)&fakememlist);

	if (!ml)
	{
		printf("AllocEntry failed\n");
		return(NULL);
	}

	/* set the stack accounting stuff */
	newTask = (struct Task *) ml->ml_ME[ME_TASK].me_Addr;

	newTask->tc_SPLower = ml->ml_ME[ME_STACK].me_Addr;
	newTask->tc_SPUpper = (APTR)((ULONG)(newTask->tc_SPLower) + stackSize);
	newTask->tc_SPReg = newTask->tc_SPUpper;

	/* misc task data structures */
	newTask->tc_Node.ln_Type = NT_TASK;
	newTask->tc_Node.ln_Pri = pri;
	newTask->tc_Node.ln_Name = name;

	/* add it to the tasks memory list */
	//NewList( &newTask->tc_MemEntry );
	list = &newTask->tc_MemEntry;
	list->lh_TailPred = (struct Node *)&(list->lh_Head);
	list->lh_Tail = NULL;
	list->lh_Head = (struct Node *)&(list->lh_Tail);
	AddHead(&newTask->tc_MemEntry, (struct Node *)ml);

	/* add the task to the system -- use the default final PC */
	AddTask(newTask, initPC, 0L);
	return(newTask);
}
#endif

int main(void)
{
	char buf[200];
	struct Task* newTask;

	//printf("exit immediately.\n");
	//return 0;

//	simpletask();

//	printf("Creating a new task 0x%08X*******************************************\n", simpletask);
	newTask = CreateTask("task 1", 0L, simpletask, 4000L);
	if (!newTask)
	{
		printf("CreateTask() failed.\n");
		return 1;
	}

	//printf("printing float %s\n", "meh");
	printf("task started. wait = %d.\n", simpleexited);

	while (!simpleexited);

	printf("task exited. wait = %d\n", simpleexited);
	//	gets(buf);
	return 0;
}
