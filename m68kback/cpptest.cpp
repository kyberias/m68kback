#include <stdio.h>

class Test
{
	char* buf;

public:
	Test()
	{
		buf = new char[500];
	}

	~Test()
	{
		delete[] buf;
	}

	void Doit()
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
