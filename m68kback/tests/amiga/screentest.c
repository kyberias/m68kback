/* Screen / window test */
#include <intuition/intuition.h>
#include <stdio.h>

#include <proto/exec.h>
#include <proto/intuition.h>

struct NewWindow nw = {
	100, 100,			/* Starting corner */
	300, 100,		/* Width, height */
	2, 1,			/* detail, block pens */
	CLOSEWINDOW | NEWSIZE,		/* IDCMP flags */
	WINDOWDEPTH | WINDOWDRAG | WINDOWCLOSE | GIMMEZEROZERO | WINDOWSIZING,
	/* Window flags */
	NULL,			/* Pointer to first gadget */
	NULL,			/* Pointer to checkmark */
	"Screen Test",	/* title */
	NULL,			/* screen pointer */
	NULL,			/* bitmap pointer */
	50, 50, 640, 400,		/* window not sized */
	CUSTOMSCREEN		/* type of screen */
};

struct NewScreen ns = {
	0, 0, 640, 400, 4,
	0, 1,
	HIRES,
	CUSTOMSCREEN,
	NULL,
	"Screen Test Screen",
	NULL, NULL
};

int get_message(struct Window *window);

int main(argc, argv)
int argc;
char *argv[];
{
	struct Window *Window;
	struct Screen *Screen;

	/*if (!(IntuitionBase = OpenLibrary("intuition.library", 37)))
	{
		printf("Can't open Intuition library\n");
		return 5;
	}

	if (!(GfxBase = OpenLibrary("graphics.library", 37)))
	{
		printf("Can't open Graphics library\n");
		CloseLibrary(IntuitionBase);
		return 5;
	}*/

	Screen = OpenScreen(&ns);
	printf("Screen = %08X\n", Screen);
	printf("Screen RastPort = %08X\n", &Screen->RastPort);
	printf("Screen RastPort Bitmap = %08X\n", &Screen->RastPort.BitMap);
	printf("Screen RastPort Bitmap Plane 0 = %08X\n", &Screen->RastPort.BitMap->Planes[0]);
	printf("Screen Bitmap = %08X\n", &Screen->BitMap);
	printf("Screen Bitmap Plane 0 = %08X\n", &Screen->BitMap.Planes[0]);

	nw.Screen = Screen;

	Window = OpenWindow(&nw);

	printf("Window = %08X\n", Window);
	printf("Window RastPort = %08X\n", Window->RPort);
	printf("Window RastPort Bitmap = %08X\n", Window->RPort->BitMap);
	if (Window->RPort->BitMap)
	{
		printf("Window RastPort Bitmap Plane 0 = %08X\n", Window->RPort->BitMap->Planes[0]);
	}

	get_message(Window);

	CloseWindow(Window);

	CloseScreen(Screen);

	printf("Done and done\n");

	/*CloseLibrary(GfxBase);
	CloseLibrary(IntuitionBase);*/

	return 0;
}

int get_message(struct Window *window)
{
	ULONG class;
	USHORT code;
	struct IntuiMessage *Message;

	while (1)
	{
		WaitPort(window->UserPort);
		if (Message = (struct IntuiMessage*)GetMsg(window->UserPort))
		{
			class = Message->Class;
			code = Message->Code;
			ReplyMsg(Message);

			/* allow any key press to end program */
			if (class == CLOSEWINDOW || class == RAWKEY)
			{
				printf("closewindow or rawkey\n");
				return 1;
			}
		}
	}
}
