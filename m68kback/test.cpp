#include <stdio.h>

class Test
{
	char* buf;

	public Test()
	{
		buf = new[500];
	}

	public ~Test()
	{
		delete[] buf;
	}

	public void Doit()
	{

	}
};

int main()
{
	Test test;
	test.Doit();

	Test* t = new Test();
	t->Doit();

	delete t;
	return 0;
}
